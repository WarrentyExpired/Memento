using System;
using Server.Items;

namespace Server.Items
{
    public class ClothStorageBox : BaseResourceBox
    {
        public override string BoxTitle => "Tailor's Storage Box";
        
        // This box accepts Cloth, Uncut Cloth, and Feathers
        public override bool IsAllowed(Item item) => item is BaseFabric;

        [Constructable]
        public ClothStorageBox() : base(0x9A9) // Wooden Box
        {
            Name = "Cloth Storage Box";
            Hue = 1150; // White/Cloth Hue
        }

        public ClothStorageBox(Serial serial) : base(serial) { }

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
