using System;
using Server;
using System.Collections;

namespace Server.Items
{
	/// <summary>
	/// Raises your defenses for a short time.
	/// </summary>
	public class Block : WeaponAbility
	{
		public Block()
		{
		}

		public override int BaseMana{ get{ return 20; } }

		public override bool CheckSkills( Mobile from )
		{
			return base.CheckSkills( from );
		}

		public override void OnHit( Mobile attacker, Mobile defender, int damage )
		{
			if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
				return;

			ClearCurrentAbility( attacker );

			attacker.SendLocalizedMessage( 1063345 ); // You block an attack!
			defender.SendLocalizedMessage( 1063346 ); // Your attack was blocked!

			attacker.FixedParticles( 0x37C4, 1, 16, 0x251D, 0x39D, 0x3, EffectLayer.RightHand );

			int bonus = (int)(10.0 * ((Math.Max( attacker.Skills[SkillName.Tactics].Value, attacker.Skills[SkillName.Anatomy].Value ) - 50.0) / 70.0 + 5));
            double tactics = attacker.Skills[SkillName.Tactics].Value;
            double anatomy = attacker.Skills[SkillName.Anatomy].Value;
            double armsLore = attacker.Skills[SkillName.ArmsLore].Value;
            int blockBonus = (int)(5.0 + (tactics / 12.0));
            if ( anatomy >= 50.0 )
            {
                int damageBasis = (int)(defender.Str * 0.20);
                int reflectPercent = (int)((anatomy - 50.0) / 2.33); // Max ~30%
                int reflectedDamage = (int)(damageBasis * (reflectPercent / 100.0));

                if ( reflectedDamage > 0 )
                {
                    AOS.Damage( defender, attacker, reflectedDamage, false, 0, 0, 0, 0, 0, 0, 100, false, false, false );
                    attacker.SendMessage( "You block the blow and counter-strike for {0} damage!", reflectedDamage );
                    defender.PlaySound( 0x3BC ); // Metal "Clang" sound
                }
            }
            if ( armsLore >= 80.0 )
            {
                attacker.Stam += (int)(armsLore / 10.0); // Restore a bit of stamina on a perfect block
                attacker.SendMessage( "Your mastery of arms allows a effortless parry." );
            }
            BeginBlock( attacker, blockBonus );
            attacker.FixedParticles( 0x37C4, 1, 16, 0x251D, 0x39D, 0x3, EffectLayer.RightHand );           
		}

		private class BlockInfo
		{
			public Mobile m_Target;
			public Timer m_Timer;
			public int m_Bonus;

			public BlockInfo( Mobile target, int bonus )
			{
				m_Target = target;
				m_Bonus = bonus;
			}
		}

		private static Hashtable m_Table = new Hashtable();

		public static bool GetBonus( Mobile targ, ref int bonus )
		{
			BlockInfo info = m_Table[targ] as BlockInfo;

			if ( info == null )
				return false;

			bonus = info.m_Bonus;
			return true;
		}

		public static void BeginBlock( Mobile m, int bonus )
		{
			EndBlock( m );

			BlockInfo info = new BlockInfo( m, bonus );
			info.m_Timer = new InternalTimer( m );

			m_Table[m] = info;
		}

		public static void EndBlock( Mobile m )
		{
			BlockInfo info = m_Table[m] as BlockInfo;

			if ( info != null )
			{
				if ( info.m_Timer != null )
					info.m_Timer.Stop();

				m_Table.Remove( m );
			}
		}

		private class InternalTimer : Timer
		{
			private Mobile m_Mobile;

			public InternalTimer( Mobile m ) : base( TimeSpan.FromSeconds( 6.0 ) )
			{
				m_Mobile = m;
				Priority = TimerPriority.TwoFiftyMS;
			}

			protected override void OnTick()
			{
				EndBlock( m_Mobile );
			}
		}
	}
}
