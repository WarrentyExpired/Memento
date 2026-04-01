using System;
using Server.Mobiles;
using Server.Items;

namespace Server.Items
{
    public class Trueflight
    {
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            if (AdrenalineManager.GetQueuedAbility(attacker) == 3)
            {
                attacker.SendMessage("You are already centering yourself for a Trueflight shot.");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 3))
                return;
            AdrenalineManager.QueueAbility(attacker, 3);
            attacker.PlaySound(0x64F); 
            attacker.SendMessage("You focus your breathing, preparing a perfect long-range shot.");
        }
        public static void OnHit(Mobile attacker, Mobile defender)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon == null) return;
            attacker.Animate(9, 8, 1, true, false, 0);
            attacker.PlaySound(0x234);
            attacker.MovingParticles(defender, 0x1BFE, 10, 0, false, true, 0, 0, 9502, 1, 0, EffectLayer.Waist, 0);
            int dist = (int)attacker.GetDistanceToSqrt(defender);
            double damageScalar = 1.5 + (Math.Max(0, dist - 2) * 0.2);
            if (damageScalar > 4.0)
                damageScalar = 4.0;
            attacker.SendMessage("You let fly a perfect shot from {0} tiles away!", dist);
            if (Hawkeye.IsActive(attacker))
            {
                attacker.SendMessage("Your Hawkeye focus makes the shot find the heart!");
                defender.FixedParticles(0x373A, 10, 30, 5024, EffectLayer.Waist);
                damageScalar += 0.5;
            }
            weapon.OnHit(attacker, defender, damageScalar);
        }
    }
}
