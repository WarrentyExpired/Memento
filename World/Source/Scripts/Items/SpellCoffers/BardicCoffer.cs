using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Targeting;
namespace Server.Items
{
  public class BardicCoffer : Item
  {
    private bool[] m_Slots;
    public bool[] Slots => m_Slots;
    private int m_Charges;
    private int m_ScrollsStored;
    private int m_SapphiresStored;
    private int m_ArcaneGemsStored;
    [CommandProperty(AccessLevel.GameMaster)]
    public int Charges { get { return m_Charges; } set { m_Charges = value; InvalidateProperties(); } }
    [CommandProperty(AccessLevel.GameMaster)]
    public int ScrollsStored { get { return m_ScrollsStored; } set { m_ScrollsStored = value; InvalidateProperties(); } }
    [CommandProperty(AccessLevel.GameMaster)]
    public int SapphiresStored { get { return m_SapphiresStored; } set { m_SapphiresStored = value; InvalidateProperties(); } }
    [CommandProperty(AccessLevel.GameMaster)]
    public int ArcaneGemsStored { get { return m_ArcaneGemsStored; } set { m_ArcaneGemsStored = value; InvalidateProperties(); } } 
    public virtual int BindingCost => 10000;
    [Constructable]
    public BardicCoffer() : base(0x1C0E)
    {
      Name = "Bardic Song Coffer";
      Weight = 10.0;
      Hue = 492;
      m_Slots = new bool[16];
    }
    public override void OnDoubleClick(Mobile from)
    {
      if (!from.InRange(GetWorldLocation(), 2)) 
        return;
      if (from.HasGump(typeof(BardicCofferGump)))
      {
        from.CloseGump(typeof(BardicCofferGump));
      }
      else
      {
        from.SendGump(new BardicCofferGump(this));
      }
    }
    public override bool OnDragDrop(Mobile from, Item dropped)
    {
      if (dropped is ArcaneGem)
      {
        m_ArcaneGemsStored += dropped.Amount;
        from.SendMessage("The coffer absorbs the arcane gems. Total: {0}/1", m_ArcaneGemsStored);
        dropped.Consume();
        CheckConversion(from);
        if (from.HasGump(typeof(BardicCofferGump)))
        {
            from.CloseGump(typeof(BardicCofferGump));
            from.SendGump(new BardicCofferGump(this));
        }       
        return true;
      }
      if (dropped is BlankScroll)
        {
          m_ScrollsStored += dropped.Amount;
          from.SendMessage("The coffer absorbs the blank scrolls. Total: {0}/16", m_ScrollsStored);
          dropped.Consume();
          CheckConversion(from);
          if (from.HasGump(typeof(BardicCofferGump)))
           {
            from.CloseGump(typeof(BardicCofferGump));
            from.SendGump(new BardicCofferGump(this));
           }          
          return true;
        }
        if (dropped is Sapphire)
        {
          m_SapphiresStored += dropped.Amount;
          from.SendMessage("The coffer absorbs the sapphires. Total: {0}/8", m_SapphiresStored);
          dropped.Consume();
          CheckConversion(from);
          if (from.HasGump(typeof(BardicCofferGump)))
          {
            from.CloseGump(typeof(BardicCofferGump));
            from.SendGump(new BardicCofferGump(this));
          } 
          return true;
        }
        if (dropped is SpellScroll scroll)
        {
          int id = scroll.SpellID;
          if (id >= 351 && id <= 366)
          {
            int index = id - 351;
            if (m_Slots[index])
            {
              from.SendMessage("That song is already preserved within the coffer.");
              return false;
            }
            m_Slots[index] = true;
            from.SendMessage("You permanently preserve the {0} song.", scroll.Name ?? "song");
            scroll.Consume();
            if (from.HasGump(typeof(BardicCofferGump)))
            {
              from.CloseGump(typeof(BardicCofferGump));
              from.SendGump(new BardicCofferGump(this));
            } 
            return true;
          }
        }
        from.SendMessage("The coffer only accepts bardic spell scrolls, sapphires, arcane gems, and blank scrolls.");
        return base.OnDragDrop(from, dropped);
      }
      private void CheckConversion(Mobile from)
      {
        while (m_ScrollsStored >= 16 && m_SapphiresStored >= 8 && m_ArcaneGemsStored >= 1)
        {
          m_ScrollsStored -= 16;
          m_SapphiresStored -= 8;
          m_ArcaneGemsStored -= 1;
          m_Charges++;
          from.SendMessage(492, "The material combines! One charged has been added to the coffer.");
          from.PlaySound(0x5D2);
        }
      }
      public void BeginBinding(Mobile from)
      {
        if (GetTotalScrolls() < 16)
        {
          from.SendMessage("The coffer must be full to bind books.");
          return;
        }
        if (from.Backpack == null || (from.Backpack.GetAmount(typeof(Gold)) < BindingCost && from.BankBox.GetAmount(typeof(Gold)) < BindingCost))
        {
                from.SendMessage("You need {0} gold to pay for the binding service.", BindingCost);
                return;
            }
            from.SendMessage("Target the empty magery spellbook you wish to fill.");
            from.Target = new InternalBindTarget(this);
        }
      private class InternalBindTarget : Target
      {
        private BardicCoffer m_Coffer;
        public InternalBindTarget(BardicCoffer coffer) : base(1, false, TargetFlags.None)
        {
          m_Coffer = coffer;
        }
        protected override void OnTarget(Mobile from, object targeted)
        {
          if (targeted is SongBook book)
          {
            if (book.SpellCount >= 16)
            {
              from.SendMessage("That spellbook is already full!");
              return;
            }
            if (m_Coffer.Charges > 0)
            {
              if (from.Backpack.ConsumeTotal(typeof(Gold), m_Coffer.BindingCost) || from.BankBox.ConsumeTotal(typeof(Gold), m_Coffer.BindingCost))
              {
                book.Content = 0xFFFF; 
                from.SendMessage("You pay the fee, and the coffer imprints the spells into your book.");
                m_Coffer.Charges--;
                from.SendMessage("The coffer has lost a charge.");
                from.PlaySound(0x242); 
              }
              else
              {
                from.SendMessage("You lack the gold fee to pay for this.");
              }
            }
          }
          else
          {
            from.SendMessage("That is not a bardic song spellbook.");
          }
        }
      }
      public int GetTotalScrolls()
      {
        int count = 0;
        if (m_Slots == null) return 0;
        for (int i = 0; i < m_Slots.Length; i++)
        if (m_Slots[i]) count++;
          return count;
      }
      public override void Serialize(GenericWriter writer)
      {
        base.Serialize(writer);
        writer.Write((int)1);
        writer.Write(m_Charges);
        writer.Write(m_ScrollsStored);
        writer.Write(m_SapphiresStored);
        writer.Write(m_ArcaneGemsStored);
        for (int i = 0; i < 16; i++) writer.Write(m_Slots[i]);
      }
      public override void Deserialize(GenericReader reader)
      {
        base.Deserialize(reader);
        int version = reader.ReadInt();
        m_Charges = reader.ReadInt();
        m_ScrollsStored = reader.ReadInt();
        m_SapphiresStored = reader.ReadInt();
        m_ArcaneGemsStored = reader.ReadInt();
        m_Slots = new bool[16];
        for (int i = 0; i < 16; i++) m_Slots[i] = reader.ReadBool();
      }
      public BardicCoffer(Serial serial) : base(serial) { }
    }
}
