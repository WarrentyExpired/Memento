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
            if (AdrenalineManager.GetQueuedAbility(attacker) == 3)
            {
                attacker.SendMessage("You are already preparing to crack their skull!");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 3))
                return;
            AdrenalineManager.QueueAbility(attacker, 3);
            attacker.PlaySound(0x64F); 
            attacker.SendMessage("You prepare a devastating blow to exploit your target's fatigue.");
        }
        public static void OnHit(Mobile attacker, Mobile defender)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;
            attacker.Animate(9, 8, 1, true, false, 0);
            attacker.PlaySound(0x211); 
            double damageScalar = 1.5; 
            int missingStam = defender.StamMax - defender.Stam;
            if (missingStam > 0)
            {
                damageScalar += (missingStam / 40.0);
                if (damageScalar > 4.0)
                    damageScalar = 4.0;
                attacker.SendMessage("You exploit their exhaustion, shattering their resolve!");
                defender.FixedParticles(0, 1, 0, 9946, 1153, 0, EffectLayer.Head);
            }
            if (defender.Stam < (defender.StamMax / 2))
            {
                defender.Paralyze(TimeSpan.FromSeconds(2.0));
            }
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon != null)
                weapon.OnHit(attacker, defender, damageScalar);
        }
    }
}
