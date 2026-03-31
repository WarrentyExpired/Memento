using System;
using Server.Mobiles;
using Server.Items;
namespace Server.Items
{
    public class ArmorBuster
    {
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            Mobile defender = attacker.Combatant as Mobile;
            if (defender == null || !defender.Alive || !attacker.InRange(defender, 2))
            {
                attacker.SendMessage("Target out of range.");
                return;
            }
            int cost = 25 - (int)(attacker.Skills[SkillName.Focus].Value / 20);
            int current = AdrenalineManager.GetAdrenaline(attacker);
            if (current < cost)
            {
                attacker.SendMessage("Need more Adrenaline!");
                return;
            }
            AdrenalineManager.SetAdrenaline(attacker, current - cost);
            attacker.Animate(9, 8, 1, true, false, 0);
            attacker.PlaySound(0x3B3);
            int stripAmount = defender.Resistances[0] / 4; 
            if (stripAmount > 0)
            {
                defender.Resistances[0] -= stripAmount;
                attacker.SendMessage("You shatter their armor, exposing a weakness!");
                defender.SendMessage("Your armor has been shattered!");
                defender.FixedParticles(0x37B9, 1, 15, 9502, 0, 3, EffectLayer.Waist);
                Timer.DelayCall(TimeSpan.FromSeconds(6.0), () =>
                {
                    if (defender != null)
                    {
                        defender.Resistances[0] += stripAmount;
                        defender.SendMessage("Your armor has been repaired.");
                    }
                });
            }
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon != null)
                weapon.OnHit(attacker, defender, 1.25);
        }
    }
}
