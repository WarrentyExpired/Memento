using System;
using Server.Network;
using Server.Multis;
using Server.Mobiles;

namespace Server.SkillHandlers
{
    public class Hiding
    {
        private static bool m_CombatOverride;

        public static bool CombatOverride
        {
            get { return m_CombatOverride; }
            set { m_CombatOverride = value; }
        }

        public static void Initialize()
        {
            SkillInfo.Table[21].Callback = new SkillUseCallback(OnUse);
        }

        public static TimeSpan OnUse(Mobile m)
        {
            if (m.Spell != null)
            {
                m.SendLocalizedMessage(501238); // You are busy doing something else and cannot hide.
                return TimeSpan.FromSeconds(1.0);
            }

            if (m.Target != null)
            {
                Targeting.Target.Cancel(m);
            }

            double bonus = 0.0;
            var outOfCombatHideSuccess = m_CombatOverride;

            if (!outOfCombatHideSuccess)
            {
                // Logic from your branch: Check for nearby houses to provide a bonus
                BaseHouse house = BaseHouse.FindHouseAt(m);

                if (house == null)
                    house = BaseHouse.FindHouseAt(new Point3D(m.X + 1, m.Y, m.Z), m.Map, 16);
                if (house == null)
                    house = BaseHouse.FindHouseAt(new Point3D(m.X, m.Y - 1, m.Z), m.Map, 16);
                if (house == null)
                    house = BaseHouse.FindHouseAt(new Point3D(m.X, m.Y + 1, m.Z), m.Map, 16);

                if (house != null)
                {
                    bonus = 50.0;
                    // Main branch logic: Friend of house gets guaranteed success
                    if (house.IsFriend(m))
                        outOfCombatHideSuccess = true;
                }

                if (!outOfCombatHideSuccess)
                {
                    // Standard skill check with house bonus applied
                    outOfCombatHideSuccess = m.CheckSkill(SkillName.Hiding, 0.0 - bonus, 100.0 - bonus);
                }
            }

            var hidingSkill = m.Skills[SkillName.Hiding].Value;

            // Merged Range Logic: Using your dynamic range calculation
            int range = Math.Min((int)((100 - hidingSkill) / 2) + 8, 18);
            if (hidingSkill > 100)
            {
                int bonusTiles = (int)((hidingSkill - 100) / 5);
                range = Math.Max(1, range - bonusTiles);
            }

            // Check if we're fighting someone (Logic from Main)
            bool hasVisibleCombatant = !m_CombatOverride
                && m.Combatant != null
                && m.InRange(m.Combatant.Location, range)
                && m.Combatant.InLOS(m);

            if (!hasVisibleCombatant && !m_CombatOverride)
            {
                // Check if someone is fighting us
                foreach (Mobile check in m.GetMobilesInRange(range))
                {
                    if (check.InLOS(m) && check.Combatant == m)
                    {
                        hasVisibleCombatant = true;
                        break;
                    }
                }
            }

            // High skill override from your branch
            if (hidingSkill >= 100 && !hasVisibleCombatant)
                outOfCombatHideSuccess = true;

            bool success = !hasVisibleCombatant && outOfCombatHideSuccess;

            // If we failed but have > 100 skill, try the "In-Combat Hide" chance from Main
            if (!success && hidingSkill > 100)
            {
                var successChance = (hidingSkill - 100) * 0.015;
                if (hidingSkill >= 120) successChance += 0.025; 

                success = m.CheckSkill(SkillName.Hiding, successChance);
            }

            if (!success)
            {
                m.RevealingAction();

                if (hasVisibleCombatant)
                    m.LocalOverheadMessage(MessageType.Regular, 0x22, 501237); // You can't seem to hide right now.
                else
                    m.LocalOverheadMessage(MessageType.Regular, 0x22, 501241); // You can't seem to hide here.

                return TimeSpan.FromSeconds(4.0);
            }
            else
            {
                m.Hidden = true;
                m.Warmode = false;

                bool showMessage = true;
                // Auto-Stealth logic from Main
                if (hidingSkill >= 100.0)
                {
                    if (Stealth.TryStealth(m, false))
                        showMessage = false;
                }

                if (showMessage)
                    m.LocalOverheadMessage(MessageType.Regular, 0x1F4, 501240); // You have hidden yourself well.

                // Pet Hiding logic from Main
                foreach (Mobile pet in m.GetMobilesInRange(3))
                {
                    if (pet is BaseCreature bc && bc.Controlled && bc.ControlMaster == m)
                    {
                        pet.Hidden = true;
                    }
                }

                return TimeSpan.FromSeconds(4.0);
            }
        }
    }
}
