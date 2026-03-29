using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Items;

namespace Server.Items
{
    public class CleaveAbility
    {
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;

            int cost = 25;
            cost -= (int)(attacker.Skills[SkillName.Focus].Value / 20);

            int current = AdrenalineManager.GetAdrenaline(attacker);
            if (current < cost)
            {
                attacker.SendMessage("You need {0} Adrenaline to Cleave!", cost);
                return;
            }

            // Find Targets
            var targets = new List<Mobile>();
            foreach (Mobile m in attacker.GetMobilesInRange(2))
            {
                if (m != attacker && m.Alive && attacker.CanBeHarmful(m))
                    targets.Add(m);
            }

            if (targets.Count == 0)
            {
                attacker.SendMessage("There is nothing nearby to cleave!");
                return;
            }

            // Pay the price
            AdrenalineManager.SetAdrenaline(attacker, current - cost);

            // 1. FIX: Cast Weapon to BaseWeapon to access OnHit
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;

            // 2. FIX: Standard Animation parameters
            // Animate( action, frameCount, repeatCount, forward, repeat, delay )
            attacker.Animate(9, 8, 1, true, false, 0); 
            attacker.PlaySound(0x23B); 

            int count = 0;
            foreach (Mobile target in targets)
            {
                if (count >= 3) break; 
                
                double damageScalar = (count == 0) ? 1.25 : 0.75;
                target.FixedEffect(0x3728, 10, 15); 
                
                // 3. FIX: Use the casted weapon variable
                if (weapon != null)
                {
                    weapon.OnHit(attacker, target, damageScalar);
                }
                
                count++;
            }
        }
    }
}
