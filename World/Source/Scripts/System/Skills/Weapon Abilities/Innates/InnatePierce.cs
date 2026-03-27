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
            int bonusDamage = 15 + (int)(tactics / 6.0);
            if (defender is BaseCreature)
            {
                ResistanceMod mod = new ResistanceMod(ResistanceType.Physical, -20);
                defender.AddResistanceMod(mod);
                Timer.DelayCall(TimeSpan.FromSeconds(6.0), delegate 
                { 
                    defender.RemoveResistanceMod(mod);
                    attacker.SendMessage("Your enemy fixed thier armor.");
                });
            }
            attacker.SendMessage("You pierce through a weak point in their armor!");
            defender.FixedParticles(0x3728, 1, 13, 0x480, 0, 0, EffectLayer.Waist);
            AOS.Damage(defender, attacker, bonusDamage, false, 0, 0, 0, 0, 0, 0, 100, false, false, false);
        }
    }
}
