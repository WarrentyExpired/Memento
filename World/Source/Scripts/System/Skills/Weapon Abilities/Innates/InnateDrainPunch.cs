using System;
using Server;
using Server.Mobiles;

namespace Server.Items
{
    public class InnateDrainPunch
    {
        public static void Apply(Mobile attacker, Mobile defender, double anatomy)
        {
            if (attacker == null || defender == null)
                return;
            double percent = (anatomy / 800.0); 
            int manaDrain = (int)(defender.Mana * percent) + 5;
            int stamDrain = (int)(defender.Stam * percent) + 5;
            defender.Mana -= manaDrain;
            defender.Stam -= stamDrain;
            //attacker.Mana += (manaDrain / 2);
            //attacker.Stam += (stamDrain / 2);
            HitLower.ApplyDefense(defender);
            attacker.SendMessage("You deliver a draining punch!");
            defender.SendMessage("You recieve a draining punch!");
            defender.FixedParticles(0x374A, 1, 15, 5054, 0x482, 0, EffectLayer.Waist);
        }
    }
}
