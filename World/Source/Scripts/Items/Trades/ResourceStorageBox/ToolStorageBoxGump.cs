using System;
using System.Collections.Generic;
using System.Linq;
using Server.Gumps;
using Server.Network;
using Server.Items;
using Server.Prompts;

namespace Server.Items
{
    public class ToolStorageGump : Gump
    {
        private ToolStorageBox m_Box;
        private int m_Page;
        private List<KeyValuePair<Type, int>> m_SortedList;

        // Added 'page' parameter to constructor
        public ToolStorageGump(Mobile from, ToolStorageBox box, int page = 0) : base(50, 50)
        {
            m_Box = box;
            m_Page = page;

            AddPage(0);
            AddBackground(0, 0, 540, 500, 9270); // Height increased to 500 for page buttons
            AddAlphaRegion(10, 10, 520, 480);
            AddLabel(210, 20, 1152, "Tool Storage Box");

            AddLabel(30, 50, 1152, "Tool Type");
            AddLabel(210, 50, 1152, "Total Uses");
            AddLabel(300, 50, 1152, "(100 / 500 / All / Amount)");

            // Sort tools alphabetically
            m_SortedList = m_Box.ToolUses.OrderBy(x => x.Key.Name).ToList();

            int itemsPerPage = 12;
            int start = m_Page * itemsPerPage;
            int end = Math.Min(start + itemsPerPage, m_SortedList.Count);

            int y = 80;
            for (int i = start; i < end; i++)
            {
                var entry = m_SortedList[i];
                string name = entry.Key.Name;
                name = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");

                AddLabel(30, y, 0x481, name);
                AddLabel(210, y, 0x481, entry.Value.ToString());

                // Unique Button ID based on row index
                int buttonID = (i * 4) + 10; 

                AddButton(305, y + 2, 2117, 2118, buttonID, GumpButtonType.Reply, 0);     // 100
                AddButton(345, y + 2, 2117, 2118, buttonID + 1, GumpButtonType.Reply, 0); // 500
                AddButton(385, y + 2, 2117, 2118, buttonID + 2, GumpButtonType.Reply, 0); // All
                AddButton(435, y + 2, 4005, 4007, buttonID + 3, GumpButtonType.Reply, 0); // ...
                AddLabel(465, y, 1152, "...");

                y += 30;
            }
            // Pagination Controls
            AddButton(150, 460, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddLabel(185, 460, 1152, "Fill from Backpack");
            if (m_Page > 0)
            {
                AddButton(30, 460, 4014, 4016, 1, GumpButtonType.Reply, 0);
                AddLabel(65, 460, 1152, "Previous");
            }

            if (end < m_SortedList.Count)
            {
                AddButton(450, 460, 4005, 4007, 2, GumpButtonType.Reply, 0);
                AddLabel(410, 460, 1152, "Next");
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0 || m_Box == null || m_Box.Deleted) return;

            // Handle Page Changes
            if (info.ButtonID == 1)
            {
                sender.Mobile.SendGump(new ToolStorageGump(sender.Mobile, m_Box, m_Page - 1));
                return;
            }
            if (info.ButtonID == 2)
            {
                sender.Mobile.SendGump(new ToolStorageGump(sender.Mobile, m_Box, m_Page + 1));
                return;
            }
            // Handle Fill from Backpack
            if (info.ButtonID == 3)
            {
                m_Box.FillFromBackpack(sender.Mobile);
                sender.Mobile.SendGump(new ToolStorageGump(sender.Mobile, m_Box, m_Page));
                return;
            }
            int val = info.ButtonID - 10;
            int itemIndex = val / 4;
            int subType = val % 4;

            var sorted = m_Box.ToolUses.OrderBy(x => x.Key.Name).ToList();

            if (itemIndex >= 0 && itemIndex < sorted.Count)
            {
                Type targetType = sorted[itemIndex].Key;
                int totalAvailable = sorted[itemIndex].Value;

                if (subType < 3)
                {
                    int amount = (subType == 0) ? 100 : (subType == 1) ? 500 : totalAvailable;
                    
                    m_Box.Withdraw(sender.Mobile, targetType, amount);
                    sender.Mobile.SendGump(new ToolStorageGump(sender.Mobile, m_Box, m_Page));
                }
                else
                {
                    sender.Mobile.SendMessage("Enter uses for the tool:");
                    sender.Mobile.Prompt = new ToolWithdrawPrompt(m_Box, targetType, m_Page);
                }
            }
        }

        private class ToolWithdrawPrompt : Prompt
        {
            private ToolStorageBox m_Box;
            private Type m_Type;
            private int m_Page;

            public ToolWithdrawPrompt(ToolStorageBox box, Type type, int page)
            {
                m_Box = box;
                m_Type = type;
                m_Page = page;
            }

            public override void OnResponse(Mobile from, string text)
            {
                int amount = Utility.ToInt32(text);
                if (amount > 0) m_Box.Withdraw(from, m_Type, amount);
                from.SendGump(new ToolStorageGump(from, m_Box, m_Page));
            }

            public override void OnCancel(Mobile from)
            {
                from.SendGump(new ToolStorageGump(from, m_Box, m_Page));
            }
        }
    }
}
