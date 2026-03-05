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

        public ResourceStorageGump(Mobile from, BaseResourceBox box) : base(50, 50)
        {
            m_Box = box;

            AddPage(0);
            AddBackground(0, 0, 480, 450, 9270);
            AddAlphaRegion(10, 10, 460, 430);
            AddLabel(160, 20, 1152, m_Box.BoxTitle);

            AddLabel(30, 50, 1152, "Resource");
            AddLabel(210, 50, 1152, "Amount");
            AddLabel(290, 50, 1152, "Withdraw (1 / 50 / 500 / ...)");

            // Alphabetical Sort
            var sortedList = m_Box.Resources.OrderBy(x => x.Key.Name).ToList();

            int y = 80;
            int buttonID = 1;

            foreach (var entry in sortedList)
            {
                string name = entry.Key.Name;
                name = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
                name = name.Replace("Base ", "");

                AddLabel(30, y, 0x481, name);
                AddLabel(210, y, 0x481, entry.Value.ToString());

                AddButton(290, y + 2, 2117, 2118, buttonID, GumpButtonType.Reply, 0); 
                AddButton(325, y + 2, 2117, 2118, buttonID + 1, GumpButtonType.Reply, 0); 
                AddButton(365, y + 2, 2117, 2118, buttonID + 2, GumpButtonType.Reply, 0); 
                
                AddButton(410, y + 2, 4005, 4007, buttonID + 3, GumpButtonType.Reply, 0);
                AddLabel(440, y, 1152, "...");

                y += 25;
                buttonID += 4;

                if (y > 380) break;
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 0 || m_Box == null || m_Box.Deleted) return;

            int val = info.ButtonID - 1;
            int index = val / 4;
            int subType = val % 4; 

            var sortedList = m_Box.Resources.OrderBy(x => x.Key.Name).ToList();

            if (index >= 0 && index < sortedList.Count)
            {
                Type targetType = sortedList[index].Key;

                if (subType < 3) 
                {
                    int amount = (subType == 0) ? 1 : (subType == 1) ? 50 : 500;
                    m_Box.Withdraw(sender.Mobile, targetType, amount);
                    sender.Mobile.SendGump(new ResourceStorageGump(sender.Mobile, m_Box));
                }
                else 
                {
                    sender.Mobile.SendMessage("Enter amount to withdraw:");
                    // This is line 82 - it will now find the nested class below
                    sender.Mobile.Prompt = new ResourceWithdrawPrompt(m_Box, targetType);
                }
            }
        }

        // NESTED CLASS: This must be inside the ResourceStorageGump curly brackets
        private class ResourceWithdrawPrompt : Prompt
        {
            private BaseResourceBox m_Box;
            private Type m_Type;

            public ResourceWithdrawPrompt(BaseResourceBox box, Type type)
            {
                m_Box = box;
                m_Type = type;
            }

            public override void OnResponse(Mobile from, string text)
            {
                int amount = Utility.ToInt32(text);
                if (amount > 0) 
                    m_Box.Withdraw(from, m_Type, amount);
                else 
                    from.SendMessage("Invalid withdrawal amount.");

                from.SendGump(new ResourceStorageGump(from, m_Box));
            }

            public override void OnCancel(Mobile from)
            {
                from.SendGump(new ResourceStorageGump(from, m_Box));
            }
        }
    }
}
