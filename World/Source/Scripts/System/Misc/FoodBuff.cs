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
        public FoodBuffTimer() : base(TimeSpan.FromSeconds(MyServerSettings.FoodBuffTimer()), TimeSpan.FromSeconds(MyServerSettings.FoodBuffTimer()))
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
                    HungerBuff(m);
                    ThirstBuff(m);
                }
            }
        }
        public static void HungerBuff(Mobile m)
        {
          bool insideInn = false;
          if (MySettings.S_Belly)
          {
            insideInn = (m.Region is PublicRegion || m.Region is CrashRegion ||
                         m.Region is PrisonArea || m.Region is SafeRegion ||
                         m.Region is StartRegion || m.Region is HouseRegion);
          }
          if (Server.Items.BaseRace.NoFood(m.RaceID) || Server.Items.BaseRace.NoFoodOrDrink(m.RaceID))
            return;
          if (insideInn)
             return;
          if (m.Hunger >= 20)
          {
            ApplyBuff(m, SkillName.Cooking, 0.2, true);
          }
          else if (m.Hunger > 15)
          {
            ApplyBuff(m, SkillName.Cooking, 0.1, true);
          }
          else if (m.Hunger > 10)
          {
            ApplyBuff(m, 10, true);
          }
          else // Hunger < 10
          {
            ApplyDebuff(m, "HitsStam");
          }
        }
        public static void ThirstBuff(Mobile m)
        {
          bool insideInn = false;
          if ( MySettings.S_Belly )
          {
                insideInn = (m.Region is PublicRegion || m.Region is CrashRegion || 
                             m.Region is PrisonArea || m.Region is SafeRegion || 
                             m.Region is StartRegion || m.Region is HouseRegion);
          }
          if (Server.Items.BaseRace.NoFoodOrDrink(m.RaceID) || Server.Items.BaseRace.BrainEater(m.RaceID))
            return;
          if (insideInn)
            return;
          if (m.Thirst >= 20)
          {
            ApplyBuff(m, SkillName.Cooking, 0.2, false);
          }
          else if (m.Thirst > 15)
          {
            ApplyBuff(m, SkillName.Cooking, 0.1, false);
          }
          else if (m.Thirst > 10)
          {
            ApplyBuff(m, 10, false);
          }
          else // Thirst < 10
          {
            ApplyDebuff(m, "Mana");
          }
        }
        private static void ApplyBuff(Mobile m, SkillName skill, double scale, bool isHunger)
        {
          int amount = (int)(m.Skills[skill].Value * scale) + 10;
          ApplyBuff(m, amount, isHunger);
        }
        private static void ApplyBuff(Mobile m, int amount, bool isHunger)
        {
          if (isHunger)
          {
            if (m.Hits < m.HitsMax) m.Hits += amount;
            if (m.Stam < m.StamMax) m.Stam += amount;
          }
          else if (m.Mana < m.ManaMax)
          {
            m.Mana += amount;
          }
        }
        private static void ApplyDebuff(Mobile m, string type)
        {
          int debuff = (int)(MySettings.S_PlayerLevelMod * 10);
          if (type == "HitsStam")
          {
            if (m.Hits > (debuff + 10)) m.Hits -= debuff;
            else if (m.Hits > 10) m.Hits = 10;
            if (m.Stam > (debuff + 10)) m.Stam -= debuff;
            else if (m.Stam > 10) m.Stam = 10;
          }
          else if (type == "Mana")
          {
            if (m.Mana > (debuff + 10)) m.Mana -= debuff;
            else if (m.Mana > 10) m.Mana = 10;
          }
        }
    }
}
