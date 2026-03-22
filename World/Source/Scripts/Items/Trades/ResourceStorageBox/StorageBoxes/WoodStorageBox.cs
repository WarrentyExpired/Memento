using System;
using Server.Items;
namespace Server.Items
{
    public class WoodStorageBox : BaseResourceBox
    {
        public override string BoxTitle => "Carpenter's Storage Box";
        public override bool IsAllowed(Item item) => item is BaseWoodBoard || item is Shaft || item is Feather;
        [Constructable]
        public WoodStorageBox() : base(0x9A9)
        {
            Name = "Wood Storage Box";
            Hue = 1191;
        }
        public WoodStorageBox(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}
