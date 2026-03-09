using System;
using Server;
using Server.Items;
using Server.Commands;

namespace Server.Commands
{
    public class ScrollCommands
    {
        public static void Initialize()
        {
            CommandSystem.Register("GiveMageScrolls", AccessLevel.GameMaster, new CommandEventHandler(GiveMage_OnCommand));
            CommandSystem.Register("GiveNecroScrolls", AccessLevel.GameMaster, new CommandEventHandler(GiveNecro_OnCommand));
            CommandSystem.Register("GiveElemScrolls", AccessLevel.GameMaster, new CommandEventHandler(GiveElem_OnCommand));
            CommandSystem.Register("GiveBardScrolls", AccessLevel.GameMaster, new CommandEventHandler(GiveBard_OnCommand));
        }

        [Usage("GiveMageScrolls")]
        public static void GiveMage_OnCommand(CommandEventArgs e)
        {
            for (int i = 0; i < 64; i++)
                e.Mobile.AddToBackpack(new SpellScroll(i, 0x1F2E + i));
            e.Mobile.SendMessage("Magery scrolls delivered.");
        }

        [Usage("GiveNecroScrolls")]
        public static void GiveNecro_OnCommand(CommandEventArgs e)
        {
            for (int i = 100; i <= 116; i++)
                e.Mobile.AddToBackpack(new SpellScroll(i, 0x2260 + (i - 100)));
            e.Mobile.SendMessage("Necromancy scrolls delivered.");
        }

[Usage("GiveElemScrolls")]
public static void GiveElem_OnCommand(CommandEventArgs e)
{
    string[] elemScrolls = new string[] { 
        "Elemental_Armor_Scroll", "Elemental_Bolt_Scroll", "Elemental_Mend_Scroll", "Elemental_Sanctuary_Scroll",
        "Elemental_Pain_Scroll", "Elemental_Protection_Scroll", "Elemental_Purge_Scroll", "Elemental_Steed_Scroll",
        "Elemental_Call_Scroll", "Elemental_Force_Scroll", "Elemental_Wall_Scroll", "Elemental_Warp_Scroll",
        "Elemental_Field_Scroll", "Elemental_Restoration_Scroll", "Elemental_Strike_Scroll", "Elemental_Void_Scroll",
        "Elemental_Blast_Scroll", "Elemental_Echo_Scroll", "Elemental_Fiend_Scroll", "Elemental_Hold_Scroll",
        "Elemental_Barrage_Scroll", "Elemental_Rune_Scroll", "Elemental_Storm_Scroll", "Elemental_Summon_Scroll",
        "Elemental_Devastation_Scroll", "Elemental_Fall_Scroll", "Elemental_Gate_Scroll", "Elemental_Havoc_Scroll",
        "Elemental_Apocalypse_Scroll", "Elemental_Lord_Scroll", "Elemental_Soul_Scroll", "Elemental_Spirit_Scroll"
    };

    int count = 0;
    foreach (string name in elemScrolls)
    {
        Item item = Reconstruct(name);
        if (item != null)
        {
            e.Mobile.AddToBackpack(item);
            count++;
        }
    }
    e.Mobile.SendMessage("Elementalism scrolls delivered: {0} found.", count);
}
        [Usage("GiveBardScrolls")]
        public static void GiveBard_OnCommand(CommandEventArgs e)
        {
            // This array uses the Class Names for the Barding Songs
            string[] bardScrolls = new string[] { 
                "ArmysPaeonScroll", "EnchantingEtudeScroll", "EnergyCarolScroll", "EnergyThrenodyScroll",
                "FireCarolScroll", "FireThrenodyScroll", "FoeRequiemScroll", "IceCarolScroll",
                "IceThrenodyScroll", "KnightsMinneScroll", "MagesBalladScroll", "MagicFinaleScroll",
                "PoisonCarolScroll", "PoisonThrenodyScroll", "SheepfoeMamboScroll", "SinewyEtudeScroll"
            };

            foreach (string name in bardScrolls)
            {
                Item item = Reconstruct(name);
                if (item != null) e.Mobile.AddToBackpack(item);
            }
            e.Mobile.SendMessage("Bardic Song scrolls delivered.");
        }

        private static Item Reconstruct(string name)
        {
            Type t = ScriptCompiler.FindTypeByName(name);
            if (t != null)
                return (Item)Activator.CreateInstance(t);
            return null;
        }
    }
}
