using System;
using Server;
using Server.Mobiles;

namespace Server.Items
{
    public class InnateAbilityManager
    {
        public static void CheckInnate(BaseWeapon weapon, Mobile attacker, Mobile defender, int damage)
        {
            if (attacker == null || defender == null || !attacker.Alive || !defender.Alive)
                return;
            double anatomy = attacker.Skills[SkillName.Anatomy].Value;
            double tactics = attacker.Skills[SkillName.Tactics].Value;
            double armsLore = attacker.Skills[SkillName.ArmsLore].Value;
            // Example: Standard 10% base + skill bonus (Max ~25% at 120 skills)
            double procChance = 0.10 + ((armsLore) / 800);
            if (Utility.RandomDouble() < procChance)
            {
                if (weapon.DefSkill == SkillName.Swords)
                {
                    ApplySwords(attacker, defender, anatomy);
                }
                else if (weapon.DefSkill == SkillName.Bludgeoning)
                {
                    ApplyBludgeoning(attacker, defender, tactics);
                }
                else if (weapon.DefSkill == SkillName.Fencing)
                {
                    ApplyFencing(attacker, defender, tactics);
                }
                else if (weapon.DefSkill == SkillName.Marksmanship)
                {
                    ApplyMarksmanship(attacker, defender, tactics);
                }
                else if (weapon.DefSkill == SkillName.FistFighting || weapon is IPugilistGlove)
                {
                    ApplyFistFighting(attacker, defender, anatomy);
                }
            }
        }

        private static void ApplySwords(Mobile attacker, Mobile defender, double anatomy)
        {
            // Swords 
            InnateBleed.Apply(attacker, defender, anatomy);
        }

        private static void ApplyBludgeoning(Mobile attacker, Mobile defender, double tactics)
        {
            // Bludgeoning
            InnateCrushingBlow.Apply(attacker, defender, tactics);
        }

        private static void ApplyFencing(Mobile attacker, Mobile defender, double tactics)
        {
            // Fencing 
            InnatePierce.Apply(attacker, defender, tactics);
        }

        private static void ApplyMarksmanship(Mobile attacker, Mobile defender, double tactics)
        {
            // Marksmanship
            InnateHinderShot.Apply(attacker, defender, tactics);
        }

        private static void ApplyFistFighting(Mobile attacker, Mobile defender, double anatomy)
        {
            // FistFighting
            InnateDrainPunch.Apply(attacker, defender, anatomy);
        }
    }
}
