using System;
using Server;
using Server.Mobiles;
using System.Collections;

namespace Server.Items
{
    public class InnateAbilityManager
    {
        private static Hashtable m_CooldownTable = new Hashtable();
        public static TimeSpan InnateCooldown = TimeSpan.FromSeconds(5.0);
        public static void CheckInnate(BaseWeapon weapon, Mobile attacker, Mobile defender, int damage)
        {
            if (attacker == null || defender == null || !attacker.Alive || !defender.Alive)
                return;
            bool isPlayer = attacker.Player;
            bool isPet = (attacker is BaseCreature && ((BaseCreature)attacker).Controlled);
            bool isWild = (!isPlayer && !isPet);
            if (isPlayer && !MySettings.S_PlayersUseInnates)
                return;
            if (isPet && !MySettings.S_PetsUseInnates)
                return;
            if (isWild && !MySettings.S_MonstersUseInnates)
                return;
            if (m_CooldownTable.Contains(attacker))
            {
                DateTime nextAllowed = (DateTime)m_CooldownTable[attacker];
                if (DateTime.UtcNow < nextAllowed)
                    return;
            }
            double anatomy = attacker.Skills[SkillName.Anatomy].Value;
            double tactics = attacker.Skills[SkillName.Tactics].Value;
            double armsLore = attacker.Skills[SkillName.ArmsLore].Value;
            // Example: Standard 10% base + skill bonus (Max ~25% at 120 skills)
            double procChance = 0.10 + ((armsLore) / 800);
            if (Utility.RandomDouble() < procChance)
            {
                m_CooldownTable[attacker] = DateTime.UtcNow + InnateCooldown;
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
                else if (weapon is IPugilistGlove)
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
            InnateConcussiveBlow.Apply(attacker, defender, tactics);
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
