using System;
using Server;
using Server.Mobiles;

namespace Server.Items
{
    public class InnatePierce
    {
        public static void Apply(Mobile attacker, Mobile defender, double tactics)
        {
            if (attacker == null || defender == null)
                return;

            // 1. Damage Calculation
            // Base bonus damage that scales with Arms Lore (15 to 35)
            int bonusDamage = 15 + (int)(tactics / 6.0);

            // 2. The "Sunder" Effect (Resistance Debuff)
            // Lowers Physical Resistance by 20 for 6 seconds
            // This makes the "Armor Ignore" feel like it actually broke the armor.
            if (defender is BaseCreature)
            {
                ResistanceMod mod = new ResistanceMod(ResistanceType.Physical, -20);
                defender.AddResistanceMod(mod);
                
                // Remove the debuff after 6 seconds
                Timer.DelayCall(TimeSpan.FromSeconds(6.0), delegate 
                { 
                    defender.RemoveResistanceMod(mod);
                    attacker.SendMessage("Your enemy fixed thier armor.");
                });
            }

            // 3. Effects and Sounds
            attacker.SendMessage("You pierce through a weak point in their armor!");
            
            defender.FixedParticles(0x3728, 1, 13, 0x480, 0, 0, EffectLayer.Waist);
            AOS.Damage(defender, attacker, bonusDamage, false, 0, 0, 0, 0, 0, 0, 100, false, false, false);
        }
    }
}
