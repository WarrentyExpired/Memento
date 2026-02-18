using System;
using Server;
using Server.Mobiles;
using Server.Items;
using System.Collections.Generic;
using System.Text;

namespace Server.Items
{
    public class TamersSatchel : Bag
    {
        private DateTime m_NextUse;
        private Timer m_CooldownTimer;

        [Constructable]
        public TamersSatchel() : base()
        {
            Name = "Tamer's Satchel";
            Hue = 2129; 
            MaxItems = 20; 
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            
            int bandages = 0;
            int foodCount = 0;

            // Fixed the pluralization error here (Items instead of Item)
            foreach (Item item in this.Items)
            {
                if (item is Bandage) 
                    bandages += item.Amount;
                else if (item is Food) 
                    foodCount += item.Amount;
            }

            StringBuilder sb = new StringBuilder();
            if (bandages > 0) sb.AppendFormat("Bandages: {0}<br>", bandages);
            if (foodCount > 0) sb.AppendFormat("Pet Food: {0}", foodCount);

            if (sb.Length > 0)
                list.Add(1060658, "Contents\t{0}", sb.ToString());
            else
                list.Add(1060658, "Contents\tEmpty");
        }

        public override bool CheckHold(Mobile m, Item item, bool message, bool checkItems, int plusItems, int plusWeight)
        {
            // Restrict contents to only Bandages and Food
            if (item is Bandage || item is Food)
            {
                return base.CheckHold(m, item, message, checkItems, plusItems, plusWeight);
            }

            if (message)
                m.SendMessage("The satchel is only for bandages and pet food.");

            return false;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001);
                return;
            }

            if (DateTime.UtcNow < m_NextUse)
            {
                from.SendMessage("You are still repacking the satchel.");
                return;
            }

            FeedPets(from);
            PerformMedical(from);
        }

        private void FeedPets(Mobile from)
        {
            List<BaseCreature> pets = new List<BaseCreature>();
            
            foreach (Mobile m in from.GetMobilesInRange(3))
            {
                if (m is BaseCreature pet && pet.Controlled && pet.ControlMaster == from)
                {
                    // If pet happiness is below max
                    if (pet.Loyalty < 100)
                        pets.Add(pet);
                }
            }

            if (pets.Count > 0)
            {
                Item food = this.FindItemByType(typeof(Food));
                
                if (food != null)
                {
                    foreach (BaseCreature pet in pets)
                    {
                        if (food.Amount > 0)
                        {
                            pet.Loyalty = 100; // Restore happiness
                            pet.PlaySound(0x3B); // Eating sound
                            food.Consume(1);
                            from.SendMessage("You feed {0}. They are now Wonderfully Happy!", pet.Name);
                        }
                    }
                    // Refresh the tooltip to show updated food count
                    this.InvalidateProperties(); 
                }
                else
                {
                    from.SendMessage("Some of your pets are hungry, but there is no food in the satchel.");
                }
            }
        }

        private void PerformMedical(Mobile from)
        {
            Item bandages = this.FindItemByType(typeof(Bandage));
            if (bandages == null || bandages.Amount < 1)
            {
                from.SendMessage("The satchel lacks bandages for medical treatment.");
                return;
            }

            List<BaseCreature> toCure = new List<BaseCreature>();
            List<BaseCreature> toHeal = new List<BaseCreature>();

            int range = Bandage.Range; 

            foreach (Mobile m in from.GetMobilesInRange(18))
            {
                if (m is BaseCreature pet && pet.Controlled && pet.ControlMaster == from)
                {
                    if (pet.Poisoned || pet.Hits < pet.HitsMax)
                    {
                        if (from.InRange(pet, range))
                        {
                            if (pet.Poisoned) toCure.Add(pet);
                            else toHeal.Add(pet);
                        }
                        else
                        {
                            from.SendMessage("{0} is too far away to be treated!", pet.Name);
                        }
                    }
                }
            }

            int totalTargets = toCure.Count + toHeal.Count;
            if (totalTargets > 0)
            {
                int processed = 0;
                double vety = from.Skills[SkillName.Veterinary].Value;
                double druid = from.Skills[SkillName.Druidism].Value;

                foreach (BaseCreature pet in toCure)
                {
                    if (processed >= bandages.Amount) break;
                    double chance = (vety / 2.0) + (druid / 2.0);
                    if (chance >= ((pet.Poison.Level * 20.0) + 10.0) && vety >= 60.0)
                    {
                        pet.CurePoison(from);
                        pet.FixedEffect(0x373A, 10, 15);
                        from.SendMessage("Cured: {0}", pet.Name);
                    }
                    processed++;
                }

                int healTargets = Math.Min(toHeal.Count, bandages.Amount - processed);
                if (healTargets > 0)
                {
                    int pool = (int)((druid / 2.0) + (vety / 2.0) + Utility.RandomMinMax(50, 100));
                    int each = pool / healTargets;

                    for (int i = 0; i < healTargets; i++)
                    {
                        BaseCreature pet = toHeal[i];
                        pet.Heal(each + (pet.HitsMax / 100), from, true);
                        pet.FixedEffect(0x3728, 10, 15);
                        processed++;
                    }
                }

                bandages.Consume(processed);
                from.PlaySound(0x57);
                
                TimeSpan delay = TimeSpan.FromSeconds(4.0);
                m_NextUse = DateTime.UtcNow + delay;
                if (m_CooldownTimer != null) m_CooldownTimer.Stop();
                m_CooldownTimer = new InternalTimer(from, delay);
                m_CooldownTimer.Start();

                this.InvalidateProperties(); 
            }
        }

        private class InternalTimer : Timer
        {
            private Mobile m_From;
            public InternalTimer(Mobile from, TimeSpan delay) : base(delay)
            {
                m_From = from;
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                if (m_From != null && !m_From.Deleted)
                {
                    // Visual/Audio feedback for cooldown finish
                    m_From.PlaySound(0x1F2);
                    m_From.SendMessage(0x3F, "Your satchel is repacked and ready.");
                }
            }
        }

        public TamersSatchel(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}
