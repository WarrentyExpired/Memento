using System;
using System.Collections.Generic;
using Server;
using Server.Items;
namespace Server.Items
{
    public interface IStorageBox
    {
        string BoxTitle { get; }
        Dictionary<Type, int> GetStorage();
        void Withdraw(Mobile from, Type type, int amount);
    }
    public abstract class BaseResourceBox : Container, IStorageBox
    {
        private Dictionary<Type, int> m_Resources;
        public Dictionary<Type, int> Resources => m_Resources;
        public Dictionary<Type, int> GetStorage() { return m_Resources; }
        public abstract bool IsAllowed(Item item);
        public abstract string BoxTitle { get; }
        public BaseResourceBox(int itemID) : base(itemID)
        {
            Weight = 20;
            m_Resources = new Dictionary<Type, int>();
        }
        public static int GetRarityValue(Type type)
        {
            string name = type.Name;
            string[] rarityOrder = new string[]
            {
                //Metal
                "IronIngot", "DullCopperIngot", "ShadowIronIngot", "CopperIngot", "BronzeIngot", 
                "GoldIngot", "AgapiteIngot", "VeriteIngot", "ValoriteIngot", "NepturiteIngot", 
                "ObsidianIngot", "SteelIngot", "BrassIngot", "MithrilIngot", "XormiteIngot", "DwarvenIngot",
                // Leather
                "Leather", "HornedLeather", "BarbedLeather", "NecroticLeather", "VolcanicLeather", 
                "FrozenLeather", "SpinedLeather", "GoliathLeather", "DraconicLeather", "HellishLeather",
                "DinosaurLeather", "AlienLeather",
                // Wood
                "Board", "AshBoard", "CherryBoard", "EbonyBoard", "GoldenOakBoard", 
                "HickoryBoard", "MahoganyBoard", "OakBoard", "PineBoard", "GhostBoard", 
                "RosewoodBoard", "WalnutBoard", "PetrifiedBoard", "DriftwoodBoard", "ElvenBoard",
                // Fabric
                "Fabric", "FurryFabric", "WoolyFabric", "SilkFabric", "HauntedFabric", 
                "ArcticFabric", "PyreFabric", "VenomousFabric", "MysteriousFabric", "VileFabric", 
                "DivineFabric", "FiendishFabric",
                // Bone
                "BrittleSkeletal", "DrowSkeletal", "OrcSkeletal", "ReptileSkeletal", "OgreSkeletl", 
                "TrollSkeletal", "GargoyleSkeletal", "MinotaurSkeletal", "LycanSkeletal",
                "SharkSkeletal", "ColossalSkeletal", "MysticalSkeletal", "VampireSkeletal", "LichSkeletal", 
                "SphinxSkeletal", "DevilSkeletal", "DracoSkeletal", "XenoSkeletal",
                // Scales
                "RedScales", "YellowScales", "BlackScales", "GreenScales", "WhiteScales", 
                "BlueScales", "DinosaurScales", "MetallicScales", "BrazenScales", 
                "UmberScales", "VioletScales", "PlatinumScales", "CadalyteScales",
            };
            for (int i = 0; i < rarityOrder.Length; i++)
                if (name == rarityOrder[i]) return i;
                return 999;
        }
        public bool CheckAccessible(Mobile from)
        {
            if (IsLockedDown || IsSecure) return true;
            from.SendMessage("The storage box must be locked down or secured in a house to use.");
            return false;
        }
        public override void OnDoubleClick(Mobile from)
        {
            if (from.InRange(GetWorldLocation(), 2) && CheckAccessible(from))
                from.SendGump(new ResourceBoxGump(from, this, "Metal", 0));
        }
        public override bool OnDragDrop(Mobile from, Item dropped)
        {
            if (!CheckAccessible(from) || !IsAllowed(dropped)) return false;
            Type resType = dropped.GetType();
            if (m_Resources.ContainsKey(resType)) m_Resources[resType] += dropped.Amount;
            else m_Resources[resType] = dropped.Amount;
            from.SendMessage("Stored {0} units.", dropped.Amount);
            dropped.Delete();
            RefreshGump(from);
            return true;
        }
        public void FillFromBackpack(Mobile from)
        {
            if (!CheckAccessible(from) || from.Backpack == null) return;
            int count = 0;
            for (int i = from.Backpack.Items.Count - 1; i >= 0; --i)
            {
                Item item = from.Backpack.Items[i];
                if (IsAllowed(item))
                {
                    Type resType = item.GetType();
                    if (m_Resources.ContainsKey(resType)) m_Resources[resType] += item.Amount;
                    else m_Resources[resType] = item.Amount;
                    item.Delete();
                    count++;
                }
            }
            if (count > 0) { from.SendMessage("Stored resources from backpack."); from.PlaySound(0x42); }
            RefreshGump(from);
        }
        private void RefreshGump(Mobile from)
        {
            if (from.HasGump(typeof(ResourceBoxGump)))
            {
                from.CloseGump(typeof(ResourceBoxGump));
                from.SendGump(new ResourceBoxGump(from, this));
            }
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
            writer.Write((int)1);
            writer.Write(m_Resources.Count);
            foreach (var entry in m_Resources)
            {
                writer.Write(entry.Key.FullName);
                writer.Write(entry.Value);
            }
        }
        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            reader.ReadInt();
            m_Resources = new Dictionary<Type, int>();
            int count = reader.ReadInt();
            for (int i = 0; i < count; i++)
            {
                Type resType = Type.GetType(reader.ReadString());
                int amount = reader.ReadInt();
                if (resType != null) m_Resources[resType] = amount;
            }
        }
    }
}
