using Server;
using System;
using System.Collections.Generic;
using System.Collections;
using Server.Items;
using Server.Multis;
using Server.Guilds;
using Server.ContextMenus;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;
using Server.Targeting;
using Server.Network;

namespace Server.Mobiles
{
	public class StoreMerchant : SBInfo
	{
		private List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
		private IShopSellInfo m_SellInfo = new InternalSellInfo();

		public StoreMerchant()
		{
		}

		public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
		public override List<GenericBuyInfo> BuyInfo { get { return m_BuyInfo; } }

		public class InternalBuyInfo : List<GenericBuyInfo>
		{
			// Add or remomve items you want to sell here. Each line is in this format:
			// Add( new GenericBuyInfo( ITEM TYPE, PRICE, QUANTITY, ITEM ID, HUE ) );
			// Keep your individual quantities below 10,000.
			public InternalBuyInfo()
			{
                          Add( new GenericBuyInfo( typeof(CagedRidableLlama ), 800, 5, 0x570b, 0 ) );
                          Add( new GenericBuyInfo( typeof(CagedHorse ), 1000, 5, 0x570c, 0 ) );
                          Add( new GenericBuyInfo( typeof( BagOfNecroReagents ), 600, 3, 0xE76, 2406 ) );
                          Add( new GenericBuyInfo( typeof( BagOfReagents ), 1400, 3, 0xE76, 2716 ) );
                          Add( new GenericBuyInfo( typeof( BagOfAlchemicReagents ), 900, 3, 0xE76, 2406 ) );
                          Add( new GenericBuyInfo( typeof( BagOfAllReagents ), 2900, 3, 0xE76, 2406 ) );
                          Add( new GenericBuyInfo( typeof( Ribs ), 10, 50, 0x9f2, 0 ) );
                          Add( new GenericBuyInfo( typeof( Bandage), 1, 1000, 0xe21, 0 ) );
		          Add( new GenericBuyInfo( typeof( Kindling ), 2, 20, 0xDE1, 0 ) );
			}
		}

		public class InternalSellInfo : GenericSellInfo
		{
			// Add or remomve items this merchant will buy, along with the gold value.
			// Prices below will be divided by 2 to ensure mercantile mechanics.
			public InternalSellInfo()
			{
				Add( typeof( Backpack ), 7 );
				Add( typeof( Pouch ), 3 );
				Add( typeof( Bag ), 3 );
				Add( typeof( Candle ), 3 );
				Add( typeof( Torch ), 4 );
				Add( typeof( Lantern ), 1 );
				Add( typeof( Lockpick ), 6 );
			}
		}
	}
}
