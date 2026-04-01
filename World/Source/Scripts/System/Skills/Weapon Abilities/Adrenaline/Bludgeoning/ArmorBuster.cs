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
            if (AdrenalineManager.GetQueuedAbility(attacker) == 1)
            {
                attacker.SendMessage("You are already preparing that move!");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 1))
                return;
            AdrenalineManager.QueueAbility(attacker, 1);
            attacker.PlaySound(0x64F);
            attacker.SendMessage("You prepare to shatter your target's armor.");
        }
        public static void OnHit(Mobile attacker, Mobile defender)
        {
            if (attacker == null || defender == null || !defender.Alive)
                return;
            attacker.Animate(9, 8, 1, true, false, 0);
            attacker.PlaySound(0x3B3);
            defender.FixedParticles(0x37B9, 1, 15, 9502, 0, 3, EffectLayer.Waist);
            int stripAmount = defender.PhysicalResistance / 4; 
            if (stripAmount > 0)
            {
                ResistanceMod mod = new ResistanceMod(ResistanceType.Physical, -stripAmount);
                defender.AddResistanceMod(mod);
                attacker.SendMessage("You shatter their armor, exposing a weakness!");
                Timer.DelayCall(TimeSpan.FromSeconds(6.0), () =>
                {
                    if (defender != null)
                    {
                        defender.RemoveResistanceMod(mod);
                        defender.SendMessage("Your armor has been repaired.");
                    }
                });
            }
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon != null)
            {
                weapon.OnHit(attacker, defender, 1.25);
            }
        }
    }
}
