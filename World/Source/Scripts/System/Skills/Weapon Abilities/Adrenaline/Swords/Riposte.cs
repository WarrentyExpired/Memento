using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Items;
namespace Server.Items
{
    public class Riposte
    {
        private static Dictionary<Mobile, int> m_Table = new Dictionary<Mobile, int>();
        public static bool IsActive(Mobile m)
        {
            return m != null && m_Table.ContainsKey(m);
        }
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            if (IsActive(attacker))
            {
                attacker.SendMessage("You are already in a defensive stance.");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 2))
                return;
            int bonus = 10 + (int)(attacker.Skills[SkillName.Swords].Value / 10);
            
            attacker.MeleeDamageAbsorb += bonus;
            attacker.MagicDamageAbsorb += bonus;
            
            attacker.SendMessage("You take a defensive stance, waiting for an opening...");
            attacker.PlaySound(0x51A); 
            attacker.FixedEffect(0x37C4, 1, 15, 0, 0); 
            m_Table[attacker] = bonus;
            Timer.DelayCall(TimeSpan.FromSeconds(6.0), () => 
            {
                StopRiposte(attacker, true);
            });
        }
        public static void CheckCounter(Mobile defender, Mobile attacker)
        {
            if (defender == null || attacker == null || !defender.Alive) return;
            defender.SendMessage("You catch them off balance and riposte!");
            defender.FixedParticles(0x3779, 1, 15, 0x158B, 0x0, 0x3, EffectLayer.Waist);
            BaseWeapon weapon = defender.Weapon as BaseWeapon;
            if (weapon != null)
            {
                weapon.OnHit(defender, attacker, 1.75);
            }
            StopRiposte(defender, false);
        }
        public static void StopRiposte(Mobile m, bool showMessage)
        {
            if (m != null && m_Table.ContainsKey(m))
            {
                int bonus = m_Table[m];
                m_Table.Remove(m);
                m.MeleeDamageAbsorb = Math.Max(0, m.MeleeDamageAbsorb - bonus);
                m.MagicDamageAbsorb = Math.Max(0, m.MagicDamageAbsorb - bonus);
                if (showMessage && m.Alive)
                    m.SendMessage("You lower your defensive guard.");
            }
        }
    }
}
