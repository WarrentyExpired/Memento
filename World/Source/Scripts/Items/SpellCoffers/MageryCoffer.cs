using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Targeting;
namespace Server.Items
{
    public class MageryCoffer : Item
    {
        private bool[] m_Slots;
        public bool[] Slots => m_Slots;
        private int m_Charges;
        private int m_ScrollsStored;
        private int m_DiamondsStored;
        private int m_ArcaneGemsStored;
        [CommandProperty(AccessLevel.GameMaster)]
        public int Charges { get { return m_Charges; } set { m_Charges = value; InvalidateProperties(); } }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ScrollsStored { get { return m_ScrollsStored; } set { m_ScrollsStored = value; InvalidateProperties(); } }
        [CommandProperty(AccessLevel.GameMaster)]
        public int DiamondsStored { get { return m_DiamondsStored; } set { m_DiamondsStored = value; InvalidateProperties(); } }
        [CommandProperty(AccessLevel.GameMaster)]
        public int ArcaneGemsStored { get { return m_ArcaneGemsStored; } set { m_ArcaneGemsStored = value; InvalidateProperties(); } }
        public virtual int BindingCost => 10000; 
        [Constructable]
        public MageryCoffer() : base(0x1C0E)
        {
            Name = "Magery Spellbound Coffer";
            Weight = 10.0;
            Hue = 33;
            m_Slots = new bool[64];
        }
        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045);
                return;
            }
            from.SendGump(new MageryCofferGump(this));
        }
        public override bool OnDragDrop(Mobile from, Item dropped)
        {
         if (dropped is BlankScroll)
         {
            m_ScrollsStored += dropped.Amount;
            from.SendMessage("The coffer absorbs the blank scrolls. Total: {0}/64", m_ScrollsStored);
            dropped.Consume();
            CheckConversion(from);
            return true;
         }
         else if (dropped is Diamond)
         {
            m_DiamondsStored += dropped.Amount;
            from.SendMessage("The coffer absorbs the diamonds. Total: {0}/8", m_DiamondsStored);
            dropped.Consume();
            CheckConversion(from);
            return true;
         }
         else if (dropped is ArcaneGem)
         {
           m_ArcaneGemsStored += dropped.Amount;
           from.SendMessage("The coffer absorbs the arcane gems. Total: {0}/1", m_ArcaneGemsStored);
           dropped.Consume();
           CheckConversion(from);
           return true;
         }
         if (dropped is SpellScroll scroll)
            {
                int id = scroll.SpellID;
                if (id >= 0 && id < 64)
                {
                    if (m_Slots[id])
                    {
                        from.SendMessage("This spell is already preserved within the coffer.");
                        return false;
                    }
                    m_Slots[id] = true;
                    from.SendMessage("You permanently preserve the {0} spell in the coffer.", scroll.Name ?? "spell");
                    dropped.Delete();
                    return true;
                }
            }
            from.SendMessage("The coffer only accepts magery spell scrolls, diamonds, arcane gems, and blank scrolls.");
            return base.OnDragDrop(from, dropped);
        }
        private void CheckConversion(Mobile from)
        {
          while (m_ScrollsStored >= 64 && m_DiamondsStored >= 8 && m_ArcaneGemsStored >= 1)
          {
            m_ScrollsStored -= 64;
            m_DiamondsStored -= 8;
            m_ArcaneGemsStored -= 1;
            m_Charges++;
            from.SendMessage(0x35, "The materials combine! One charge has been added to the coffer.");
            from.PlaySound(0x242);
          }
        }
        public void BeginBinding(Mobile from)
        {
            if (GetTotalScrolls() < 64)
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
            private MageryCoffer m_Coffer;
            public InternalBindTarget(MageryCoffer coffer) : base(1, false, TargetFlags.None)
            {
                m_Coffer = coffer;
            }
            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Spellbook book)
                {
                    if (book.SpellCount >= 64)
                    {
                        from.SendMessage("That spellbook is already full!");
                        return;
                    }
                    if (m_Coffer.Charges > 0)
                    {
                      if (from.Backpack.ConsumeTotal(typeof(Gold), m_Coffer.BindingCost) || from.BankBox.ConsumeTotal(typeof(Gold), m_Coffer.BindingCost))
                      {
                        book.Content = ulong.MaxValue; 
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
                    from.SendMessage("That is not a magery spellbook.");
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
            writer.Write(m_DiamondsStored);
            writer.Write(m_ArcaneGemsStored);
            for (int i = 0; i < 64; i++)
                writer.Write(m_Slots[i]);
        }
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_Charges = reader.ReadInt();
            m_ScrollsStored = reader.ReadInt();
            m_DiamondsStored = reader.ReadInt();
            m_ArcaneGemsStored = reader.ReadInt();
            m_Slots = new bool[64];
            for (int i = 0; i < 64; i++)
                m_Slots[i] = reader.ReadBool();
        }
        public MageryCoffer(Serial serial) : base(serial) { }
    }
}
