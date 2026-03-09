using System;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class BardicCofferGump : Gump
    {
        private BardicCoffer m_Coffer;
        private static string[] m_SongNames = new string[] 
        { 
            "Army's Paeon", "Enchanting Etude", "Energy Carol", "Energy Threnody",
            "Fire Carol", "Fire Threnody", "Foe Requiem", "Ice Carol",
            "Ice Threnody", "Knight's Minne", "Mage's Ballad", "Magic Finale",
            "Poison Carol", "Poison Threnody", "Shepherd's Dance", "Sinewy Etude"
        };
        public BardicCofferGump(BardicCoffer coffer) : base(150, 150)
        {
            m_Coffer = coffer;
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            AddPage(0);
            AddBackground(0, 0, 300, 600, 9270);
            AddAlphaRegion(10, 10, 280, 580);
            AddLabel(80, 15, 492, "Bardic Song Preservation");
            int x = 25;
            int y = 50;
            for (int i = 0; i < m_SongNames.Length; i++)
            {
                bool hasSpell = m_Coffer.Slots[i];
                AddImage(x, y, hasSpell ? 2103 : 2104);
                AddLabel(x + 25, y, hasSpell ? 67 : 907, string.Format("{0}. {1}", i + 1, m_SongNames[i]));
                y += 25;
            }
            string scrollStatus = string.Format("Scrolls: {0} / 17", m_Coffer.ScrollsStored);
            string sapphireStatus = string.Format("Sapphires: {0} / 8", m_Coffer.SapphiresStored);
            string arcanegemStatus = string.Format("Arcane Gems: {0} / 1", m_Coffer.ArcaneGemsStored);
            AddLabel(25, 465, m_Coffer.ScrollsStored >= 16 ? 67 : 907, scrollStatus);
            AddLabel(25, 490, m_Coffer.SapphiresStored >= 8 ? 67 : 907, sapphireStatus);
            AddLabel(25, 510, m_Coffer.ArcaneGemsStored >= 1 ? 67 : 907, arcanegemStatus);
            AddLabel(25, 530, 492, string.Format("Remaining Charges: {0}", m_Coffer.Charges));
            if (m_Coffer.Charges > 0)
            {
                AddLabel(25, 550, 492, "Fill Bardic Songbook (10,000gp)");
                AddButton(230, 545, 4005, 4006, 1, GumpButtonType.Reply, 0);
            }
            else
            {
              AddLabel(25, 550, 907, "Coffer needs recharging to fill books.");
            }
        }
        public override void OnResponse(NetState sender, RelayInfo info)
        {
          if (info.ButtonID == 1)
            m_Coffer.BeginBinding(sender.Mobile);
        }
    }
}
