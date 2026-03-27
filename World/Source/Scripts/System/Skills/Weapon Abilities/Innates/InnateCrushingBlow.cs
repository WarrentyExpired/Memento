using System;
using Server;
using Server.Mobiles;
namespace Server.Items
{
    public class InnateCrushingBlow
    {
        public static void Apply(Mobile attacker, Mobile defender, double tactics)
        {
            if (attacker == null || defender == null)
                return;
            int bonusDamage = 10 + (int)(tactics / 6.0);
            defender.FixedParticles(0, 1, 0, 9946, EffectLayer.Head);
            AOS.Damage(defender, attacker, bonusDamage, false, 100, 0, 0, 0, 0, 0, 0, false, false, false);
            attacker.SendMessage("You deliver a powerful blow!");
        }
    }
}
