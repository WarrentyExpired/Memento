using System;
using System.Collections.Generic;
using Server;
using Server.Items;
using Server.Network;
using Server.Mobiles;

namespace Server.Mobiles
{
    public class AnimalTrainerLord : BaseCreature
    {
        // Controls how often he shouts so he doesn't spam the journal
        private DateTime m_NextShout;

        [Constructable]
        public AnimalTrainerLord() : base(AIType.AI_Thief, FightMode.None, 10, 1, 0.4, 1.6)
        {
            InitStats(85, 75, 65);
            Name = this.Female ? NameList.RandomName("female") : NameList.RandomName("male");
            Title = "the animal broker";

            Body = 0x191;
            Hue = Utility.RandomSkinHue();

            AddItem(new Boots(Utility.RandomBirdHue()));
            AddItem(new ShepherdsCrook());
            AddItem(new Cloak(Utility.RandomBirdHue()));
            AddItem(new FancyShirt(Utility.RandomBirdHue()));
            AddItem(new Kilt(Utility.RandomBirdHue()));
            AddItem(new BodySash(Utility.RandomBirdHue()));
        }

        // This math is required by your TamingBODGump and MonsterContractGump
        public static int ValuatePet(BaseCreature pet, Mobile from)
        {
            int val = (int)(pet.HitsMax + pet.StamMax + pet.ManaMax);
            val += (int)(pet.SkillsTotal / 10);
            return val;
        }

        // This triggers when anyone moves near him
        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (m is PlayerMobile && m.Alive && m.InRange(this, 3) && DateTime.UtcNow >= m_NextShout)
            {
                // Look at the player
                Direction = GetDirectionTo(m);

                switch (Utility.Random(3))
                {
                    case 0: Say("Hail, traveler! Are you here to sign a taming contract?"); break;
                    case 1: Say("The zoo is looking for rare specimens. Check the broker's book for orders!"); break;
                    case 2: Say("Expert tamers needed! Use the Broker's book to see our latest needs."); break;
                }

                // Point toward the stone (assuming you place it nearby)
                this.Animate(17, 5, 1, true, false, 0); 
                
                // Wait 15 seconds before shouting again
                m_NextShout = DateTime.UtcNow + TimeSpan.FromSeconds(15);
            }
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (from.InRange(this, 4))
            {
                this.Say("I just manage the paperwork. If you want a contract, use the Broker's book right there.");
            }
        }

        public AnimalTrainerLord(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}
