using System; 
using Server;
using Server.Items;

namespace Server.Mobiles 
{ 
	public class Syth : BaseCreature 
	{
		public int actionCount;
		[CommandProperty(AccessLevel.Owner)]
		public int action_Count { get { return actionCount; } set { actionCount = value; InvalidateProperties(); } }

		public override int BreathPhysicalDamage{ get{ return 0; } }
		public override int BreathFireDamage{ get{ return 0; } }
		public override int BreathColdDamage{ get{ return 0; } }
		public override int BreathPoisonDamage{ get{ return 0; } }
		public override int BreathEnergyDamage{ get{ return 100; } }
		public override int BreathEffectHue{ get{ return 0x9C2; } }
		public override int BreathEffectSound{ get{ return 0x665; } }
		public override int BreathEffectItemID{ get{ return 0x3818; } }
		public override bool ReacquireOnMovement{ get{ return !Controlled; } }
		public override bool HasBreath{ get{ return true; } }
		public override double BreathEffectDelay{ get{ return 0.1; } }
		public override void BreathDealDamage( Mobile target, int form ){ base.BreathDealDamage( target, 21 ); }

		[Constructable] 
		public Syth() : base( AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4 ) 
		{
			SpeechHue = Utility.RandomTalkHue();
			Title = "the Syth";
			Hue = Utility.RandomSkinColor();
				if ( Utility.RandomBool() ){ Hue = Utility.RandomList( 0x6F6, 0x97F, 0x99B, 0x6E4, 0x5E0, 0xB38, 0xB2B ); }

			if ( this.Female = Utility.RandomBool() )
			{
				Body = 0x191;
				Name = NameList.RandomName( "dark_elf_prefix_male" ) + NameList.RandomName( "dark_elf_suffix_female" );
				Utility.AssignRandomHair( this );
				HairHue = Utility.RandomHairHue();
			}
			else
			{
				Body = 0x190;
				Name = NameList.RandomName( "dark_elf_prefix_female" ) + NameList.RandomName( "dark_elf_suffix_male" );
				Utility.AssignRandomHair( this );
				int HairColor = Utility.RandomHairHue();
				FacialHairItemID = Utility.RandomList( 0, 8254, 8255, 8256, 8257, 8267, 8268, 8269 );
				HairHue = HairColor;
				FacialHairHue = HairColor;
			}

			SetStr( 350, 400 );
			SetDex( 177, 255 );
			SetInt( 350, 400 );

			SetHits( 502, 531 );

			SetDamage( 22, 29 );

			SetDamageType( ResistanceType.Physical, 50 );
			SetDamageType( ResistanceType.Energy, 50 );

			SetResistance( ResistanceType.Physical, 45, 55 );
			SetResistance( ResistanceType.Fire, 30, 40 );
			SetResistance( ResistanceType.Cold, 30, 40 );
			SetResistance( ResistanceType.Poison, 40, 50 );
			SetResistance( ResistanceType.Energy, 60, 70 );

			SetSkill( SkillName.Psychology, 100.5, 150.0 );
			SetSkill( SkillName.Magery, 100.5, 150.0 );
			SetSkill( SkillName.Meditation, 100.5, 150.0 );
			SetSkill( SkillName.MagicResist, 100.5, 150.0 );

			SetSkill( SkillName.Anatomy, 100.5, 150.0 );
			SetSkill( SkillName.MagicResist, 100.5, 150.0 );
			SetSkill( SkillName.FistFighting, 100.5, 150.0 );
			SetSkill( SkillName.Swords, 100.5, 150.0 );
			SetSkill( SkillName.Tactics, 100.5, 150.0 );

			Fame = 14000;
			Karma = -14000;

			int color = Utility.RandomList( 0xB80, 0xB5E, 0xB39, 0xB3A, 0xA9F, 0x99E, 0x997, 0x8D9, 0x8DA, 0x8DB, 0x8DC, 0x8B9 );

			VirtualArmor = 90;

			Item robe = new AssassinRobe( color );
				robe.Name = "Syth robe";
				AddItem( robe );
				if ( Utility.RandomMinMax( 1, 10 ) == 1 ){ SpecialItem( robe, this ); }

			Item boots = new ElvenBoots( color );
				boots.Name = "Syth boots";
				AddItem( boots );
				if ( Utility.RandomMinMax( 1, 10 ) == 1 ){ SpecialItem( boots, this ); }

			if ( Utility.RandomBool() )
			{
				Item cloak = new Cloak( color );
					cloak.Name = "Syth cloak";
					AddItem( cloak );
				if ( Utility.RandomMinMax( 1, 10 ) == 1 ){ SpecialItem( cloak, this ); }
			}

			if ( Utility.RandomBool() )
			{
				Item gloves = new LeatherGloves();
					gloves.Hue = color;
					gloves.Name = "Syth gloves";
					AddItem( gloves );
				if ( Utility.RandomMinMax( 1, 10 ) == 1 ){ SpecialItem( gloves, this ); }
			}

			Item hood = new FloppyHat( color );
				hood.Name = "Syth hood";
				hood.ItemID = 0x4CDA;
				if ( Utility.RandomMinMax( 1, 4 ) == 1 ){ hood.Name = "Syth cowl"; hood.ItemID = 0x4CDC; }
				AddItem( hood );
			if ( Utility.RandomMinMax( 1, 10 ) == 1 ){ SpecialItem( hood, this ); }

			if ( Utility.RandomMinMax( 1, 5 ) == 1 )
			{
				if ( Utility.RandomMinMax( 1, 5 ) == 1 )
				{
					Item sword = new DoubleLaserSword();
						sword.Name = "Syth double laser sword";
						((BaseWeapon)sword).Attributes.SpellChanneling = 1;
						AddItem( sword );
						if ( Utility.RandomMinMax( 1, 10 ) == 1 ){ SpecialItem( sword, this ); }
				}
				else
				{
					Item sword = new LightSword();
						sword.Name = "Syth laser sword";
						((BaseWeapon)sword).Attributes.SpellChanneling = 1;
						AddItem( sword );
						if ( Utility.RandomMinMax( 1, 10 ) == 1 ){ SpecialItem( sword, this ); }
				}
			}
			else
			{
				Item sword = new Longsword();
					sword.Name = "Syth sword";
					((BaseWeapon)sword).Attributes.SpellChanneling = 1;
					AddItem( sword );
					if ( Utility.RandomMinMax( 1, 10 ) == 1 ){ SpecialItem( sword, this ); }
					else if ( CraftResources.GetType( ResourceMods.SearchResource(sword) ) == CraftResourceType.Metal ){ ((BaseWeapon)sword).Resource = ResourceMods.SciFiResource( ((BaseWeapon)sword).Resource ); }
			}
		}

