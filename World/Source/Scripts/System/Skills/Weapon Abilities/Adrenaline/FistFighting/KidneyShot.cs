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
            if (attacker.Weapon == null || !(attacker.Weapon is IPugilistGlove))
            {
                attacker.SendMessage("You must be wearing Pugilist Gloves to deliver a Kidney Shot!");
                return;
            }
            if (AdrenalineManager.GetQueuedAbility(attacker) == 1)
            {
                attacker.SendMessage("You are already lining up a kidney shot!");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 1))
                return;
            AdrenalineManager.QueueAbility(attacker, 1);
            attacker.PlaySound(0x64F); 
            attacker.SendMessage("You prepare to land a sickening blow to their midsection.");
        }
        public static void OnHit(Mobile attacker, Mobile defender)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon == null) return;
            attacker.Animate(31, 5, 1, true, false, 0); 
            attacker.PlaySound(0x133);
            defender.FixedParticles(0x37C4, 1, 8, 9916, 0x21, 0, EffectLayer.Waist);
            int stamDrain = 20 + (int)(attacker.Skills[SkillName.Anatomy].Value / 10);
            defender.Stam -= stamDrain;
            attacker.SendMessage("You land a sickening blow to their kidney!");
            weapon.OnHit(attacker, defender, 1.25);
        }
    }
}
