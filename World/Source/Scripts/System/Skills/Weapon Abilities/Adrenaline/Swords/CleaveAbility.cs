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
            int cost = 20 - (int)(attacker.Skills[SkillName.Focus].Value / 20);
            int current = AdrenalineManager.GetAdrenaline(attacker);
            if (current < cost)
            {
                attacker.SendMessage("You need atleast {0} Adrenaline to use Cleave", cost);
                return;
            }
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
            AdrenalineManager.SetAdrenaline(attacker, current - cost);
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            attacker.Animate(9, 8, 1, true, false, 0); 
            attacker.PlaySound(0x23B); 
            int count = 0;
            foreach (Mobile target in targets)
            {
                if (count >= 3) break; 
                attacker.DoHarmful(target);
                double damageScalar = (count == 0) ? 1.25 : 0.75;
                target.FixedEffect(0x3728, 10, 15); 
                if (weapon != null)
                {
                    weapon.OnHit(attacker, target, damageScalar);
                }
                if (target is BaseCreature)
                {
                    BaseCreature bc = (BaseCreature)target;
                    bc.OnHarmfulSpell(attacker);
                    bc.Combatant = attacker;
                }
                count++;
            }
        }
    }
}
