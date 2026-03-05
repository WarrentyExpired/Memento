using System;
using Server.Gumps;
using Server.Network;
using Server.Items;

namespace Server.Items
{
    public class MasterCrafterGump : Gump
    {
        private MasterCrafterLedger m_Ledger;

        public MasterCrafterGump(Mobile from, MasterCrafterLedger ledger) : base(100, 100)
        {
            m_Ledger = ledger;

            AddPage(0);
            AddBackground(0, 0, 300, 400, 9270);
            AddAlphaRegion(10, 10, 280, 380);
            AddLabel(75, 20, 1152, "MASTER CRAFTER HUB");

            int y = 60;

            // List the categories
            DrawEntry(from, "Blacksmithing", m_Ledger.MetalBox, 1, y); y += 40;
            DrawEntry(from, "Carpentry", m_Ledger.WoodBox, 2, y); y += 40;
            DrawEntry(from, "Tailoring", m_Ledger.ClothBox, 3, y); y += 40;
            DrawEntry(from, "Tanning", m_Ledger.LeatherBox, 4, y); y += 40;
            DrawEntry(from, "Tools", m_Ledger.ToolBox, 5, y); y += 40;

            AddButton(40, 300, 4005, 4007, 10, GumpButtonType.Reply, 0);
            AddLabel(75, 300, 1152, "Link a Storage Box");

            AddButton(40, 340, 4011, 4013, 20, GumpButtonType.Reply, 0);
            AddLabel(75, 340, 1152, "DEPOSIT ALL ITEMS");
        }

        private void DrawEntry(Mobile from, string name, Item box, int buttonID, int y)
        {
            AddButton(20, y, 4005, 4007, buttonID, GumpButtonType.Reply, 0);
            if (box != null && !box.Deleted)
                AddLabel(55, y, 0x481, name);
            else
                AddLabel(55, y, 0x384, name + " (Not Linked)");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch (info.ButtonID)
            {
                case 1: if (m_Ledger.MetalBox != null) from.SendGump(new ResourceStorageGump(from, m_Ledger.MetalBox)); break;
                case 2: if (m_Ledger.WoodBox != null) from.SendGump(new ResourceStorageGump(from, m_Ledger.WoodBox)); break;
                case 3: if (m_Ledger.ClothBox != null) from.SendGump(new ResourceStorageGump(from, m_Ledger.ClothBox)); break;
                case 4: if (m_Ledger.LeatherBox != null) from.SendGump(new ResourceStorageGump(from, m_Ledger.LeatherBox)); break;
                case 5: if (m_Ledger.ToolBox != null) from.SendGump(new ToolStorageGump(from, m_Ledger.ToolBox)); break;
                
                case 10: m_Ledger.BeginLink(from); break;

                case 20: // DEPOSIT ALL
                    if (m_Ledger.MetalBox != null) m_Ledger.MetalBox.FillFromBackpack(from);
                    if (m_Ledger.WoodBox != null) m_Ledger.WoodBox.FillFromBackpack(from);
                    if (m_Ledger.ClothBox != null) m_Ledger.ClothBox.FillFromBackpack(from);
                    if (m_Ledger.LeatherBox != null) m_Ledger.LeatherBox.FillFromBackpack(from);
                    if (m_Ledger.ToolBox != null) m_Ledger.ToolBox.FillFromBackpack(from);
                    from.SendGump(new MasterCrafterGump(from, m_Ledger));
                    break;
            }
        }
    }
}
