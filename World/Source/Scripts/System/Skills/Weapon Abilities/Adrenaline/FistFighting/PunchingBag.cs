using System;
using System.Collections.Generic;
using Server.Mobiles;
namespace Server.Items
{
    public class PunchingBag
    {
        private static HashSet<Mobile> m_ActiveSet = new HashSet<Mobile>();
        public static bool IsActive(Mobile m)
        {
            return m != null && m_ActiveSet.Contains(m);
        }
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            if (attacker.Weapon == null || !(attacker.Weapon is IPugilistGlove))
            {
                attacker.SendMessage("You must be wearing Pugilist Gloves to brace yourself!");
                return;
            }
            if (IsActive(attacker))
            {
                attacker.SendMessage("You are already absorbing punishment!");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 2))
                return;
            attacker.PlaySound(0x524); 
            attacker.FixedEffect(0x376A, 9, 32, 0x481, 0); 
            attacker.SendMessage("You brace yourself, becoming a human punching bag!");
            attacker.Stam += 25;
            m_ActiveSet.Add(attacker);
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), () =>
            {
                if (m_ActiveSet.Contains(attacker))
                {
                    m_ActiveSet.Remove(attacker);
                    if (attacker.Alive)
                        attacker.SendMessage("You can no longer shrug off the pain.");
                }
            });
        }
        public static void CheckAbsorb(Mobile defender)
        {
            if (defender == null || !defender.Alive) return;
            if (defender.Weapon == null || !(defender.Weapon is IPugilistGlove))
            {
                m_ActiveSet.Remove(defender);
                return;
            }
            defender.Hits += Utility.RandomMinMax(5, 10);
            defender.Stam += 10;
            defender.FixedEffect(0x37C4, 1, 8, 0x481, 0);
            defender.SendMessage("You absorb the impact!");
        }
    }
}
