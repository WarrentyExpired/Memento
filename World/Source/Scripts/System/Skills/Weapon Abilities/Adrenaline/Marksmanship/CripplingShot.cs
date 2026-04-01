using System;
using Server.Mobiles;
using Server.Items;
namespace Server.Items
{
    public class CripplingShot
    {
        // 1. THE BUTTON CLICK: Queues the move
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            if (AdrenalineManager.GetQueuedAbility(attacker) == 1)
            {
                attacker.SendMessage("You are already lining up a crippling shot!");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 1))
                return;
            AdrenalineManager.QueueAbility(attacker, 1);
            attacker.PlaySound(0x64F); 
            attacker.SendMessage("You aim for the target's joints, preparing to cripple them.");
        }
        public static void OnHit(Mobile attacker, Mobile defender)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon == null) return;
            attacker.PlaySound(0x234); 
            defender.FixedEffect(0x376A, 9, 32, 1168, 0);
            int dexReduction = (int)(attacker.Skills[SkillName.Focus].Value / 4);
            if (dexReduction < 10) dexReduction = 10;
            defender.AddStatMod(new StatMod(StatType.Dex, "CripplingShot", -dexReduction, TimeSpan.FromSeconds(6.0)));
            attacker.SendMessage("Your shot cripples the enemy's movement!");
            weapon.OnHit(attacker, defender, 1.5);
        }
    }
}
