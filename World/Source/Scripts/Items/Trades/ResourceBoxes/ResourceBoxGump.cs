using System;
using System.Collections.Generic;
using System.Linq;
using Server.Gumps;
using Server.Network;
using Server.Items;
using Server.Prompts;

namespace Server.Items
{
    public class ResourceBoxGump : Gump
    {
        private BaseResourceBox m_Box;
        private string m_Category;
        private int m_Page;
        private List<KeyValuePair<Type, int>> m_FilteredList;

        // Category List
        private static string[] Categories = { "Metal", "Leather", "Wood", "Cloth", "Bones", "Scales" };

        public ResourceBoxGump(Mobile from, BaseResourceBox box, string category = "Metal", int page = 0) : base(50, 50)
        {
            m_Box = box;
            m_Category = category;
            m_Page = page;

            AddPage(0);
            AddBackground(0, 0, 580, 520, 9270); // Slightly widened background
            AddAlphaRegion(10, 10, 560, 500);
            AddLabel(230, 15, 1152, box.BoxTitle);

            // Render Category Navigation with increased spacing
            for (int i = 0; i < Categories.Length; i++)
            {
                int x = 15 + (i * 85); // Increased from 72 to 80 to prevent overlap
                int hue = (m_Category == Categories[i]) ? 0x35 : 1152;
                
                AddButton(x, 45, 4005, 4007, 10 + i, GumpButtonType.Reply, 0);
                AddLabel(x + 35, 45, hue, Categories[i]);
            }

            // Filter and Sort Data
            m_FilteredList = m_Box.Resources
                .Where(x => IsInCategory(x.Key, m_Category))
                .OrderBy(x => BaseResourceBox.GetRarityValue(x.Key))
                .ToList();

            // Header Labels with better alignment
            AddLabel(30, 80, 1152, "Resource Type");
            AddLabel(240, 80, 1152, "Amount");
            AddLabel(345, 80, 1152, "100");
            AddLabel(385, 80, 1152, "500");
            AddLabel(430, 80, 1152, "All");
            AddLabel(475, 80, 1152, "Amount");

            int itemsPerPage = 10;
            int start = m_Page * itemsPerPage;
            int end = Math.Min(start + itemsPerPage, m_FilteredList.Count);

            int y = 110;
            for (int i = start; i < end; i++)
            {
                var entry = m_FilteredList[i];
                string friendlyName = GetFriendlyName(entry.Key);

                AddLabel(30, y, 0x481, friendlyName);
                AddLabel(240, y, 0x481, entry.Value.ToString());

                int buttonID = 100 + (i * 4);
                // Shifted buttons to match headers
                AddButton(345, y + 2, 2117, 2118, buttonID, GumpButtonType.Reply, 0);     
                AddButton(385, y + 2, 2117, 2118, buttonID + 1, GumpButtonType.Reply, 0); 
                AddButton(430, y + 2, 2117, 2118, buttonID + 2, GumpButtonType.Reply, 0); 
                AddButton(475, y + 2, 4005, 4007, buttonID + 3, GumpButtonType.Reply, 0); 
                y += 35;
            }

            // Pagination and Utility
            if (m_Page > 0) 
                AddButton(30, 470, 4014, 4016, 1, GumpButtonType.Reply, 0);
            
            if (end < m_FilteredList.Count) 
                AddButton(520, 470, 4005, 4007, 2, GumpButtonType.Reply, 0);
            
            AddButton(210, 470, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddLabel(245, 470, 1152, "Fill from Backpack");
        }

        private bool IsInCategory(Type t, string cat)
        {
            switch (cat)
            {
                case "Metal": return typeof(BaseIngot).IsAssignableFrom(t);
                case "Leather": return typeof(BaseLeather).IsAssignableFrom(t);
                case "Wood": return typeof(BaseWoodBoard).IsAssignableFrom(t) || t == typeof(Shaft);
                case "Cloth": return typeof(BaseFabric).IsAssignableFrom(t) || t == typeof(Feather);
                case "Bones": return typeof(BaseSkeletal).IsAssignableFrom(t);
                case "Scales": return typeof(BaseScales).IsAssignableFrom(t);
            }
            return true; 
        }
        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;
            int bid = info.ButtonID;
            if (bid == 0) return;
            if (bid == 1) from.SendGump(new ResourceBoxGump(from, m_Box, m_Category, m_Page - 1));
            else if (bid == 2) from.SendGump(new ResourceBoxGump(from, m_Box, m_Category, m_Page + 1));
            else if (bid == 3) { m_Box.FillFromBackpack(from); from.SendGump(new ResourceBoxGump(from, m_Box, m_Category, m_Page)); }
            
            else if (bid >= 10 && bid < 10 + Categories.Length)
            {
                from.SendGump(new ResourceBoxGump(from, m_Box, Categories[bid - 10], 0));
            }
            
            else if (bid >= 100)
            {
                int index = (bid - 100) / 4;
                int type = (bid - 100) % 4;

                if (index < m_FilteredList.Count)
                {
                    Type targetType = m_FilteredList[index].Key;
                    int available = m_FilteredList[index].Value;

                    if (type == 0) m_Box.Withdraw(from, targetType, 100);
                    else if (type == 1) m_Box.Withdraw(from, targetType, 500);
                    else if (type == 2) m_Box.Withdraw(from, targetType, available);
                    else
                    {
                        from.SendMessage("Enter amount to withdraw:");
                        from.Prompt = new StorageWithdrawPrompt(m_Box, targetType, m_Category, m_Page);
                        return;
                    }
                }
                from.SendGump(new ResourceBoxGump(from, m_Box, m_Category, m_Page));
            }
        }

        private class StorageWithdrawPrompt : Prompt
        {
            private BaseResourceBox m_Box;
            private Type m_Type;
            private string m_Cat;
            private int m_Page;

            public StorageWithdrawPrompt(BaseResourceBox box, Type type, string cat, int page)
            {
                m_Box = box; m_Type = type; m_Cat = cat; m_Page = page;
            }

            public override void OnResponse(Mobile from, string text)
            {
                int amount = Utility.ToInt32(text);
                if (amount > 0) m_Box.Withdraw(from, m_Type, amount);
                from.SendGump(new ResourceBoxGump(from, m_Box, m_Cat, m_Page));
            }
        }
        private string GetFriendlyName(Type t)
        {
            string name = t.Name;
            string friendlyName = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
            friendlyName = friendlyName.Replace("Fabric", "Cloth");
            switch (name)
            {
                // Leathers
                case "HornedLeather": return "Lizard Leather";
                case "BarbedLeather": return "Serpent Leather";
                case "SpinedLeather": return "Deep Sea Leather";
                // Scales
                case "RedScales": return "Crimson Scales";
                case "YellowScales": return "Golden Scales";
                case "BlackScales": return "Dark Scales";
                case "GreenScales": return "Viridian Scales";
                case "WhiteScales": return "Ivory Scales";
                case "BlueScales": return "Azure Scales";
            }
            return friendlyName;
        }
    }
}
