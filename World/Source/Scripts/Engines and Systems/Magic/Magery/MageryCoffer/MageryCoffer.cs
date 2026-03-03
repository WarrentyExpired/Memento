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

        // The gold cost to fill a book
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
                    from.SendMessage("You add the {0} scroll to the collection.", scroll.Name ?? "spell");
                    dropped.Delete();
                    return true;
                }
                
                from.SendMessage("That is not a Magery scroll.");
                return false;
            }

            from.SendMessage("The coffer only accepts spell scrolls.");
            return false;
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

            from.SendMessage("Target the empty spellbook you wish to fill.");
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

                    if (from.Backpack.ConsumeTotal(typeof(Gold), m_Coffer.BindingCost) || from.BankBox.ConsumeTotal(typeof(Gold), m_Coffer.BindingCost))
                    {
                        book.Content = ulong.MaxValue; 
                        from.SendMessage("You pay the fee, and the coffer imprints the spells into your book.");
                        from.PlaySound(0x242); 
                    }
                    else
                    {
                        from.SendMessage("You no longer have enough gold.");
                    }
                }
                else
                {
                    from.SendMessage("That is not a spellbook.");
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
            writer.Write((int)0);
            for (int i = 0; i < 64; i++)
                writer.Write(m_Slots[i]);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_Slots = new bool[64];
            for (int i = 0; i < 64; i++)
                m_Slots[i] = reader.ReadBool();
        }

        public MageryCoffer(Serial serial) : base(serial) { }
    }
}
