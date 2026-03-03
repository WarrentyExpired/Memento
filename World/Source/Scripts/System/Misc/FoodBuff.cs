using System;
using Server;
using Server.Network;
using Server.Mobiles;
using Server.Regions;

namespace Server.Misc
{
    public class FoodBuffTimer : Timer
    {
        public static void Initialize()
        {
            new FoodBuffTimer().Start();
        }

        // Ticking every 1 second for the scrolling "Juiced" effect
        public FoodBuffTimer() : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
        {
            Priority = TimerPriority.OneSecond;
        }

        protected override void OnTick()
        {
            foreach (NetState state in NetState.Instances)
            {
                Mobile m = state.Mobile;
                if (m is PlayerMobile && m.Alive)
                {
                    ProcessStats(m);
                }
            }
        }

        public static void ProcessStats(Mobile m)
        {
            if (IsInsideSafeRegion(m))
                return;

            double timerDuration = (double)MyServerSettings.FoodBuffTimer();
            
            // Camping Threshold: 0 Skill = 10, 125 Skill = 5.
            double campingBonus = (m.Skills[SkillName.Camping].Value / 125.0) * 5.0;
            double starvationThreshold = 10.0 - campingBonus;

            // 1. Hunger Effects (Hits/Stam)
            if (!Server.Items.BaseRace.NoFood(m.RaceID) && !Server.Items.BaseRace.NoFoodOrDrink(m.RaceID))
            {
                if (m.Hunger >= starvationThreshold)
                {
                    // Optimization: Only run math if they actually need stats
                    if (m.Hits < m.HitsMax || m.Stam < m.StamMax)
                        ApplyJuicedEffect(m, timerDuration, true);
                }
                else
                {
                    ApplyJuicedDebuff(m, timerDuration, true);
                }
            }

            // 2. Thirst Effects (Mana)
            if (!Server.Items.BaseRace.NoFoodOrDrink(m.RaceID) && !Server.Items.BaseRace.BrainEater(m.RaceID))
            {
                if (m.Thirst >= starvationThreshold)
                {
                    if (m.Mana < m.ManaMax)
                        ApplyJuicedEffect(m, timerDuration, false);
                }
                else
                {
                    ApplyJuicedDebuff(m, timerDuration, false);
                }
            }
        }

        private static void ApplyJuicedEffect(Mobile m, double duration, bool isHunger)
        {
            // 10% Base Pool + 1% for every 5 points in Cooking
            double basePercent = 0.10; 
            double cookingBonus = (m.Skills[SkillName.Cooking].Value / 5.0) * 0.01; 
            double totalPercent = basePercent + cookingBonus;
            
            // Saturation Efficiency: Benefit scales with how full you are
            double saturationMult = (isHunger ? m.Hunger : m.Thirst) / 20.0;
            
            double totalPoints = (isHunger ? m.HitsMax : m.ManaMax) * totalPercent * saturationMult;
            double perSecond = totalPoints / duration;

            if (isHunger)
            {
                int gain = (int)Math.Max(1, perSecond);
                if (m.Hits < m.HitsMax) m.Hits += gain;
                if (m.Stam < m.StamMax) m.Stam += gain;
            }
            else
            {
                if (m.Mana < m.ManaMax) m.Mana += (int)Math.Max(1, perSecond);
            }
        }

        private static void ApplyJuicedDebuff(Mobile m, double duration, bool isHunger)
        {
            // Starvation: 5% pool loss distributed per second over the window
            double lossPerWindow = 0.05; 
            double lossPerSecond = ((isHunger ? m.HitsMax : m.ManaMax) * lossPerWindow) / duration;
            int loss = (int)Math.Max(1, lossPerSecond);

            if (isHunger)
            {
                if (m.Hits > (loss + 10)) m.Hits -= loss;
                else m.Hits = 10;

                if (m.Stam > (loss + 10)) m.Stam -= loss;
                else m.Stam = 10;
            }
            else
            {
                if (m.Mana > (loss + 10)) m.Mana -= loss;
                else m.Mana = 10;
            }
        }

        private static bool IsInsideSafeRegion(Mobile m)
        {
            if (!MySettings.S_Belly) return false;

            return (m.Region is PublicRegion || m.Region is CrashRegion || 
                    m.Region is PrisonArea || m.Region is SafeRegion || 
                    m.Region is StartRegion || m.Region is HouseRegion);
        }
    }
}
