using System;
using Server;
using Server.Network;
using Server.Mobiles;
using Server.Regions;

namespace Server.Misc
{
	public class FoodDecayTimer : Timer
	{
		public static void Initialize()
		{
			new FoodDecayTimer().Start();
		}

		public FoodDecayTimer() : base( TimeSpan.FromMinutes( MyServerSettings.FoodCheck() ), TimeSpan.FromMinutes( MyServerSettings.FoodCheck() ) )
		{
			Priority = TimerPriority.OneMinute;
		}

		protected override void OnTick()
		{
			FoodDecay();			
		}

		public static void FoodDecay()
		{
			foreach ( NetState state in NetState.Instances )
			{
				HungerDecay( state.Mobile );
				ThirstDecay( state.Mobile );
			}
		}

		public static void HungerDecay( Mobile m )
		{
			if ( m != null && m is PlayerMobile )
			{
				Server.Items.BaseRace.SyncRace( m, true );
				BuffInfo.CleanupIcons( m, false );

				if ( IsInSafeZone( m ) )
					return;

				// Dynamic Threshold: 0 Camping = 10, 125 Camping = 5
				double campingBonus = (m.Skills[SkillName.Camping].Value / 125.0) * 5.0;
				double starvationThreshold = 10.0 - campingBonus;

				if ( m.Skills[SkillName.Camping].Value >= Utility.RandomMinMax( 1, 200 ) ){}
				else if ( Server.Items.BaseRace.NoFood( m.RaceID ) ){ m.Hunger = 20; }
				else if ( Server.Items.BaseRace.NoFoodOrDrink( m.RaceID ) ){ m.Thirst = 20; m.Hunger = 20; }
				else
				{
					if ( m.Hunger >= 1 )
					{
						m.Hunger -= 1;

						// Trigger "Extremely Hungry" when they hit the starvation threshold
						if ( m.Hunger <= starvationThreshold )
						{
							m.SendMessage( "You are extremely hungry and beginning to weaken." );
							m.LocalOverheadMessage(MessageType.Emote, 0x916, true, "I am extremely hungry.");
						}
						else if ( m.Hunger < 10 )
						{
							m.SendMessage( "You are getting very hungry." );
							m.LocalOverheadMessage(MessageType.Emote, 0x916, true, "I am getting very hungry.");
						}
					}	
					else
					{
						m.SendMessage( "You are starving to death!" );
						m.LocalOverheadMessage(MessageType.Emote, 0x916, true, "I am starving to death!");
					}
				}
			}
			else if ( m is BaseCreature && ((BaseCreature)m).Controlled && m.Hunger >= 1 )
			{
				m.Hunger -= 1;
			}
		}

		public static void ThirstDecay( Mobile m )
		{
			if ( m != null && m is PlayerMobile )
			{
				Server.Items.BaseRace.SyncRace( m, true );

				if ( IsInSafeZone( m ) )
					return;

				// Dynamic Threshold: 0 Camping = 10, 125 Camping = 5
				double campingBonus = (m.Skills[SkillName.Camping].Value / 125.0) * 5.0;
				double starvationThreshold = 10.0 - campingBonus;

				if ( m.Skills[SkillName.Camping].Value >= Utility.RandomMinMax( 1, 200 ) ){}
				else if ( Server.Items.BaseRace.NoFoodOrDrink( m.RaceID ) ){ m.Thirst = 20; m.Hunger = 20; }
				else if ( Server.Items.BaseRace.BrainEater( m.RaceID ) ){ m.Thirst = 20; }
				else
				{
					if ( m.Thirst >= 1 )
					{
						m.Thirst -= 1;

						// Trigger "Extremely Thirsty" when they hit the starvation threshold
						if ( m.Thirst <= starvationThreshold )
						{
							m.SendMessage( "You are exhausted from thirst and losing focus." );
							m.LocalOverheadMessage(MessageType.Emote, 0x916, true, "I am extremely thirsty.");
						}
						else if ( m.Thirst < 10 )
						{
							m.SendMessage( "You are getting thirsty." );
							m.LocalOverheadMessage(MessageType.Emote, 0x916, true, "I am getting thirsty.");
						}
					}
					else
					{
						m.SendMessage( "You are dying of thirst!" );
						m.LocalOverheadMessage(MessageType.Emote, 0x916, true, "I am dying of thirst!");
					}
				}
			}
			else if ( m is BaseCreature && ((BaseCreature)m).Controlled && m.Thirst >= 1 )
			{
				m.Thirst -= 1;
			}
		}

		private static bool IsInSafeZone( Mobile m )
		{
			if ( !MySettings.S_Belly ) return false;

			return ( m.Region is PublicRegion || m.Region is CrashRegion ||
					 m.Region is PrisonArea || m.Region is SafeRegion ||
					 m.Region is StartRegion || m.Region is HouseRegion );
		}
	}
}
