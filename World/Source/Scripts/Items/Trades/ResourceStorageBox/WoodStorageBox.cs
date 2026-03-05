using System;
using Server.Items;

namespace Server.Items
{
    public class WoodStorageBox : BaseResourceBox
    {
        public override string BoxTitle => "Carpenter's Storage Box";
        
        // This box accepts Boards and Shafts
        public override bool IsAllowed(Item item) => item is BaseWoodBoard;

        [Constructable]
        public WoodStorageBox() : base(0x9A9) // Small Wood Crate
        {
            Name = "Wood Storage Box";
            Hue = 1191; // Natural Wood Hue
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
