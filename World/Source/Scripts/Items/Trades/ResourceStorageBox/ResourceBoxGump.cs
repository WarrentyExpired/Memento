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
        private IStorageBox m_Box;
        private int m_Page;
        private List<KeyValuePair<Type, int>> m_SortedList;

        public ResourceStorageGump(Mobile from, IStorageBox box, int page = 0) : base(50, 50)
        {
            m_Box = box;
            m_Page = page;

            AddPage(0);
            AddBackground(0, 0, 540, 500, 9270); 
            AddAlphaRegion(10, 10, 520, 480);
            AddLabel(210, 20, 1152, m_Box.BoxTitle);

            AddLabel(30, 50, 1152, m_Box is ToolStorageBox ? "Tool Type" : "Resource");
            AddLabel(210, 50, 1152, m_Box is ToolStorageBox ? "Total Uses" : "Amount");
            AddLabel(300, 50, 1152, "Withdraw (100 / 500 / All / ...)");

            // --- SMART SORTING ---
            // Sort Alphabetically if it's a Tool Box or Arcane Box
            if (m_Box is ToolStorageBox || m_Box is ArcaneStorageBox)
                m_SortedList = m_Box.GetStorage().OrderBy(x => x.Key.Name).ToList();
            else
                m_SortedList = m_Box.GetStorage().OrderBy(x => BaseResourceBox.GetRarityValue(x.Key)).ToList();

            int itemsPerPage = 12;
            int start = m_Page * itemsPerPage;
            int end = Math.Min(start + itemsPerPage, m_SortedList.Count);

            int y = 80;
            for (int i = start; i < end; i++)
            {
                var entry = m_SortedList[i];
                string name = entry.Key.Name;
                name = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
                name = name.Replace("Base ", "").Replace("Fabric", "Cloth");

                AddLabel(30, y, 0x481, name);
                AddLabel(210, y, 0x481, entry.Value.ToString());

                int buttonID = (i * 4) + 10; 
                AddButton(305, y + 2, 2117, 2118, buttonID, GumpButtonType.Reply, 0);
                AddButton(340, y + 2, 2117, 2118, buttonID + 1, GumpButtonType.Reply, 0);
                AddButton(380, y + 2, 2117, 2118, buttonID + 2, GumpButtonType.Reply, 0);
                AddButton(430, y + 2, 4005, 4007, buttonID + 3, GumpButtonType.Reply, 0);
                AddLabel(460, y, 1152, "...");
                y += 30;
            }

            if (m_Page > 0) AddButton(30, 460, 4014, 4016, 1, GumpButtonType.Reply, 0);
            if (end < m_SortedList.Count) AddButton(450, 460, 4005, 4007, 2, GumpButtonType.Reply, 0);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0 || m_Box == null) return;

            if (info.ButtonID == 1) sender.Mobile.SendGump(new ResourceStorageGump(sender.Mobile, m_Box, m_Page - 1));
            else if (info.ButtonID == 2) sender.Mobile.SendGump(new ResourceStorageGump(sender.Mobile, m_Box, m_Page + 1));
            else if (info.ButtonID >= 10)
            {
                int val = info.ButtonID - 10;
                int itemIndex = val / 4;
                int subType = val % 4;

                if (itemIndex >= 0 && itemIndex < m_SortedList.Count)
                {
                    Type targetType = m_SortedList[itemIndex].Key;
                    if (subType < 3)
                    {
                        int amount = (subType == 0) ? 100 : (subType == 1) ? 500 : m_SortedList[itemIndex].Value;
                        m_Box.Withdraw(sender.Mobile, targetType, amount);
                        sender.Mobile.SendGump(new ResourceStorageGump(sender.Mobile, m_Box, m_Page));
                    }
                    else
                    {
                        sender.Mobile.SendMessage("Enter amount:");
                        sender.Mobile.Prompt = new StorageWithdrawPrompt(m_Box, targetType, m_Page);
                    }
                }
            }
        }

        private class StorageWithdrawPrompt : Prompt
        {
            private IStorageBox m_Box; private Type m_Type; private int m_Page;
            public StorageWithdrawPrompt(IStorageBox box, Type type, int page) { m_Box = box; m_Type = type; m_Page = page; }
            public override void OnResponse(Mobile from, string text)
            {
                int amount = Utility.ToInt32(text);
                if (amount > 0) m_Box.Withdraw(from, m_Type, amount);
                from.SendGump(new ResourceStorageGump(from, m_Box, m_Page));
            }
        }
    }
}
