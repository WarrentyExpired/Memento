using System;
using System.Collections;
using Server.Mobiles;
namespace Server.Items
{
    public class PunchingBag
    {
        private static Hashtable m_Table = new Hashtable();
        public static bool IsActive(Mobile m)
        {
            return m != null && m_Table.Contains(m.Serial);
        }
        public static void CheckAbsorb(Mobile defender)
        {
            if (defender == null || !IsActive(defender))
                return;
            BaseWeapon weapon = defender.Weapon as BaseWeapon;
            if (weapon != null && (weapon is IPugilistGlove || weapon is PugilistGloveWeapon))
            {
                if (Utility.RandomDouble() < 1.0) 
                {
                    defender.Stam += 10;
                    if (defender.Stam > defender.StamMax)
                        defender.Stam = defender.StamMax;
                    defender.Hits += Utility.RandomMinMax(5, 10);
                    defender.FixedEffect(0x37C4, 1, 8, 0x481, 0);
                    defender.SendMessage("You absorb the hit!");
                }
            }
            else
            {
                defender.SendMessage("You must have your gloves equipped to absorb the impact!");
            }
        }
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            int cost = 30 - (int)(attacker.Skills[SkillName.Focus].Value / 20);
            int current = AdrenalineManager.GetAdrenaline(attacker);
            if (current < cost)
            {
                attacker.SendMessage("You need atleast {0} Adrenaline to use Punching Bag.", cost);
                return;
            }
            if (IsActive(attacker))
            {
                attacker.SendMessage("You are already absorbing punishment!");
                return;
            }
            AdrenalineManager.SetAdrenaline(attacker, current - cost);
            attacker.PlaySound(0x524); 
            attacker.FixedEffect(0x376A, 9, 32, 0x481, 0); 
            attacker.SendMessage("You brace yourself, becoming a human punching bag!");
            ResistanceMod resMod = new ResistanceMod(ResistanceType.Physical, +15);
            attacker.AddResistanceMod(resMod);
            m_Table[attacker.Serial] = resMod;
            if (attacker is PlayerMobile)
            {
                attacker.Stam += 50;
                attacker.SendMessage("You take a deep breath.");
            }
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), () =>
            {
                if (m_Table.Contains(attacker.Serial))
                {
                    attacker.RemoveResistanceMod((ResistanceMod)m_Table[attacker]);
                    m_Table.Remove(attacker.Serial);
                    attacker.SendMessage("You can no longer shrug off the pain.");
                }
            });
        }
    }
}
