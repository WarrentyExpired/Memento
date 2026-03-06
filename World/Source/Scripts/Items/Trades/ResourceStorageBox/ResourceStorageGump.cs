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
        private object m_Owner; // Can be IStorageBox (Boxes) or AllInOneResourceShelf
        private Dictionary<Type, int> m_Data;
        private string m_Title;
        private int m_Page;
        private List<KeyValuePair<Type, int>> m_SortedList;

        // Universal Constructor for both Shelf and Boxes
        public ResourceStorageGump(Mobile from, object owner, Dictionary<Type, int> data, string title, int page = 0) : base(50, 50)
        {
            m_Owner = owner;
            m_Data = data;
            m_Title = title;
            m_Page = page;
            
            AddPage(0);
            AddBackground(0, 0, 540, 500, 9270); 
            AddAlphaRegion(10, 10, 520, 480);
            AddLabel(210, 20, 1152, m_Title);

            AddLabel(30, 50, 1152, owner is ToolStorageBox ? "Tool Type" : "Resource");
            AddLabel(210, 50, 1152, owner is ToolStorageBox ? "Total Uses" : "Amount");
            AddLabel(300, 50, 1152, "(100 / 500 / All / Amount)");

            // Smart Sorting: A-Z for Arcane and Tools, Rarity for others
            if (m_Title.Contains("Arcane") || owner is ToolStorageBox || owner is ArcaneStorageBox)
                m_SortedList = m_Data.OrderBy(x => x.Key.Name).ToList();
            else
                m_SortedList = m_Data.OrderBy(x => BaseResourceBox.GetRarityValue(x.Key)).ToList();

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
                name = name.Replace("Spined Leather", "Deep Sea Leather");
                name = name.Replace("Horned Leather", "Lizard Leather");
                name = name.Replace("Barbed Leather", "Serpent Leather");
                AddLabel(30, y, 0x481, name);
                AddLabel(210, y, 0x481, entry.Value.ToString());

                int buttonID = (i * 4) + 10; 
                AddButton(305, y + 2, 2117, 2118, buttonID, GumpButtonType.Reply, 0);     // 100
                AddButton(340, y + 2, 2117, 2118, buttonID + 1, GumpButtonType.Reply, 0); // 500
                AddButton(380, y + 2, 2117, 2118, buttonID + 2, GumpButtonType.Reply, 0); // All
                AddButton(430, y + 2, 4005, 4007, buttonID + 3, GumpButtonType.Reply, 0); // Custom
                AddLabel(460, y, 1152, "...");

                y += 30;
            }

            // Pagination and Deposit Buttons
            if (m_Page > 0)
                AddButton(30, 460, 4014, 4016, 1, GumpButtonType.Reply, 0);

            if (end < m_SortedList.Count)
                AddButton(450, 460, 4005, 4007, 2, GumpButtonType.Reply, 0);

            // Fill from Backpack Button
            AddButton(150, 460, 4005, 4007, 3, GumpButtonType.Reply, 0);
            AddLabel(185, 460, 1152, "Fill from Backpack");
        }

        // Helper constructor to keep physical box logic simple
        public ResourceStorageGump(Mobile from, IStorageBox box, int page = 0) 
            : this(from, box, box.GetStorage(), box.BoxTitle, page) { }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0 || m_Owner == null) return;

            // Pagination
            if (info.ButtonID == 1) sender.Mobile.SendGump(new ResourceStorageGump(sender.Mobile, m_Owner, m_Data, m_Title, m_Page - 1));
            else if (info.ButtonID == 2) sender.Mobile.SendGump(new ResourceStorageGump(sender.Mobile, m_Owner, m_Data, m_Title, m_Page + 1));
            
            // Fill from Backpack Logic
            else if (info.ButtonID == 3)
            {
                if (m_Owner is BaseResourceBox resBox) resBox.FillFromBackpack(sender.Mobile);
                else if (m_Owner is ToolStorageBox toolBox) toolBox.FillFromBackpack(sender.Mobile);
                else if (m_Owner is AllInOneResourceShelf shelf) shelf.DepositAll(sender.Mobile);

                sender.Mobile.SendGump(new ResourceStorageGump(sender.Mobile, m_Owner, m_Data, m_Title, m_Page));
            }
            
            // Withdrawal Logic
            else if (info.ButtonID >= 10)
            {
                int val = info.ButtonID - 10;
                int itemIndex = val / 4;
                int subType = val % 4;

                if (itemIndex >= 0 && itemIndex < m_SortedList.Count)
                {
                    Type targetType = m_SortedList[itemIndex].Key;
                    int available = m_SortedList[itemIndex].Value;

                    if (subType < 3) // 100, 500, or All
                    {
                        int amt = (subType == 0) ? 100 : (subType == 1) ? 500 : available;

                        if (m_Owner is IStorageBox box) box.Withdraw(sender.Mobile, targetType, amt);
                        else if (m_Owner is AllInOneResourceShelf shelf) shelf.Withdraw(sender.Mobile, m_Data, targetType, amt);

                        sender.Mobile.SendGump(new ResourceStorageGump(sender.Mobile, m_Owner, m_Data, m_Title, m_Page));
                    }
                    else // Custom Amount Prompt
                    {
                        sender.Mobile.SendMessage("Enter amount to withdraw:");
                        sender.Mobile.Prompt = new StorageWithdrawPrompt(m_Owner, m_Data, m_Title, targetType, m_Page);
                    }
                }
            }
        }

        private class StorageWithdrawPrompt : Prompt
        {
            private object m_Owner;
            private Dictionary<Type, int> m_Data;
            private string m_Title;
            private Type m_Type;
            private int m_Page;

            public StorageWithdrawPrompt(object owner, Dictionary<Type, int> data, string title, Type type, int page)
            {
                m_Owner = owner; m_Data = data; m_Title = title; m_Type = type; m_Page = page;
            }

            public override void OnResponse(Mobile from, string text)
            {
                int amount = Utility.ToInt32(text);
                if (amount > 0)
                {
                    if (m_Owner is IStorageBox box) box.Withdraw(from, m_Type, amount);
                    else if (m_Owner is AllInOneResourceShelf shelf) shelf.Withdraw(from, m_Data, m_Type, amount);
                }
                from.SendGump(new ResourceStorageGump(from, m_Owner, m_Data, m_Title, m_Page));
            }
        }
    }
}
