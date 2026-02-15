using System;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;
using Server.Items;

namespace Server.SkillHandlers
{
	public class Provocation
	{
		public static void Initialize()
		{
			SkillInfo.Table[(int)SkillName.Provocation].Callback = new SkillUseCallback( OnUse );
		}

		public static TimeSpan OnUse( Mobile m )
		{
			m.RevealingAction();

			BaseInstrument.PickInstrument( m, new InstrumentPickedCallback( OnPickedInstrument ) );

			return TimeSpan.FromSeconds( 1.0 );
		}

		public static void OnPickedInstrument( Mobile from, BaseInstrument instrument )
		{
			from.RevealingAction();
			from.SendLocalizedMessage( 501587 ); // Whom do you wish to incite?
			from.Target = new InternalFirstTarget( from, instrument );
		}

		private class InternalFirstTarget : Target
		{
			private BaseInstrument m_Instrument;

			public InternalFirstTarget( Mobile from, BaseInstrument instrument ) : base( BaseInstrument.GetBardRange( from, SkillName.Provocation ), false, TargetFlags.None )
			{
				m_Instrument = instrument;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				from.RevealingAction();

				if ( targeted is BaseCreature && from.CanBeHarmful( (Mobile)targeted, true ) )
				{
					BaseCreature creature = (BaseCreature)targeted;

					if ( m_Instrument.Parent != from && !m_Instrument.IsChildOf( from.Backpack ) )
					{
						from.SendLocalizedMessage( 1062334 ); // This instrument must be in your backpack or equipped.
					}
                                        else if ( creature.Controlled )
                                        {
                                          from.SendLocalizedMessage( 501590 ); // They are to loyal
                                        }
			                else if ( creature.IsParagon && BaseInstrument.GetBaseDifficulty( creature ) >= 160.0 )
                                        {
                                            from.SendLocalizedMessage( 1049446 ); // You have no chance of provoking 
                                        {       
					else
					{
                                                from.RevealingAction();
                                                m_Instrument.PlayInstrumentWell( from );
						from.SendLocalizedMessage( 501589 ); // To whom do you wish to incite them?
						from.Target = new InternalSecondTarget( from, m_Instrument, creature );
					}
				}
				else
				{
					from.SendLocalizedMessage( 501588 ); // You cannot provoke that!
				}
			}
		}

		private class InternalSecondTarget : Target
		{
			private BaseInstrument m_Instrument;
			private BaseCreature m_Creature;

			public InternalSecondTarget( Mobile from, BaseInstrument instrument, BaseCreature creature ) : base( BaseInstrument.GetBardRange( from, SkillName.Provocation ), false, TargetFlags.None )
			{
				m_Instrument = instrument;
				m_Creature = creature;
			}

			protected override void OnTarget( Mobile from, object targeted )
			{
				from.RevealingAction();

				if ( targeted is BaseCreature )
				{
					BaseCreature creature = (BaseCreature)targeted;

					if ( creature == m_Creature )
					{
						if ( from.CanBeHarmful( creature, true ) )
						{
							if ( !BaseInstrument.CheckMusicianship( from ) )
							{
								from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds( 3 );
								from.SendLocalizedMessage( 500612 );
								m_Instrument.PlayInstrumentBadly( from );
								m_Instrument.ConsumeUse( from );
							}
							else
							{
								double diff = m_Instrument.GetDifficultyFor( creature ) + 25.0;
								double skill = from.Skills[SkillName.Provocation].Value;
								
								// Manual calculation of success chance based on -25/+25 spread
								double chance = (skill - (diff - 25.0)) / 50.0;
								if (chance < 0.0) chance = 0.0;
								if (chance > 1.0) chance = 1.0;

								if ( !from.CheckTargetSkill( SkillName.Provocation, creature, diff - 25.0, diff + 25.0 ) )
								{
									from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds( 3 );
									from.SendLocalizedMessage( 501599 );
									m_Instrument.PlayInstrumentBadly( from );
									m_Instrument.ConsumeUse( from );
								}
								else
								{
									from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds( 3.0 ); 
									m_Instrument.PlayInstrumentWell( from );
									m_Instrument.ConsumeUse( from );

									double prov = from.Skills[SkillName.Provocation].Value;
									double mus = from.Skills[SkillName.Musicianship].Value;

									int baseDamage = (int)((prov + mus) / 4);
									int scaledDamage = (int)(baseDamage * (0.5 + (chance * 0.5)));
									int damage = Utility.RandomMinMax( (int)(scaledDamage * 0.9), (int)(scaledDamage * 1.1) );

									creature.Damage( Math.Max( 1, damage ), from );
									creature.PlaySound( 0x1F9 );

									if ( Utility.RandomDouble() < chance )
									{
										int duration = 5 + (int)(10 * chance);
										int reduction = (int)((prov + mus) / 20);

										ResistanceMod mod = new ResistanceMod( ResistanceType.Physical, -reduction );
										creature.AddResistanceMod( mod );

										Timer.DelayCall( TimeSpan.FromSeconds( duration ), () => 
										{
											if ( creature != null && !creature.Deleted )
												creature.RemoveResistanceMod( mod );
										});

										from.SendMessage( "The music shatters their armor for {0} seconds!", duration );
										creature.FixedParticles( 0x37B9, 10, 20, 5013, 0x480, 2, EffectLayer.Waist );
									}
									else
									{
										from.SendMessage( "Your music confuses them, but their armor remains intact." );
									}
								}
							}
						}
						return; 
					}

					if ( m_Creature.Unprovokable || creature.Unprovokable )
					{
						from.SendLocalizedMessage( 1049446 );
					}
					else if ( m_Creature.Map != creature.Map || !m_Creature.InRange( creature, BaseInstrument.GetBardRange( from, SkillName.Provocation ) ) )
					{
						from.SendLocalizedMessage( 501591 );
					}
					else
					{
						int diff = (int)((m_Instrument.GetDifficultyFor( m_Creature ) + m_Instrument.GetDifficultyFor( creature )) * 0.5) + 25;

						if ( from.CanBeHarmful( m_Creature, true ) && from.CanBeHarmful( creature, true ) )
						{
							if ( !BaseInstrument.CheckMusicianship( from ) )
							{
								from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds( 3 );
								from.SendLocalizedMessage( 500612 );
								m_Instrument.PlayInstrumentBadly( from );
								m_Instrument.ConsumeUse( from );
							}
							else if ( !from.CheckTargetSkill( SkillName.Provocation, creature, diff - 25, diff + 25 ) )
							{
								from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds( 3 );
								from.SendLocalizedMessage( 501599 );
								m_Instrument.PlayInstrumentBadly( from );
								m_Instrument.ConsumeUse( from );
							}
							else
							{
								from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds( 6 );
								from.SendLocalizedMessage( 501602 );
								m_Instrument.PlayInstrumentWell( from );
								m_Instrument.ConsumeUse( from );
								m_Creature.Provoke( from, creature, true );
							}
						}
					}
				}
				else
				{
					from.SendLocalizedMessage( 501588 );
				}
			}
		}
	}
}
