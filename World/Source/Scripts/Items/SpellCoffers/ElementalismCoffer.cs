using System;
using Server;
using Server.Items;
using Server.Network;
using Server.Targeting;

namespace Server.Items
{
    public class ElementalismCoffer : Item
    {
        private bool[] m_Slots;
        public bool[] Slots => m_Slots;

        public virtual int BindingCost => 10000; 

        [Constructable]
        public ElementalismCoffer() : base(0x1C0E)
        {
            Name = "Elementalism Spellbound Coffer";
            Weight = 10.0;
            Hue = 1266; 
            m_Slots = new bool[32]; // Set to 32 spells
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045);
                return;
            }

            from.SendGump(new ElementalismCofferGump(this));
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped is SpellScroll scroll) 
            {
                int id = scroll.SpellID;

                // Adjust the starting ID (300 is a placeholder)
                if (id >= 300 && id <= 331) 
                {
                    int index = id - 300;

                    if (m_Slots[index])
                    {
                        from.SendMessage("That spell is already preserved.");
                        return false;
                    }

                    m_Slots[index] = true;
                    from.SendMessage("The coffer absorbs the elemental scroll.");
                    dropped.Delete();
                    return true;
                }
            }

            from.SendMessage("The coffer only accepts Elementalism scrolls.");
            return false;
        }

        public void BeginBinding(Mobile from)
        {
            if (GetTotalScrolls() < 32)
            {
                from.SendMessage("The coffer needs all 32 elemental scrolls.");
                return;
            }

            if (from.Backpack.GetAmount(typeof(Gold)) < BindingCost && from.BankBox.GetAmount(typeof(Gold)) < BindingCost)
            {
                from.SendMessage("You need {0} gold for the binding fee.", BindingCost);
                return;
            }

            from.SendMessage("Target the empty elemental spellbook.");
            from.Target = new InternalBindTarget(this);
        }

        private class InternalBindTarget : Target
        {
            private ElementalismCoffer m_Coffer;
            public InternalBindTarget(ElementalismCoffer coffer) : base(1, false, TargetFlags.None) { m_Coffer = coffer; }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Spellbook book) 
                {
                    if (from.Backpack.ConsumeTotal(typeof(Gold), m_Coffer.BindingCost) || from.BankBox.ConsumeTotal(typeof(Gold), m_Coffer.BindingCost))
                    {
                        book.Content = 0xFFFFFFFF; 
                        from.SendMessage("The coffer fills your book with elemental power.");
                        from.PlaySound(0x5C3);
                    }
                }
            }
        }

        public int GetTotalScrolls()
        {
            int count = 0;
            for (int i = 0; i < m_Slots.Length; i++)
                if (m_Slots[i]) count++;
            return count;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
            for (int i = 0; i < 32; i++) writer.Write(m_Slots[i]);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_Slots = new bool[32];
            for (int i = 0; i < 32; i++) m_Slots[i] = reader.ReadBool();
        }

        public ElementalismCoffer(Serial serial) : base(serial) { }
    }
}
