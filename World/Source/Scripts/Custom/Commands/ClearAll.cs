using System;
using System.Collections.Generic;
using Server;
using Server.Commands;
using Server.Mobiles;
using Server.Items;

namespace Server.Commands
{
    public class ClearAllCommand
    {
        public static void Initialize()
        {
            // Registers the [ClearAll command. Requires Administrator access.
            CommandSystem.Register("ClearAll", AccessLevel.Administrator, new CommandEventHandler(ClearAll_OnCommand));
        }

        [Usage("ClearAll")]
        [Description("Deletes all items and mobiles across the first 5 maps.")]
        public static void ClearAll_OnCommand(CommandEventArgs e)
        {
            Mobile from = e.Mobile;
            from.SendMessage("Starting global map wipe. Please wait...");

            List<Item> itemsToDelete = new List<Item>();
            List<Mobile> mobilesToDelete = new List<Mobile>();

            // Find all items on maps 0 through 4
            foreach (Item item in World.Items.Values)
            {
                if (item.Map != null && item.Map.MapID >= 0 && item.Map.MapID < 5 && item.Parent == null)
                {
                    itemsToDelete.Add(item);
                }
            }

            // Find all mobiles on maps 0 through 4
            foreach (Mobile m in World.Mobiles.Values)
            {
                if (m.Map != null && m.Map.MapID >= 0 && m.Map.MapID < 5 && m is BaseCreature && !m.Player)
                {
                    mobilesToDelete.Add(m);
                }
            }

            int itemsDeleted = itemsToDelete.Count;
            int mobilesDeleted = mobilesToDelete.Count;

            // Delete gathered items
            for (int i = 0; i < itemsToDelete.Count; i++)
            {
                itemsToDelete[i].Delete();
            }

            // Delete gathered mobiles
            for (int i = 0; i < mobilesToDelete.Count; i++)
            {
                mobilesToDelete[i].Delete();
            }

            from.SendMessage($"Wipe complete! Cleared maps 0-4. Deleted {itemsDeleted} items and {mobilesDeleted} creatures.");
        }
    }
}
