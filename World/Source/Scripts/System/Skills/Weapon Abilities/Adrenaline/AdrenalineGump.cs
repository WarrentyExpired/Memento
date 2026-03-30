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
            if (pm == null) return;
            pm.CloseGump(typeof(AdrenalineGump));
            pm.SendGump(new AdrenalineGump(pm));
        }
        public AdrenalineGump(PlayerMobile pm) : base(100, 130)
        {
            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
            int current = AdrenalineManager.GetAdrenaline(pm);
            double focusBonus = pm.Skills[SkillName.Focus].Value / 20;
            int cost1 = 25 - (int)focusBonus;
            int cost2 = 40 - (int)focusBonus;
            int cost3 = 60 - (int)focusBonus;
            string color1 = (current >= cost1) ? "#00FF00" : "#888888";
            string color2 = (current >= cost2) ? "#00FF00" : "#888888";
            string color3 = (current >= cost3) ? "#00FF00" : "#888888";
            int width = 160;
            int height = 75;
            AddPage(0);
            AddAlphaRegion(0, 0, width, height); 
            AddBackground(0, 0, width, height, 9270); 
            AddAlphaRegion(2, 2, width, height);
            AddHtml(10, 5, 140, 20, $"<BASEFONT Color=#888888>ADRENALINE: <BASEFONT Color=#FF4500>{current}%", false, false);
            int barWidth = (int)(current * 1.4);
            if (barWidth > 0)
                AddImageTiled(10, 25, barWidth, 8, 2056); 
            for (int i = 0; i <= 10; i++)
            {
                int xOffset = 10 + (i * 14);
                if (i == 0 || i == 10)
                {
                    AddImageTiled(xOffset, 25, 2, 8, 2626);
                }
                else
                {
                    AddImageTiled(xOffset, 25, 1, 8, 2622);
                }
            }
            // Button 1
            AddButton(10, 42, 2225, 2225, 1, GumpButtonType.Reply, 0); 
            AddHtml(35, 45, 30, 20, $"<BASEFONT Color={color1}>{cost1}", false, false);
            // Button 2
            AddButton(60, 42, 2226, 2226, 2, GumpButtonType.Reply, 0); 
            AddHtml(85, 45, 30, 20, $"<BASEFONT Color={color2}>{cost2}", false, false);
            // Button 3
            AddButton(110, 42, 2227, 2227, 3, GumpButtonType.Reply, 0); 
            AddHtml(135, 45, 30, 20, $"<BASEFONT Color={color3}>{cost3}", false, false);
        }
        public override void OnResponse(NetState sender, RelayInfo info)
        {
            PlayerMobile pm = sender.Mobile as PlayerMobile;
            if (pm == null) return;
            switch (info.ButtonID)
            {
                case 1: Server.Items.AdrenalineManager.AdrenalineCommands.OnAbility1(new CommandEventArgs(pm, "Adrenaline1", "", null)); break;
                case 2: Server.Items.AdrenalineManager.AdrenalineCommands.OnAbility2(new CommandEventArgs(pm, "Adrenaline2", "", null)); break;
                case 3: Server.Items.AdrenalineManager.AdrenalineCommands.OnAbility3(new CommandEventArgs(pm, "Adrenaline3", "", null)); break;
            }
            SendGump(pm);
        }
    }
    public class AdrenalineGumpCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("Adrenaline", AccessLevel.Player, new CommandEventHandler(Adrenaline_OnCommand));
        }
        [Usage("Adrenaline")]
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
