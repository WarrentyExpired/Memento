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
            Name = "Shepherd's Resolve";
            Hue = 2129; 
            MaxItems = 20; 
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);
            
            int bandages = 0;
            int foodCount = 0;

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
            if (item is Bandage || item is Food)
            {
                return base.CheckHold(m, item, message, checkItems, plusItems, plusWeight);
            }

            if (message)
                m.SendMessage("The satchel only accepts medical supplies and pet food.");

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

            bool fed = FeedPets(from);
            bool healed = PerformMedical(from);

            if (!fed && !healed)
            {
                from.SendMessage("There are no pets nearby in need of food or medical attention.");
            }
        }

        private bool FeedPets(Mobile from)
        {
            List<BaseCreature> pets = new List<BaseCreature>();
            
            foreach (Mobile m in from.GetMobilesInRange(3))
            {
                if (m is BaseCreature pet && pet.Controlled && pet.ControlMaster == from)
                {
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
                            pet.Loyalty = 100; 
                            pet.PlaySound(0x3B);
                            food.Consume(1);
                            from.SendMessage("You feed {0}. They are now Wonderfully Happy!", pet.Name);
                        }
                    }
                    this.InvalidateProperties(); 
                    return true;
                }
                else
                {
                    from.SendMessage("Some of your pets are hungry, but there is no food in the satchel.");
                    return true; 
                }
            }
            return false;
        }

        private bool PerformMedical(Mobile from)
        {
            List<BaseCreature> toCure = new List<BaseCreature>();
            List<BaseCreature> toHeal = new List<BaseCreature>();
            bool foundWounded = false;

            int range = Bandage.Range; 

            foreach (Mobile m in from.GetMobilesInRange(18))
            {
                if (m is BaseCreature pet && pet.Controlled && pet.ControlMaster == from)
                {
                    if (pet.Poisoned || pet.Hits < pet.HitsMax)
                    {
                        foundWounded = true;

                        if (IsBeingBandaged(pet))
                        {
                            from.SendMessage("{0} is already being tended to by a manual bandage.", pet.Name);
                            continue; 
                        }

                        if (from.InRange(pet, range))
                        {
                            if (pet.Poisoned) toCure.Add(pet);
                            else toHeal.Add(pet);
                        }
                    }
                }
            }

            if (toCure.Count > 0 || toHeal.Count > 0)
            {
                Item bandages = this.FindItemByType(typeof(Bandage));
                if (bandages != null && bandages.Amount > 0)
                {
                    ApplyMedicalEffects(from, bandages, toCure, toHeal);
                }
                else
                {
                    from.SendMessage("The satchel lacks bandages for medical treatment.");
                }
                return true;
            }

            return foundWounded;
        }

        private void ApplyMedicalEffects(Mobile from, Item bandages, List<BaseCreature> toCure, List<BaseCreature> toHeal)
        {
            int processed = 0;
            double vety = from.Skills[SkillName.Veterinary].Value;
            double druid = from.Skills[SkillName.Druidism].Value;

            // Notification for curing
            foreach (BaseCreature pet in toCure)
            {
                if (processed >= bandages.Amount) break;
                if ( vety < 60.0 )
                {
                  from.SendMessage("You lack the Veterinary skill required to cure {0}'s poison.", pet.Name);
                }
                else
                {
                  double chance = (vety /  2.0) + (druid / 2.0);
                  if (chance >= ((pet.Poison.Level * 20) + 10))
                  {
                    pet.CurePoison(from);
                    pet.FixedEffect(0x373A, 10, 15);
                    from.SendMessage("Cured: {0}", pet.Name);
                  }
                  else
                  {
                    from.SendMessage("You failed to cure {0}'s poison.", pet.Name);
                  }
                }
                processed++;
            }

            // Notification for healing
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
                    from.SendMessage("The satchel's medical supplies have healed {0}.", pet.Name);
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

        private bool IsBeingBandaged(Mobile patient)
        {
            try
            {
                var field = typeof(BandageContext).GetField("m_Table", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                if (field != null)
                {
                    var table = field.GetValue(null) as Dictionary<Mobile, BandageContext>;
                    if (table != null)
                    {
                        foreach (BandageContext context in table.Values)
                        {
                            var patientField = typeof(BandageContext).GetField("m_Patient", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                            if (patientField != null && patientField.GetValue(context) == patient)
                                return true;
                        }
                    }
                }
            }
            catch { }
            return false;
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
