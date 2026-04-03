using System;
using Server.Mobiles;
using Server.Items;
namespace Server.Items
{
    public class VipersStrike
    {
        public static void OnUse(Mobile attacker)
        {
            if (attacker == null || !attacker.Alive) return;
            if (AdrenalineManager.GetQueuedAbility(attacker) == 1)
            {
                attacker.SendMessage("You are already lining up for a vipers strike!");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 1))
                return;
            AdrenalineManager.QueueAbility(attacker, 1);
            attacker.PlaySound(0x64F); 
            attacker.FixedEffect(0x376A, 1, 32, 0x47, 0);
            attacker.SendMessage("You prepare a damaging vipers strike!");
        }
        public static void OnHit(Mobile attacker, Mobile defender)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon == null) return;
            double poisonSkill = attacker.Skills[SkillName.Poisoning].Value;
            bool isPoisonedBlade = (weapon.Poison != null && weapon.PoisonCharges > 0);
            if (poisonSkill >= 30.0 || isPoisonedBlade)
            {
                attacker.SendMessage("You deliver a venomous strike!");
                ResistanceMod mod = new ResistanceMod(ResistanceType.Poison, -10);
                defender.AddResistanceMod(mod);
                Timer.DelayCall(TimeSpan.FromSeconds(10.0), () => defender.RemoveResistanceMod(mod));
                Poison p = weapon.Poison ?? Poison.Lesser;
                int newLevel = Math.Min(4, p.Level + 1);
                defender.ApplyPoison(attacker, Poison.GetPoison(newLevel));
                defender.FixedParticles(0x374A, 10, 15, 5021, 0x47, 0, EffectLayer.Waist);
                attacker.PlaySound(0xDD);
                weapon.OnHit(attacker, defender);
            }
            else
            {
                attacker.SendMessage("You bypass their defenses with a precision strike!");
                defender.FixedParticles(0x3728, 10, 15, 5021, 0, 0, EffectLayer.Waist);
                
                weapon.OnHit(attacker, defender, 1.5);
            }
        }
    }
}
