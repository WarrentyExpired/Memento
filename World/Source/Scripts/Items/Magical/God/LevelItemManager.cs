using System;
using Server;
using Server.Mobiles;

namespace Server.Items
{
	public class LevelItemManager
	{
		/// <summary>
		/// The Number of levels our items can go to. If you
		/// change this, be sure the Exp table has the correct
		/// number of Integer values in it.
		/// </summary>

		#region Level calculation method

		private static int[] m_Table;

		public static int[] ExpTable
		{
			get{ return m_Table; }
		}

		public static void Initialize()
		{
			// The following will build the Level experence table */
			m_Table = new int[LevelItems.MaxLevelsCap];
			m_Table[0] = 0;

			for ( int i = 1; i < LevelItems.MaxLevelsCap; ++i )
			{
				m_Table[i] = ExpToLevel( i );
			}
		}

		public static int ExpToLevel( int currentlevel )
		{
			double req = ( currentlevel + 1 ) * 10;

			req = Math.Pow( req, 2 );

			req -= 100.0;

			return ( (int)Math.Round( req ) );
		}

		#endregion

		#region Exp calculation methods

		private static bool IsMageryCreature( BaseCreature bc )
		{
			return ( bc != null && bc.AI == AIType.AI_Mage && bc.Skills[SkillName.Magery].Base > 5.0 );
		}

		private static bool IsFireBreathingCreature( BaseCreature bc )
		{
			if ( bc == null )
				return false;

			return bc.HasBreath;
		}

		private static bool IsPoisonImmune( BaseCreature bc )
		{
			return ( bc != null && bc.PoisonImmune != null );
		}

		private static int GetPoisonLevel( BaseCreature bc )
		{
			if ( bc == null )
				return 0;

			Poison p = bc.HitPoison;

			if ( p == null )
				return 0;

			return p.Level + 1;
		}

		public static int CalcExp( Mobile targ )
		{
			double val = targ.Hits + targ.Stam + targ.Mana;

			for ( int i = 0; i < targ.Skills.Length; i++ )
				val += targ.Skills[i].Base;

			if ( val > 700 )
				val = 700 + ((val - 700) / 3.66667);

			BaseCreature bc = targ as BaseCreature;

			if ( IsMageryCreature( bc ) )
				val += 100;

			if ( IsFireBreathingCreature( bc ) )
				val += 100;

			if ( IsPoisonImmune( bc ) )
				val += 100;

			if ( targ is VampireBat || targ is VampireBatFamiliar )
				val += 100;

			val += GetPoisonLevel( bc ) * 20;

			val /= 10;

			return (int)val;
		}

		public static int CalcExpCap( int level )
		{
			int req = ExpToLevel( level + 1 );

			return ( req / 20 );
		}

		#endregion

		public static void CheckItems( Mobile killer, Mobile killed )
		{
			if ( killer != null )
			{
				for( int i = 0; i < 25; ++i )
				{
					Item item = killer.FindItemOnLayer( (Layer)i );

					if ( item != null && item is ILevelable )
						CheckLevelable( (ILevelable)item, killer, killed );
				}
			}
		}

		public static void RepairItems( Mobile from )
		{
			if ( from != null )
			{
				for( int i = 0; i < 25; ++i )
				{
					Item item = from.FindItemOnLayer( (Layer)i );

					if ( item is WizardWand )
					{
						BaseWeapon lvlBw = (BaseWeapon)item;
						lvlBw.MaxHitPoints = 100;
						lvlBw.HitPoints = lvlBw.MaxHitPoints;
					}
					else if ( item is BaseArmor && item is ILevelable ) // SO ITEMS NEVER WEAR OUT
					{
						BaseArmor lvlBa = (BaseArmor)item;
						lvlBa.MaxHitPoints = 100;
						lvlBa.HitPoints = lvlBa.MaxHitPoints;
					}
					else if ( item is BaseWeapon && item is ILevelable ) // SO ITEMS NEVER WEAR OUT
					{
						BaseWeapon lvlBw = (BaseWeapon)item;
						lvlBw.MaxHitPoints = 100;
						lvlBw.HitPoints = lvlBw.MaxHitPoints;
					}
					else if ( item is BaseClothing && item is ILevelable ) // SO ITEMS NEVER WEAR OUT
					{
						BaseClothing lvlBc = (BaseClothing)item;
						lvlBc.MaxHitPoints = 100;
						lvlBc.HitPoints = lvlBc.MaxHitPoints;
					}
					else if ( item is BaseTrinket && item is ILevelable ) // SO ITEMS NEVER WEAR OUT
					{
						BaseTrinket lvlBc = (BaseTrinket)item;
						lvlBc.MaxHitPoints = 100;
						lvlBc.HitPoints = lvlBc.MaxHitPoints;
					}
				}
			}
		}

