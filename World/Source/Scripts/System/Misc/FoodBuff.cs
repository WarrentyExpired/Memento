using System;
using Server;
using Server.Network;
using Server.Mobiles;

namespace Server.Misc
{
  public class FoodBuffTimer : Timer
  {
    public static void Initialize()
    {
      new FoodBuffTimer().Start();
    }
    public FoodBuffTimer() : base( TimeSpan.FromSeconds( MyServerSettings.FoodBuffTimer() ), TimeSpan.FromSeconds( MyServerSettings.FoodBuffTimer() ) )
    {
      Priority = TimerPriority.OneSecond;
    }
    protected override void OnTick()
    {
      FoodBuffTick();
    }
    public static void FoodBuffTick()
    {
      foreach ( NetState state in NetState.Instances )
      {
        HungerBuff( state.Mobile );
        ThirstBuff( state.Mobile );
      }
    }
    public static void HungerBuff( Mobile m )
    {
      if ( m is PlayerMobile )
      {
        if ( Server.Items.BaseRace.NoFood( m.RaceID ) ){ m.Hunger = 20; } // No buff for monster races
        else if ( Server.Items.BaseRace.NoFoodOrDrink( m.RaceID ) ){ m.Thirst = 20; m.Hunger = 20; } // No buff for monster races
        else
        {
          if ( m.Hunger >=11 )
          {
            var buff = (int) ( m.Skills[SkillName.Cooking].Value * 0.1 ) + m.Hunger;
            if ( m.Hits < m.HitsMax)
                 m.Hits += buff;
            if ( m.Stam < m.StamMax)
                 m.Stam += buff;
          }
          else if ( m.Hunger <= 9  )
          {
            var hitsdebuff = 10;
            var stamdebuff = 15;
            if ( m.Hits > 20 )
                 m.Hits -= hitsdebuff;
            if ( m.Stam > 30)
                 m.Stam -= stamdebuff;
          }
        } 
      }     
    }
    public static void ThirstBuff( Mobile m )
    {
      if ( m is PlayerMobile )
      {
        if ( Server.Items.BaseRace.NoFoodOrDrink( m.RaceID ) ){ m.Thirst = 20; m.Hunger = 20; } // No buff for monsters that don't need food/water
        else if ( Server.Items.BaseRace.BrainEater( m.RaceID ) ){ m.Thirst = 20; } // No buff for monsters that eat brains
        else
        {
          if ( m.Thirst >= 11 )
          {
            var buff = (int) ( m.Skills[SkillName.Cooking].Value * 0.1 ) + m.Thirst;
            if ( m.Mana < m.ManaMax)
                 m.Mana += buff;
          }
          else if ( m.Thirst <= 9 )
          {
            var debuff = 20;
            if ( m.Mana > 32 )
                 m.Mana -= debuff;
          }
        }
      }
    }
  }
}
