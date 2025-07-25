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
	public class BaneOfInsanity : BaseCreature 
	{
		[Constructable] 
		public BaneOfInsanity() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 ) 
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
			Title = "the Bane of Insanity";

			SetStr( 350 );
			SetDex( 150 );
			SetInt( 120 );

			SetHits( 300 );

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

			Fame = 8000;
			Karma = -8000;

			VirtualArmor = 30;

			PlateChest chest = new PlateChest();
				chest.Hue = 0x83F;
				chest.Name = "plate tunic of insanity";
				chest.Durability = ArmorDurabilityLevel.Indestructible;
				chest.ProtectionLevel = ArmorProtectionLevel.Invulnerability;
				AddItem( chest );
			PlateArms arms = new PlateArms();
				arms.Hue = 0x83F;
				arms.Name = "plate arms of insanity";
				arms.Durability = ArmorDurabilityLevel.Indestructible;
				arms.ProtectionLevel = ArmorProtectionLevel.Invulnerability;
				AddItem( arms );
			PlateLegs legs = new PlateLegs();
				legs.Hue = 0x83F;
				legs.Name = "plate leggings of insanity";
				legs.Durability = ArmorDurabilityLevel.Indestructible;
				legs.ProtectionLevel = ArmorProtectionLevel.Invulnerability;
				AddItem( legs );
			PlateGorget neck = new PlateGorget();
				neck.Hue = 0x83F;
				neck.Name = "plate gorget of insanity";
				neck.Durability = ArmorDurabilityLevel.Indestructible;
				neck.ProtectionLevel = ArmorProtectionLevel.Invulnerability;
				AddItem( neck );
			PlateGloves gloves = new PlateGloves();
				gloves.Hue = 0x83F;
				gloves.Name = "plate gloves of insanity";
				gloves.Durability = ArmorDurabilityLevel.Indestructible;
				gloves.ProtectionLevel = ArmorProtectionLevel.Invulnerability;
				AddItem( gloves );
			PlateHelm helm = new PlateHelm();
				helm.Hue = 0x83F;
				helm.ItemID = 0x2645;
				helm.Name = "plate helm of insanity";
				helm.Durability = ArmorDurabilityLevel.Indestructible;
				helm.ProtectionLevel = ArmorProtectionLevel.Invulnerability;
				AddItem( helm );
			Halberd weapon = new Halberd();
				weapon.Hue = 0x83F;
				weapon.Name = "halberd of insanity";
				weapon.AccuracyLevel = WeaponAccuracyLevel.Supremely;
				weapon.DamageLevel = WeaponDamageLevel.Vanq;
				weapon.DurabilityLevel = WeaponDurabilityLevel.Indestructible;
				AddItem( weapon );
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.FilthyRich );
			AddLoot( LootPack.Average );
			AddLoot( LootPack.Rich );
		}

		public override bool ClickTitle{ get{ return false; } }
		public override bool ShowFameTitle{ get{ return false; } }
		public override bool AlwaysAttackable{ get{ return true; } }
		public override bool CanRummageCorpses{ get{ return true; } }
		public override bool BardImmune { get { return true; } }
		public override bool IsScaredOfScaryThings{ get{ return false; } }
		public override bool IsScaryToPets{ get{ return true; } }

		public override bool OnBeforeDeath()
		{
			int CanDie = 0;
			Mobile winner = this;
			ArrayList targets = new ArrayList();

			foreach ( Mobile m in this.GetMobilesInRange( 30 ) )
			{
				if ( m is PlayerMobile && m.Map == this.Map && !m.Blessed )
				{
					Item flame = m.Backpack.FindItemByType( typeof ( OrbOfLogic ) );
					if ( flame != null && flame is OrbOfLogic && ((OrbOfLogic)flame).Owner == m )
					{
						targets.Add( flame );
						CanDie = 1;
						winner = m;
						m.SendMessage( "The Orb of Logic has vanished after dispatching the Chaos Bane." );
						Server.Items.QuestSouvenir.GiveReward( m, flame.Name, flame.Hue, flame.ItemID );
					}
				}
			}

			if ( CanDie == 0 )
			{
				Say("Fool! You think chaos can be slain to easily?");
				this.Hits = this.HitsMax;
				this.FixedParticles( 0x376A, 9, 32, 5030, EffectLayer.Waist );
				this.PlaySound( 0x202 );
				return false;
			}
			else
			{
				this.Body = 13;
				this.Hue = 0x845;

				string Iam = "the Bane of Insanity";
				PlayerMobile killer = MobileUtilities.TryGetKillingPlayer( this );
				Server.Misc.LoggingFunctions.LogSlayingLord( killer, Iam );

				for ( int i = 0; i < targets.Count; ++i )
				{
					Item item = ( Item )targets[ i ];
					item.Delete();
				}

				if ( winner is PlayerMobile )
				{
					winner.AddToBackpack( new BlackrockSerpentChaos() );
					winner.SendMessage( "You have obtained the Blackrock Serpent of Chaos!" );
					LoggingFunctions.LogGenericQuest( winner, "has obtained the blackrock serpent of chaos" );
				}

				return base.OnBeforeDeath();
			}
		}

		public override void OnGotMeleeAttack( Mobile attacker )
		{
			base.OnGotMeleeAttack( attacker );
			Server.Misc.IntelligentAction.CryOut( this );
		}

		public BaneOfInsanity( Serial serial ) : base( serial ) 
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