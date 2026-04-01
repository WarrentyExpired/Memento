using System;
using Server.Mobiles;
using Server.Items;
namespace Server.Items
{
    public class ExecutionerStrike
    {
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            if (AdrenalineManager.GetQueuedAbility(attacker) == 3)
            {
                attacker.SendMessage("You are already preparing an execution!");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 3))
                return;
            AdrenalineManager.QueueAbility(attacker, 3);
            attacker.PlaySound(0x64F); 
            attacker.SendMessage("You prepare a final, decisive strike.");
        }
        public static void OnHit(Mobile attacker, Mobile defender)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon == null) return;
            attacker.Animate(9, 8, 1, true, false, 0);
            attacker.PlaySound(0x213);
            double damageScalar = 1.75;
            if (InnateBleed.IsBleeding(defender))
            {
                defender.FixedParticles(0x377A, 244, 25, 9533, 994, 0, EffectLayer.Waist);
                attacker.SendMessage("You execute the wounded foe, consuming the bleed!");
                damageScalar += 1.0;
                InnateBleed.ClearBleed(defender);
            }
            if (defender.Hits < (defender.HitsMax / 3))
            {
                double hpPercent = (double)defender.Hits / defender.HitsMax;
                double executeBonus = (0.5 - hpPercent) * 2.0;
                damageScalar += Math.Max(0, executeBonus);
                attacker.SendMessage("Your target is weak, taking extra damage!");
            }
            weapon.OnHit(attacker, defender, damageScalar);
        }
    }
}
