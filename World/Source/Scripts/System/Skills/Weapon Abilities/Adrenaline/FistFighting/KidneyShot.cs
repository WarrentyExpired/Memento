using System;
using Server.Mobiles;
using Server.Items;
namespace Server.Items
{
    public class KidneyShot
    {
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            Mobile defender = attacker.Combatant as Mobile;
            if (defender == null || !defender.Alive || !attacker.InRange(defender, 1))
            {
                attacker.SendMessage("You must be in close quarters to land a kidney shot!");
                return;
            }
            int cost = 20 - (int)(attacker.Skills[SkillName.Focus].Value / 20);
            int current = AdrenalineManager.GetAdrenaline(attacker);
            if (current < cost)
            {
                attacker.SendMessage("You need atleast {0} Adrenaline to use Kidney Shot.", cost);
                return;
            }
            AdrenalineManager.SetAdrenaline(attacker, current - cost);
            attacker.Animate(31, 5, 1, true, false, 0); 
            attacker.PlaySound(0x133);
            int stamDrain = 20 + (int)(attacker.Skills[SkillName.Anatomy].Value / 10);
            defender.Stam -= stamDrain;
            attacker.SendMessage("You land a sickening blow to their kidney!");
            defender.SendMessage("A sharp pain in your side leaves you winded!");
            defender.FixedParticles(0x37C4, 1, 8, 9916, 0x21, 0, EffectLayer.Waist);
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon != null)
            {
                weapon.OnHit(attacker, defender, 1.25);
            }
        }
    }
}
