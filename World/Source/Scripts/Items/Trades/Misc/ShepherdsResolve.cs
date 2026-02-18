using System;
using System.Collections.Generic;
using Server;
using Server.Mobiles;
using Server.Targeting;

namespace Server.Items
{
    public class TamersCrook : ShepherdsCrook
    {
        private DateTime m_NextUse;
        private bool m_IsActive; // Track if a target is currently being hunted

        [Constructable]
        public TamersCrook()
        {
            Name = "Shepherd's Resolve";
            Hue = 2213; 
            Layer = Layer.TwoHanded;
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (Parent != from)
            {
                from.SendMessage("You must be wielding the staff to command your flock.");
                return;
            }

            if (m_IsActive)
            {
                from.SendMessage("The pack is already focused on a prey. Wait for the hunt to end.");
                return;
            }

            if (DateTime.UtcNow < m_NextUse)
            {
                from.SendMessage("The staff is still recovering its resonance.");
                return;
            }

            from.SendMessage("Mark the prey for the pack.");
            from.Target = new VigilTarget(this);
        }

        private class VigilTarget : Target
        {
            private TamersCrook m_Staff;

            public VigilTarget(TamersCrook staff) : base(12, false, TargetFlags.Harmful)
            {
                m_Staff = staff;
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is Mobile && targeted != from)
                {
                    Mobile enemy = (Mobile)targeted;
                    List<BaseCreature> pack = new List<BaseCreature>();

                    foreach (Mobile m in from.GetMobilesInRange(8))
                    {
                        if (m is BaseCreature pet && pet.Controlled && pet.ControlMaster == from)
                            pack.Add(pet);
                    }

                    if (pack.Count > 0)
                    {
                        double skillTotal = from.Skills[SkillName.Herding].Value + 
                                           from.Skills[SkillName.Druidism].Value + 
                                           from.Skills[SkillName.Taming].Value;

                        int bonusAmount = (int)(skillTotal / 15) + (pack.Count * 2);
                        m_Staff.m_IsActive = true;

                        foreach (BaseCreature pet in pack)
                        {
                            pet.FixedEffect(0x373A, 10, 15);
                            pet.PlaySound(0x1FB);

                            SkillMod tacticsMod = new DefaultSkillMod(SkillName.Tactics, true, bonusAmount);
                            pet.AddSkillMod(tacticsMod);

                            SkillMod psychMod = null;
                            if (pet.Skills[SkillName.Psychology].Value > 0)
                            {
                                psychMod = new DefaultSkillMod(SkillName.Psychology, true, bonusAmount);
                                pet.AddSkillMod(psychMod);
                            }

                            pet.ControlTarget = enemy;
                            pet.ControlOrder = OrderType.Attack;

                            // Pass the staff reference to the timer to clear m_IsActive and set the 10s cooldown
                            new VigilMonitorTimer(pet, enemy, tacticsMod, psychMod, from, m_Staff).Start();
                        }

                        from.PlaySound(0x512);
                        from.PublicOverheadMessage(Server.Network.MessageType.Regular, 0x3B2, false, String.Format("*The pack marks {0} as prey!*", enemy.Name));
                        
                        // Fallback: If hunt fails to conclude naturally, 2 minute hard cooldown
                        m_Staff.m_NextUse = DateTime.UtcNow + TimeSpan.FromMinutes(2.0);
                    }
                    else
                    {
                        from.SendMessage("There are no pets nearby to rally.");
                    }
                }
            }
        }

        private class VigilMonitorTimer : Timer
        {
            private BaseCreature m_Pet;
            private Mobile m_Enemy;
            private SkillMod m_TacticsMod;
            private SkillMod m_PsychMod;
            private Mobile m_Tamer;
            private TamersCrook m_Staff;
            private int m_Ticks;

            public VigilMonitorTimer(BaseCreature pet, Mobile enemy, SkillMod t, SkillMod p, Mobile tamer, TamersCrook staff) 
                : base(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0))
            {
                m_Pet = pet;
                m_Enemy = enemy;
                m_TacticsMod = t;
                m_PsychMod = p;
                m_Tamer = tamer;
                m_Staff = staff;
                m_Ticks = 0;
            }

            protected override void OnTick()
            {
                m_Ticks++;

                // End conditions: Target dead, Pet dead, or too much time passed (2 mins)
                bool enemyDead = (m_Enemy == null || m_Enemy.Deleted || !m_Enemy.Alive);
                bool abort = (m_Pet == null || m_Pet.Deleted || !m_Pet.Alive || m_Ticks > 120);

                if (enemyDead || abort)
                {
                    if (m_Pet != null && !m_Pet.Deleted)
                    {
                        m_Pet.RemoveSkillMod(m_TacticsMod);
                        if (m_PsychMod != null) m_Pet.RemoveSkillMod(m_PsychMod);
                        
                        if (m_Tamer != null)
                            m_Tamer.SendMessage("The frenzy has ended for {0}.", m_Pet.Name);
                    }

                    if (m_Staff != null)
                    {
                        m_Staff.m_IsActive = false;
                        
                        // If the enemy actually died, give the 10 second fast-cooldown
                        if (enemyDead)
                        {
                            m_Staff.m_NextUse = DateTime.UtcNow + TimeSpan.FromSeconds(10.0);
                            if (m_Tamer != null)
                                m_Tamer.SendMessage("Prey defeated. The staff will be ready in 10 seconds.");
                        }
                    }
                    Stop();
                }
            }
        }

        public TamersCrook(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)0); }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); reader.ReadInt(); }
    }
}
