using System;
using Server;
using Server.Items;
using Server.Network;

namespace Server.Items
{
    public class EGlobeOfVaelenComponent : AddonComponent
    {
        [Constructable]
        public EGlobeOfVaelenComponent(int itemID)
            : base(itemID)
        {
            Weight = 100.0;
            Movable = false;
        }

        public override int LabelNumber { get { return 1076681; } }
        public EGlobeOfVaelenComponent(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(this.GetWorldLocation(), 2))
            {
                from.LocalOverheadMessage(MessageType.Regular, 906, 1019045); // I can't reach that.
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class EGlobeOfVaelenAddon : BaseAddon
    {
        public override BaseAddonDeed Deed { get { return new EGlobeOfVaelenDeed(); } }

        [Constructable]
        public EGlobeOfVaelenAddon()
        {
            AddComponent(new EGlobeOfVaelenComponent(0x3657), 1, 0, 0);
            AddComponent(new EGlobeOfVaelenComponent(0x3658), 0, 0, 0);
            AddComponent(new EGlobeOfVaelenComponent(0x3661), 1, 0, 0);
            AddComponent(new EGlobeOfVaelenComponent(0x3659), 1, -1, 0);
        }

        public EGlobeOfVaelenAddon(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }

    public class EGlobeOfVaelenDeed : BaseAddonDeed
    {
        public override BaseAddon Addon { get { return new EGlobeOfVaelenAddon(); } }
        public override int LabelNumber { get { return 1076681; } }

        [Constructable]
        public EGlobeOfVaelenDeed()
        {
            ItemID = 0x14EF;
            Hue = 0x774;
            Weight = 1.0;
            LootType = LootType.Blessed;
        }

        public EGlobeOfVaelenDeed(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}
