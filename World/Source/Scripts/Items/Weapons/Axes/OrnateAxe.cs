using System;
using Server.Network;
using Server.Items;

namespace Server.Items
{
	public class OrnateAxe : BaseAxe
	{
        public override WeaponAbility PrimaryAbility{ get{ return WeaponAbility.BleedAttack; } }
		public override WeaponAbility SecondaryAbility{ get{ return WeaponAbility.WhirlwindAttack; } }
		public override WeaponAbility ThirdAbility{ get{ return WeaponAbility.ArmorIgnore; } }
		public override WeaponAbility FourthAbility{ get{ return WeaponAbility.DoubleStrike; } }
		public override WeaponAbility FifthAbility{ get{ return WeaponAbility.DevastatingBlow; } }

		public override int AosStrengthReq{ get{ return 40; } }
		public override int AosMinDamage{ get{ return 15; } }
		public override int AosMaxDamage{ get{ return 17; } }
		public override int AosSpeed{ get{ return 28; } }
		public override float MlSpeed{ get{ return 3.75f; } }

		public override int OldStrengthReq{ get{ return 40; } }
		public override int OldMinDamage{ get{ return 6; } }
		public override int OldMaxDamage{ get{ return 34; } }
		public override int OldSpeed{ get{ return 30; } }

		public override int DefHitSound{ get{ return 0x232; } }
		public override int DefMissSound{ get{ return 0x23A; } }

		public override int InitMinHits{ get{ return 31; } }
		public override int InitMaxHits{ get{ return 100; } }

		[Constructable]
		public OrnateAxe() : base( 0x2D28 )
		{
			Weight = 8.0;
			Name = "barbarian axe";
			Layer = Layer.OneHanded;
			ItemID = Utility.RandomList( 0x2D28, 0x2D34, 0x265C, 0x265C );
		}

		public OrnateAxe( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadEncodedInt();
		}
	}
}
