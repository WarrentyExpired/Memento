using Server.Mobiles;
using Server.Commands;
using Server.Network;
using Server.Items;
namespace Server.Gumps
{
    public class CombatBar
    {
        public static void Initialize()
        {
            CommandSystem.Register("combatbar", AccessLevel.Player, new CommandEventHandler(ToolBar_OnCommand));
        }

        public static void ToolBar_OnCommand(CommandEventArgs e)
        {
            Refresh(e.Mobile as PlayerMobile, true);
        }
        public static void Refresh(Mobile from, bool force = false)
        {
            if (from is PlayerMobile pm)
            {
                if (pm.HasGump(typeof(CombatBarGump)) || force)
                {
                    pm.CloseGump(typeof(CombatBarGump));
                    pm.SendGump(new CombatBarGump(pm));
                }
            }
        }
        public class CombatBarGump : Gump
        {
            private PlayerMobile m_Player;
            public CombatBarGump(PlayerMobile player) : base(100, 100)
            {
                m_Player = player;
                Closable = true;
                Disposable = true;
                Dragable = true;
                AddAlphaRegion(0, 0, 750, 25); 
                AddBackground(0, 0, 750, 25, 9270); 
                AddAlphaRegion(2, 2, 746, 21);
                Render();
            }
            private void Render()
            {
                int x = 15; 
                int y = 3; 
                DrawStat(x, y, "Fame", m_Player.Fame, "#FFD700");
                x += 85;
                string karmaColor = m_Player.Karma >= 0 ? "#ADD8E6" : "#FF4500";
                DrawStat(x, y, "Karma", m_Player.Karma, karmaColor);
                x += 95;
                DrawStat(x, y, "Hunger", m_Player.Hunger, "#9ACD32");
                x += 75;
                DrawStat(x, y, "Thirst", m_Player.Thirst, "#00BFFF");
                x += 75;
                DrawStat(x, y, "Tithing", m_Player.TithingPoints, "#FFCC00");
                x += 110;
                int bandages = m_Player.Backpack?.GetAmount(typeof(Bandage)) ?? 0;
                DrawStat(x, y, "Bandages", bandages, "#FFFFFF");
                x += 115;
                if (m_Player.Avatar != null && m_Player.Avatar.Active)
                {
                    int coins = m_Player.Avatar.PointsFarmed + m_Player.Avatar.PointsSaved;
                    DrawStat(x, y, "Avatar Coins", coins, "#55FF55");
                }
                else
                {
                    string content = "<BASEFONT Color=#888888>Avatar Coins: </BASEFONT><BASEFONT Color=#FF0000>Not Enabled</BASEFONT>";
                    AddHtml(x, y, 250, 20, content, false, false);
                }
            }
            private void DrawStat(int x, int y, string label, int amount, string color)
            {
                string formatted = amount.ToString("N0");
                string content = $"<BASEFONT Color=#888888>{label}: </BASEFONT><BASEFONT Color={color}>{formatted}</BASEFONT>";
                AddHtml(x, y, 150, 20, content, false, false);
            }
            public override void OnResponse(NetState sender, RelayInfo info)
            {
            }
        }
    }
}
