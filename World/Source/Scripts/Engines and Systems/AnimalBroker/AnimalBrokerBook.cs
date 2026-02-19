using System;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
    public class AnimalBrokerBook : Item
    {
        private static Dictionary<Mobile, DateTime> LastUsers = new Dictionary<Mobile, DateTime>();
        private static TimeSpan Delay = TimeSpan.FromHours(2.0);

        [Constructable]
        public AnimalBrokerBook() : base(0xFBE)
        {
            Movable = false;
            Name = "Animal Broker's Book";
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2))
            {
                from.SendLocalizedMessage(500446); // That is too far away.
                return;
            }

            from.CloseGump(typeof(BrokerBookGump));
            from.SendGump(new BrokerBookGump(from));
        }

        private class BrokerBookGump : Gump
        {
            public BrokerBookGump(Mobile from) : base(150, 150)
            {
                AddPage(0);
                AddBackground(0, 0, 300, 150, 9270);
                AddAlphaRegion(10, 10, 280, 130);

                AddHtml(0, 15, 300, 20, "<CENTER>ANIMAL BROKER SERVICES</CENTER>", false, false);

                // Taming BOD Button
                AddButton(20, 50, 4005, 4007, 1, GumpButtonType.Reply, 0);
                AddLabel(55, 50, 1152, "Claim Taming BOD (Skill 50+)");

                // Monster Contract Button
                AddButton(20, 90, 4005, 4007, 2, GumpButtonType.Reply, 0);
                AddLabel(55, 90, 1152, "Claim Monster Contract (Skill 90+)");
            }

            public override void OnResponse(NetState sender, RelayInfo info)
            {
                Mobile from = sender.Mobile;

                if (info.ButtonID == 0) return;

                double skill = from.Skills[SkillName.Taming].Value + from.Skills[SkillName.Druidism].Value;

                // Skill Checks
                if (info.ButtonID == 1 && skill < 50.0)
                {
                    from.SendMessage("You lack the basic knowledge of animal taming (50.0) to take this order.");
                    return;
                }
                if (info.ButtonID == 2 && skill < 90.0)
                {
                    from.SendMessage("You must be a master tamer (90.0) to handle monster hunting contracts.");
                    return;
                }

                // Cooldown Check
                if (LastUsers.TryGetValue(from, out DateTime lastUse))
                {
                    TimeSpan cooldown = Delay - (DateTime.UtcNow - lastUse);
                    if (cooldown > TimeSpan.Zero)
                    {
                        from.SendMessage("You must wait {0} hours and {1} minutes before taking another contract.", cooldown.Hours, cooldown.Minutes);
                        return;
                    }
                }

                // Rewards
                if (info.ButtonID == 1)
                {
                    int amount = Utility.RandomMinMax(10, 20); // Basic amount
                    from.AddToBackpack(new TamingBOD(amount));
                    from.SendMessage("You have taken a new taming order.");
                }
                else if (info.ButtonID == 2)
                {
                    from.AddToBackpack(new MonsterContract());
                    from.SendMessage("You have signed a specialized monster hunting contract.");
                }

                LastUsers[from] = DateTime.UtcNow;
            }
        }

        public AnimalBrokerBook(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}
