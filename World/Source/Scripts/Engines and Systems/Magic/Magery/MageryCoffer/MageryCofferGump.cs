using System;
using Server.Gumps;
using Server.Network;

namespace Server.Items
{
    public class MageryCofferGump : Gump
    {
        private MageryCoffer m_Coffer;
        private static string[] m_SpellNames = new string[]
        {
          // Circle 1
          "Clumsy", "Feeblemind", "Heal", "Night Sight", "Reactive Armor", "Magic Arrow", "Poison", "Weaken",
          // Circle 2
          "Agility", "Cunning", "Cure", "Harm", "Magic Trap", "Magic Untrap", "Protection", "Strength",
          // Circle 3
          "Bless", "Fireball", "Magic Lock", "Poison", "Telekinesis", "Teleport", "Unlock", "Wall of Stone",
          // Circle 4
          "Arch Cure", "Arch Protection", "Curse", "Fire Field", "Greater Heal", "Lightning", "Mana Drain", "Recall",
          // Circle 5
          "Blade Spirits", "Disp. Field", "Incognito", "Magic Reflect", "Mind Blast", "Paralyze", "Poison Field", "Summ. Creature",
          // Circle 6
          "Dispel", "Energy Bolt", "Explosion", "Invisibility", "Mark", "Mass Curse", "Paralyze Field", "Reveal",
          // Circle 7
          "Chain Lightning", "Energy Field", "Flame Strike", "Gate Travel", "Mana Vampire", "Mass Dispel", "Meteor Swarm", "Polymorph",
          // Circle 8
          "Earthquake", "Energy Vortex", "Resurrection", "Air Elemental", "Summon Daemon", "Earth Elemental", "Fire Elemental", "Water Elemental"
        };
        public MageryCofferGump(MageryCoffer coffer) : base(150, 150)
        {
            m_Coffer = coffer;

            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;

            AddPage(0);
            AddBackground(0, 0, 800, 550, 9270);
            AddAlphaRegion(10, 10, 780, 530);
            AddLabel(320, 15, 1152, "Magery Preservation Coffer");

            int startX = 20;
            int startY = 50;
            int x = startX;
            int y = startY;

            for (int i = 0; i < 64; i++)
            {
                if (i > 0 && i % 16 == 0)
                {
                    x += 195;
                    y = startY;
                }

                bool hasSpell = m_Coffer.Slots[i];
                AddImage(x, y, hasSpell ? 2103 : 2104);
                
                int labelHue = hasSpell ? 67 : 907;
                string spellDisplay = String.Format("{0}. {1}", i + 1, m_SpellNames[i]);
                AddLabel(x + 25, y, labelHue, spellDisplay);

                y += 25;
            }

            string progress = String.Format("<BASEFONT COLOR=#FFFFFF>Collected: {0} / 64</BASEFONT>", m_Coffer.GetTotalScrolls());
            AddHtml(25, 510, 200, 20, progress, false, false);

            if (m_Coffer.GetTotalScrolls() >= 64)
            {
                AddButton(620, 505, 4005, 4007, 1, GumpButtonType.Reply, 0);
                AddLabel(655, 505, 67, String.Format("Fill Book ({0}gp)", m_Coffer.BindingCost));
            }
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            if (info.ButtonID == 1)
            {
                m_Coffer.BeginBinding(sender.Mobile);
            }
        }
    }
}
