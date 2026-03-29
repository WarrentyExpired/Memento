using System;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using Server.Mobiles;
using Server.Items;

namespace Server.Gumps
{
    public class AdrenalineGump : Gump
    {
        public static void SendGump(PlayerMobile pm)
        {
            pm.CloseGump(typeof(AdrenalineGump));
            pm.SendGump(new AdrenalineGump(pm));
        }

        public AdrenalineGump(PlayerMobile pm) : base(100, 130) // Adjusted position to sit near toolbar
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            int current = AdrenalineManager.GetAdrenaline(pm);
            
            // Width matches the smaller version of your combat bar
            int width = 160; 
            int height = 45;

            AddPage(0);

            // Background matching CombatBarH: Alpha region for transparency + Border 9270
            AddAlphaRegion(0, 0, width, height); 
            AddBackground(0, 0, width, height, 9270);
            AddAlphaRegion(2, 2, width, height);

            // The Label - Using the grey color from your toolbar stats (#888888)
            AddHtml(10, 5, 140, 20, $"<BASEFONT Color=#888888>ADRENALINE: <BASEFONT Color=#FF4500>{current}%", false, false);

            // The Progress Bar Background (Empty slot)
            AddImageTiled(10, 25, 140, 10, 2626); // Darker inset look
            
            // The actual "fill" 
            // We scale 'current' from 100% to 140 pixels wide
            int barWidth = (int)(current * 1.4);
            if (barWidth > 0)
            {
                // Using 2056 (Reddish/Orange) to match the "Hot" theme of Adrenaline
                AddImageTiled(10, 25, barWidth, 10, 2056); 
            }
        }
    }

    public class AdrenalineCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("Adrenaline", AccessLevel.Player, new CommandEventHandler(Adrenaline_OnCommand));
        }

        [Usage("Adrenaline")]
        [Description("Toggles the Adrenaline display bar.")]
        private static void Adrenaline_OnCommand(CommandEventArgs e)
        {
            PlayerMobile pm = e.Mobile as PlayerMobile;
            if (pm != null)
            {
                if (pm.HasGump(typeof(AdrenalineGump)))
                    pm.CloseGump(typeof(AdrenalineGump));
                else
                    AdrenalineGump.SendGump(pm);
            }
        }
    }
}
