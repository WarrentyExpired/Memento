using System;
using Server;
using Server.Items;
using System.Text;
using Server.Mobiles;
using Server.Network;
using Server.Commands;
using Server.Accounting;

namespace Server.Scripts.Commands
{
	public class AccountWalletCommands
	{
		public static void Initialize()
		{
			Properties.Initialize();
				Register( "WB", AccessLevel.Player, new CommandEventHandler( WB_OnCommand ) );
				Register( "WW", AccessLevel.Player, new CommandEventHandler( WW_OnCommand ) );
		}

		public static void Register( string command, AccessLevel access, CommandEventHandler handler )
		{
			CommandSystem.Register(command, access, handler);
		}

		[Usage( "WB" )]
		[Description( "Account Wallet Gold balance." )]
		public static void WB_OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;
			var player = from as PlayerMobile;
			player.SendMessage("Your Account Wallet balance is: " + player.AccountGold);
		}

		[Usage( "WW" )]
		[Description( "Withdraw Gold from your Account Wallet.")]
		public static void WW_OnCommand( CommandEventArgs e )
		{
			Mobile from = e.Mobile;
			if (e.Arguments.Length != 1)
			{
				e.Mobile.SendMessage("[WW amount ");
			}
			
			var player = from as PlayerMobile;
			int balance = player.AccountGold;
			var amount = e.GetInt32(0);
			if ( balance < amount)
			{	
				player.SendMessage("You do not have " + amount + " gold to withdraw.");
			}
			else if (amount > 1000000)
			{
				player.SendMessage("You can only withdraw a maximum of 1,000,000 gold at a time.");
			}
			else
			{
				if (amount > 60000)
				{
					player.AccountGold -= amount;
					player.BankBox.DropItem( new BankCheck( amount ) );
					player.SendMessage("A Bank Check for " + amount + " has been placed in your Bank Box");
					player.SendMessage("Your new balance is " + player.AccountGold);
				}
				else
				{
					player.AccountGold -= amount;
					player.BankBox.DropItem( new Gold( amount ) );
					player.SendMessage("You have withdrawn " + amount + " Gold to your Bank Box");
					player.SendMessage("Your new balance is " + player.AccountGold);
				}
			}
			
		}
	}
}
