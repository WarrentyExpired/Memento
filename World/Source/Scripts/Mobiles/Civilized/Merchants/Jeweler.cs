using System.Collections.Generic;
using Server.Items;

namespace Server.Mobiles
{
	public class Jeweler : BaseVendor
	{
		private List<SBInfo> m_SBInfos = new List<SBInfo>();
		protected override List<SBInfo> SBInfos{ get { return m_SBInfos; } }
		public override bool IsBlackMarket { get { return true; } }

		public override NpcGuild NpcGuild{ get{ return NpcGuild.MerchantsGuild; } }

		[Constructable]
		public Jeweler() : base( "the jeweler" )
		{
			SetSkill( SkillName.Mercantile, 64.0, 100.0 );
		}

		public override void InitSBInfo( Mobile m )
		{
			m_Merchant = m;
			m_SBInfos.Add( new MyStock() );
		}

		public class MyStock: SBInfo
		{
			private List<GenericBuyInfo> m_BuyInfo = new InternalBuyInfo();
			private IShopSellInfo m_SellInfo = new InternalSellInfo();

			public MyStock()
			{
			}

			public override IShopSellInfo SellInfo { get { return m_SellInfo; } }
			public override List<GenericBuyInfo> BuyInfo { get { return m_BuyInfo; } }

			public class InternalBuyInfo : List<GenericBuyInfo>
			{
				public InternalBuyInfo()
				{
					ItemInformation.GetSellList( m_Merchant, this, 	ItemSalesInfo.Category.All,		ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Jeweler,		ItemSalesInfo.World.None,	null	 );
				}
			}

			public class InternalSellInfo : GenericSellInfo
			{
				public InternalSellInfo()
				{
					ItemInformation.GetBuysList( m_Merchant, this, 	ItemSalesInfo.Category.All,		ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Jeweler,		ItemSalesInfo.World.None,	null	 );
				}
			}
		}

		public override void UpdateBlackMarket()
		{
			base.UpdateBlackMarket();

			if ( IsBlackMarket && MyServerSettings.BlackMarket() )
			{
				int v=20; while ( v > 0 ){ v--;
				ItemInformation.BlackMarketList( this, ItemSalesInfo.Category.All,			ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Jeweler,		ItemSalesInfo.World.None,	typeof( JewelryBracelet )	 );
				ItemInformation.BlackMarketList( this, ItemSalesInfo.Category.All,			ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Jeweler,		ItemSalesInfo.World.None,	typeof( JewelryCirclet )	 );
				ItemInformation.BlackMarketList( this, ItemSalesInfo.Category.All,			ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Jeweler,		ItemSalesInfo.World.None,	typeof( JewelryEarrings )	 );
				ItemInformation.BlackMarketList( this, ItemSalesInfo.Category.All,			ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Jeweler,		ItemSalesInfo.World.None,	typeof( JewelryNecklace )	 );
				ItemInformation.BlackMarketList( this, ItemSalesInfo.Category.All,			ItemSalesInfo.Material.None,		ItemSalesInfo.Market.Jeweler,		ItemSalesInfo.World.None,	typeof( JewelryRing )	 );
				}
			}
		}

		public Jeweler( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}