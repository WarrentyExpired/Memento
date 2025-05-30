using System;
using System.Collections;
using System.Collections.Generic;
using Server.Items;
using Server.ContextMenus;
using Server.Misc;
using Server.Network;
using System.Text;
using Server;
using Server.Commands;
using Server.Commands.Generic;
using System.IO;
using Server.Mobiles;
using Server.Gumps;
using Server.Accounting;

namespace Server
{
    public class Announce
    {
        public static void Initialize()
        {
            EventSink.Login += new LoginEventHandler(World_Login);
            EventSink.Logout += new LogoutEventHandler(World_Logout);
            EventSink.Disconnected += new DisconnectedEventHandler(World_Leave);
            EventSink.PlayerDeath += new PlayerDeathEventHandler(OnDeath);
        }

        private static void World_Login(LoginEventArgs args)
        {
            Mobile m = args.Mobile;
			PlayerMobile pm = (PlayerMobile)m;
			PlayerMobile z = (PlayerMobile)m;
			Mobile s = args.Mobile;

			if ( m.Hue >= 33770 ){ m.Hue = m.Hue - 32768; }

			m.RaceBody();

			if ( ((PlayerMobile)m).GumpHue > 0 && m.RecordSkinColor == 0 )
			{
				m.RecordsHair( true );

				// THESE 3 LINES CAN BE REMOVED...MAYBE BY 1-JAN-2022. STORAGE VALUES REPLACED.
				m.RecordHairColor = ((PlayerMobile)m).WeaponBarOpen;
				m.RecordBeardColor = ((PlayerMobile)m).WeaponBarOpen;
				m.RecordSkinColor = ((PlayerMobile)m).GumpHue;

				((PlayerMobile)m).WeaponBarOpen = 1;
				((PlayerMobile)m).GumpHue = 1;
			}

			if ( m.RecordSkinColor >= 33770 ){ m.RecordSkinColor = m.RecordSkinColor - 32768; m.Hue = m.RecordSkinColor; }

			m.RecordFeatures( false );
			m.Stam = m.StamMax;

			if ( !MySettings.S_AllowCustomTitles ){ m.Title = null; }

			LoggingFunctions.LogAccess( m, "login" );

			if ( m.Region.GetLogoutDelay( m ) == TimeSpan.Zero && !m.Poisoned ){ m.Hits = 1000; m.Stam = 1000; m.Mana = 1000; } // FULLY REST UP ON LOGIN
        }

        private static void World_Leave(DisconnectedEventArgs args)
        {
			if ( MySettings.S_SaveOnCharacterLogout ){ World.Save( true, false ); }
        }

        private static void World_Logout(LogoutEventArgs args)
        {
            Mobile m = args.Mobile;
			LoggingFunctions.LogAccess( m, "logout" );
        }
		
        public static void OnDeath(PlayerDeathEventArgs args)
        {
            Mobile m = args.Mobile;
			GhostHelper.OnGhostWalking( m );
        }
    }
}