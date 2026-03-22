using System;
using Server.Items;
namespace Server.Items
{
    public class MetalStorageBox : BaseResourceBox
    {
        public override string BoxTitle => "Blacksmith's Storage Box";
        public override bool IsAllowed(Item item) => item is BaseIngot;
        [Constructable]
        public MetalStorageBox() : base(0x9A9)
        {
            Name = "Metal Storage Box";
            Hue = 2407;
        }
        public MetalStorageBox(Serial serial) : base(serial) { }
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
