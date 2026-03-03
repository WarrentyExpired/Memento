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

        public virtual int BindingCost => 12000; 

        [Constructable]
        public NecromancyCoffer() : base(0x1C0E)
        {
            Name = "Necromancy Spellbound Coffer";
            Weight = 10.0;
            Hue = 1157; 
            m_Slots = new bool[17]; // Expanded to 17
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
            if (dropped is SpellScroll scroll) 
            {
                int id = scroll.SpellID;

                // Necromancy range: 100 to 116 (Exorcism)
                if (id >= 100 && id <= 116) 
                {
                    int index = id - 100;

                    if (m_Slots[index])
                    {
                        from.SendMessage("That dark ritual is already preserved.");
                        return false;
                    }

                    m_Slots[index] = true;
                    from.SendMessage("The coffer chills the air as it absorbs the {0}.", scroll.Name ?? "necromancy scroll");
                    dropped.Delete();
                    from.PlaySound(0x1FB); 
                    return true;
                }
            }

            from.SendMessage("The coffer only accepts Necromancy scrolls (IDs 100-116).");
            return false;
        }

        public void BeginBinding(Mobile from)
        {
            if (GetTotalScrolls() < 17)
            {
                from.SendMessage("The coffer requires all 17 dark rituals to function.");
                return;
            }

            if (from.Backpack.GetAmount(typeof(Gold)) < BindingCost && from.BankBox.GetAmount(typeof(Gold)) < BindingCost)
            {
                from.SendMessage("You need {0} gold for the necrotic reagents.", BindingCost);
                return;
            }

            from.SendMessage("Target the empty necromancer spellbook you wish to fill.");
            from.Target = new InternalBindTarget(this);
        }

        private class InternalBindTarget : Target
        {
            private NecromancyCoffer m_Coffer;
            public InternalBindTarget(NecromancyCoffer coffer) : base(1, false, TargetFlags.None) { m_Coffer = coffer; }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is NecromancerSpellbook book) 
                {
                    if (from.Backpack.ConsumeTotal(typeof(Gold), m_Coffer.BindingCost) || from.BankBox.ConsumeTotal(typeof(Gold), m_Coffer.BindingCost))
                    {
                        // 0x1FFFF fills 17 bits (11111111111111111 in binary)
                        book.Content = 0x1FFFF; 
                        from.SendMessage("The coffer imprints the dark arts into your book.");
                        from.PlaySound(0x482);
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
            writer.Write((int)0);
            for (int i = 0; i < 17; i++) 
                writer.Write(m_Slots[i]);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_Slots = new bool[17];
            for (int i = 0; i < 17; i++) 
                m_Slots[i] = reader.ReadBool();
        }

        public NecromancyCoffer(Serial serial) : base(serial) { }
    }
}
