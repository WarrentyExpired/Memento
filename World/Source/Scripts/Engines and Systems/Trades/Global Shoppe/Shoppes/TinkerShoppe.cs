using Server.Items;
using Server.Misc;
using Server.Mobiles;

namespace Server.Engines.GlobalShoppe
{
    [Flipable(0x3D03, 0x3D04)]
    public class TinkerShoppe : ShoppeBase
    {
        [Constructable]
        public TinkerShoppe() : base(0x3D03)
        {
            Name = "Tinker Work Shoppe";
        }

        public TinkerShoppe(Serial serial) : base(serial)
        {
        }

        public override NpcGuild Guild { get { return NpcGuild.TinkersGuild; } }

        protected override SkillName PrimarySkill { get { return SkillName.Tinkering; } }
        protected override ShoppeType ShoppeType { get { return ShoppeType.Tinker; } }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped is TinkerTools)
                return AddTools(from, dropped);
            if (dropped.ItemID >= 0x1BE3 && dropped.ItemID <= 0x1BFA)
                return AddResource(from, dropped);

            return base.OnDragDrop(from, dropped);
        }

        protected override string CreateTask(TradeSkillContext context)
        {
            string task = null;

            switch (Utility.RandomMinMax(1, 6))
            {
                case 1: task = "Repair"; break;
                case 2: task = "Fix"; break;
                case 3: task = "Enhance"; break;
                case 4: task = "Modify"; break;
                case 5: task = "Resize"; break;
                case 6: task = "Alter"; break;
            }

            string forr = "their";
            switch (Utility.RandomMinMax(0, 15))
            {
                case 0: forr = "their"; break;
                case 1: forr = "their friend's"; break;
                case 2: forr = "this"; break;
                case 3: forr = "their father's"; break;
                case 4: forr = "their mother's"; break;
                case 5: forr = "their brother's"; break;
                case 6: forr = "their sister's"; break;
                case 7: forr = "their uncle's"; break;
                case 8: forr = "their aunt's"; break;
                case 9: forr = "their cousin's"; break;
                case 10: forr = "their grandparent's"; break;
            }

            if (Utility.RandomMinMax(1, 3) == 1)
            {
                Item item = null;

                switch (Utility.RandomMinMax(1, 7))
                {
                    case 1: item = new TrinketLantern(); break;
                    case 2: item = new TrinketCandle(); break;
                    case 3: item = new JewelryRing(); break;
                    case 4: item = new JewelryNecklace(); break;
                    case 5: item = new JewelryEarrings(); break;
                    case 6: item = new JewelryBracelet(); break;
                    case 7: item = new JewelryCirclet(); break;
                }

                bool evil = false;
                bool orient = false;

                switch (Utility.RandomMinMax(1, 8))
                {
                    case 1: evil = true; break;
                    case 2: orient = true; break;
                }

                string sAdjective = "unusual";
                string eAdjective = "might";

                sAdjective = RandomThings.MagicItemAdj("start", orient, evil, item.ItemID);
                eAdjective = RandomThings.MagicItemAdj("end", orient, evil, item.ItemID);

                string name = "item";
                string xName = ContainerFunctions.GetOwner("property");

                if (item.Name != null && item.Name != "") { name = item.Name.ToLower(); }
                if (name == "item") { item.SyncName(); name = (item.Name).ToLower(); }

                switch (Utility.RandomMinMax(0, 5))
                {
                    case 0: name = sAdjective + " " + name + " of " + xName; break;
                    case 1: name = name + " of " + xName; break;
                    case 2: name = sAdjective + " " + name; break;
                    case 3: name = sAdjective + " " + name + " of " + xName; break;
                    case 4: name = name + " of " + xName; break;
                    case 5: name = sAdjective + " " + name; break;
                }

                item.Delete();

                task = task + " " + forr + " " + name;
            }
            else
            {
                string[] sTinks = new string[] { " jointing plane ", " moulding plane ", " smoothing plane ", " clock ", " axle ", " rolling pin ", " scissors ", " mortar pestle ", " scorp ", " draw knife ", " sewing kit ", " druid's cauldron ", " saw ", " dovetail saw ", " froe ", " shovel ", " hammer ", " tongs ", " inshave ", " pickaxe ", " lockpick ", " skillet ", " flour sifter ", " bowcrafting tools ", " mapmakers pen ", " scribes pen ", " skinning knife ", " witch's cauldron ", " waxing pot ", " sextant ", " spoon ", " plate ", " forkleft ", " cleaver ", " knife ", " goblet ", " mug ", " candle ", " scales ", " key ", " key ring ", " globe ", " spyglass ", " lantern ", " heating stand ", " amulet ", " bracelet ", " ring ", " earrings ", " potion keg " };
                string sTink = sTinks[Utility.RandomMinMax(0, (sTinks.Length - 1))];

                task = task + " " + forr + sTink;
            }

            return task;
        }

        protected override ShoppeGump GetGump(PlayerMobile from)
        {
            var context = GetOrCreateContext(from);

            return new ShoppeGump(
                from,
                this,
                context,
                "TINKER WORK SHOPPE",
                "Tinker Tools",
                "Ingots"
            );
        }

        protected override void OnJobFailed(Mobile from, TradeSkillContext context, CustomerContext customer)
        {
            base.OnJobFailed(from, context, customer);

            from.SendSound(0x542); // Tinker
        }

        protected override void OnJobSuccess(Mobile from, TradeSkillContext context, CustomerContext customer)
        {
            base.OnJobSuccess(from, context, customer);

            from.SendSound(0x542); // Tinker
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }
    }
}