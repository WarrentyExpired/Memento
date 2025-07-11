using Server.Engines.Craft;

namespace Server.Items
{
	public class GodSewing : BaseTool, IRunicTool
	{
		public override CraftSystem CraftSystem{ get{ return DefStitching.CraftSystem; } }

		public int RunicMinAttributes { get { return 2; } }
		public int RunicMaxAttributes { get { return 2; } }
		public int RunicMinIntensity { get { return 50; } }
		public int RunicMaxIntensity { get { return 50; } }

		[Constructable]
		public GodSewing() : base( 0x6600 )
		{
			Weight = 1.0;
			Name = "stitching tools";
			UsesRemaining = 10;
			Hue = 0xB17;
			NotModAble = true;
		}

		public GodSewing( Serial serial ) : base( serial )
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
			Name = "stitching tools";
			Hue = 0xB17;
			ItemID = 0x6600;
			NotModAble = true;
		}
	}
}