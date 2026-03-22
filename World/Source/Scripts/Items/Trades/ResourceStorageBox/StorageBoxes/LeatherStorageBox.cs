using System;
using Server.Items;
namespace Server.Items
{
    public class LeatherStorageBox : BaseResourceBox
    {
        public override string BoxTitle => "Tanner's Storage Box";
        
        public override bool IsAllowed(Item item) => item is BaseLeather;
        [Constructable]
        public LeatherStorageBox() : base(0x9A9)
        {
            Name = "Leather Storage Box";
            Hue = 1110;
        }

        public LeatherStorageBox(Serial serial) : base(serial) { }
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
