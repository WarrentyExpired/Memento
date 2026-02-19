using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
    public class FluteOfVeterinary : Item
    {
        [Constructable]
        public FluteOfVeterinary() : base(0x2805) // Using a Flute Graphic
        {
            Name = "Tamer's Soothing Flute";
            Weight = 1.0;
            Hue = 2125; // A light blue/heal color
        }

        public override void OnDoubleClick(Mobile from)
        {
            // Check if the item is in the user's backpack
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
                return;
            }

            // Optional: Add a cooldown (e.g., 5 seconds)
            if (DateTime.UtcNow < m_NextUse)
            {
                from.SendMessage("The flute is still echoing... wait a moment.");
                return;
            }

            int range = 8; // Heal distance
            bool healedAny = false;

            // Iterate through mobiles in range
            foreach (Mobile m in from.GetMobilesInRange(range))
            {
                // Only heal BaseCreatures (Pets) that are controlled by the user
                if (m is BaseCreature pet && pet.Controlled && pet.ControlMaster == from)
                {
                    if (pet.Hits < pet.HitsMax)
                    {
                        // Heal amount based on Veterinary skill (standard formula)
                        double vetSkill = from.Skills[SkillName.Veterinary].Value;
                        int toHeal = (int)(vetSkill / 5) + Utility.RandomMinMax(5, 15);
                        
                        pet.Hits += toHeal;
                        pet.FixedEffect(0x376A, 9, 32); // Healing sparkle effect
                        healedAny = true;
                    }
                }
            }

            if (healedAny)
            {
                from.PlaySound(0x504); // Play a soothing flute sound
                from.SendMessage("Your pets feel revitalized by the music.");
                m_NextUse = DateTime.UtcNow + TimeSpan.FromSeconds(5.0); // Cooldown
            }
            else
            {
                from.SendMessage("None of your pets nearby need healing.");
            }
        }

        private DateTime m_NextUse;

        public FluteOfVeterinary(Serial serial) : base(serial) { }

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
