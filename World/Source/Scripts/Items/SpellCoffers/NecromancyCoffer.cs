using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Targeting;
namespace Server.Items
{
    public class NecromancyCoffer : Item
    {
        private bool[] m_Slots;
        public bool[] Slots => m_Slots;
        public virtual int BindingCost => 10000;
        private int m_Charges;
        private int m_ScrollsStored;
        private int m_EmeraldsStored;
        private int m_ArcaneGemsStored;
        [CommandProperty(AccessLevel.GameMaster)]
        public int Charges { get { return m_Charges; } set { m_Charges = value; InvalidateProperties(); } }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ScrollsStored { get { return m_ScrollsStored; } set { m_ScrollsStored = value; InvalidateProperties(); } }
        [CommandProperty(AccessLevel.GameMaster)]
        public int EmeraldsStored { get { return m_EmeraldsStored; } set { m_EmeraldsStored = value; InvalidateProperties(); } }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ArcaneGemsStored { get { return m_ArcaneGemsStored; } set { m_ArcaneGemsStored = value; InvalidateProperties(); } }
       [Constructable]
        public NecromancyCoffer() : base(0x1C0E)
        {
            Name = "Necromancy Spellbound Coffer";
            Weight = 10.0;
            Hue = 1157; 
            m_Slots = new bool[17];
        }
        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045);
                return;
            }
            from.SendGump(new NecromancyCofferGump(this));
        }
        public override bool OnDragDrop(Mobile from, Item dropped)
        {
          if (dropped is ArcaneGem)
          {
            m_ArcaneGemsStored += dropped.Amount;
            from.SendMessage("The coffer absorbs the arcane gem. Total: {0}/1", m_ArcaneGemsStored);
            dropped.Consume();
            CheckConversion(from);
            return true;
          }
          if (dropped is BlankScroll)
          {
            m_ScrollsStored += dropped.Amount;
            from.SendMessage("The coffer absorbs the blank scrolls. Total: {0}/17", m_ScrollsStored);
            dropped.Consume();
            CheckConversion(from);
            return true;
          }
          else if (dropped is Emerald)
          {
            m_EmeraldsStored += dropped.Amount;
            from.SendMessage("The coffer absorbs the Emeralds. Total: {0}/8", m_EmeraldsStored);
            dropped.Consume();
            CheckConversion(from);
            return true;
          }
          if (dropped is SpellScroll scroll) 
          {
            int id = scroll.SpellID;
            if (id >= 100 && id <= 116) 
            {
              int index = id - 100;
              if (m_Slots[index])
                {
                  from.SendMessage("This spell is already preserved within the coffer.");
                  return false;
                }
                m_Slots[index] = true;
                from.SendMessage("You permanetly preserve the {0} spell in the coffer.", scroll.Name ?? "scroll");
                dropped.Delete();
                from.PlaySound(0x1FB); 
                return true;
            }
          }
          from.SendMessage("The coffer only accepts necromancy scrolls, emeralds, arcane gems, and blank scrolls");
          return base.OnDragDrop(from, dropped);
        }
        private void CheckConversion(Mobile from)
        {
          while (m_ScrollsStored >= 17 && m_EmeraldsStored >= 8 && m_ArcaneGemsStored >= 1)
          {
            m_ScrollsStored -= 17;
            m_EmeraldsStored -= 8;
            m_ArcaneGemsStored -= 1;
            m_Charges++;
            from.SendMessage(0x35, "The materials combine! One charge has been added to the coffer.");
            from.PlaySound(0x242);
          }
        }
        public void BeginBinding(Mobile from)
        {
            if (GetTotalScrolls() < 17)
            {
                from.SendMessage("The coffer must be full to bind books.");
                return;
            }
            if (from.Backpack.GetAmount(typeof(Gold)) < BindingCost && from.BankBox.GetAmount(typeof(Gold)) < BindingCost)
            {
                from.SendMessage("You need {0} gold to pay for the binding service.", BindingCost);
                return;
            }
            from.SendMessage("Target the empty necromancer spellbook you wish to fill.");
            from.Target = new InternalBindTarget(this);
        }
        private class InternalBindTarget : Target
        {
            private NecromancyCoffer m_Coffer;
            public InternalBindTarget(NecromancyCoffer coffer) : base(1, false, TargetFlags.None) 
            { 
              m_Coffer = coffer; 
            }
            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is NecromancerSpellbook book) 
                {
                  if (book.SpellCount >= 17)
                  {
                    from.SendMessage("That spellbook is already full!");
                    return;
                  }
                  if (m_Coffer.Charges > 0)
                  {
                    if (from.Backpack.ConsumeTotal(typeof(Gold), m_Coffer.BindingCost) || from.BankBox.ConsumeTotal(typeof(Gold), m_Coffer.BindingCost))
                    {
                      book.Content = 0x1FFFF; 
                      from.SendMessage("You pay the fee, and the coffer imprints the spells into your book.");
                      m_Coffer.Charges--;
                      from.SendMessage("The coffer has lost a charge");
                      from.PlaySound(0x482);
                    }
                    else
                    {
                      from.SendMessage("You lack the gold fee to pay for this.");
                    }
                  }
                }
                else
                {
                    from.SendMessage("That is not a necromancer spellbook.");
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
            writer.Write(m_EmeraldsStored);
            writer.Write(m_ArcaneGemsStored);
            for (int i = 0; i < 17; i++) 
                writer.Write(m_Slots[i]);
        }
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_Charges = reader.ReadInt();
            m_ScrollsStored = reader.ReadInt();
            m_EmeraldsStored = reader.ReadInt();
            m_ArcaneGemsStored = reader.ReadInt();
            m_Slots = new bool[17];
            for (int i = 0; i < 17; i++) 
                m_Slots[i] = reader.ReadBool();
        }
        public NecromancyCoffer(Serial serial) : base(serial) { }
    }
}
