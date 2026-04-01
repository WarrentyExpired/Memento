using System;
using Server.Mobiles;
using Server.Items;
namespace Server.Items
{
    public class CripplingShot
    {
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            Mobile defender = attacker.Combatant as Mobile;
            if (defender == null || !defender.Alive || !attacker.InRange(defender, 12))
            {
                attacker.SendMessage("You must have a target in range to cripple them!");
                return;
            }
            // --- ADRENALINE COST ---
            int cost = 20 - (int)(attacker.Skills[SkillName.Focus].Value / 20);
            int current = AdrenalineManager.GetAdrenaline(attacker);
            if (current < cost)
            {
                attacker.SendMessage("You need atleast {0} Adrenaline to use Crippling Shot!", cost);
                return;
            }
            AdrenalineManager.SetAdrenaline(attacker, current - cost);
            attacker.PlaySound(0x234); 
            defender.FixedEffect(0x376A, 9, 32, 1168, 0); // Green sparkle effect
            int dexReduction = (int)(attacker.Skills[SkillName.Focus].Value / 4);
            if (dexReduction < 10) dexReduction = 10;
            defender.AddStatMod(new StatMod(StatType.Dex, "CripplingShot", -dexReduction, TimeSpan.FromSeconds(6.0)));
            attacker.SendMessage("Your shot cripples the enemy's movement!");
            defender.SendMessage("You have been crippled, slowing your actions!");
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon != null)
            {
                weapon.OnHit(attacker, defender, 1.5);
            }
        }
    }
}
