using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "an elemental corpse" )]
	public class IceElemental : BaseCreature
	{
		public override int BreathPhysicalDamage{ get{ return 50; } }
		public override int BreathFireDamage{ get{ return 0; } }
		public override int BreathColdDamage{ get{ return 50; } }
		public override int BreathPoisonDamage{ get{ return 0; } }
		public override int BreathEnergyDamage{ get{ return 0; } }
		public override int BreathEffectHue{ get{ return 0; } }
		public override int BreathEffectSound{ get{ return 0x658; } }
		public override int BreathEffectItemID{ get{ return 0; } }
		public override bool ReacquireOnMovement{ get{ return !Controlled; } }
		public override bool HasBreath{ get{ return true; } }
		public override double BreathEffectDelay{ get{ return 0.1; } }
		public override void BreathDealDamage( Mobile target, int form ){ base.BreathDealDamage( target, 32 ); }

		[Constructable]
		public IceElemental () : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "an ice elemental";
			Body = Utility.RandomList( 755, 462, 446, 974 );
			Hue = 0xAF3;
			Resource = CraftResource.IceBlock;

			if ( Body == 462 )
				Hue = 0;

			BaseSoundID = 268;

			SetStr( 156, 185 );
			SetDex( 96, 115 );
			SetInt( 171, 192 );

			SetHits( 94, 111 );

			SetDamage( 10, 21 );

			SetDamageType( ResistanceType.Physical, 25 );
			SetDamageType( ResistanceType.Cold, 75 );

			SetResistance( ResistanceType.Physical, 35, 45 );
			SetResistance( ResistanceType.Fire, 5, 10 );
			SetResistance( ResistanceType.Cold, 50, 60 );
			SetResistance( ResistanceType.Poison, 20, 30 );
			SetResistance( ResistanceType.Energy, 20, 30 );

			SetSkill( SkillName.Psychology, 10.5, 60.0 );
			SetSkill( SkillName.Magery, 10.5, 60.0 );
			SetSkill( SkillName.MagicResist, 30.1, 80.0 );
			SetSkill( SkillName.Tactics, 70.1, 100.0 );
			SetSkill( SkillName.FistFighting, 60.1, 100.0 );

			Fame = 4000;
			Karma = -4000;

			VirtualArmor = 40;

			PackItem( new BlackPearl() );
			PackReg( 3 );

			AddItem( new LightSource() );
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Average, 2 );
			AddLoot( LootPack.Gems, 1 );
		}

		public override bool BleedImmune{ get{ return true; } }
		public override int Rocks{ get{ return Utility.RandomMinMax( 1, 5 ); } }
		public override RockType RockType{ get{ return ResourceRocks(); } }

		public IceElemental( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}