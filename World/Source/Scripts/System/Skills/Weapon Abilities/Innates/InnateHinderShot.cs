using System;
using Server;
using Server.Mobiles;

namespace Server.Items
{
    public class InnateHinderShot
    {
        public static void Apply(Mobile attacker, Mobile defender, double tactics)
        {
            if (attacker == null || defender == null)
                return;
            double seconds = 3 + (tactics / 60.0);
            TimeSpan duration = TimeSpan.FromSeconds(seconds);
            defender.Freeze(duration);
            int extraDamage = 15 + (int)(tactics / 6.0);
            AOS.Damage(defender, attacker, extraDamage, false, 100, 0, 0, 0, 0, 0, 0, false, true, false);
            defender.FixedParticles(0x376A, 1, 32, 0x1535, 0x0, 0, EffectLayer.Waist);
            attacker.SendMessage("Your shot hinders the enemy's movement!");
            defender.SendMessage("The shot hinders your movment!");
        }
    }
}
