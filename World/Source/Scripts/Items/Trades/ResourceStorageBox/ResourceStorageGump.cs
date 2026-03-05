using System;
using System.Collections.Generic;
using System.Linq;
using Server.Gumps;
using Server.Network;
using Server.Items;
using Server.Prompts;

namespace Server.Items
{
    public class ResourceStorageGump : Gump
    {
        private BaseResourceBox m_Box;
        private int m_Page;
        private List<KeyValuePair<Type, int>> m_SortedList;

        public ResourceStorageGump(Mobile from, BaseResourceBox box, int page = 0) : base(50, 50)
        {
            m_Box = box;
            m_Page = page;

            AddPage(0);
            AddBackground(0, 0, 540, 500, 9270); 
            AddAlphaRegion(10, 10, 520, 480);
            AddLabel(210, 20, 1152, m_Box.BoxTitle);

            AddLabel(30, 50, 1152, "Resource");
            AddLabel(210, 50, 1152, "Amount");
            // Updated Header Label
            AddLabel(300, 50, 1152, "(100 / 500 / All / Amount)");

            m_SortedList = m_Box.Resources.OrderBy(x => GetRarityIndex(x.Key)).ToList();

            int itemsPerPage = 12;
            int start = m_Page * itemsPerPage;
            int end = Math.Min(start + itemsPerPage, m_SortedList.Count);

            int y = 80;
            for (int i = start; i < end; i++)
            {
                var entry = m_SortedList[i];
                string name = entry.Key.Name;
                
                name = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
                name = name.Replace("Base ", "");
                name = name.Replace("Fabric", "Cloth");
                name = name.Replace("Spined Leather", "Deep Sea Leather");
                name = name.Replace("Horned Leather", "Lizard Leather");
                name = name.Replace("Barbed Leather", "Serpent Leather");

                AddLabel(30, y, 0x481, name);
                AddLabel(210, y, 0x481, entry.Value.ToString());

                int buttonID = (i * 4) + 10; 

                AddButton(305, y + 2, 2117, 2118, buttonID, GumpButtonType.Reply, 0); // 100
                AddButton(345, y + 2, 2117, 2118, buttonID + 1, GumpButtonType.Reply, 0); // 500
                AddButton(385, y + 2, 2117, 2118, buttonID + 2, GumpButtonType.Reply, 0); // All
                
                AddButton(435, y + 2, 4005, 4007, buttonID + 3, GumpButtonType.Reply, 0); // Custom
                AddLabel(465, y, 1152, "...");

                y += 30;
            }

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

        private int GetRarityIndex(Type type)
        {
            return BaseResourceBox.GetRarityValue(type);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0 || m_Box == null || m_Box.Deleted) return;

            if (info.ButtonID == 1)
            {
                sender.Mobile.SendGump(new ResourceStorageGump(sender.Mobile, m_Box, m_Page - 1));
                return;
            }
            if (info.ButtonID == 2)
            {
                sender.Mobile.SendGump(new ResourceStorageGump(sender.Mobile, m_Box, m_Page + 1));
                return;
            }

            int val = info.ButtonID - 10;
            int itemIndex = val / 4;
            int subType = val % 4;

            var sorted = m_Box.Resources.OrderBy(x => GetRarityIndex(x.Key)).ToList();

            if (itemIndex >= 0 && itemIndex < sorted.Count)
            {
                Type targetType = sorted[itemIndex].Key;
                int available = sorted[itemIndex].Value;

                if (subType < 3) // Fixed Amounts (100, 500, All)
                {
                    int amount = 0;
                    if (subType == 0) amount = 100;
                    else if (subType == 1) amount = 500;
                    else amount = available; // "All" logic

                    m_Box.Withdraw(sender.Mobile, targetType, amount);
                    sender.Mobile.SendGump(new ResourceStorageGump(sender.Mobile, m_Box, m_Page));
                }
                else // Custom
                {
                    sender.Mobile.SendMessage("Enter amount to withdraw:");
                    sender.Mobile.Prompt = new ResourceWithdrawPrompt(m_Box, targetType, m_Page);
                }
            }
        }

        private class ResourceWithdrawPrompt : Prompt
        {
            private BaseResourceBox m_Box;
            private Type m_Type;
            private int m_Page;

            public ResourceWithdrawPrompt(BaseResourceBox box, Type type, int page)
            {
                m_Box = box;
                m_Type = type;
                m_Page = page;
            }

            public override void OnResponse(Mobile from, string text)
            {
                int amount = Utility.ToInt32(text);
                if (amount > 0) m_Box.Withdraw(from, m_Type, amount);
                from.SendGump(new ResourceStorageGump(from, m_Box, m_Page));
            }

            public override void OnCancel(Mobile from)
            {
                from.SendGump(new ResourceStorageGump(from, m_Box, m_Page));
            }
        }
    }
}
