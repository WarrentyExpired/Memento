using System;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class ElementalismCofferGump : Gump
    {
        private ElementalismCoffer m_Coffer;

        private static string[] m_SpellNames = new string[] 
        { 
          // Sphere 1
          "Armor", "Bolt", "Mend", "Sanctuary",
          // Sphere 2
          "Pain", "Protection", "Purge", "Stead",
          // Sphere 3
          "Call", "Force", "Wall", "Warp",
          // Sphere 4
          "Field", "Restoration", "Strike", "Void",
          // Sphere 5
          "Blast", "Echo", "Fiend", "Hold",
          // Sphere 6
          "Barrage", "Rune", "Storm", "Summon",
          // Sphere 7
          "Devastation", "Fall", "Gate", "Havoc",
          // Sphere 8
          "Apocalypse", "Lord", "Soul", "Spirit"
        };

        public ElementalismCofferGump(ElementalismCoffer coffer) : base(150, 150)
        {
            m_Coffer = coffer;

            AddPage(0);
            AddBackground(0, 0, 450, 550, 9270); // Slimmer width
            AddAlphaRegion(10, 10, 430, 530);
            AddLabel(130, 15, 1266, "Elementalism Preservation Coffer");

            int startX = 20;
            int startY = 50;
            int x = startX;
            int y = startY;

            for (int i = 0; i < 32; i++)
            {
                if (i > 0 && i % 16 == 0)
                {
                    x += 210; // Shift for the second column
                    y = startY;
                }

                bool hasSpell = m_Coffer.Slots[i];
                AddImage(x, y, hasSpell ? 2103 : 2104);
                
                int labelHue = hasSpell ? 67 : 907;
                string spellDisplay = String.Format("{0}. {1}", i + 1, m_SpellNames[i]);
                AddLabel(x + 25, y, labelHue, spellDisplay);

                y += 25;
            }

            /*string progress = String.Format("<BASEFONT COLOR=#FFFFFF>Collected: {0} / 32</BASEFONT>", m_Coffer.GetTotalScrolls());
            AddHtml(25, 510, 200, 20, progress, false, false);

            if (m_Coffer.GetTotalScrolls() >= 32)
            {
                AddButton(270, 505, 4005, 4007, 1, GumpButtonType.Reply, 0);
                AddLabel(305, 505, 67, String.Format("Fill Book ({0}gp)", m_Coffer.BindingCost));
            }*/
            AddLabel(25, 450, 1266, string.Format("Remaining Charges: {0}", m_Coffer.Charges));
            string scrollStatus = string.Format("Scrolls: {0} / 32", m_Coffer.ScrollsStored);
            string rubyStatus = string.Format("Rubies: {0} / 8", m_Coffer.RubysStored);
            AddLabel(25, 470, m_Coffer.ScrollsStored >= 32 ? 67 : 907, scrollStatus);
            AddLabel(25, 490, m_Coffer.RubysStored >= 8 ? 67 : 907, rubyStatus);
            if (m_Coffer.Charges > 0)
            {
                AddLabel(25, 510, 1266, "Fill Elementalism Book (10,000gp)");
                AddButton(250, 505, 4005, 4006, 1, GumpButtonType.Reply, 0);
            }
            else
            {
                AddLabel(25, 510, 907, "Coffer needs Rubies and Scrolls.");
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
                m_Coffer.BeginBinding(sender.Mobile);
        }
    }
}