		public static void InvalidateLevel( ILevelable item )
		{
			for( int i = 0; i < ExpTable.Length; ++i )
			{
				if ( item.Experience < ExpTable[i] )
					return;

				item.Level = i + 1;
			}
		}

		public static void CheckLevelable( ILevelable item, Mobile killer, Mobile killed )
		{
			if ( (item.Level >= LevelItems.MaxLevelsCap) || (item.Level >= item.MaxLevel) )
				return;

			int exp = CalcExp( killed );
			int oldLevel = item.Level;
			int expcap = CalcExpCap( oldLevel );

			if ( LevelItems.EnableExpCap && exp > expcap )
				exp = expcap;
			GrantExperience( item, exp, killer );
		}

		public static void GrantExperience( ILevelable item, int exp, Mobile owner = null )
		{
			if ( (item.Level >= LevelItems.MaxLevelsCap) || (item.Level >= item.MaxLevel) )
				return;

			int oldLevel = item.Level;

			item.Experience += exp;

			InvalidateLevel( item );

			if ( item.Level != oldLevel )
				OnLevel( item, oldLevel, item.Level, owner );

			if ( item is Item )
				((Item)item).InvalidateProperties();
		}

        public static void OnLevel(ILevelable item, int oldLevel, int newLevel, Mobile from = null)
        {
            if (newLevel == LevelItems.MaxLevelsCap)
            {
                item.Points += LevelItems.PointsPerLevel * 2;
            }
            else
            {
                item.Points += LevelItems.PointsPerLevel;
            }

			if (from == null) return;

			from.PlaySound( 0x20F );
			from.FixedParticles( 0x376A, 1, 31, 9961, 1160, 0, EffectLayer.Waist );
			from.FixedParticles( 0x37C4, 1, 31, 9502, 43, 2, EffectLayer.Waist );

            string itemdesc;
			if ( item is BaseWeapon )
				itemdesc = "weapon";
			else if ( item is BaseArmor )
				itemdesc = "armor";
			else if ( item is BaseTrinket && item is LevelCandle )
				itemdesc = "candle";
			else if ( item is BaseTrinket && item is LevelLantern )
				itemdesc = "lantern";
			else if ( item is BaseTrinket && item is LevelTorch )
				itemdesc = "torch";
			else if ( item is BaseTrinket && item is LevelTalismanLeather )
				itemdesc = "talisman";
			else if ( item is BaseTrinket && item is LevelTalismanSnake )
				itemdesc = "talisman";
			else if ( item is BaseTrinket && item is LevelTalismanTotem )
				itemdesc = "talisman";
			else if ( item is BaseTrinket && item is LevelTalismanHoly )
				itemdesc = "talisman";
			else if ( item is BaseTrinket && item is LevelBelt )
				itemdesc = "belt";
			else if ( item is BaseTrinket && item is LevelLoinCloth )
				itemdesc = "loin cloth";
			else if ( item is BaseTrinket )
				itemdesc = "jewelry";
			else if ( item is BaseClothing )
				itemdesc = "clothing";
			else
				itemdesc = "item";

			from.SendMessage( "Your "+itemdesc+" has gained a level. It is now level {0}.", newLevel );
        }

		public static void ExtractExperienceToken<T>(T item) where T : Item, ILevelable
		{
			if (item.Experience < 1) return;

			var token = new ItemExperienceToken();
			token.Experience = item.Experience;
			item.Experience = 0;

			if ( item.Parent != null )
			{
				var parent = item.Parent;
				if (parent is Container)
				{
					((Container)parent).DropItem(token);
				}
				else if (parent is Mobile)
				{
					var backpack = ((Mobile)parent).Backpack;
					if (backpack != null)
						backpack.DropItem(token);
				}
			}
			else
			{
				token.Map = item.Map;
				token.Location = item.Location;
			}
		}
	}
}
