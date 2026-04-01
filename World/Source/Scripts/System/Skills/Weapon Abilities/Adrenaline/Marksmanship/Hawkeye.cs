using System;
using System.Collections;
using Server.Mobiles;
namespace Server.Items
{
    public class Hawkeye
    {
        private static Hashtable m_Table = new Hashtable();
        public static bool IsActive(Mobile m)
        {
            return m != null && m_Table.Contains(m);
        }
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            int cost = 30 - (int)(attacker.Skills[SkillName.Focus].Value / 20);
            int current = AdrenalineManager.GetAdrenaline(attacker);
            if (current < cost)
            {
                attacker.SendMessage("You need atleast {0} Adrenaline to use Hawkeye.", cost);
                return;
            }
            if (IsActive(attacker))
            {
                attacker.SendMessage("Your vision is already focused.");
                return;
            }
            AdrenalineManager.SetAdrenaline(attacker, current - cost);
            attacker.PlaySound(0x2E6); 
            attacker.FixedEffect(0x375A, 10, 20); 
            attacker.SendMessage("Your vision sharpens, highlighting enemy weaknesses.");
            m_Table[attacker] = true;
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), () =>
            {
                if (m_Table.Contains(attacker))
                {
                    m_Table.Remove(attacker);
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
                // AOS.Damage(defender, attacker, damage, phys, fire, cold, pois, nrgy, bonus, etc)
                AOS.Damage(defender, attacker, extraDamage, false, 0, 0, 0, 0, 0, 0, 100, false, true, false);
                attacker.SendMessage("Your precise aim finds a gap in thier armor!");
                defender.FixedEffect(0x37B9, 1, 16, 0x1, 0);
            }
        }
    }
}
