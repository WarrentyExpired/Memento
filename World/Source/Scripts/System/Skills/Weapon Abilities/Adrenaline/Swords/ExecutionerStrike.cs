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
            Mobile defender = attacker.Combatant as Mobile;
            if (defender == null || !defender.Alive || !attacker.InRange(defender, 2))
            {
                attacker.SendMessage("You must be closer to your target to execute this move!");
                return;
            }
            int cost = 60;
            cost -= (int)(attacker.Skills[SkillName.Focus].Value / 20);
            int current = AdrenalineManager.GetAdrenaline(attacker);
            if (current < cost)
            {
                attacker.SendMessage("You need {0} Adrenaline for an Executioner's Strike!", cost);
                return;
            }
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon == null || weapon.Skill != SkillName.Swords)
            {
                attacker.SendMessage("You must be wielding a blade to use this ability!");
                return;
            }
            AdrenalineManager.SetAdrenaline(attacker, current - cost);
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
                attacker.SendMessage("Your target is weak taking extra damage!");
            }
            weapon.OnHit(attacker, defender, damageScalar);
        }
    }
}
