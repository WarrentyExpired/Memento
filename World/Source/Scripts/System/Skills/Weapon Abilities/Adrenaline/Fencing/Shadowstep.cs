using System;
using Server.Mobiles;
using Server.Spells;
namespace Server.Items
{
    public class Shadowstep
    {
        public static bool IsActive(Mobile m)
        {
            return m != null && m.GetStatMod("Shadowstep") != null;
        }
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            if (IsActive(attacker))
            {
                attacker.SendMessage("You are already moving with supernatural speed.");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 2))
                return;
            double hidingSkill = attacker.Skills[SkillName.Hiding].Value;
            if (hidingSkill >= 80.0)
            {
                attacker.PlaySound(0x3C4);
                attacker.FixedParticles(0x37C4, 1, 8, 9916, 0, 3, EffectLayer.Waist);
                attacker.Hidden = true;
                attacker.AllowedStealthSteps = 20 + (int)(attacker.Skills[SkillName.Stealth].Value / 5);
                attacker.Combatant = null;
                attacker.Warmode = false;
                attacker.SendMessage("You vanish into the shadows, repositioning for your next strike.");
            }
            else
            {
                attacker.PlaySound(0x2E2);
                attacker.FixedEffect(0x3779, 10, 20);
                int dexBonus = (int)(attacker.Skills[SkillName.Fencing].Value / 5);
                if (dexBonus < 10) dexBonus = 10;
                attacker.AddStatMod(new StatMod(StatType.Dex, "Shadowstep", dexBonus, TimeSpan.FromSeconds(10.0)));
                attacker.SendMessage("You move with a blur of speed, your reflexes sharpening!");
                Timer.DelayCall(TimeSpan.FromSeconds(10.0), () =>
                {
                    if (attacker.Alive)
                        attacker.SendMessage("Your supernatural speed fades.");
                });
            }
        }
    }
}
