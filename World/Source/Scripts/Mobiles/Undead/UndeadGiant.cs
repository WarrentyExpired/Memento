using System;
using Server;
using System.Collections;
using Server.Items;
using Server.Targeting;
using Server.Misc;

namespace Server.Mobiles
{
	[CorpseName( "a giant corpse" )]
	public class UndeadGiant : BaseCreature
	{
		public override WeaponAbility GetWeaponAbility()
		{
			return WeaponAbility.Dismount;
		}

		[Constructable]
		public UndeadGiant() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "an undead giant";
			Body = 325;
			BaseSoundID = 471;
			Hue = 0xB97;

			SetStr( 336, 385 );
			SetDex( 96, 115 );
			SetInt( 31, 55 );

			SetHits( 202, 231 );
			SetMana( 0 );

			SetDamage( 7, 23 );

			SetDamageType( ResistanceType.Physical, 50 );
			SetDamageType( ResistanceType.Poison, 50 );

			SetResistance( ResistanceType.Physical, 45, 50 );
			SetResistance( ResistanceType.Fire, 20, 30 );
			SetResistance( ResistanceType.Cold, 5, 10 );
			SetResistance( ResistanceType.Poison, 70, 80 );
			SetResistance( ResistanceType.Energy, 30, 40 );

			SetSkill( SkillName.MagicResist, 60.3, 105.0 );
			SetSkill( SkillName.Tactics, 80.1, 100.0 );
			SetSkill( SkillName.FistFighting, 80.1, 90.0 );

			Fame = 4500;
			Karma = -4500;

			VirtualArmor = 48;
		}

		public override void OnDeath( Container c )
		{
			base.OnDeath( c );

			int killerLuck = MobileUtilities.GetLuckFromKiller( this );
			if ( GetPlayerInfo.LuckyKiller( killerLuck ) && Utility.RandomMinMax( 1, 4 ) == 1 )
			{
				BaseArmor skin = null;
				switch ( Utility.RandomMinMax( 0, 5 ) )
				{
					case 0: skin = new LeatherLegs(); skin.Resource = CraftResource.DeadSkin; c.DropItem( skin ); break;
					case 1: skin = new LeatherGloves(); skin.Resource = CraftResource.DeadSkin; c.DropItem( skin ); break;
					case 2: skin = new LeatherGorget(); skin.Resource = CraftResource.DeadSkin; c.DropItem( skin ); break;
					case 3: skin = new LeatherArms(); skin.Resource = CraftResource.DeadSkin; c.DropItem( skin ); break;
					case 4: skin = new LeatherChest(); skin.Resource = CraftResource.DeadSkin; c.DropItem( skin ); break;
					case 5: skin = new LeatherCap(); skin.Resource = CraftResource.DeadSkin; c.DropItem( skin ); break;
				}
			}

			if ( GetPlayerInfo.LuckyKiller( killerLuck ) && Utility.RandomMinMax( 1, 4 ) == 1 )
			{
				BaseWeapon axe = new Halberd();
				axe.AccuracyLevel = WeaponAccuracyLevel.Supremely;
				axe.MinDamage = axe.MinDamage + 5;
				axe.MaxDamage = axe.MaxDamage + 10;
				axe.DurabilityLevel = WeaponDurabilityLevel.Indestructible;
				axe.AosElementDamages.Poison=50;
				axe.Name = "dead giant's hand axe";
				axe.Hue = 0xB98;
				c.DropItem( axe );
			}
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Rich );
			AddLoot( LootPack.Average );
		}

		public override int TreasureMapLevel{ get{ return 3; } }
		public override bool BleedImmune{ get{ return true; } }
		public override Poison PoisonImmune{ get{ return Poison.Regular; } }
		public override Poison HitPoison{ get{ return Poison.Greater; } }
		public override int Hides{ get{ return 18; } }
		public override HideType HideType{ get{ if ( Utility.RandomBool() ){ return HideType.Necrotic; } else { return HideType.Goliath; } } }
		public override int Skin{ get{ return Utility.Random(4); } }
		public override SkinType SkinType{ get{ return SkinType.Dead; } }
		public override int Skeletal{ get{ return Utility.Random(3); } }
		public override SkeletalType SkeletalType{ get{ return SkeletalType.Colossal; } }

		public UndeadGiant( Serial serial ) : base( serial )
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