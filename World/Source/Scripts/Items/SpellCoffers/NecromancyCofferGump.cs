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
            "Exorcism"
        };
        public NecromancyCofferGump(NecromancyCoffer coffer) : base(150, 150)
        {
            m_Coffer = coffer;
            AddPage(0);
            AddBackground(0, 0, 260, 640, 9270);
            AddAlphaRegion(10, 10, 240, 620);
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
            string scrollStatus = string.Format("Scrolls: {0} / 17", m_Coffer.ScrollsStored);
            string emeraldStatus = string.Format("Emeralds: {0} / 8", m_Coffer.EmeraldsStored);
            string arcanegemStatus = string.Format("Arcane Gems: {0} / 1", m_Coffer.ArcaneGemsStored);
            AddLabel(25, 480, m_Coffer.ScrollsStored >= 17 ? 67 : 907, scrollStatus); 
            AddLabel(25, 500, m_Coffer.EmeraldsStored >= 8 ? 67 : 907, emeraldStatus);
            AddLabel(25, 520, m_Coffer.ArcaneGemsStored >= 1 ? 67 : 907, arcanegemStatus);
            AddLabel(25, 540, 1157, string.Format("Remaining Charges: {0}", m_Coffer.Charges));
            if (m_Coffer.Charges > 0)
            {
              AddLabel(25, 560, 1157, "Fill Spellbook (10,000gp)");
              AddButton(210, 555, 4005, 4006, 1, GumpButtonType.Reply, 0);
            }
            else
            {
              AddLabel(25, 560, 907, "Coffer needs ritual components.");
            }
        }
        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
                m_Coffer.BeginBinding(sender.Mobile);
        }
    }
}
