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
            int bonusDamage = 15 + (int)(tactics / 8.0);
            if (defender is BaseCreature)
            {
                ResistanceMod mod = new ResistanceMod(ResistanceType.Physical, -10);
                defender.AddResistanceMod(mod);
                Timer.DelayCall(TimeSpan.FromSeconds(6.0), delegate 
                { 
                    defender.RemoveResistanceMod(mod);
                    attacker.SendMessage("Your enemy fixed thier armor.");
                    defender.SendMessage("You fixed your armor.");
                });
            }
            attacker.SendMessage("You pierce through a weak point in their armor!");
            defender.SendMessage("Your armor has been weakened!");
            defender.FixedParticles(0x3728, 1, 13, 0x480, 0, 0, EffectLayer.Waist);
            AOS.Damage(defender, attacker, bonusDamage, false, 0, 0, 0, 0, 0, 0, 100, false, false, false);
        }
    }
}
