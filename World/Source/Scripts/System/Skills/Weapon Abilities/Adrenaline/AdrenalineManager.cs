using System;
using System.Collections;
using Server.Mobiles;
using Server.Gumps;
using Server.Network;
namespace Server.Items
{
    public class AdrenalineManager
    {
        private static Hashtable m_Table = new Hashtable();
        public static void Initialize()
        {
            new AdrenalineTimer().Start();
        }
        private class AdrenalineTimer : Timer
        {
            public AdrenalineTimer() : base(TimeSpan.FromSeconds(8.0), TimeSpan.FromSeconds(8.0))
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

            // If the value changed, refresh the Gump
            if (oldVal != val && m is PlayerMobile)
                AdrenalineGump.SendGump((PlayerMobile)m);
        }

        public static void OnHit(Mobile attacker)
        {
            if (!attacker.Player) return;

            double focus = attacker.Skills[SkillName.Focus].Value;
            
            // Gain Logic: Base 4 + up to 6 bonus from Focus (Total 10 per hit max)
            int gain = 4 + (int)(focus / 20); 
            
            SetAdrenaline(attacker, GetAdrenaline(attacker) + gain);
        }
        public static void Slice(Mobile m)
        {
            int current = GetAdrenaline(m);
            if (current <= 0) return;
            double focus = m.Skills[SkillName.Focus].Value;
            int loss = 5 - (int)(focus / 20);
            if (loss > 0)
            {
                SetAdrenaline(m, current - loss);
                m.SendMessage("Adrenaline Decaying: -{0}", loss);
            }
        }
    }
}
