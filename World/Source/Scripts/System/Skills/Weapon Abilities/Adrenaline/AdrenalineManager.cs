using System;
using System.Collections;
using Server.Mobiles;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using Server.Items;
namespace Server.Items
{
    public class AdrenalineManager
    {
        public static void OnCombatEvent(BaseWeapon weapon, Mobile attacker, Mobile defender)
        {
            if (attacker == null || defender == null)
                return;
            OnHit(attacker);
            // --- Defensive abilities section
            PlayerMobile pmDef = defender as PlayerMobile;
            if (pmDef != null)
            {
                if (PunchingBag.IsActive(pmDef))
                {
                    PunchingBag.CheckAbsorb(pmDef);
                }
                if (RiposteAbility.IsActive(pmDef))
                {
                    RiposteAbility.CheckCounter(pmDef, attacker);
                }
            }
            // --- Offensive abilities section
            if (weapon == null)
                return;
            if (weapon.Skill == SkillName.Marksmanship && Hawkeye.IsActive(attacker))
            {
                Hawkeye.CheckDamage(attacker, defender, weapon);
            }
            else if (weapon.Skill == SkillName.FistFighting)
            {
                Haymaker.CheckFinish(attacker, defender);
            }
        }
        public class AdrenalineCommands
        {
            public static void Initialize()
            {
                CommandSystem.Register("Adrenaline1", AccessLevel.Player, new CommandEventHandler(OnAbility1));
                CommandSystem.Register("Adrenaline2", AccessLevel.Player, new CommandEventHandler(OnAbility2));
                CommandSystem.Register("Adrenaline3", AccessLevel.Player, new CommandEventHandler(OnAbility3));
                new AdrenalineTimer().Start();
            }
            [Usage("Adrenaline1")]
            public static void OnAbility1(CommandEventArgs e)
            {
                HandleAbility(e.Mobile as PlayerMobile, 1);
            }
            [Usage("Adrenaline2")]
            public static void OnAbility2(CommandEventArgs e)
            {
                HandleAbility(e.Mobile as PlayerMobile, 2);
            }
            [Usage("Adrenaline3")]
            public static void OnAbility3(CommandEventArgs e)
            {
                HandleAbility(e.Mobile as PlayerMobile, 3);
            }
            private static void HandleAbility(PlayerMobile pm, int slot)
            {
                if (pm == null || !pm.Alive) return;
                BaseWeapon weapon = pm.Weapon as BaseWeapon;
                if (weapon == null) return;
                if (weapon.Skill == SkillName.Swords)
                {
                    switch (slot)
                    {
                        case 1: CleaveAbility.OnUse(pm); break;
                        case 2: RiposteAbility.OnUse(pm); break;
                        case 3: ExecutionerStrike.OnUse(pm); break;
                    }
                }
                else if (weapon.Skill == SkillName.Marksmanship)
                {
                    switch (slot)
                    {
                        case 1: CripplingShot.OnUse(pm); break;
                        case 2: Hawkeye.OnUse(pm); break;
                        case 3: Trueflight.OnUse(pm); break;
                    }
                }
                else if (weapon.Skill == SkillName.Fencing)
                {
                    pm.SendMessage("Fencing abilites coming soon!");
                }
                else if (weapon.Skill == SkillName.Bludgeoning)
                {
                    switch (slot)
                    {
                        case 1: ArmorBuster.OnUse(pm); break;
                        case 2: IronWill.OnUse(pm); break;
                        case 3: Skullcracker.OnUse(pm); break;
                    }
                }
                else if (weapon.Skill == SkillName.FistFighting)
                {
                    switch (slot)
                    {
                        case 1: KidneyShot.OnUse(pm); break;
                        case 2: PunchingBag.OnUse(pm); break;
                        case 3: Haymaker.OnUse(pm); break;
                    }
                }
                else
                {
                    pm.SendMessage("Your current weapon does not support Adrenaline abilites.");
                }
            }
        }
        private static Hashtable m_Table = new Hashtable();
        public static void Initialize()
        {
            new AdrenalineTimer().Start();
        }
        private class AdrenalineTimer : Timer
        {
            public AdrenalineTimer() : base(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10))
            {
                Priority = TimerPriority.OneSecond;
            }
            protected override void OnTick()
            {
                foreach (NetState state in NetState.Instances)
                {
                    Mobile m = state.Mobile;
                    if (m != null && m.Player && m.Alive)
                    {
                        AdrenalineManager.Slice(m);
                    }
                }
            }
        }
        public static int GetAdrenaline(Mobile m)
        {
            if (m == null || !m_Table.Contains(m)) return 0;
            return (int)m_Table[m];
        }
        public static void SetAdrenaline(Mobile m, int val)
        {
            if (m == null) return;
            if (val < 0) val = 0;
            if (val > 100) val = 100;
            int oldVal = GetAdrenaline(m);
            m_Table[m] = val;
            if (oldVal != val && m is PlayerMobile)
                AdrenalineGump.SendGump((PlayerMobile)m);
        }
        public static void OnHit(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon == null || (weapon.Skill == SkillName.FistFighting && !(weapon is IPugilistGlove)))
                return;
            double speedFactor = weapon.Speed * 2.5;
            double focusBonus = attacker.Skills[SkillName.Focus].Value / 20.0;
            int gain = (int)(speedFactor + focusBonus);
            if (gain < 5) gain = 5;
            SetAdrenaline(attacker, GetAdrenaline(attacker) + gain);
        }
        public static void Slice(Mobile m)
        {
            int current = GetAdrenaline(m);
            if (current <= 0) return;
            double focus = m.Skills[SkillName.Focus].Value;
            int loss = 5 - (int)(focus / 20);
            if (loss < 1) loss = 1;
            SetAdrenaline(m, current - loss);
        }
    }
}
