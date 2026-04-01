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
            Mobile defender = attacker.Combatant as Mobile;
            if (defender == null || !defender.Alive || !attacker.InRange(defender, 12))
            {
                attacker.SendMessage("You must have a target in sight for a Trueflight shot!");
                return;
            }
            int cost = 50 - (int)(attacker.Skills[SkillName.Focus].Value / 20);
            int current = AdrenalineManager.GetAdrenaline(attacker);
            if (current < cost)
            {
                attacker.SendMessage("You need atleast {0} Adrenaline to use Trueflight.", cost);
                return;
            }
            AdrenalineManager.SetAdrenaline(attacker, current - cost);
            attacker.Animate(9, 8, 1, true, false, 0);
            attacker.PlaySound(0x234);
            int dist = (int)attacker.GetDistanceToSqrt(defender);
            double damageScalar = 1.5 + (Math.Max(0, dist - 2) * 0.2);
            if (damageScalar > 4.0)
                damageScalar = 4.0;
            attacker.SendMessage("You let fly a perfect shot from {0} tiles away!", dist);
            if (Hawkeye.IsActive(attacker))
            {
                attacker.SendMessage("Your Hawkeye focus makes the shot find the heart!");
                BaseWeapon weapon = attacker.Weapon as BaseWeapon;
                int damage = (int)(weapon.MaxDamage * damageScalar);
                AOS.Damage(defender, attacker, damage, 0, 0, 0, 0, 0);
                
                defender.FixedParticles(0x373A, 10, 30, 5024, EffectLayer.Waist);
            }
            else
            {
                BaseWeapon weapon = attacker.Weapon as BaseWeapon;
                if (weapon != null)
                    weapon.OnHit(attacker, defender, damageScalar);
            }
            
            attacker.MovingParticles(defender, 0x1BFE, 10, 0, false, true, 0, 0, 9502);
        }
    }
}
