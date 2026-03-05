using System;
using System.Collections.Generic;
using Server;
using Server.Items;

namespace Server.Items
{
    public abstract class BaseResourceBox : Item
    {
        private Dictionary<Type, int> m_Resources;
        public Dictionary<Type, int> Resources => m_Resources;

        // Each box will define what it's allowed to hold
        public abstract bool IsAllowed(Item item);
        public abstract string BoxTitle { get; }

        public BaseResourceBox(int itemID) : base(itemID)
        {
            Weight = 20.0;
            LootType = LootType.Blessed;
            m_Resources = new Dictionary<Type, int>();
        }
        public bool CheckAccessible(Mobile from)
        {
            // If the box is locked down or secured, it is accessible
            if (IsLockedDown || IsSecure)
                return true;
            from.SendMessage("The storage box must be locked down or secured in a house to use.");
            return false;
        }
        public override void OnDoubleClick(Mobile from)
        {
            if (!from.InRange(GetWorldLocation(), 2)) 
              return;
            if (!CheckAccessible(from))
              return;
            from.SendGump(new ResourceStorageGump(from, this));
        }

        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (!CheckAccessible(from))
                return false;
            if (IsAllowed(dropped))
            {
                Type resType = dropped.GetType();
                int amount = dropped.Amount;

                if (m_Resources.ContainsKey(resType)) m_Resources[resType] += amount;
                else m_Resources[resType] = amount;

                from.SendMessage("Stored {0} units.", amount);
                dropped.Delete();
                from.PlaySound(0x42);
                return true;
            }

            from.SendMessage("That doesn't belong in this storage box.");
            return false;
        }

        public void Withdraw(Mobile from, Type type, int amount)
        {
            if (m_Resources.ContainsKey(type) && m_Resources[type] >= amount)
            {
                Item resource = Activator.CreateInstance(type) as Item;
                if (resource != null)
                {
                    if (amount > 60000) amount = 60000;
                    resource.Amount = amount;
                    m_Resources[type] -= amount;
                    if (m_Resources[type] <= 0) m_Resources.Remove(type);
                    from.AddToBackpack(resource);
                }
            }
        }

        public BaseResourceBox(Serial serial) : base(serial) { }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
            writer.Write(m_Resources.Count);
            foreach (KeyValuePair<Type, int> entry in m_Resources)
            {
                writer.Write(entry.Key.FullName);
                writer.Write(entry.Value);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
            m_Resources = new Dictionary<Type, int>();
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                string typeName = reader.ReadString();
                int amount = reader.ReadInt();
                Type resType = Type.GetType(typeName);
                if (resType != null) m_Resources[resType] = amount;
            }
        }
    }
}
