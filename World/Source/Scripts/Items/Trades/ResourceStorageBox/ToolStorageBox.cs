using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Items
{
    public class ToolStorageBox : Container
    {
        private Dictionary<Type, int> m_ToolUses;
        public Dictionary<Type, int> ToolUses => m_ToolUses;

        [Constructable]
        public ToolStorageBox() : base(0x9AA) // Wooden Box graphic
        {
            Name = "Tool Storage Box";
            Hue = 1161; // Teal-ish hue to distinguish from resources
            Weight = 20.0;
            m_ToolUses = new Dictionary<Type, int>();
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2)) return;
            
            if (!IsLockedDown && !IsSecure && from.AccessLevel == AccessLevel.Player)
            {
                from.SendMessage("The tool ledger must be locked down or secured to use.");
                return;
            }

            from.SendGump(new ToolStorageGump(from, this));
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped is BaseTool tool)
            {
                Type toolType = tool.GetType();
                int uses = tool.UsesRemaining;

                if (m_ToolUses.ContainsKey(toolType))
                    m_ToolUses[toolType] += uses;
                else
                    m_ToolUses[toolType] = uses;

                from.SendMessage("Stored {0} uses for {1}.", uses, dropped.Name ?? toolType.Name);
                dropped.Delete();
                from.PlaySound(0x249); // Tool clink sound
                if (from.HasGump(typeof(ToolStorageGump)))
                {
                    // To refresh, we close the old one and send a new one
                    // We don't need to track the page here as it will default to page 0 
                    // which is usually where a player wants to see their new additions.
                    from.CloseGump(typeof(ToolStorageGump));
                    from.SendGump(new ToolStorageGump(from, this));
                }
                return true;
            }
            from.SendMessage("This box only accepts crafting tools.");
            return false;
        }
        public void FillFromBackpack(Mobile from)
        {
            if (!IsLockedDown && !IsSecure && from.AccessLevel == AccessLevel.Player)
            {
                from.SendMessage("The tool ledger must be locked down or secured to use.");
                return;
            }

            Container pack = from.Backpack;
            if (pack == null) return;

            int count = 0;
            for (int i = pack.Items.Count - 1; i >= 0; --i)
            {
                if (i >= pack.Items.Count) continue;

                Item item = pack.Items[i];

                if (item is BaseTool tool)
                {
                    Type toolType = tool.GetType();
                    int uses = tool.UsesRemaining;

                    if (m_ToolUses.ContainsKey(toolType))
                        m_ToolUses[toolType] += uses;
                    else
                        m_ToolUses[toolType] = uses;

                    count++;
                    item.Delete();
                }
            }

            if (count > 0)
            {
                from.SendMessage("Stored uses from {0} tools in your backpack.", count);
                from.PlaySound(0x249);
            }
            else
            {
                from.SendMessage("No crafting tools were found in your backpack.");
            }
        }
        public void Withdraw(Mobile from, Type type, int amount)
        {
            if (m_ToolUses.ContainsKey(type) && m_ToolUses[type] >= amount)
            {
                BaseTool tool = Activator.CreateInstance(type) as BaseTool;
                if (tool != null)
                {
                    tool.UsesRemaining = amount;
                    m_ToolUses[type] -= amount;

                    if (m_ToolUses[type] <= 0)
                        m_ToolUses.Remove(type);

                    from.AddToBackpack(tool);
                    from.SendMessage("You withdrew a tool with {0} uses.", amount);
                }
            }
            else
            {
                from.SendMessage("There aren't enough uses stored for that tool.");
            }
        }

        public ToolStorageBox(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1);
            writer.Write(m_ToolUses.Count);
            foreach (KeyValuePair<Type, int> entry in m_ToolUses)
            {
                writer.Write(entry.Key.FullName);
                writer.Write(entry.Value);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_ToolUses = new Dictionary<Type, int>();
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                string typeName = reader.ReadString();
                int uses = reader.ReadInt();
                Type toolType = Type.GetType(typeName);
                if (toolType != null) m_ToolUses[toolType] = uses;
            }
        }
    }
}
