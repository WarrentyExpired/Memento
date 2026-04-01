using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Items;
namespace Server.Items
{
    public class Haymaker
    {
        private static HashSet<Mobile> m_ActiveSet = new HashSet<Mobile>();
        public static bool IsActive(Mobile m)
        {
            return m != null && m_ActiveSet.Contains(m);
        }
        public static void OnUse(Mobile attacker)
        {
            if (attacker == null || !attacker.Alive) return;
            if (attacker.Weapon == null || !(attacker.Weapon is IPugilistGlove))
            {
                attacker.SendMessage("You must be wearing Pugilist Gloves to deliver a Haymaker!");
                return;
            }
            if (AdrenalineManager.GetQueuedAbility(attacker) == 3)
            {
                attacker.SendMessage("You are already winding up a Haymaker!");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 3))
                return;
            AdrenalineManager.QueueAbility(attacker, 3);
            attacker.SendMessage("You ready a devastating Haymaker!");
            attacker.FixedParticles(0x377A, 1, 32, 0x5022, 0x480, 0, EffectLayer.Waist);
            Timer.DelayCall(TimeSpan.FromSeconds(10.0), () =>
            {
                if (AdrenalineManager.GetQueuedAbility(attacker) == 3)
                {
                    AdrenalineManager.ClearAbility(attacker);
                    attacker.SendMessage("You lose your opening for a Haymaker.");
                }
            });
        }
        public static void OnHit(Mobile attacker, Mobile defender)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;
            if (attacker.Weapon == null || !(attacker.Weapon is IPugilistGlove))
            {
                m_ActiveSet.Remove(attacker);
                return;
            }
            int missingStam = defender.StamMax - defender.Stam;
            int currentAdrenaline = AdrenalineManager.GetAdrenaline(attacker);
            
            double multiplier = 0.5 + (currentAdrenaline / 100.0); 
            int bonusDamage = (int)(missingStam * multiplier) + 15;
            if (bonusDamage > 80) bonusDamage = 80;
            attacker.SendMessage("HAYMAKER!");
            attacker.PlaySound(0x23B);
            defender.FixedParticles(0x37B9, 1, 16, 0x1, 0x480, 0, EffectLayer.Waist);
            defender.Damage(bonusDamage, attacker);
            AdrenalineManager.SetAdrenaline(attacker, 0);
            m_ActiveSet.Remove(attacker);
        }
    }
}
