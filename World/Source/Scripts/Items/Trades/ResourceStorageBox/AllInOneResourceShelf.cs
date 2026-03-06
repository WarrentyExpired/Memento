using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Items
{
    public class AllInOneResourceShelf : Container
    {
        private Dictionary<Type, int> m_Metal = new Dictionary<Type, int>();
        private Dictionary<Type, int> m_Wood = new Dictionary<Type, int>();
        private Dictionary<Type, int> m_Leather = new Dictionary<Type, int>();
        private Dictionary<Type, int> m_Cloth = new Dictionary<Type, int>();
        private Dictionary<Type, int> m_Arcane = new Dictionary<Type, int>();
        private Dictionary<Type, int> m_Tools = new Dictionary<Type, int>(); // New Tool Storage

        public Dictionary<Type, int> Metal => m_Metal;
        public Dictionary<Type, int> Wood => m_Wood;
        public Dictionary<Type, int> Leather => m_Leather;
        public Dictionary<Type, int> Cloth => m_Cloth;
        public Dictionary<Type, int> Arcane => m_Arcane;
        public Dictionary<Type, int> Tools => m_Tools; // Accessor for the Gump

        [Constructable]
        public AllInOneResourceShelf() : base(0x3CC6)
        {
            Name = "Grand Resource Repository";
            Hue = 1150;
            Weight = 50.0;
            LootType = LootType.Regular; 
        }

        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2)) return;
            
            if (IsLockedDown || IsSecure || from.AccessLevel > AccessLevel.Player)
                from.SendGump(new MasterResourceHubGump(from, this));
            else
                from.SendMessage("This repository must be locked down or secured to use.");
        }

        public void Withdraw(Mobile from, Dictionary<Type, int> targetDict, Type type, int amount)
        {
            if (targetDict != null && targetDict.ContainsKey(type))
            {
                if (targetDict[type] < amount)
                    amount = targetDict[type];

                Item item = Activator.CreateInstance(type) as Item;
                if (item != null)
                {
                    // Logic for Tools vs Resources
                    if (item is BaseTool tool)
                        tool.UsesRemaining = amount;
                    else
                    targetDict[type] -= amount;
                    if (targetDict[type] <= 0) targetDict.Remove(type);
                    
                    from.AddToBackpack(item);
                    from.SendMessage("You withdraw {0} from the repository.", item is BaseTool ? "the tool" : amount.ToString());
                }
            }
        }

        public void DepositAll(Mobile from)
        {
            Container pack = from.Backpack;
            if (pack == null) return;

            int count = 0;
            for (int i = pack.Items.Count - 1; i >= 0; --i)
            {
                Item item = pack.Items[i];
                if (item != null && StoreItem(item))
                {
                    count++;
                    item.Delete();
                }
            }
            if (count > 0)
            {
                from.SendMessage("Added {0} items to the Grand Repository.", count);
                from.PlaySound(0x42);
            }
        }

        private bool StoreItem(Item item)
        {
            Dictionary<Type, int> dict = null;
            int amountToAdd = item.Amount;

            if (item is BaseIngot) dict = m_Metal;
            else if (item is BaseWoodBoard || item is Log || item is Shaft) dict = m_Wood;
            else if (item is BaseLeather) dict = m_Leather;
            else if (item is BaseFabric) dict = m_Cloth;
            else if (IsArcane(item)) dict = m_Arcane;
            else if (item is BaseTool tool) 
            { 
                dict = m_Tools; 
                amountToAdd = tool.UsesRemaining; // Store uses, not stack amount
            }

            if (dict != null)
            {
                Type t = item.GetType();
                if (dict.ContainsKey(t)) dict[t] += amountToAdd;
                else dict[t] = amountToAdd;
                return true;
            }
            return false;
        }

        private bool IsArcane(Item item)
        {
            return (item is BaseReagent || item is Bottle || item is BlankScroll);
        }

        public AllInOneResourceShelf(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)1); // Increased version
            WriteDict(writer, m_Metal);
            WriteDict(writer, m_Wood);
            WriteDict(writer, m_Leather);
            WriteDict(writer, m_Cloth);
            WriteDict(writer, m_Arcane);
            WriteDict(writer, m_Tools); // Save Tools
        }

        private void WriteDict(GenericWriter writer, Dictionary<Type, int> dict)
        {
            writer.Write(dict.Count);
            foreach (var entry in dict) { writer.Write(entry.Key.FullName); writer.Write(entry.Value); }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_Metal = ReadDict(reader);
            m_Wood = ReadDict(reader);
            m_Leather = ReadDict(reader);
            m_Cloth = ReadDict(reader);
            m_Arcane = ReadDict(reader);
            if (version >= 1) m_Tools = ReadDict(reader); // Load Tools
        }

        private Dictionary<Type, int> ReadDict(GenericReader reader)
        {
            var dict = new Dictionary<Type, int>();
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                string typeName = reader.ReadString();
                int amt = reader.ReadInt();
                Type t = Type.GetType(typeName);
                if (t != null) dict[t] = amt;
            }
            return dict;
        }
    }
}
