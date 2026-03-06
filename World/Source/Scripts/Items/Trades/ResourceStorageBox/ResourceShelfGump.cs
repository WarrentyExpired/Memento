using System;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class MasterResourceHubGump : Gump
    {
        private AllInOneResourceShelf m_Shelf;

        public MasterResourceHubGump(Mobile from, AllInOneResourceShelf shelf) : base(100, 100)
        {
            m_Shelf = shelf;

            AddPage(0);
            // Height set to 350 since we have fewer buttons now
            AddBackground(0, 0, 300, 350, 9270);
            AddAlphaRegion(10, 10, 280, 330);
            AddLabel(75, 20, 1152, "RESOURCE REPOSITORY");

            int y = 60;
            string[] names = { "Blacksmith", "Woodworking", "Tanning", "Sewing", "Reagents" };
            
            for (int i = 0; i < names.Length; i++)
            {
                AddButton(40, y, 4005, 4007, i + 1, GumpButtonType.Reply, 0);
                AddLabel(75, y, 0x481, names[i]);
                y += 40;
            }
            AddButton(40, 260, 4005, 4007, 6, GumpButtonType.Reply, 0);
            AddLabel(75, 260, 0x481, "Tools");
            // Deposit button positioned at the bottom
            AddButton(40, 300, 4011, 4013, 10, GumpButtonType.Reply, 0);
            AddLabel(75, 300, 1152, "DEPOSIT ALL FROM PACK");
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            if (m_Shelf == null || m_Shelf.Deleted)
                return;

            switch (info.ButtonID)
            {
                case 1: from.SendGump(new ResourceStorageGump(from, m_Shelf, m_Shelf.Metal, "Shelf: Metal Storage")); break;
                case 2: from.SendGump(new ResourceStorageGump(from, m_Shelf, m_Shelf.Wood, "Shelf: Wood Storage")); break;
                case 3: from.SendGump(new ResourceStorageGump(from, m_Shelf, m_Shelf.Leather, "Shelf: Leather Storage")); break;
                case 4: from.SendGump(new ResourceStorageGump(from, m_Shelf, m_Shelf.Cloth, "Shelf: Cloth Storage")); break;
                case 5: from.SendGump(new ResourceStorageGump(from, m_Shelf, m_Shelf.Arcane, "Shelf: Arcane Storage")); break;
                case 6: from.SendGump(new ResourceStorageGump(from, m_Shelf, m_Shelf.Tools, "Shelf: Tool Storage")); break;
                
                case 10: // Deposit All
                    m_Shelf.DepositAll(from); 
                    from.SendGump(new MasterResourceHubGump(from, m_Shelf)); 
                    break;
            }
        }
    }
}
