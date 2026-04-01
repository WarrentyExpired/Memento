using System;
using Server.Mobiles;
using Server.Items;

namespace Server.Items
{
    public class Skullcracker
    {
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            Mobile defender = attacker.Combatant as Mobile;

            if (defender == null || !defender.Alive || !attacker.InRange(defender, 2))
            {
                attacker.SendMessage("Target is out of reach.");
                return;
            }

            int cost = 50 - (int)(attacker.Skills[SkillName.Focus].Value / 20);
            int current = AdrenalineManager.GetAdrenaline(attacker);

            if (current < cost)
            {
                attacker.SendMessage("You need atleast {0} Adrenaline to use Skullcracker.", cost);
                return;
            }

            AdrenalineManager.SetAdrenaline(attacker, current - cost);

            attacker.Animate(9, 8, 1, true, false, 0);
            attacker.PlaySound(0x211); 

            // --- DAMAGE CALCULATION ---
            // Base is 1.5x damage
            double damageScalar = 1.5; 

            // Calculate missing stamina
            int missingStam = defender.StamMax - defender.Stam;

            if (missingStam > 0)
            {
                // Every 10 points of missing stamina adds 0.25 to the multiplier
                // Example: Target is missing 60 stamina? +1.5 scalar (3.0x total)
                damageScalar += (missingStam / 40.0);

                // Cap the scalar so it doesn't become infinite (Max 4.0x)
                if (damageScalar > 4.0)
                    damageScalar = 4.0;

                attacker.SendMessage("You exploit their exhaustion, shattering their resolve!");
                defender.FixedParticles(0, 1, 0, 9946, 1153, 0, EffectLayer.Head);
            }

            // If they are missing more than half their stamina, add the Stun
            if (defender.Stam < (defender.StamMax / 2))
            {
                defender.Paralyze(TimeSpan.FromSeconds(2.0));
                defender.SendMessage("The blow leaves you completely dazed!");
            }

            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon != null)
                weapon.OnHit(attacker, defender, damageScalar);
        }
    }
}
