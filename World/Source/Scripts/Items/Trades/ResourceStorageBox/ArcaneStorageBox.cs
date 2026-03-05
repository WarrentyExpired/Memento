using System;
using Server;
using Server.Items;

namespace Server.Items
{
    public class ArcaneStorageBox : BaseResourceBox
    {
        public override string BoxTitle => "Arcane & Alchemy Storage Box";

        [Constructable]
        public ArcaneStorageBox() : base(0x9A9) // Wooden box graphic
        {
            Name = "Arcane Storage Box";
            Hue = 1101; // A deep blue/purple for magic
            Weight = 20;
        }

        public override bool IsAllowed(Item item)
        {
            // Reagents
            if (item is BlackPearl || item is Bloodmoss || item is Garlic || item is Ginseng || 
                item is Nightshade || item is SpidersSilk || item is SulfurousAsh || item is MandrakeRoot ||
                item is BeetleShell || item is Brimstone || item is ButterflyWings || item is FairyEgg ||
                item is GargoyleEar || item is MoonCrystal || item is PixieSkull || item is RedLotus ||
                item is SeaSalt || item is SilverWidow || item is SwampBerries || item is BatWing ||
                item is DaemonBlood || item is GraveDust || item is NoxCrystal || item is PigIron ||
                item is BitterRoot || item is BlackSand || item is BloodRose || item is DriedToad || 
                item is Maggot || item is MummyWrap || item is VioletFungus || item is WerewolfClaw ||
                item is Wolfsbane || item is EyeOfToad
                )
                return true;

            // Alchemy & Inscription supplies
            if (item is Bottle || item is BlankScroll)
                return true;

            return false;
        }

        public ArcaneStorageBox(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int version = reader.ReadInt(); }
    }
}
