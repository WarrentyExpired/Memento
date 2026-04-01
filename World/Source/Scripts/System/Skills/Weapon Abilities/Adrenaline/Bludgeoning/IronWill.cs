using System;
using Server.Mobiles;
namespace Server.Items
{
    public class IronWill
    {
        public static bool IsActive(Mobile m)
        {
            return m != null && m.GetStatMod("IronWill") != null;
        }
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            if (IsActive(attacker))
            {
                attacker.SendMessage("Your resolve is already hardened.");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 2))
                return;
            double focus = attacker.Skills[SkillName.Focus].Value;
            int strBonus = (int)(focus / 5);
            if (strBonus < 5) strBonus = 5;
            attacker.PlaySound(0x6B4); 
            attacker.FixedEffect(0x376A, 9, 32, 5015, 0); 
            attacker.SendMessage("You harden your resolve, surging with temporary strength!");
            attacker.AddStatMod(new StatMod(StatType.Str, "IronWill", strBonus, TimeSpan.FromSeconds(8.0)));
            attacker.Hits += (strBonus * 2); 
            Timer.DelayCall(TimeSpan.FromSeconds(8.0), () => 
            {
                if (attacker.Alive)
                    attacker.SendMessage("Your iron resolve fades!");
            });
        }
    }
}
