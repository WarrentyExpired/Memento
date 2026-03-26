using System;
using Server.Items;

namespace Server.Items
{
    public class CraftingResourceBox : BaseResourceBox
    {
        public override string BoxTitle => "Crafting Resouce Storage Box";

        public override bool IsAllowed(Item item)
        {
            return item is BaseIngot || item is BaseWoodBoard || item is Shaft || 
                   item is Feather || item is BaseLeather || item is BaseFabric || 
                   item is BaseSkeletal || item is BaseScales || item is BlackPearl || 
                   item is Bloodmoss || item is Garlic || item is Ginseng || 
                   item is Nightshade || item is SpidersSilk || item is SulfurousAsh || 
                   item is MandrakeRoot || item is BeetleShell || item is Brimstone || 
                   item is ButterflyWings || item is FairyEgg || item is GargoyleEar || 
                   item is MoonCrystal || item is PixieSkull || item is RedLotus ||
                   item is SeaSalt || item is SilverWidow || item is SwampBerries ||
                   item is BatWing || item is DaemonBlood || item is GraveDust || 
                   item is NoxCrystal || item is PigIron || item is BitterRoot || 
                   item is BlackSand || item is BloodRose || item is DriedToad || 
                   item is Maggot || item is MummyWrap || item is VioletFungus || 
                   item is WerewolfClaw || item is Wolfsbane || item is EyeOfToad || 
                   item is Bottle || item is BlankScroll;
        }

        [Constructable]
        public CraftingResourceBox() : base(0x9A9)
        {
            Name = "Crafting Resource Box";
            Hue = 1150;
        }

        public CraftingResourceBox(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write(0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}
