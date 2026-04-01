using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Items;
namespace Server.Items
{
    public class Hawkeye
    {
        private static HashSet<Mobile> m_ActiveSet = new HashSet<Mobile>();
        public static bool IsActive(Mobile m)
        {
            return m != null && m_ActiveSet.Contains(m);
        }
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            if (IsActive(attacker))
            {
                attacker.SendMessage("Your vision is already focused.");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 2))
                return;
            attacker.PlaySound(0x2E6); 
            attacker.FixedEffect(0x375A, 10, 20); 
            attacker.SendMessage("Your vision sharpens, highlighting enemy weaknesses.");
            m_ActiveSet.Add(attacker);
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), () =>
            {
                if (m_ActiveSet.Contains(attacker))
                {
                    m_ActiveSet.Remove(attacker);
                    if (attacker.Alive)
                        attacker.SendMessage("Your Hawkeye focus fades.");
                }
            });
        }
        public static void CheckDamage(Mobile attacker, Mobile defender, BaseWeapon weapon)
        {
            if (attacker == null || defender == null || weapon == null)
                return;
            double bonus = defender.PhysicalResistance / 80.0;
            int extraDamage = (int)(weapon.MaxDamage * bonus);
            if (extraDamage > 0)
            {
                defender.Damage(extraDamage, attacker);
                attacker.SendMessage("Your precise aim finds a gap in their armor!");
                defender.FixedEffect(0x37B9, 1, 16, 0x1, 0); 
            }
        }
    }
}
