using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Targeting;

namespace Server.Items
{
    public class MasterCrafterLedger : Item
    {
        // Store references to the boxes by their Serial
        private MetalStorageBox m_MetalBox;
        private WoodStorageBox m_WoodBox;
        private LeatherStorageBox m_LeatherBox;
        private ClothStorageBox m_ClothBox;
        private ToolStorageBox m_ToolBox;
        private ArcaneStorageBox m_ArcaneBox;
        // Accessors
        public MetalStorageBox MetalBox { get => m_MetalBox; set => m_MetalBox = value; }
        public WoodStorageBox WoodBox { get => m_WoodBox; set => m_WoodBox = value; }
        public LeatherStorageBox LeatherBox { get => m_LeatherBox; set => m_LeatherBox = value; }
        public ClothStorageBox ClothBox { get => m_ClothBox; set => m_ClothBox = value; }
        public ToolStorageBox ToolBox { get => m_ToolBox; set => m_ToolBox = value; }
        public ArcaneStorageBox ArcaneBox { get => m_ArcaneBox; set => m_ArcaneBox = value; }
        [Constructable]
        public MasterCrafterLedger() : base(0x2259) // Book graphic
        {
            Name = "Master Crafter's Ledger";
            Hue = 1153; // White/Gold hue
            LootType = LootType.Blessed;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2)) return;

            // If the user holds Shift or uses a context menu (or we just provide a link button)
            // For now, let's open the main Hub
            from.SendGump(new MasterCrafterGump(from, this));
        }

        public void BeginLink(Mobile from)
        {
            from.SendMessage("Target the storage box you wish to link to this ledger.");
            from.Target = new InternalLinkTarget(this);
        }

        private class InternalLinkTarget : Target
        {
            private MasterCrafterLedger m_Ledger;

            public InternalLinkTarget(MasterCrafterLedger ledger) : base(2, false, TargetFlags.None)
            {
                m_Ledger = ledger;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is MetalStorageBox metal) { m_Ledger.MetalBox = metal; from.SendMessage("Metal Ledger linked."); }
                else if (targeted is WoodStorageBox wood) { m_Ledger.WoodBox = wood; from.SendMessage("Wood Ledger linked."); }
                else if (targeted is LeatherStorageBox leather) { m_Ledger.LeatherBox = leather; from.SendMessage("Leather Ledger linked."); }
                else if (targeted is ClothStorageBox cloth) { m_Ledger.ClothBox = cloth; from.SendMessage("Cloth Ledger linked."); }
                else if (targeted is ToolStorageBox tool) { m_Ledger.ToolBox = tool; from.SendMessage("Tool Ledger linked."); }
                else if (targeted is ArcaneStorageBox arcane) { m_Ledger.ArcaneBox = arcane; from.SendMessage("Arcane Ledger linked."); }
                else { from.SendMessage("That is not a compatible storage box."); return; }

                from.SendGump(new MasterCrafterGump(from, m_Ledger));
            }
        }

        public MasterCrafterLedger(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
            writer.Write(m_MetalBox);
            writer.Write(m_WoodBox);
            writer.Write(m_LeatherBox);
            writer.Write(m_ClothBox);
            writer.Write(m_ToolBox);
            writer.Write(m_ArcaneBox);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_MetalBox = reader.ReadItem() as MetalStorageBox;
            m_WoodBox = reader.ReadItem() as WoodStorageBox;
            m_LeatherBox = reader.ReadItem() as LeatherStorageBox;
            m_ClothBox = reader.ReadItem() as ClothStorageBox;
            m_ToolBox = reader.ReadItem() as ToolStorageBox;
            m_ArcaneBox = reader.ReadItem() as ArcaneStorageBox;
        }
    }
}
