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
        public static void Register(string command, AccessLevel access, CommandEventHandler handler)
        {
            CommandSystem.Register(command, access, handler);
        }
        [Usage("CombatBar")]
        [Description("Opens the Horizontal Combat Bar.")]
        public static void ToolBar_OnCommand(CommandEventArgs e)
        {
            Refresh(e.Mobile as PlayerMobile, true);
        }
        public static void Refresh(Mobile from, bool force = false)
        {
            if (from is PlayerMobile)
            {
                if (from.HasGump(typeof(CombatBarGump)) || force)
                {
                    from.CloseGump(typeof(CombatBarGump));
                    from.SendGump(new CombatBarGump((PlayerMobile)from));
                }
            }
        }
        public class CombatBarGump : Gump
        {
            public CombatBarGump(PlayerMobile player) : base(100, 100)
            {
                this.Closable = true;
                this.Disposable = true;
                this.Dragable = true;
                AddAlphaRegion(0, 0, 750, 35); 
                AddBackground(0, 0, 750, 35, 9270); 
                AddAlphaRegion(2, 2, 746, 31);
                int x = 15;
                int y = 8;
                string fame = string.Format("<BASEFONT Color=#888888>Fame: </BASEFONT><BASEFONT Color=#FFD700>{0}</BASEFONT>", player.Fame);
                AddHtml(x, y, 150, 20, fame, false, false);
                x += 85;
                string karmaColor = player.Karma >= 0 ? "#ADD8E6" : "#FF4500";
                string karma = string.Format("<BASEFONT Color=#888888>Karma: </BASEFONT><BASEFONT Color={0}>{1}</BASEFONT>", karmaColor, player.Karma); 
                AddHtml(x, y, 150, 20, karma, false, false);
                x += 95;
                string hunger = string.Format("<BASEFONT Color=#888888>Hunger: </BASEFONT><BASEFONT Color=#9ACD32>{0}</BASEFONT>", player.Hunger); 
                AddHtml(x, y, 150, 20, hunger, false, false);
                x += 75;
                string thirst = string.Format("<BASEFONT Color=#888888>Thirst: </BASEFONT><BASEFONT Color=#00BFFF>{0}</BASEFONT>", player.Thirst);
                AddHtml(x, y, 150, 20, thirst, false, false);
                x += 75;
                int bandages = player.Backpack?.GetAmount(typeof(Bandage)) ?? 0;
                string bandage = string.Format("<BASEFONT Color=#888888>Bandages: </BASEFONT><BASEFONT Color=#FFFFFF>{0}</BASEFONT>", bandages);
                AddHtml(x, y, 150, 20, bandage, false, false);
                x += 110;
                string thithing = string.Format("<BASEFONT Color=#888888>Thithing: </BASEFONT><BASEFONT Color=#FFCC00>{0}</BASEFONT>", player.TithingPoints);
                AddHtml(x, y, 150, 20, thithing, false, false);
                x += 115;
                if (player.Avatar != null && player.Avatar.Active)
                {
                    int coins = player.Avatar.PointsFarmed + player.Avatar.PointsSaved;
                    string coin = string.Format("<BASEFONT Color=#888888>Avatar Coins: </BASEFONT><BASEFONT Color=#55ff55>{0}</BASEFONT>",coins);
                    AddHtml(x, y, 150, 20, coin, false, false);
                }
                else
                {
                    string context = "<BASEFONT Color=#888888>Avatar Coins:</BASEFONT><BASEFONT Color=#FF0000> Not Enabled</BASEFONT>";
                    AddHtml(x, y, 250, 20, context, false, false);
                }
            }
        }
    }
}
