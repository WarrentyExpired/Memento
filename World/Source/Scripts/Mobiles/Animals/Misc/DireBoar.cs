using System;
using Server;
using System.Collections;
using Server.Items;
using Server.Targeting;
using Server.Misc;

namespace Server.Mobiles
{
	[CorpseName( "a dire boar corpse" )]
	public class DireBoar : BaseCreature
	{
		[Constructable]
		public DireBoar() : base( AIType.AI_Melee,FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "a dire boar";
			Body = 0x122;
			Hue = 0x908;
			BaseSoundID = 0xC4;

			SetStr( 45 );
			SetDex( 35 );
			SetInt( 10 );

			SetHits( 35 );
			SetMana( 0 );

			SetDamage( 6, 9 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 10, 15 );
			SetResistance( ResistanceType.Fire, 5, 10 );
			SetResistance( ResistanceType.Poison, 5, 10 );

			SetSkill( SkillName.MagicResist, 9.0 );
			SetSkill( SkillName.Tactics, 9.0 );
			SetSkill( SkillName.FistFighting, 9.0 );

			Fame = 400;
			Karma = 0;

			VirtualArmor = 10;

			Tamable = true;
			ControlSlots = 1;
			MinTameSkill = 49.1;
		}

		public override int Meat{ get{ return 2; } }
		public override MeatType MeatType{ get{ return MeatType.Pigs; } }
		public override FoodType FavoriteFood{ get{ return FoodType.FruitsAndVegies | FoodType.GrainsAndHay; } }

		public override void OnDeath( Container c )
		{
			base.OnDeath( c );

			int killerLuck = MobileUtilities.GetLuckFromKiller( this );
			if ( GetPlayerInfo.LuckyKiller( killerLuck ) && Utility.RandomMinMax( 0, 100 ) > 50 )
			{
				c.DropItem( new IvoryTusk() );
			}
		}

		public DireBoar(Serial serial) : base(serial)
		{
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write((int) 0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();
		}
	}
}