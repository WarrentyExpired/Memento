using System;
using Server;
using System.Collections; 
using Server.Items; 
using Server.ContextMenus; 
using Server.Misc; 
using Server.Network;
using Server.Mobiles;

namespace Server.Mobiles 
{
	public class BlackKnight : BaseCreature 
	{
		[Constructable] 
		public BlackKnight() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 ) 
		{
			Body = 0x190; 

			SpeechHue = Utility.RandomTalkHue();
			Hue = Utility.RandomSkinColor();

			Name = NameList.RandomName( "male" );
			Utility.AssignRandomHair( this );
			int HairColor = Utility.RandomHairHue();
			FacialHairItemID = Utility.RandomList( 0, 8254, 8255, 8256, 8257, 8267, 8268, 8269 );
			HairHue = HairColor;
			FacialHairHue = HairColor;
			Title = "the Black Knight";

			SetStr( 230 );
			SetDex( 150 );
			SetInt( 120 );

			SetHits( 230 );

			SetDamage( 12, 23 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 40 );
			SetResistance( ResistanceType.Fire, 30 );
			SetResistance( ResistanceType.Cold, 30 );
			SetResistance( ResistanceType.Poison, 30 );
			SetResistance( ResistanceType.Energy, 30 );

			SetSkill( SkillName.Searching, 80.0 );
			SetSkill( SkillName.Anatomy, 110.0 );
			SetSkill( SkillName.MagicResist, 80.0 );
			SetSkill( SkillName.Bludgeoning, 110.0 );
			SetSkill( SkillName.Fencing, 110.0 );
			SetSkill( SkillName.FistFighting, 110.0 );
			SetSkill( SkillName.Swords, 110.0 );
			SetSkill( SkillName.Tactics, 110.0 );

			Fame = 7000;
			Karma = -7000;

			VirtualArmor = 30;

			AddItem( new PlateChest() );
			AddItem( new PlateArms() );
			AddItem( new PlateLegs() );
			AddItem( new PlateGorget() );
			AddItem( new PlateGloves() );
			AddItem( new PlateHelm() );
			AddItem( new VikingSword() );
			AddItem( new ChaosShield() );
			AddItem( new Boots() );

			MorphingTime.BlessMyClothes( this );
			MorphingTime.ColorMyClothes( this, 2860, 0 );
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Average );
			AddLoot( LootPack.Average );
			AddLoot( LootPack.Meager );
		}

		public override bool ClickTitle{ get{ return false; } }
		public override bool ShowFameTitle{ get{ return false; } }
		public override bool AlwaysAttackable{ get{ return true; } }
		public override int Skeletal{ get{ return Utility.Random(3); } }
		public override SkeletalType SkeletalType{ get{ return SkeletalType.Brittle; } }

		public override bool OnBeforeDeath()
		{
			BlackKnightBox MyChest = new BlackKnightBox();

			Map map = this.Map;

			bool validLocation = false;
			Point3D loc = this.Location;

			for ( int j = 0; !validLocation && j < 10; ++j )
			{
				int x = X + Utility.Random( 3 ) - 1;
				int y = Y + Utility.Random( 3 ) - 1;
				int z = map.GetAverageZ( x, y );

				if ( validLocation = map.CanFit( x, y, this.Z, 16, false, false ) )
					loc = new Point3D( x, y, Z );
				else if ( validLocation = map.CanFit( x, y, z, 16, false, false ) )
					loc = new Point3D( x, y, z );
			}

			MyChest.MoveToWorld( loc, map );
			QuestGlow MyGlow = new QuestGlow();
			MyGlow.MoveToWorld( loc, map );

			return base.OnBeforeDeath();
		}

		public override void OnDeath( Container c )
		{
			base.OnDeath( c );

			PlayerMobile killer = MobileUtilities.TryGetKillingPlayer( this );

			if ( killer != null )
			{
				int killerLuck = MobileUtilities.GetLuckFromKiller( this );

				if ( GetPlayerInfo.VeryLuckyKiller( killerLuck ) )
				{
					Item loot = null;

					switch( Utility.RandomMinMax( 0, 9 ) )
					{
						case 0: loot = new PlateChest(); break;
						case 1: loot = new PlateArms(); break;
						case 2: loot = new PlateLegs(); break;
						case 3: loot = new PlateGorget(); break;
						case 4: loot = new PlateGloves(); break;
						case 5: loot = new PlateHelm(); break;
						case 6: loot = new VikingSword(); break;
						case 7: loot = new ChaosShield(); break;
						case 8: loot = new Boots(); break;
						case 9: loot = Loot.RandomJewelry(); break;
					}

					if ( loot != null )
					{
						ResourceMods.SetResource( loot, CraftResource.DreadSpec );
						loot = Server.LootPackEntry.Enchant( killer, 500, loot );
						loot.InfoText1 = "The Black Knight";
						c.DropItem( loot ); 
					}
				}
			}
		}

		public override void OnGotMeleeAttack( Mobile attacker )
		{
			base.OnGotMeleeAttack( attacker );
			Server.Misc.IntelligentAction.CryOut( this );
		}

		public BlackKnight( Serial serial ) : base( serial ) 
		{ 
		} 

		public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 
			writer.Write( (int) 0 ); // version 
		}

		public override void Deserialize( GenericReader reader ) 
		{ 
			base.Deserialize( reader ); 
			int version = reader.ReadInt(); 
		} 
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

namespace Server.Items
{
	public class BlackKnightBox : Item
	{
		[Constructable]
		public BlackKnightBox() : base( 0xE80 )
		{
			Name = "the Black Knights's box";
			Movable = false;
			Hue = 0x96C;
			ItemRemovalTimer thisTimer = new ItemRemovalTimer( this ); 
			thisTimer.Start(); 
		}

		public BlackKnightBox( Serial serial ) : base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.InRange( this.GetWorldLocation(), 2 ) )
			{
				if ( PlayerSettings.GetKeys( from, "BlackKnightKey" ) )
				{
					from.PrivateOverheadMessage(MessageType.Regular, 1150, false, "You find nothing of interest.", from.NetState);
				}
				else
				{
					PlayerSettings.SetKeys( from, "BlackKnightKey", true );
					from.SendSound( 0x3D );
					from.PrivateOverheadMessage(MessageType.Regular, 1150, false, "You found a blackened key with a symbol of a sword on it.", from.NetState);
				}
			}
			else
			{
				from.SendLocalizedMessage( 502138 ); // That is too far away for you to use
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			this.Delete(); // none when the world starts 
		}

		public class ItemRemovalTimer : Timer 
		{ 
			private Item i_item; 
			public ItemRemovalTimer( Item item ) : base( TimeSpan.FromMinutes( 10.0 ) ) 
			{ 
				Priority = TimerPriority.OneSecond; 
				i_item = item; 
			} 

			protected override void OnTick() 
			{ 
				if (( i_item != null ) && ( !i_item.Deleted ))
				{
					i_item.Delete();
				}
			} 
		} 
	}
}