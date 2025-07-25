using System;
using Server;
using Server.Items;

namespace Server.Mobiles
{
	[CorpseName( "a ghostly corpse" )]
	public class SeaGhost : BaseCreature
	{
		[Constructable]
		public SeaGhost() : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a specter";
			Body = 37;
			BaseSoundID = 0x482;
			CanSwim = true;

			SetStr( 171, 200 );
			SetDex( 126, 145 );
			SetInt( 276, 305 );

			SetHits( 103, 120 );

			SetDamage( 24, 26 );

			SetDamageType( ResistanceType.Physical, 10 );
			SetDamageType( ResistanceType.Cold, 40 );
			SetDamageType( ResistanceType.Energy, 50 );

			SetResistance( ResistanceType.Physical, 40, 60 );
			SetResistance( ResistanceType.Fire, 20, 30 );
			SetResistance( ResistanceType.Cold, 50, 60 );
			SetResistance( ResistanceType.Poison, 55, 65 );
			SetResistance( ResistanceType.Energy, 40, 50 );

			SetSkill( SkillName.Necromancy, 89, 99.1 );
			SetSkill( SkillName.Spiritualism, 90.0, 99.0 );
			SetSkill( SkillName.Psychology, 100.0 );
			SetSkill( SkillName.Magery, 70.1, 80.0 );
			SetSkill( SkillName.Meditation, 85.1, 95.0 );
			SetSkill( SkillName.MagicResist, 80.1, 100.0 );
			SetSkill( SkillName.Tactics, 70.1, 90.0 );

			Fame = 8000;
			Karma = -8000;

			VirtualArmor = 50;
			PackItem( new GraveDust( 10 ) );
			PackReg( 17, 24 );

			AddItem( new LightSource() );
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Average );
			AddLoot( LootPack.MedScrolls, 1 );
		}

		public override void OnDeath( Container c )
		{
			base.OnDeath( c );

			int killerLuck = MobileUtilities.GetLuckFromKiller( this );
			if ( Server.Misc.GetPlayerInfo.LuckyKiller( killerLuck ) )
			{
				int Magic = 0;
				int Speed = 0;

				Robe robe = new Robe();
					robe.Name = "shroud of sea specter";
					robe.Hue = this.Hue;
					robe.Attributes.CastRecovery = Speed;
					robe.Attributes.CastSpeed = Speed;
					robe.Attributes.LowerManaCost = 5 + Magic;
					robe.Attributes.LowerRegCost = 5 + Magic;
					robe.Attributes.SpellDamage = 5 + Magic;
					c.DropItem( robe );
			}
		}

		public override bool OnBeforeDeath()
		{
			this.Body = 13;
			return base.OnBeforeDeath();
		}

		public override bool ShowFameTitle{ get{ return false; } }
		public override bool BleedImmune{ get{ return true; } }
		public override Poison PoisonImmune{ get{ return Poison.Deadly; } }
		public override int TreasureMapLevel{ get{ return 3; } }
		public override bool AlwaysAttackable{ get{ return true; } }
		public override int Cloths{ get{ return 5; } }
		public override ClothType ClothType{ get{ return ClothType.Haunted; } }

		public SeaGhost( Serial serial ) : base( serial )
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