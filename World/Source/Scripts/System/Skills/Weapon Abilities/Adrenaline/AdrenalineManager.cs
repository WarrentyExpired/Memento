using System;
using System.Collections;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Gumps;
using Server.Commands;
namespace Server.Items
{
    public class AdrenalineManager
    {
        private static Hashtable m_Table = new Hashtable();
        
        private static Dictionary<Mobile, int> m_ActiveAbility = new Dictionary<Mobile, int>();
        public static void Initialize()
        {
            CommandSystem.Register("Adrenaline1", AccessLevel.Player, new CommandEventHandler(AdrenalineCommands.OnAbility1));
            CommandSystem.Register("Adrenaline2", AccessLevel.Player, new CommandEventHandler(AdrenalineCommands.OnAbility2));
            CommandSystem.Register("Adrenaline3", AccessLevel.Player, new CommandEventHandler(AdrenalineCommands.OnAbility3));
            new AdrenalineTimer().Start();
        }
        private class AdrenalineTimer : Timer
        {
            public AdrenalineTimer() : base(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0))
            {
                Priority = TimerPriority.OneSecond;
            }
            protected override void OnTick()
            {
                if (m_Table.Count == 0) return;
                List<Mobile> targets = new List<Mobile>();
                foreach (Mobile m in m_Table.Keys)
                    targets.Add(m);
                foreach (Mobile m in targets)
                {
                    if (m != null && !m.Deleted)
                        AdrenalineManager.Slice(m);
                }
            }
        }
        public static int GetCost(Mobile m, int slot)
        {
            int baseCost = 20;
            if (slot == 2) baseCost = 30;
            else if (slot == 3) baseCost = 50;
            if (m != null)
            {
                double focus = m.Skills[SkillName.Focus].Value;
                baseCost -= (int)(focus / 10.0); 
            }
            return Math.Max(5, baseCost);
        }
        public static bool HasAndConsume(Mobile m, int slot)
        {
            int cost = GetCost(m, slot);
            int current = GetAdrenaline(m);
            if (current >= cost)
            {
                SetAdrenaline(m, current - cost);
                return true;
            }
            m.SendMessage("You need {0} Adrenaline to use this ability!", cost);
            return false;
        }
        public static void QueueAbility(Mobile m, int slot)
        {
            if (m != null)
                m_ActiveAbility[m] = slot;
        }
        public static int GetQueuedAbility(Mobile m)
        {
            if (m != null && m_ActiveAbility.ContainsKey(m))
                return m_ActiveAbility[m];
            return 0;
        }
        public static void ClearAbility(Mobile m)
        {
            if (m != null)
            m_ActiveAbility.Remove(m);
        }
        // --- THE OFFENSIVE SENSOR (From BaseWeapon) ---
        public static void ProcessOffense(Mobile attacker, Mobile defender, BaseWeapon weapon)
        {
            if (attacker == null || defender == null || weapon == null)
                return;
            int queuedSlot = GetQueuedAbility(attacker);
            if (queuedSlot > 0)
            {
                ClearAbility(attacker);
                switch (weapon.Skill)
                {
                    case SkillName.Bludgeoning:
                        if (queuedSlot == 1) ArmorBuster.OnHit(attacker, defender);
                        else if (queuedSlot == 3)
                            Skullcracker.OnHit(attacker, defender);
                        break;
                    case SkillName.Swords:
                        if (queuedSlot == 1)
                            Cleave.OnHit(attacker, defender);
                        else if (queuedSlot == 3)
                            ExecutionerStrike.OnHit(attacker, defender);
                        break;
                    case SkillName.Marksmanship:
                        if (queuedSlot == 1)
                            CripplingShot.OnHit(attacker, defender);
                        else if (queuedSlot == 3)
                            Trueflight.OnHit(attacker, defender);
                        break;
                    case SkillName.FistFighting:
                        if (queuedSlot == 1)
                            KidneyShot.OnHit(attacker, defender);
                        else if (queuedSlot == 3)
                            Haymaker.OnHit(attacker, defender);
                        break;
                    case SkillName.Fencing:
                        if (queuedSlot == 1)
                            VipersStrike.OnHit(attacker, defender);
                        else if (queuedSlot == 3)
                            AortasStrike.OnHit(attacker, defender);
                        break;
                }
                return;
            }
            // --- Standard (None ability) hits 
            if (attacker.Player)
            {
                if (Hawkeye.IsActive(attacker))
                {
                    Hawkeye.CheckDamage(attacker, defender, weapon);
                }
                if (weapon.Skill == SkillName.FistFighting && !(weapon is IPugilistGlove))
                {
                    return;
                }
                double speedGain = weapon.Speed;// / 5.0;
                double focusGain = attacker.Skills[SkillName.Focus].Value / 10.0;
                int totalGain = (int)(speedGain + focusGain);
                SetAdrenaline(attacker, GetAdrenaline(attacker) + Math.Max(2, totalGain));
            }
        }
        // --- THE DEFENSIVE SENSOR (From PlayerMobile) ---
        public static void ProcessDefense(PlayerMobile defender, Mobile attacker, int damage)
        {
            if (defender == null || !defender.Alive)
                return;
            BaseWeapon weapon = defender.Weapon as BaseWeapon;
            if (weapon == null || (weapon.Skill == SkillName.FistFighting && !(weapon is IPugilistGlove)))
            {
                return;
            }
            // 1. Small "Pain" Bump: 1 Adrenaline per 10 damage taken
            if (damage > 0)
            {
                int bump = Math.Max(1, damage / 10);
                SetAdrenaline(defender, GetAdrenaline(defender) + bump);
            }
            if (Riposte.IsActive(defender))
            {
                Riposte.CheckCounter(defender, attacker);
            }
            if (PunchingBag.IsActive(defender))
            {
                PunchingBag.CheckAbsorb(defender);
            }
        }
        // --- UTILITY: GET / SET / SLICE ---
        public static int GetAdrenaline(Mobile m)
        {
            if (m == null || !m_Table.Contains(m)) return 0;
            return (int)m_Table[m];
        }
        public static void SetAdrenaline(Mobile m, int val)
        {
            if (m == null) return;
            val = Math.Min(100, Math.Max(0, val));
            m_Table[m] = val;
            if (m is PlayerMobile)
                AdrenalineGump.SendGump((PlayerMobile)m);
        }
        public static void Slice(Mobile m)
        {
            int current = GetAdrenaline(m);
            if (current > 0)
                SetAdrenaline(m, current - 1);
        }

