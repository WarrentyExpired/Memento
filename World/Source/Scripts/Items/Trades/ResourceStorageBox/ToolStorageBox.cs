using System;
using System.Collections.Generic;
using Server;
using Server.Items;
namespace Server.Items
{
    public class ToolStorageBox : Container, IStorageBox
    {
        private Dictionary<Type, int> m_ToolUses;
        public Dictionary<Type, int> ToolUses => m_ToolUses;
        public string BoxTitle => "Tool Storage Box";
        public Dictionary<Type, int> GetStorage() { return m_ToolUses; }
        [Constructable]
        public ToolStorageBox() : base(0x9A9)
        {
            Name = "Tool Storage Box";
            Hue = 1161;
            Weight = 20.0;
            m_ToolUses = new Dictionary<Type, int>();
        }
        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2)) return;
            
            if (!IsLockedDown && !IsSecure && from.AccessLevel == AccessLevel.Player)
            {
                from.SendMessage("The tool storage box must be locked down or secured to use.");
                return;
            }
            from.SendGump(new ResourceStorageGump(from, this));
        }
        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (dropped is BaseTool tool)
            {
                Type toolType = tool.GetType();
                int uses = tool.UsesRemaining;

                if (m_ToolUses.ContainsKey(toolType)) m_ToolUses[toolType] += uses;
                else m_ToolUses[toolType] = uses;

                from.SendMessage("Stored {0} uses for {1}.", uses, dropped.Name ?? toolType.Name);
                dropped.Delete();
                from.PlaySound(0x249);
                if (from.HasGump(typeof(ResourceStorageGump)))
                {
                    from.CloseGump(typeof(ResourceStorageGump));
                    from.SendGump(new ResourceStorageGump(from, this));
                }
                return true;
            }
            return false;
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
                    if (m_ToolUses[type] <= 0) m_ToolUses.Remove(type);
                    from.AddToBackpack(tool);
                }
            }
        }
        public void FillFromBackpack(Mobile from)
        {
            Container pack = from.Backpack;
            if (pack == null) return;
            int count = 0;
            for (int i = pack.Items.Count - 1; i >= 0; --i)
            {
                Item item = pack.Items[i];
                if (item is BaseTool tool)
                {
                    int uses = tool.UsesRemaining;
                    Type t = tool.GetType();
                    if (m_ToolUses.ContainsKey(t)) m_ToolUses[t] += uses;
                    else m_ToolUses[t] = uses;
                    count++;
                    item.Delete();
                }
            }
            if (count > 0) from.PlaySound(0x249);
        }
        public ToolStorageBox(Serial serial) : base(serial) { }
        public override void Serialize(GenericWriter writer) { base.Serialize(writer); writer.Write((int)1); writer.Write(m_ToolUses.Count); foreach (var e in m_ToolUses) { writer.Write(e.Key.FullName); writer.Write(e.Value); } }
        public override void Deserialize(GenericReader reader) { base.Deserialize(reader); int v = reader.ReadInt(); m_ToolUses = new Dictionary<Type, int>(); int c = reader.ReadInt(); for (int i = 0; i < c; i++) { Type t = Type.GetType(reader.ReadString()); int u = reader.ReadInt(); if (t != null) m_ToolUses[t] = u; } }
    }
}
