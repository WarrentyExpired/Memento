// $Id: //depot/c%23/RunUO Core Scripts/RunUO Core Scripts/Items/Weapons/Abilities/ForceofNature.cs#2 $

using System;
using Server;
using System.Collections;

namespace Server.Items
{
	public class ForceOfNature : WeaponAbility
	{
		public ForceOfNature()
		{
		}

		public override int BaseMana { get { return 40; } }

		public override void OnHit(Mobile attacker, Mobile defender, int damage)
		{
            if ( !Validate( attacker ) || !CheckMana( attacker, true ) )
                return;
                ClearCurrentAbility( attacker );
                double tactics = attacker.Skills[SkillName.Tactics].Value;
                double armsLore = attacker.Skills[SkillName.ArmsLore].Value;
                // 1. The Explosion Damage (Scales with Tactics)
                // Range: 15 to 35 bonus damage based on Tactics skill
                int bonusDamage = 15 + (int)(tactics / 6.0); 
                // 2. The Resistance Breach (Scales with Arms Lore)
                // High Arms Lore allows the Force of Nature to hit like a truck by ignoring resists
                int directPercent = 0;
                if ( armsLore >= 80.0 )
                {
                    // At 120 Arms Lore, 50% of this bonus damage becomes "Direct" (ignores all resists)
                    directPercent = (int)((armsLore - 80.0) * 1.25); 
                }
                // Visuals and Sound
                attacker.PlaySound( 0x107 ); // Primal roar/wind sound
                attacker.FixedParticles( 0x376A, 1, 32, 0x1535, 0x0, 0, EffectLayer.Waist ); // Sparking energy
                // 3. Apply the Explosion
                // We deal the bonus damage. If directPercent > 0, we split it.
                int directDamage = (int)(bonusDamage * (directPercent / 100.0));
                int elementalDamage = bonusDamage - directDamage;
                if ( directDamage > 0 )
                {
                    // Direct portion (Pure breach)
                    AOS.Damage( defender, attacker, directDamage, false, 0, 0, 0, 0, 0, 0, 100, false, false, false );
                }
                if ( elementalDamage > 0 )
                {
                    // Elemental portion (Nature's wrath - split across all elements)
                    AOS.Damage( defender, attacker, elementalDamage, false, 0, 25, 25, 25, 25, 0, 0, false, false, false );
                }
                // 4. The Secondary Effect: Daze
                // Forces the defender to lose some stamina, representing the physical shock
                int stamLoss = (int)(tactics / 5.0);
                defender.Stam -= stamLoss;
                attacker.SendMessage( "You unleash the Force of Nature, breaching their defenses!" );
                defender.SendMessage( "You are staggered by a primal elemental blast!" );
        }
		private static Hashtable m_Table = new Hashtable();

		public static bool RemoveCurse(Mobile m)
		{
			Timer t = (Timer)m_Table[m];

			if (t == null)
				return false;

			t.Stop();
			m.SendLocalizedMessage(1061687); // You can breath normally again.

			m_Table.Remove(m);
			return true;
		}

		private class InternalTimer : Timer
		{
			private Mobile m_Target, m_From;
			private double m_MinBaseDamage, m_MaxBaseDamage;

			private DateTime m_NextHit;
			private int m_HitDelay;

			private int m_Count, m_MaxCount;

			public InternalTimer(Mobile target, Mobile from)
				: base(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.1))
			{
				Priority = TimerPriority.FiftyMS;

				m_Target = target;
				m_From = from;

				double spiritLevel = from.Skills[SkillName.Spiritualism].Value / 15;

				m_MinBaseDamage = spiritLevel - 2;
				m_MaxBaseDamage = spiritLevel + 1;

				m_HitDelay = 5;
				m_NextHit = DateTime.Now + TimeSpan.FromSeconds(m_HitDelay);

				m_Count = (int)spiritLevel;

				if (m_Count < 4)
					m_Count = 4;

				m_MaxCount = m_Count;
			}

			protected override void OnTick()
			{
				if (!m_Target.Alive)
				{
					m_Table.Remove(m_Target);
					Stop();
				}

				if (!m_Target.Alive || DateTime.Now < m_NextHit)
					return;

				--m_Count;

				if (m_HitDelay > 1)
				{
					if (m_MaxCount < 5)
					{
						--m_HitDelay;
					}
					else
					{
						int delay = (int)(Math.Ceiling((1.0 + (5 * m_Count)) / m_MaxCount));

						if (delay <= 5)
							m_HitDelay = delay;
						else
							m_HitDelay = 5;
					}
				}

				if (m_Count == 0)
				{
					m_Target.SendLocalizedMessage(1061687); // You can breath normally again.
					m_Table.Remove(m_Target);
					Stop();
				}
				else
				{
					m_NextHit = DateTime.Now + TimeSpan.FromSeconds(m_HitDelay);

					double damage = m_MinBaseDamage + (Utility.RandomDouble() * (m_MaxBaseDamage - m_MinBaseDamage));

					damage *= (3 - (((double)m_Target.Stam / m_Target.StamMax) * 2));

					if (damage < 1)
						damage = 1;

					if (!m_Target.Player)
						damage *= 1.75;

					AOS.Damage(m_Target, m_From, (int)damage, 0, 0, 0, 100, 0);
				}
			}
		}
	}
}
