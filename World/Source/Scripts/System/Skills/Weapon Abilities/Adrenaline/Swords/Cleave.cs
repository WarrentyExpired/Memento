using System;
using System.Collections.Generic;
using Server.Mobiles;
using Server.Items;
namespace Server.Items
{
    public class Cleave
    {
        public static void OnUse(Mobile attacker)
        {
            if (!attacker.Player || !attacker.Alive) return;
            if (AdrenalineManager.GetQueuedAbility(attacker) == 1)
            {
                attacker.SendMessage("You are already preparing a wide swing!");
                return;
            }
            if (!AdrenalineManager.HasAndConsume(attacker, 1))
                return;
            AdrenalineManager.QueueAbility(attacker, 1);
            attacker.PlaySound(0x64F); 
            attacker.SendMessage("You prepare a wide, devastating swing.");
        }
        public static void OnHit(Mobile attacker, Mobile defender)
        {
            if (attacker == null || defender == null)
                return;
            BaseWeapon weapon = attacker.Weapon as BaseWeapon;
            if (weapon == null) return;
            attacker.Animate(9, 8, 1, true, false, 0); 
            attacker.PlaySound(0x23B); 
            weapon.OnHit(attacker, defender, 1.25);
            defender.FixedEffect(0x3728, 10, 15);
            int count = 0;
            foreach (Mobile target in attacker.GetMobilesInRange(2))
            {
                if (target == attacker || target == defender || !target.Alive || !attacker.CanBeHarmful(target))
                    continue;
                if (count >= 2) break;
                attacker.DoHarmful(target);
                target.FixedEffect(0x3728, 10, 15);
                weapon.OnHit(attacker, target, 0.75);
                if (target is BaseCreature bc)
                {
                    bc.OnHarmfulSpell(attacker);
                    bc.Combatant = attacker;
                }
                count++;
            }
            
            attacker.SendMessage("Your blade cleaves through multiple foes!");
        }
    }
}
