using System;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class NecromancyCofferGump : Gump
    {
        private NecromancyCoffer m_Coffer;

        private static string[] m_SpellNames = new string[] 
        { 
            "Animate Dead", "Blood Oath", "Corpse Skin", "Curse Weapon",
            "Evil Omen", "Horrific Beast", "Lich Form", "Mind Rot",
            "Pain Spike", "Poison Strike", "Strangle", "Summon Familiar",
            "Vampiric Embrace", "Vengeful Spirit", "Wither", "Wraith Form",
            "Exorcism" // The 17th spell
        };

        public NecromancyCofferGump(NecromancyCoffer coffer) : base(150, 150)
        {
            m_Coffer = coffer;

            AddPage(0);
            AddBackground(0, 0, 260, 580, 9270); // Height increased to 580
            AddAlphaRegion(10, 10, 240, 560);
            AddLabel(50, 15, 1157, "Necromancy Preservation");

            int x = 25;
            int y = 50;

            for (int i = 0; i < m_SpellNames.Length; i++)
            {
                bool hasSpell = (i < m_Coffer.Slots.Length) && m_Coffer.Slots[i];
                AddImage(x, y, hasSpell ? 2103 : 2104);
                
                int labelHue = hasSpell ? 67 : 907;
                string spellDisplay = String.Format("{0}. {1}", i + 1, m_SpellNames[i]);
                AddLabel(x + 25, y, labelHue, spellDisplay);

                y += 25;
            }

            string progress = String.Format("<BASEFONT COLOR=#FFFFFF>Rituals: {0} / 17</BASEFONT>", m_Coffer.GetTotalScrolls());
            AddHtml(25, 500, 200, 20, progress, false, false);

            if (m_Coffer.GetTotalScrolls() >= 17)
            {
                AddButton(25, 530, 4005, 4007, 1, GumpButtonType.Reply, 0);
                AddLabel(60, 530, 67, String.Format("Fill Book ({0}gp)", m_Coffer.BindingCost));
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
                m_Coffer.BeginBinding(sender.Mobile);
        }
    }
}
