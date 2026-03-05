using System;
using Server.Items;

namespace Server.Items
{
    public class MetalStorageBox : BaseResourceBox
    {
        public override string BoxTitle => "Blacksmith's Storage Box";
        
        // This box only accepts Ingots
        public override bool IsAllowed(Item item) => item is BaseIngot;

        [Constructable]
        public MetalStorageBox() : base(0x9A9) // Metal Chest Graphic
        {
            Name = "Metal Storage Box";
            Hue = 2407; // Metallic Blue/Steel
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