		public static void SpecialItem( Item item, Mobile m )
		{
			LootPackEntry.MakeFixedDrop( item, 8 );
		}

		public override void OnDeath( Container c )
		{
			base.OnDeath( c );

			if ( Utility.RandomBool() )
			{
				LoreBook book = new LoreBook();
				book.BookTitle = "The Rule of One";
				book.Name = book.BookTitle;
				book.BookAuthor = "Asajj Ventress the Syth Lord";
				LoreBook.SetBookCover( 78, book );
				book.ItemID = 0x4CDF;
				book.Light = LightType.Circle225;
				LoreBook.GetText( book );
				c.DropItem( book );
			}
		}

		public override void OnGaveMeleeAttack( Mobile defender )
		{
			SwitchTactics();
			base.OnGaveMeleeAttack( defender );
		}

		public override void OnGotMeleeAttack( Mobile attacker )
		{
			SwitchTactics();
			base.OnGotMeleeAttack( attacker );
		}

		public override void OnDamage( int amount, Mobile from, bool willKill )
		{
			SwitchTactics();
			base.OnDamage( amount, from, willKill );
		}

		public override void OnDamagedBySpell( Mobile from )
		{
			SwitchTactics();
			base.OnDamagedBySpell( from );
		}

		public void SwitchTactics()
		{
			actionCount++;

			if ( actionCount > 20 )
			{
				actionCount = 0;
				if ( AI == AIType.AI_Melee ){ AI = AIType.AI_Mage; }
				else { AI = AIType.AI_Melee; }
			}
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.FilthyRich, 2 );
			AddLoot( LootPack.Rich );
			AddLoot( LootPack.Gems, 1 );
		}

		public override bool ClickTitle{ get{ return false; } }
		public override bool ShowFameTitle{ get{ return false; } }
		public override bool CanRummageCorpses{ get{ return true; } }
		public override bool AlwaysAttackable{ get{ return true; } }
		public override int Meat{ get{ return 1; } }
		public override int Skeletal{ get{ return Utility.Random(3); } }
		public override SkeletalType SkeletalType{ get{ return SkeletalType.SciFi; } }
		public override int Hides{ get{ return Utility.Random(3); } }
		public override HideType HideType{ get{ return HideType.Alien; } }

		public Syth( Serial serial ) : base( serial ) 
		{ 
		} 

		public override void Serialize( GenericWriter writer ) 
		{ 
			base.Serialize( writer ); 
			writer.Write( (int) 0 ); 
			writer.Write( actionCount );
		} 

		public override void Deserialize( GenericReader reader ) 
		{ 
			base.Deserialize( reader ); 
			int version = reader.ReadInt(); 
			actionCount = reader.ReadInt();
		} 
	} 
}