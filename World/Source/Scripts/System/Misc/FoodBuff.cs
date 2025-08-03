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
        if ( Server.Items.BaseRace.NoFood( m.RaceID ) ){ m.Hunger = 10; }
        else if ( Server.Items.BaseRace.NoFoodOrDrink( m.RaceID ) ){ m.Thirst = 10; m.Hunger = 10; }
        else
        {
          if ( m.Hunger >=20 )
          {
            var buff = (int) ( m.Skills[SkillName.Cooking].Value * 0.1 ) + m.Hunger;
            if ( m.Hits < m.HitsMax)
                 m.Hits += buff;
            if ( m.Stam < m.StamMax)
                 m.Stam += buff;
          }
          else if ( m.Hunger > 15 && m.Hunger < 20 )
          {
            var buff = (int) ( m.Skills[SkillName.Cooking].Value * 0.1 ) + m.Hunger;
            if ( m.Hits < m.HitsMax)
                 m.Hits += buff;
            if ( m.Stam < m.StamMax)
                 m.Stam += buff;
          }
          else if ( m.Hunger > 10 && m.Hunger < 15 )
          {
            var buff = (int) ( m.Skills[SkillName.Cooking].Value * 0.1 ) + m.Hunger;
            if ( m.Hits < m.HitsMax)
                 m.Hits += buff;
            if ( m.Stam < m.StamMax)
                 m.Stam += buff;
          }
          else if ( m.Hunger < 10 )
          {
            var debuff = 5;
            if ( m.Hits > 10 )
                 m.Hits -= debuff;
            if ( m.Stam > 20)
                 m.Stam -= debuff;
          }
        } 
      }     
    }
    public static void ThirstBuff( Mobile m )
    {
      if ( m is PlayerMobile )
      {
        if ( Server.Items.BaseRace.NoFoodOrDrink( m.RaceID ) ){ m.Thirst = 10; m.Hunger = 10; }
        else if ( Server.Items.BaseRace.BrainEater( m.RaceID ) ){ m.Thirst = 10; }
        else
        {
          if ( m.Thirst >= 20 )
          {
            var buff = (int) ( m.Skills[SkillName.Cooking].Value * 0.1 ) + m.Thirst;
            if ( m.Mana < m.ManaMax)
                 m.Mana += buff;
          }
          else if (m.Thirst > 15 && m.Thirst < 20 )
          {
            var buff = (int) ( m.Skills[SkillName.Cooking].Value * 0.1 ) + m.Thirst;
            if (m.Mana < m.ManaMax)
                m.Mana += buff;
          }
          else if ( m.Thirst > 10 && m.Thirst < 15 )
          {
            var buff = (int) ( m.Skills[SkillName.Cooking].Value * 0.1 ) + m.Thirst;
            if ( m.Mana < m.ManaMax )
                 m.Mana += buff;
          }
          else if ( m.Thirst < 10 )
          {
            var debuff = 5;
            if ( m.Mana < m.ManaMax )
                 m.Mana -= debuff;
          }
        }
      }
    }
  }
}
