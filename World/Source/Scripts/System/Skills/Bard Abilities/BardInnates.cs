using System;
using Server;
using Server.Mobiles;

namespace Server.Items
{
    public class BardicInnate
    {
        public static void CheckProc(Mobile bard, Mobile target, SkillName bardSkill)
        {
            double skillValue = bard.Skills[bardSkill].Value;
            double musicValue = bard.Skills[SkillName.Musicianship].Value;
            // --- SUCCESS RATE LOGIC ---
            // Start with a flat 10% (0.10)
            // Add Music skill bonus: (Music / 300.0) 
            // At 120 Music, (120/300) = 0.40. 
            // Total: 0.10 + 0.40 = 0.50 (50% Chance)
            double successChance = 0.10 + (musicValue / 300.0);
            if (Utility.RandomDouble() < (skillValue / 480.0))
            {
                switch (bardSkill)
                {
                    case SkillName.Peacemaking:
                        ApplyBullseye(bard, target, musicValue);
                        break;
                    case SkillName.Provocation:
                        ApplyCacophony(bard, target, musicValue);
                        break;
                    case SkillName.Discordance:
                        ApplyEchoOfPain(bard, target, musicValue);
                        break;
                }
            }
        }

        // PEACEMAKING: Bullseye (Damage Buff)
        private static void ApplyBullseye(Mobile bard, Mobile target, double music)
        {
            if (target is BaseCreature)
            {
                BaseCreature bc = (BaseCreature)target;
                // POWER: Music determines how much skill is stripped
                // 120 Music / 5.0 = 24 points stripped from all combat skills
                double penalty = (music / 5.0);
                // DURATION: Music determines how long the entrance lasts
                double seconds = 10.0 + (music / 4.0); // 120 Music = 40 Seconds
                TimeSpan duration = TimeSpan.FromSeconds(seconds);
                // Store old values to restore them later
                double oldFencing = bc.Skills[SkillName.Fencing].Base;
                double oldParry = bc.Skills[SkillName.Parry].Base;
                double oldSwords = bc.Skills[SkillName.Swords].Base;
                double oldMarksmanship = bc.Skills[SkillName.Marksmanship].Base;
                double oldFistFighting = bc.Skills[SkillName.FistFighting].Base;
                double oldBludgeoning = bc.Skills[SkillName.Bludgeoning].Base;
                // Apply Debuffs
                bc.Skills[SkillName.Fencing].Base -= penalty;
                bc.Skills[SkillName.Parry].Base -= penalty;
                bc.Skills[SkillName.Swords].Base -= penalty;
                bc.Skills[SkillName.Marksmanship].Base -= penalty;
                bc.Skills[SkillName.FistFighting].Base -= penalty;
                bc.Skills[SkillName.Bludgeoning].Base -= penalty;
                // Lower the creature's overall power level
                bc.BardLevelScalar = 0.90;
                // Restore Timer
                Timer.DelayCall(duration, delegate
                {
                    if (bc != null && !bc.Deleted)
                    {
                        bc.Skills[SkillName.Fencing].Base = oldFencing;
                        bc.Skills[SkillName.Parry].Base = oldParry;
                        bc.Skills[SkillName.Swords].Base = oldSwords;
                        bc.Skills[SkillName.Marksmanship].Base = oldMarksmanship;
                        bc.Skills[SkillName.FistFighting].Base = oldFistFighting;
                        bc.Skills[SkillName.Bludgeoning].Base = oldBludgeoning;
                        bc.BardLevelScalar = 1.0;
                    }
                });
                if (bc.BardLevelScalar < 1.0)
                {
                    bard.SendMessage("You refine your song, maintaining the creature's entrance!");
                }
                else
                {
                    bard.SendMessage("The target is calmed and now easier to hit!");
                }
                bc.FixedParticles(0x377A, 1, 32, 0x1536, 0x480, 2, EffectLayer.Waist);
            }
        }

        // PROVOCATION: Cacophony (Self induced Pain)
        private static void ApplyCacophony(Mobile bard, Mobile target, double music)
        {    
            double seconds = 10.0 + (music / 5.0); 
            target.BeginAction("Cacophony");
            Timer.DelayCall(TimeSpan.FromSeconds(seconds), delegate { target.EndAction("Cacophony"); });
            bard.SendMessage("The creature falls into a confused frenzy, striking at its own shadows!");
            target.FixedParticles(0x377A, 1, 32, 0x1536, 0x480, 2, EffectLayer.Waist);
        }
        // PROVOCATION: The Damage Logic (Called from BaseCreature.cs)
        public static void CheckCacophony(Mobile victim, int damage)
        {
            // 20% chance when damaged that the monster hits itself
            if (Utility.RandomDouble() < 0.50)
            {
                Mobile attacker = victim.Combatant;
                if (attacker == victim)
                    return;
                // Calculate the "Self-Hit" damage (25% of incoming + 5 flat)
                int selfDamage = (int)(damage * 0.50) + 5; 
                // Apply damage to itself. Using 'victim' as both source and target.
                AOS.Damage(victim, victim, selfDamage, false, 0, 0, 0, 0, 0, 0, 100, false, false, false );
                //victim.Damage(selfDamage, victim); 
                victim.PlaySound(0x11D); 
                victim.FixedEffect(0x37B9, 10, 5);
                victim.PublicOverheadMessage(Server.Network.MessageType.Regular, 0x3B, false, "*Strikes itself in confusion!*");
            }
        }
        // DISCORDANCE: Echo of Pain (DoT)
        private static void ApplyEchoOfPain(Mobile bard, Mobile target, double music)
        {
            // DAMAGE & TIMESPAN: Music scales the "Ringing Ear Damage" ticks
            int tickDamage = (int)(music / 40.0); // 120 Music = 6 damage per tick
            
            Timer.DelayCall(TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(1.0), 5, delegate
            {
                if (target.Alive)
                {
                    AOS.Damage(target, bard, tickDamage, false, 0, 0, 0, 0, 0, 0, 100, false, false, false);
                }
            });

            bard.SendMessage("You leave the targets ears ringing in pain!");
        }
    }
}
