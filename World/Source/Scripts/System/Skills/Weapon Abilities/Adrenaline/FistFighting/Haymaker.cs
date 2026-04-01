using System;
using System.Collections;
using Server.Mobiles;
using Server.Items;
namespace Server.Items
{
    public class Haymaker
    {
        private static Hashtable m_Table = new Hashtable();
        public static void OnUse(PlayerMobile attacker)
        {
            if (attacker == null || !attacker.Alive)
                return;
            int current = AdrenalineManager.GetAdrenaline(attacker);
            int cost = 50 - (int)(attacker.Skills[SkillName.Focus].Value / 20);
            if (current < cost)
            {
                attacker.SendMessage("You need atleast {0} Adrenaline to use Haymaker.", cost);
                return;
            }
            if (m_Table.Contains(attacker))
            {
                attacker.SendMessage("You are already winding up a Haymaker.");
                return;
            }
            m_Table[attacker] = true;
            attacker.SendMessage("You ready a devastating Haymaker!");
            
            attacker.FixedParticles(0x377A, 1, 32, 0x5022, 0x480, 0, EffectLayer.Waist);
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), () =>
            {
                if (m_Table.Contains(attacker))
                {
                    m_Table.Remove(attacker);
                    attacker.SendMessage(" You lose your opening for a Haymaker.");
                }
            });
        }
        public static bool IsActive(Mobile m)
        {
            return m != null && m_Table.Contains(m);
        }
        public static void CheckFinish(Mobile attacker, Mobile defender)
        {
            if (attacker == null || defender == null || !IsActive(attacker))
                return;
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon != null && (weapon is IPugilistGlove || weapon is PugilistGloveWeapon))
            {
                int missingStam = defender.StamMax - defender.Stam;
                int adrenaline = AdrenalineManager.GetAdrenaline(attacker);
                double multiplier = 0.5 + (adrenaline / 100.0); 
                int damage = (int)(missingStam * multiplier) + 15;
                if (damage > 80) damage = 80;
                attacker.SendMessage("HAYMAKER!");
                defender.SendMessage("You are nearly knocked unconscious by a Haymaker!");
                attacker.PlaySound(0x23B);
                defender.FixedParticles(0x37B9, 1, 16, 0x1, 0x480, 0, EffectLayer.Waist);
                AOS.Damage(defender, attacker, damage, 100, 0, 0, 0, 0);
                AdrenalineManager.SetAdrenaline(attacker, 0);
                m_Table.Remove(attacker);
            }
            else
            {
                attacker.SendMessage("You must have your gloves equipped to deliver a Haymaker!");
                m_Table.Remove(attacker);
            }
        }
    }
}
