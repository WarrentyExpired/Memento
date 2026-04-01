using System;
using Server.Mobiles;
using Server.Items;
namespace Server.Items
{
    public class AortasStrike
    {
        public static void OnUse(Mobile attacker)
        {
            if (attacker == null || !attacker.Alive) return;
            if (AdrenalineManager.GetQueuedAbility(attacker) == 3)
            {
                attacker.SendMessage("You are already aiming for their vitals!");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 3))
                return;
            AdrenalineManager.QueueAbility(attacker, 3);
            attacker.PlaySound(0x64F); 
            attacker.SendMessage("You prepare a precise strike to a vital artery.");
        }
        public static void OnHit(Mobile attacker, Mobile defender)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon == null) return;
            attacker.PlaySound(0x211);
            attacker.Animate(9, 8, 1, true, false, 0);
            defender.FixedParticles(0x377A, 244, 25, 9533, 33, 0, EffectLayer.Waist);
            double skillSum = attacker.Skills[SkillName.Fencing].Value + attacker.Skills[SkillName.Anatomy].Value;
            int bleedTick = (int)(skillSum / 15); // e.g. 120/120 = 16 damage per tick
            attacker.SendMessage("You slice a vital artery!");
            defender.SendMessage("You are bleeding uncontrollably from a vital wound!");
            int ticks = 0;
            Timer.DelayCall(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0), 5, () =>
            {
                if (defender != null && defender.Alive)
                {
                    defender.Damage(bleedTick, attacker);
                    defender.PlaySound(0x133);
                    defender.FixedParticles(0x377A, 1, 15, 9533, 33, 0, EffectLayer.Waist);
                }
            });
            weapon.OnHit(attacker, defender, 1.5);
        }
    }
}
