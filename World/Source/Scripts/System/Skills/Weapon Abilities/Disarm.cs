using System;

namespace Server.Items
{
	public class Disarm : WeaponAbility
	{
		public Disarm()
		{
		}
		public override int BaseMana{ get{ return 20; } }
		public override bool RequiresTactics( Mobile from )
		{
			BaseWeapon weapon = from.Weapon as BaseWeapon;
			if ( weapon == null )
				return false;
			return weapon.Skill != SkillName.FistFighting;
		}
		public static readonly TimeSpan BlockEquipDuration = TimeSpan.FromSeconds( 5.0 );
		public override void OnHit( Mobile attacker, Mobile defender, int damage )
		{
            if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
                return;
            ClearCurrentAbility( attacker );
            double armsLore = attacker.Skills[SkillName.ArmsLore].Value;
            double tactics = attacker.Skills[SkillName.Tactics].Value;
            attacker.SendLocalizedMessage( 1060092 ); // You disarm your opponent!
            defender.SendLocalizedMessage( 1060093 ); // You have been disarmed!
            defender.PlaySound( 0x3B3 ); // Sound of a weapon hitting the ground
            if ( armsLore >= 60.0 )
            {
                // Calculate the resist strip: 10 base + (Arms Lore / 6) = ~30 points at 120
                int resistStrip = 10 + (int)(armsLore / 6.0);
                int duration = 10; // 10 seconds of vulnerability
                // We use a timer to lower their physical resistance    
                ResistanceMod mod = new ResistanceMod( ResistanceType.Physical, -resistStrip );
                defender.AddResistanceMod( mod );
                attacker.SendMessage( "Your knowledge of arms exposes a massive flaw in their armor! (-{0} Physical Resist)", resistStrip );
                // Remove the debuff after the duration
                Timer.DelayCall( TimeSpan.FromSeconds( duration ), delegate 
                { 
                    defender.RemoveResistanceMod( mod ); 
                    defender.SendMessage( "Your armor defenses have recovered." );
                });
            }
            // 3. The Tactics Bonus
            // High tactics makes the disarm last slightly longer or prevents immediate re-equipping
            if ( tactics >= 100.0 )
            {
                // On some shards, you can set a 'NextReequip' timer here.
                // For now, let's just add a nice visual "Cracked Armor" effect
                defender.FixedParticles( 0x37B9, 2, 25, 43, 0, 4, EffectLayer.Waist );
            }
            // Call the original base Disarm logic (to actually move the item to their pack)
            base.OnHit( attacker, defender, damage );
        }
	}
}
