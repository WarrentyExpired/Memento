using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
    public class PetCagingLeash : Item
    {
        private int m_Charges;

        [CommandProperty(AccessLevel.GameMaster)]
        public int Charges
        {
            get { return m_Charges; }
            set { m_Charges = value; InvalidateProperties(); }
        }

        [Constructable]
        public PetCagingLeash() : base(0x1375)
        {
            Name = "Pet Caging Leash";
            Weight = 1.0;
            Hue = 1165; // Optional: A magical purple/blue hue
            m_Charges = 10;
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            list.Add("Charges: {0}", m_Charges);
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (m_Charges <= 0)
            {
                from.SendMessage("The Leash is to worn to use anymore.");
                return;
            }

            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                return;
            }

            from.SendMessage("Which mount would you like to cage?");
            from.Target = new InternalTarget(this);
        }

        private class InternalTarget : Target
        {
            private PetCagingLeash m_Leash;

            public InternalTarget(PetCagingLeash Leash) : base(8, false, TargetFlags.None)
            {
                m_Leash = Leash;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (m_Leash.Deleted || m_Leash.Charges <= 0)
                    return;

                if (targeted is BaseMount mount)
                {
                    if (!mount.Controlled || mount.ControlMaster != from)
                    {
                        from.SendMessage("You can only cage your own tamed mounts.");
                        return;
                    }

                    // Map the mount to the correct Cage type from your AnimalCages.cs
                    BaseCaged cage = null;

                    if (mount is Horse)
                        cage = new CagedHorse();
                    else if (mount is RidableLlama)
                        cage = new CagedRidableLlama();
                    else
                    {
                        from.SendMessage("This Leash only works on standard mounts (Horses and Llamas).");
                        return;
                    }

                    if (cage != null)
                    {
                        from.AddToBackpack(cage);
                        mount.Delete(); // Remove the live pet from the world
                        
                        m_Leash.Charges--;
                        from.PlaySound(0x1FE); // Magical sound
                        from.SendMessage("The mount is safely compressed into the cage. Charges remaining: {0}", m_Leash.Charges);

                        if (m_Leash.Charges <= 0)
                            from.SendMessage("The Leash breaks as its final charge is used.");
                    }
                }
                else
                {
                    from.SendMessage("That is not a mount.");
                }
            }
        }

        public PetCagingLeash(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
            writer.Write(m_Charges);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_Charges = reader.ReadInt();
        }
    }
}