        // --- COMMANDS (Traffic Redirection) ---
        public class AdrenalineCommands
        {
            public static void OnAbility1(CommandEventArgs e)
            {
                PlayerMobile pm = e.Mobile as PlayerMobile;
                if (pm == null || !pm.Alive) return;
                   BaseWeapon weapon = pm.Weapon as BaseWeapon;
                if (weapon == null) return;
                switch (weapon.Skill)
                {
                    case SkillName.Bludgeoning:
                        ArmorBuster.OnUse(pm); break;
                    case SkillName.Swords:
                        Cleave.OnUse(pm); break;
                    case SkillName.Fencing:
                        VipersStrike.OnUse(pm); break;
                    case SkillName.Marksmanship:
                       CripplingShot.OnUse(pm); break;
                    case SkillName.FistFighting:
                        KidneyShot.OnUse(pm); break;
                }
            }
            public static void OnAbility2(CommandEventArgs e)
            {
                PlayerMobile pm = e.Mobile as PlayerMobile;
                if (pm == null || !pm.Alive) return;
                BaseWeapon weapon = pm.Weapon as BaseWeapon;
                if (weapon == null) return;
                switch (weapon.Skill)
                {
                    case SkillName.Bludgeoning:
                        IronWill.OnUse(pm); break;
                    case SkillName.Swords:
                        Riposte.OnUse(pm); break;
                    case SkillName.Fencing:
                        Shadowstep.OnUse(pm); break;
                    case SkillName.Marksmanship:
                        Hawkeye.OnUse(pm); break;
                    case SkillName.FistFighting:
                        PunchingBag.OnUse(pm); break;
                }
            }
            public static void OnAbility3(CommandEventArgs e)
            {
                PlayerMobile pm = e.Mobile as PlayerMobile;
                if (pm == null || !pm.Alive) return;
                BaseWeapon weapon = pm.Weapon as BaseWeapon;
                if (weapon == null) return;
                switch (weapon.Skill)
                {
                    case SkillName.Bludgeoning:
                        Skullcracker.OnUse(pm); break;
                    case SkillName.Swords:
                        ExecutionerStrike.OnUse(pm); break;
                    case SkillName.Fencing:
                        AortasStrike.OnUse(pm); break;
                    case SkillName.Marksmanship:
                        Trueflight.OnUse(pm); break;
                    case SkillName.FistFighting:
                        Haymaker.OnUse(pm); break;
                }
            }
        }
    }
}
