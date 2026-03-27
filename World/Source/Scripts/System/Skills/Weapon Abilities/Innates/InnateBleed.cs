using System;
using System.Collections;
using Server.Mobiles;
using Server.Network;
namespace Server.Items
{
    public class InnateBleed
    {
        private static Hashtable m_Table = new Hashtable();
        public static bool IsBleeding(Mobile m)
        {
            return m_Table.Contains(m);
        }
        public static void Apply(Mobile attacker, Mobile defender, double anatomy)
        {
            if (attacker == null || defender == null)
                return;
            if (defender is BaseCreature && ((BaseCreature)defender).BleedImmune)
                return;
            attacker.SendMessage("Your blade opens a deep, jagged wound!");
            defender.SendMessage("You are wounded and start bleeding!");
            Timer t = (Timer)m_Table[defender];
            if (t != null)
                t.Stop();
            t = new InternalTimer(attacker, defender, anatomy);
            m_Table[defender] = t;
            t.Start();
        }
        private class InternalTimer : Timer
        {
            private Mobile m_From;
            private Mobile m_Mobile;
            private int m_Count;
            private double m_Anatomy;
            public InternalTimer(Mobile from, Mobile m, double anatomy) : base(TimeSpan.FromSeconds(2.0), TimeSpan.FromSeconds(2.0))
            {
                m_From = from;
                m_Mobile = m;
                m_Anatomy = anatomy;
                m_Count = 5;
            }
            protected override void OnTick()
            {
                if (!m_Mobile.Alive)
                {
                    Stop();
                    m_Table.Remove(m_Mobile);
                    return;
                }
                int damage = (int)(m_Anatomy / 10.0) + Utility.Random(5);
                m_Mobile.PlaySound(0x133);
                AOS.Damage(m_Mobile, m_From, damage, false, 100, 0, 0, 0, 0, 0, 0, false, false, false);
                Blood blood = new Blood();
                blood.ItemID = Utility.Random(0x122A, 5);
                blood.MoveToWorld(m_Mobile.Location, m_Mobile.Map);
                m_Count--;
                if (m_Count <= 0)
                {
                    Stop();
                    m_Table.Remove(m_Mobile);
                }
            }
        }
    }
}
