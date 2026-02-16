using System;
using System.Collections;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;
using Server.Items;

namespace Server.SkillHandlers
{
  public class Provocation
  {
    // Table to track active shard timers to prevent stacking and manage cleanup
    private static Hashtable m_ShardTable = new Hashtable();

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
            from.SendLocalizedMessage( 1062488 ); // The instrument you are trying to play is no longer in your backpack!
          }
          else
          {
            from.RevealingAction();
            from.SendLocalizedMessage( 1008085 ); // Whom do they beat on?
            from.Target = new InternalSecondTarget( from, m_Instrument, creature );
          }
        }
        else
        {
          from.SendLocalizedMessage( 501589 ); // You can't incite that!
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

        if ( targeted is Mobile )
        {
          Mobile creature = (Mobile)targeted;

          if ( m_Creature.Unprovokable || (creature is BaseCreature && ((BaseCreature)creature).Unprovokable) )
          {
            from.SendLocalizedMessage( 1049446 ); // You cannot provoke them!
          }
          else if ( m_Creature.Map != creature.Map || !m_Creature.InRange( creature, BaseInstrument.GetBardRange( from, SkillName.Provocation ) ) )
          {
            from.SendLocalizedMessage( 501591 ); // You are too far away to provoke them!
          }
          else
          {
            int diff = (int)((m_Instrument.GetDifficultyFor( m_Creature ) + m_Instrument.GetDifficultyFor( creature )) * 0.5) + 25;

            if ( from.CanBeHarmful( m_Creature, true ) && from.CanBeHarmful( creature, true ) )
            {
              if ( !BaseInstrument.CheckMusicianship( from ) )
              {
                from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds( 3.0 );
                from.SendLocalizedMessage( 500612 );
                m_Instrument.PlayInstrumentBadly( from );
                m_Instrument.ConsumeUse( from );
              }
              else if ( !from.CheckTargetSkill( SkillName.Provocation, creature, diff - 25, diff + 25 ) )
              {
                from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds( 3.0 );
                from.SendLocalizedMessage( 501599 );
                m_Instrument.PlayInstrumentBadly( from );
                m_Instrument.ConsumeUse( from );
              }
              else
              {
                from.NextSkillTime = DateTime.Now + TimeSpan.FromSeconds( 6.0 );
                m_Instrument.PlayInstrumentWell( from );
                m_Instrument.ConsumeUse( from );

                // SHATTER & SHARDED SKIN LOGIC (Self-Provoke)
                if ( creature == m_Creature || creature == from )
                {
                  double prov = from.Skills[SkillName.Provocation].Value;
                  int shatterAmount = (int)(prov / -5); 
                  TimeSpan duration = TimeSpan.FromSeconds( prov / 2 );

                  // Refresh logic: Stop old timer if it exists
                  Timer existingTimer = (Timer)m_ShardTable[m_Creature];
                  if ( existingTimer != null )
                    existingTimer.Stop();

                  ResistanceMod shatterMod = new ResistanceMod( ResistanceType.Physical, shatterAmount );
                  m_Creature.AddResistanceMod( shatterMod );

                  // Start the Sharded Skin DoT and Aggro Timer
                  ShardedSkinTimer newTimer = new ShardedSkinTimer( m_Creature, from, duration );
                  m_ShardTable[m_Creature] = newTimer;
                  newTimer.Start();

                  // Immediate Aggro: Make them turn on you immediately
                  m_Creature.Combatant = from;
                  m_Creature.Warmode = true;

                  from.SendMessage( "The creature's armor shatters! It roars in pain and fixes its eyes on you!" );
                  m_Creature.FixedParticles( 0x37B9, 10, 5, 5052, EffectLayer.LeftFoot );

                  Timer.DelayCall( duration, delegate { m_Creature.RemoveResistanceMod( shatterMod ); } );
                }
                else
                {
                  from.SendLocalizedMessage( 501602 ); // Your music succeeds, as you start a fight.
                  m_Creature.Provoke( from, creature, true );
                }
              }
            }
          }
        }
        else
        {
          from.SendLocalizedMessage( 501589 );
        }
      }
    }

    private class ShardedSkinTimer : Timer
    {
      private Mobile m_Victim;
      private Mobile m_Attacker;
      private DateTime m_End;

      public ShardedSkinTimer( Mobile victim, Mobile attacker, TimeSpan duration ) : base( TimeSpan.FromSeconds( 2.0 ), TimeSpan.FromSeconds( 2.0 ) )
      {
        m_Victim = victim;
        m_Attacker = attacker;
        m_End = DateTime.Now + duration;
        Priority = TimerPriority.TwoFiftyMS;
      }

      protected override void OnTick()
      {
        if ( m_Victim == null || !m_Victim.Alive || m_Victim.Deleted || DateTime.Now > m_End )
        {
          if ( m_Victim != null && m_ShardTable.Contains( m_Victim ) )
            m_ShardTable.Remove( m_Victim );
          Stop();
          return;
        }

        int damage = (int)(m_Attacker.Skills[SkillName.Provocation].Value / 20);
        if ( damage < 1 ) damage = 1;

        // Apply DoT damage
        m_Victim.Damage( damage, m_Attacker );

        // FORCE AGGRO: Every tick, re-affirm the player as the primary target
        if ( m_Victim.Combatant == null || m_Victim.Combatant == m_Victim )
        {
          m_Victim.Combatant = m_Attacker;
          m_Victim.Warmode = true;
        }

        m_Victim.PlaySound( 0x133 ); // Metal clink
        m_Victim.FixedEffect( 0x377A, 1, 32 ); // Small spark
      }
    }
  }
}
