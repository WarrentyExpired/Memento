using Server.Accounting;
using Server.Commands.Generic;
using Server.Commands;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Network;
using Server.Regions;
using Server;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;
using Server.Engines.CannedEvil;

namespace Server.Misc
{
    class BuildTreasureChests
    {
        public static void CreateTreasureChests()
        {
            DungeonChestSpawner chestSpawner = new DungeonChestSpawner(1, 0.1); chestSpawner.Delete();

            ArrayList DStargets = new ArrayList();
            foreach (Item item in World.Items.Values)
                if ((item is DungeonChest) || (item is DungeonChestSpawner))
                {
                    DStargets.Add(item);
                }
            for (int i = 0; i < DStargets.Count; ++i)
            {
                Item item = (Item)DStargets[i];
                item.Delete();
            }

            int Heat = 0;
            int ChestLevel = 0;
            ChestLevel = 5; chestSpawner = new DungeonChestSpawner(1, 0.1); chestSpawner.MoveToWorld(new Point3D(2394, 425, 0), Map.Vaelen); Heat = Server.Difficult.GetDifficulty(chestSpawner.Location, chestSpawner.Map); if (Heat < 0) { Heat = 0; }
            chestSpawner.SpawnerLevel = ChestLevel + Heat;
        }
    }
}

namespace Server.Commands
{
    public class Decorate
    {
        public static void Initialize()
        {
            CommandSystem.Register("Decorate", AccessLevel.Administrator, new CommandEventHandler(Decorate_OnCommand));
        }

        [Usage("Decorate")]
        [Description("Generates world decoration.")]
        public static void Decorate_OnCommand(CommandEventArgs e)
        {
            m_Mobile = e.Mobile;
            m_Count = 0;

            m_Mobile.SendMessage("Removing current world decorations, please wait.");

            ArrayList targets = new ArrayList();
            foreach (Item it in World.Items.Values)
            {
                if (it.Weight == -2)
                {
                    if (MySettings.S_PersistentBlackjack && it is CEOBlackJack) { /* LEAVE BLACKJACK TABLES ALONE */ }
                    else if (it is TrashChest) { /* LEAVE BLACKJACK TABLES ALONE */ }
                    else
                        targets.Add(it);
                }
            }
            for (int i = 0; i < targets.Count; ++i)
            {
                Item item = (Item)targets[i];
                item.Delete();
            }

            m_Mobile.SendMessage("Generating world decoration, please wait.");
            GenerateFile("Data/Decoration", "MeetingSpots.cfg", Map.Vaelen);
            GenerateFile("Data/Decoration", "Teleporters.cfg", Map.Vaelen);
            GenerateFile("Data/Decoration", "Doors.cfg", Map.Vaelen);
            GenerateFile("Data/Decoration", "Signs.cfg", Map.Vaelen);
            GenerateFile("Info/Decorations", "Vaelen.cfg", Map.Vaelen);

            ///// BUILD THE SEARCH PEDESTALS ///////////////////////////////////////
            BuildQuests.SearchCreate();

            ///// BUILD THE STEAL PEDESTALS ////////////////////////////////////////
            BuildPedestals.CreateStealPeds();

            ///// BUILD THE DUNGEON CHEST SPAWNERS /////////////////////////////////
            BuildTreasureChests.CreateTreasureChests();

            ///// PLANT THE GARDENS //////////////////////////////////////
            Farms.PlantGardens();

            m_Mobile.SendMessage("World generating complete. {0} items were generated.", m_Count);
        }

        public static void Generate(string folder, params Map[] maps)
        {
            if (!Directory.Exists(folder))
                return;

            string[] files = Directory.GetFiles(folder, "*.cfg");

            for (int i = 0; i < files.Length; ++i)
            {
                ArrayList list = DecorationList.ReadAll(files[i]);

                for (int j = 0; j < list.Count; ++j)
                    m_Count += ((DecorationList)list[j]).Generate(maps);
            }
        }

        public static void GenerateFile(string folder, string file, params Map[] maps)
        {
            if (!Directory.Exists(folder))
                return;

            string[] files = Directory.GetFiles(folder, file);

            for (int i = 0; i < files.Length; ++i)
            {
                ArrayList list = DecorationList.ReadAll(files[i]);

                for (int j = 0; j < list.Count; ++j)
                    m_Count += ((DecorationList)list[j]).Generate(maps);
            }
        }

        private static Mobile m_Mobile;
        private static int m_Count;
    }

    public class DecorationList
    {
        private Type m_Type;
        private int m_ItemID;
        private string[] m_Params;
        private ArrayList m_Entries;

        public DecorationList()
        {
        }

        private static Type typeofStatic = typeof(Static);
        private static Type typeofLocalizedStatic = typeof(LocalizedStatic);
        private static Type typeofBaseDoor = typeof(BaseDoor);
        private static Type typeofAnkhWest = typeof(AnkhWest);
        private static Type typeofAnkhNorth = typeof(AnkhNorth);
        private static Type typeofBeverage = typeof(BaseBeverage);
        private static Type typeofLocalizedSign = typeof(LocalizedSign);
        private static Type typeofWarningItem = typeof(WarningItem);
        private static Type typeofHintItem = typeof(HintItem);
        private static Type typeofSerpentPillar = typeof(SerpentPillar);
        private static Type typeofChampionSpawn = typeof(ChampionSpawn);

        public Item Construct()
        {
            Item item;

            try
            {
                if (m_Type == typeofStatic)
                {
                    item = new Static(m_ItemID);
                }
                else if (m_Type == typeofLocalizedStatic)
                {
                    int labelNumber = 0;

                    for (int i = 0; i < m_Params.Length; ++i)
                    {
                        if (m_Params[i].StartsWith("LabelNumber"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                            {
                                labelNumber = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                                break;
                            }
                        }
                    }

                    item = new LocalizedStatic(m_ItemID, labelNumber);
                }
                else if (m_Type == typeofLocalizedSign)
                {
                    int labelNumber = 0;

                    for (int i = 0; i < m_Params.Length; ++i)
                    {
                        if (m_Params[i].StartsWith("LabelNumber"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                            {
                                labelNumber = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                                break;
                            }
                        }
                    }

                    item = new LocalizedSign(m_ItemID, labelNumber);
                }
                else if (m_Type == typeofAnkhWest || m_Type == typeofAnkhNorth)
                {
                    bool bloodied = false;

                    for (int i = 0; !bloodied && i < m_Params.Length; ++i)
                        bloodied = (m_Params[i] == "Bloodied");

                    if (m_Type == typeofAnkhWest)
                        item = new AnkhWest(bloodied);
                    else
                        item = new AnkhNorth(bloodied);
                }
                else if (m_Type == typeofHintItem)
                {
                    int range = 0;
                    int messageNumber = 0;
                    string messageString = null;
                    int hintNumber = 0;
                    string hintString = null;
                    TimeSpan resetDelay = TimeSpan.Zero;

                    for (int i = 0; i < m_Params.Length; ++i)
                    {
                        if (m_Params[i].StartsWith("Range"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                                range = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                        }
                        else if (m_Params[i].StartsWith("WarningString"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                                messageString = m_Params[i].Substring(++indexOf);
                        }
                        else if (m_Params[i].StartsWith("WarningNumber"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                                messageNumber = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                        }
                        else if (m_Params[i].StartsWith("HintString"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                                hintString = m_Params[i].Substring(++indexOf);
                        }
                        else if (m_Params[i].StartsWith("HintNumber"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                                hintNumber = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                        }
                        else if (m_Params[i].StartsWith("ResetDelay"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                                resetDelay = TimeSpan.Parse(m_Params[i].Substring(++indexOf));
                        }
                    }

                    HintItem hi = new HintItem(m_ItemID, range, messageNumber, hintNumber);

                    hi.WarningString = messageString;
                    hi.HintString = hintString;
                    hi.ResetDelay = resetDelay;

                    item = hi;
                }
                else if (m_Type == typeofWarningItem)
                {
                    int range = 0;
                    int messageNumber = 0;
                    string messageString = null;
                    TimeSpan resetDelay = TimeSpan.Zero;

                    for (int i = 0; i < m_Params.Length; ++i)
                    {
                        if (m_Params[i].StartsWith("Range"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                                range = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                        }
                        else if (m_Params[i].StartsWith("WarningString"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                                messageString = m_Params[i].Substring(++indexOf);
                        }
                        else if (m_Params[i].StartsWith("WarningNumber"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                                messageNumber = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                        }
                        else if (m_Params[i].StartsWith("ResetDelay"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                                resetDelay = TimeSpan.Parse(m_Params[i].Substring(++indexOf));
                        }
                    }

                    WarningItem wi = new WarningItem(m_ItemID, range, messageNumber);

                    wi.WarningString = messageString;
                    wi.ResetDelay = resetDelay;

                    item = wi;
                }
                else if (m_Type == typeofSerpentPillar)
                {
                    string word = null;
                    Rectangle2D destination = new Rectangle2D();

                    for (int i = 0; i < m_Params.Length; ++i)
                    {
                        if (m_Params[i].StartsWith("Word"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                                word = m_Params[i].Substring(++indexOf);
                        }
                        else if (m_Params[i].StartsWith("DestStart"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                                destination.Start = Point2D.Parse(m_Params[i].Substring(++indexOf));
                        }
                        else if (m_Params[i].StartsWith("DestEnd"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                                destination.End = Point2D.Parse(m_Params[i].Substring(++indexOf));
                        }
                    }

                    item = new SerpentPillar(word, destination);
                }
                else if (m_Type.IsSubclassOf(typeofBeverage))
                {
                    BeverageType content = BeverageType.Liquor;
                    bool fill = false;

                    for (int i = 0; !fill && i < m_Params.Length; ++i)
                    {
                        if (m_Params[i].StartsWith("Content"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                            {
                                content = (BeverageType)Enum.Parse(typeof(BeverageType), m_Params[i].Substring(++indexOf), true);
                                fill = true;
                            }
                        }
                    }

                    if (fill)
                        item = (Item)Activator.CreateInstance(m_Type, new object[] { content });
                    else
                        item = (Item)Activator.CreateInstance(m_Type);
                }
                else if (m_Type.IsSubclassOf(typeofBaseDoor))
                {
                    DoorFacing facing = DoorFacing.WestCW;

                    for (int i = 0; i < m_Params.Length; ++i)
                    {
                        if (m_Params[i].StartsWith("Facing"))
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                            {
                                facing = (DoorFacing)Enum.Parse(typeof(DoorFacing), m_Params[i].Substring(++indexOf), true);
                                break;
                            }
                        }
                    }

                    item = (Item)Activator.CreateInstance(m_Type, new object[] { facing });
                }
                else if (m_Type == typeofChampionSpawn)
                {
                    var spawn = new ChampionSpawn(false);
                    var rect = spawn.SpawnArea;
                    item = spawn;

                    for (int i = 0; i < m_Params.Length; ++i)
                    {
                        bool isSpawnStart = m_Params[i].StartsWith("SpawnArea.Start");
                        bool isSpawnEnd = m_Params[i].StartsWith("SpawnArea.End");
                        if (isSpawnStart || isSpawnEnd)
                        {
                            int indexOf = m_Params[i].IndexOf('=');

                            if (indexOf >= 0)
                            {
                                var splitValue = m_Params[i].Substring(++indexOf);
                                Point2D point = Point2D.Parse(splitValue);
                                if (isSpawnStart)
                                    rect.Start = point;
                                else
                                    rect.End = point;
                            }
                        }
                    }

                    // Have to execute the call after the object is moved
                    Timer.DelayCall(TimeSpan.Zero, () => spawn.SpawnArea = rect);

                }
                else
                {
                    item = (Item)Activator.CreateInstance(m_Type);
                }
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Bad type: {0}", m_Type), e);
            }

            if (item is BaseAddon)
            {
                if (m_ItemID > 0)
                {
                    List<AddonComponent> comps = ((BaseAddon)item).Components;

                    for (int i = 0; i < comps.Count; ++i)
                    {
                        AddonComponent comp = (AddonComponent)comps[i];

                        if (comp.Offset == Point3D.Zero)
                            comp.ItemID = m_ItemID;
                    }
                }
            }
            else if (item is BaseLight)
            {
                bool unlit = false, unprotected = false;

                for (int i = 0; i < m_Params.Length; ++i)
                {
                    if (!unlit && m_Params[i] == "Unlit")
                        unlit = true;
                    else if (!unprotected && m_Params[i] == "Unprotected")
                        unprotected = true;

                    if (unlit && unprotected)
                        break;
                }

                if (!unlit)
                    ((BaseLight)item).Ignite();
                if (!unprotected)
                    ((BaseLight)item).Protected = true;

                if (m_ItemID > 0)
                    item.ItemID = m_ItemID;
            }
            else if (item is Server.Mobiles.Spawner)
            {
                Server.Mobiles.Spawner sp = (Server.Mobiles.Spawner)item;

                sp.NextSpawn = TimeSpan.Zero;

                for (int i = 0; i < m_Params.Length; ++i)
                {
                    if (m_Params[i].StartsWith("Spawn"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            sp.SpawnNames.Add(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("MinDelay"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            sp.MinDelay = TimeSpan.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("MaxDelay"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            sp.MaxDelay = TimeSpan.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("NextSpawn"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            sp.NextSpawn = TimeSpan.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Count"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            sp.Count = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Team"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            sp.Team = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("HomeRange"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            sp.HomeRange = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Running"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            sp.Running = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Group"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            sp.Group = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                }
            }
            else if (item is RecallRune)
            {
                RecallRune rune = (RecallRune)item;

                for (int i = 0; i < m_Params.Length; ++i)
                {
                    if (m_Params[i].StartsWith("Description"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            rune.Description = m_Params[i].Substring(++indexOf);
                    }
                    else if (m_Params[i].StartsWith("Marked"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            rune.Marked = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("TargetMap"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            rune.TargetMap = Map.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Target"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            rune.Target = Point3D.Parse(m_Params[i].Substring(++indexOf));
                    }
                }
            }
            else if (item is QuestTransporter)
            {
                QuestTransporter tp = (QuestTransporter)item;

                for (int i = 0; i < m_Params.Length; ++i)
                {
                    if (m_Params[i].StartsWith("TeleportName"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.TeleportName = m_Params[i].Substring(++indexOf);
                    }
                    else if (m_Params[i].StartsWith("Required"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Required = m_Params[i].Substring(++indexOf);
                    }
                    else if (m_Params[i].StartsWith("MessageString"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.MessageString = m_Params[i].Substring(++indexOf);
                    }
                    else if (m_Params[i].StartsWith("PointDest"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.PointDest = Point3D.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("MapDest"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.MapDest = Map.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Creatures"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Creatures = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("SourceEffect"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.SourceEffect = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("DestEffect"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.DestEffect = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("SoundID"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.SoundID = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Delay"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Delay = TimeSpan.Parse(m_Params[i].Substring(++indexOf));
                    }
                }

                if (m_ItemID > 0)
                    item.ItemID = m_ItemID;
            }
            else if (item is SkillTeleporter)
            {
                SkillTeleporter tp = (SkillTeleporter)item;

                for (int i = 0; i < m_Params.Length; ++i)
                {
                    if (m_Params[i].StartsWith("Skill"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Skill = (SkillName)Enum.Parse(typeof(SkillName), m_Params[i].Substring(++indexOf), true);
                    }
                    else if (m_Params[i].StartsWith("RequiredFixedPoint"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Required = Utility.ToInt32(m_Params[i].Substring(++indexOf)) * 0.01;
                    }
                    else if (m_Params[i].StartsWith("Required"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Required = Utility.ToDouble(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("MessageString"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.MessageString = m_Params[i].Substring(++indexOf);
                    }
                    else if (m_Params[i].StartsWith("MessageNumber"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.MessageNumber = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("PointDest"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.PointDest = Point3D.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("MapDest"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.MapDest = Map.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Creatures"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Creatures = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("SourceEffect"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.SourceEffect = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("DestEffect"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.DestEffect = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("SoundID"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.SoundID = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Delay"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Delay = TimeSpan.Parse(m_Params[i].Substring(++indexOf));
                    }
                }

                if (m_ItemID > 0)
                    item.ItemID = m_ItemID;
            }
            else if (item is KeywordTeleporter)
            {
                KeywordTeleporter tp = (KeywordTeleporter)item;

                for (int i = 0; i < m_Params.Length; ++i)
                {
                    if (m_Params[i].StartsWith("Substring"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Substring = m_Params[i].Substring(++indexOf);
                    }
                    else if (m_Params[i].StartsWith("Keyword"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Keyword = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Range"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Range = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("PointDest"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.PointDest = Point3D.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("MapDest"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.MapDest = Map.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Creatures"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Creatures = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("SourceEffect"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.SourceEffect = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("DestEffect"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.DestEffect = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("SoundID"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.SoundID = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Delay"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Delay = TimeSpan.Parse(m_Params[i].Substring(++indexOf));
                    }
                }

                if (m_ItemID > 0)
                    item.ItemID = m_ItemID;
            }
            else if (item is Teleporter)
            {
                Teleporter tp = (Teleporter)item;

                for (int i = 0; i < m_Params.Length; ++i)
                {
                    if (m_Params[i].StartsWith("PointDest"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.PointDest = Point3D.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("MapDest"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.MapDest = Map.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Creatures"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Creatures = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("SourceEffect"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.SourceEffect = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("DestEffect"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.DestEffect = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("SoundID"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.SoundID = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Delay"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Delay = TimeSpan.Parse(m_Params[i].Substring(++indexOf));
                    }
                }

                if (m_ItemID > 0)
                    item.ItemID = m_ItemID;
            }
            else if (item is ThruDoor)
            {
                ThruDoor tp = (ThruDoor)item;

                for (int i = 0; i < m_Params.Length; ++i)
                {
                    if (m_Params[i].StartsWith("PointDest"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.PointDest = Point3D.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("MapDest"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.MapDest = Map.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Rules"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Rules = int.Parse(m_Params[i].Substring(++indexOf));
                    }
                }

                if (m_ItemID > 0)
                    item.ItemID = m_ItemID;
            }
            else if (item is moongates) // ADDED THIS TO MAKE MY OWN MOONGATES THAT CAN BE DECORATED //
            {
                moongates tp = (moongates)item;

                for (int i = 0; i < m_Params.Length; ++i)
                {
                    if (m_Params[i].StartsWith("PointDest"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.PointDest = Point3D.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("MapDest"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.MapDest = Map.Parse(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Creatures"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Creatures = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("SourceEffect"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.SourceEffect = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("DestEffect"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.DestEffect = Utility.ToBoolean(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("SoundID"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.SoundID = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                    }
                    else if (m_Params[i].StartsWith("Delay"))
                    {
                        int indexOf = m_Params[i].IndexOf('=');

                        if (indexOf >= 0)
                            tp.Delay = TimeSpan.Parse(m_Params[i].Substring(++indexOf));
                    }
                }

                if (m_ItemID > 0)
                    item.ItemID = m_ItemID;
            }
            else if (m_ItemID > 0)
            {
                item.ItemID = m_ItemID;
            }

            item.Movable = false;

            for (int i = 0; i < m_Params.Length; ++i)
            {
                if (m_Params[i].StartsWith("Light"))
                {
                    int indexOf = m_Params[i].IndexOf('=');

                    if (indexOf >= 0)
                        item.Light = (LightType)Enum.Parse(typeof(LightType), m_Params[i].Substring(++indexOf), true);
                }
                else if (m_Params[i].StartsWith("Hue"))
                {
                    int indexOf = m_Params[i].IndexOf('=');

                    if (indexOf >= 0)
                    {
                        int hue = Utility.ToInt32(m_Params[i].Substring(++indexOf));

                        if (item is DyeTub)
                            ((DyeTub)item).DyedHue = hue;
                        else
                            item.Hue = hue;
                    }
                }
                else if (m_Params[i].StartsWith("Name"))
                {
                    int indexOf = m_Params[i].IndexOf('=');

                    if (indexOf >= 0)
                        item.Name = m_Params[i].Substring(++indexOf);
                }
                else if (m_Params[i].StartsWith("Visible"))
                {
                    int indexOf = m_Params[i].IndexOf('=');

                    if (indexOf >= 0)
                    {
                        if (m_Params[i].Substring(++indexOf) == "false")
                        {
                            item.Visible = false;
                        }
                        else
                        {
                            item.Visible = true;
                        }
                    }
                }
                else if (m_Params[i].StartsWith("Movable"))
                {
                    int indexOf = m_Params[i].IndexOf('=');

                    if (indexOf >= 0)
                    {
                        if (m_Params[i].Substring(++indexOf) == "false")
                        {
                            item.Movable = false;
                        }
                        else
                        {
                            item.Movable = true;
                        }
                    }
                }
                else if (m_Params[i].StartsWith("Amount"))
                {
                    int indexOf = m_Params[i].IndexOf('=');

                    if (indexOf >= 0)
                    {
                        // Must supress stackable warnings

                        bool wasStackable = item.Stackable;

                        item.Stackable = true;
                        item.Amount = Utility.ToInt32(m_Params[i].Substring(++indexOf));
                        item.Stackable = wasStackable;
                    }
                }
            }

            item.Weight = -2;

            if (item is CEOBlackJack && MySettings.S_PersistentBlackjack) { item.Delete(); }
            if (item is TrashChest) { item.Delete(); }

            return item;
        }

        private static Queue m_DeleteQueue = new Queue();

        private static bool FindItem(int x, int y, int z, Map map, Item srcItem)
        {
            int itemID = srcItem.ItemID;

            bool res = false;

            IPooledEnumerable eable;

            if (srcItem is BaseDoor)
            {
                eable = map.GetItemsInRange(new Point3D(x, y, z), 1);

                foreach (Item item in eable)
                {
                    if (!(item is BaseDoor))
                        continue;

                    BaseDoor bd = (BaseDoor)item;
                    Point3D p;
                    int bdItemID;

                    if (bd.Open)
                    {
                        p = new Point3D(bd.X - bd.Offset.X, bd.Y - bd.Offset.Y, bd.Z - bd.Offset.Z);
                        bdItemID = bd.ClosedID;
                    }
                    else
                    {
                        p = bd.Location;
                        bdItemID = bd.ItemID;
                    }

                    if (p.X != x || p.Y != y)
                        continue;

                    if (item.Z == z && bdItemID == itemID)
                        res = true;
                    else if (Math.Abs(item.Z - z) < 8)
                        m_DeleteQueue.Enqueue(item);
                }
            }
            else if ((TileData.ItemTable[itemID & TileData.MaxItemValue].Flags & TileFlag.LightSource) != 0)
            {
                eable = map.GetItemsInRange(new Point3D(x, y, z), 0);

                LightType lt = srcItem.Light;
                string srcName = srcItem.ItemData.Name;

                foreach (Item item in eable)
                {
                    if (item.Z == z)
                    {
                        if (item.ItemID == itemID)
                        {
                            if (item.Light != lt)
                                m_DeleteQueue.Enqueue(item);
                            else
                                res = true;
                        }
                        else if ((item.ItemData.Flags & TileFlag.LightSource) != 0 && item.ItemData.Name == srcName)
                        {
                            m_DeleteQueue.Enqueue(item);
                        }
                    }
                }
            }
            else if (srcItem is Teleporter || srcItem is BaseBook)
            {
                eable = map.GetItemsInRange(new Point3D(x, y, z), 0);

                Type type = srcItem.GetType();

                foreach (Item item in eable)
                {
                    if (item.Z == z && item.ItemID == itemID)
                    {
                        if (item.GetType() != type)
                            m_DeleteQueue.Enqueue(item);
                        else
                            res = true;
                    }
                }
            }
            else
            {
                eable = map.GetItemsInRange(new Point3D(x, y, z), 0);

                foreach (Item item in eable)
                {
                    if (item.Z == z && item.ItemID == itemID)
                    {
                        eable.Free();
                        return true;
                    }
                }
            }

            eable.Free();

            while (m_DeleteQueue.Count > 0)
                ((Item)m_DeleteQueue.Dequeue()).Delete();

            return res;
        }

        public int Generate(Map[] maps)
        {
            int count = 0;

            Item item = null;

            for (int i = 0; i < m_Entries.Count; ++i)
            {
                DecorationEntry entry = (DecorationEntry)m_Entries[i];
                Point3D loc = entry.Location;
                string extra = entry.Extra;

                for (int j = 0; j < maps.Length; ++j)
                {
                    if (item == null)
                        item = Construct();

                    if (item == null)
                        continue;

                    item.MoveToWorld(loc, maps[j]);
                    item.OnAfterSpawn();
                    ++count;

                    if (item is BaseDoor)
                    {
                        IPooledEnumerable eable = maps[j].GetItemsInRange(loc, 1);

                        Type itemType = item.GetType();

                        foreach (Item link in eable)
                        {
                            if (link != item && link.Z == item.Z && link.GetType() == itemType)
                            {
                                ((BaseDoor)item).Link = (BaseDoor)link;
                                ((BaseDoor)link).Link = (BaseDoor)item;
                                break;
                            }
                        }

                        eable.Free();
                    }

                    item = null;
                }
            }

            if (item != null)
                item.Delete();

            return count;
        }

        public static ArrayList ReadAll(string path)
        {
            using (StreamReader ip = new StreamReader(path))
            {
                ArrayList list = new ArrayList();

                for (DecorationList v = Read(ip); v != null; v = Read(ip))
                    list.Add(v);

                return list;
            }
        }

        private static string[] m_EmptyParams = new string[0];

        public static DecorationList Read(StreamReader ip)
        {
            string line;

            while ((line = ip.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.Length > 0 && !line.StartsWith("#"))
                    break;
            }

            if (string.IsNullOrEmpty(line))
                return null;

            DecorationList list = new DecorationList();

            int indexOf = line.IndexOf(' ');

            list.m_Type = ScriptCompiler.FindTypeByName(line.Substring(0, indexOf++), true);

            if (list.m_Type == null)
                throw new ArgumentException(String.Format("Type not found for header: '{0}'", line));

            line = line.Substring(indexOf);
            indexOf = line.IndexOf('(');
            if (indexOf >= 0)
            {
                list.m_ItemID = Utility.ToInt32(line.Substring(0, indexOf - 1));

                string parms = line.Substring(++indexOf);

                if (line.EndsWith(")"))
                    parms = parms.Substring(0, parms.Length - 1);

                list.m_Params = parms.Split(';');

                for (int i = 0; i < list.m_Params.Length; ++i)
                    list.m_Params[i] = list.m_Params[i].Trim();
            }
            else
            {
                list.m_ItemID = Utility.ToInt32(line);
                list.m_Params = m_EmptyParams;
            }

            list.m_Entries = new ArrayList();

            while ((line = ip.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.Length == 0)
                    break;

                if (line.StartsWith("#"))
                    continue;

                list.m_Entries.Add(new DecorationEntry(line));
            }

            return list;
        }
    }

    public class DecorationEntry
    {
        private Point3D m_Location;
        private string m_Extra;

        public Point3D Location { get { return m_Location; } }
        public string Extra { get { return m_Extra; } }

        public DecorationEntry(string line)
        {
            string x, y, z;

            Pop(out x, ref line);
            Pop(out y, ref line);
            Pop(out z, ref line);

            m_Location = new Point3D(Utility.ToInt32(x), Utility.ToInt32(y), Utility.ToInt32(z));
            m_Extra = line;
        }

        public void Pop(out string v, ref string line)
        {
            int space = line.IndexOf(' ');

            if (space >= 0)
            {
                v = line.Substring(0, space++);
                line = line.Substring(space);
            }
            else
            {
                v = line;
                line = "";
            }
        }
    }
}

namespace Server.Misc
{
    class BuildPedestals
    {
        public static Item ChooseType()
        {
            Item item = null;

            item = new StealBase();

            return item;
        }

        public static void CreateStealPeds()
        {
            Item stealPedestal = new StealBase(); stealPedestal.Delete();

            ArrayList SBtargets = new ArrayList();
            foreach (Item item in World.Items.Values)
                if ((item is StealBase) || (item is StealBaseEmpty))
                {
                    SBtargets.Add(item);
                }
            for (int i = 0; i < SBtargets.Count; ++i)
            {
                Item item = (Item)SBtargets[i];
                item.Delete();
            }

            stealPedestal = ChooseType(); stealPedestal.MoveToWorld(new Point3D(2393, 416, 0), Map.Vaelen);


            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            EssenceBase essPedestal = new EssenceBase("drow"); essPedestal.Delete();

            ArrayList EStargets = new ArrayList();
            foreach (Item item in World.Items.Values)
                if ((item is EssenceBase) || (item is EssenceBaseEmpty))
                {
                    EStargets.Add(item);
                }
            for (int i = 0; i < EStargets.Count; ++i)
            {
                Item item = (Item)EStargets[i];
                item.Delete();
            }

            essPedestal = new EssenceBase("tritun"); essPedestal.MoveToWorld(new Point3D(920, 3259, 40), Map.SavagedEmpire);
            essPedestal = new EssenceBase("ork"); essPedestal.MoveToWorld(new Point3D(1044, 2425, -28), Map.SavagedEmpire);
            essPedestal = new EssenceBase("ork"); essPedestal.MoveToWorld(new Point3D(248, 1946, -28), Map.SavagedEmpire);
            essPedestal = new EssenceBase("drow"); essPedestal.MoveToWorld(new Point3D(5432, 1348, 0), Map.Lodor);
            essPedestal = new EssenceBase("vampire"); essPedestal.MoveToWorld(new Point3D(5774, 2746, 5), Map.Lodor);
            essPedestal = new EssenceBase("ghost"); essPedestal.MoveToWorld(new Point3D(6500, 649, 0), Map.Vaelen);
            essPedestal = new EssenceBase("demon"); essPedestal.MoveToWorld(new Point3D(6121, 208, 22), Map.Lodor);
            essPedestal = new EssenceBase("ice"); essPedestal.MoveToWorld(new Point3D(6432, 526, 0), Map.Lodor);
            essPedestal = new EssenceBase("fire"); essPedestal.MoveToWorld(new Point3D(6251, 2482, 0), Map.Lodor);
            essPedestal = new EssenceBase("shadow"); essPedestal.MoveToWorld(new Point3D(5042, 3517, 0), Map.Vaelen);
            essPedestal = new EssenceBase("dark"); essPedestal.MoveToWorld(new Point3D(411, 2124, -1), Map.SavagedEmpire);
            essPedestal = new EssenceBase("lizard"); essPedestal.MoveToWorld(new Point3D(6223, 1341, 0), Map.Lodor);
            essPedestal = new EssenceBase("darkness"); essPedestal.MoveToWorld(new Point3D(6788, 2340, 0), Map.Lodor);
            essPedestal = new EssenceBase("radiated"); essPedestal.MoveToWorld(new Point3D(1066, 3736, 0), Map.SavagedEmpire);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            ArrayList FMtargets = new ArrayList();
            foreach (Item item in World.Items.Values)
                if (item is RunesBase || item is RunesBaseEmpty || item is FlamesBase || item is FlamesBaseEmpty || item is BaneBase || item is BaneBaseEmpty || item is PaganBase || item is PaganBaseEmpty)
                {
                    FMtargets.Add(item);
                }
            for (int i = 0; i < FMtargets.Count; ++i)
            {
                Item item = (Item)FMtargets[i];
                item.Delete();
            }

            int most = 79;
            int choice = 0;

            string KeepTrack = "_";

            choice = Utility.RandomMinMax(1, most); KeepTrack = KeepTrack + choice.ToString() + "_";
            while (UsedNumberCheck(KeepTrack, choice) == true) { choice = Utility.RandomMinMax(1, most); }
            KeepTrack = KeepTrack + choice.ToString() + "_";
            CreateSpecialPedestal(choice, 1, "shadowlord"); // BOOK OF TRUTH

            while (UsedNumberCheck(KeepTrack, choice) == true) { choice = Utility.RandomMinMax(1, most); }
            KeepTrack = KeepTrack + choice.ToString() + "_";
            CreateSpecialPedestal(choice, 2, "shadowlord"); // BELL OF COURAGE

            while (UsedNumberCheck(KeepTrack, choice) == true) { choice = Utility.RandomMinMax(1, most); }
            KeepTrack = KeepTrack + choice.ToString() + "_";
            CreateSpecialPedestal(choice, 3, "shadowlord"); // CANDLE OF LOVE

            while (UsedNumberCheck(KeepTrack, choice) == true) { choice = Utility.RandomMinMax(1, most); }
            KeepTrack = KeepTrack + choice.ToString() + "_";
            CreateSpecialPedestal(choice, 1, "serpent"); // SCALES OF ETHICALITY

            while (UsedNumberCheck(KeepTrack, choice) == true) { choice = Utility.RandomMinMax(1, most); }
            KeepTrack = KeepTrack + choice.ToString() + "_";
            CreateSpecialPedestal(choice, 2, "serpent"); // ORB OF LOGIC

            while (UsedNumberCheck(KeepTrack, choice) == true) { choice = Utility.RandomMinMax(1, most); }
            KeepTrack = KeepTrack + choice.ToString() + "_";
            CreateSpecialPedestal(choice, 3, "serpent"); // LANTERN OF DISCIPLINE

            while (UsedNumberCheck(KeepTrack, choice) == true) { choice = Utility.RandomMinMax(1, most); }
            KeepTrack = KeepTrack + choice.ToString() + "_";
            CreateSpecialPedestal(choice, 1, "pagan"); // BREATH OF AIR

            while (UsedNumberCheck(KeepTrack, choice) == true) { choice = Utility.RandomMinMax(1, most); }
            KeepTrack = KeepTrack + choice.ToString() + "_";
            CreateSpecialPedestal(choice, 2, "pagan"); // TONGUE OF FLAME

            while (UsedNumberCheck(KeepTrack, choice) == true) { choice = Utility.RandomMinMax(1, most); }
            KeepTrack = KeepTrack + choice.ToString() + "_";
            CreateSpecialPedestal(choice, 3, "pagan"); // HEART OF EARTH

            while (UsedNumberCheck(KeepTrack, choice) == true) { choice = Utility.RandomMinMax(1, most); }
            KeepTrack = KeepTrack + choice.ToString() + "_";
            CreateSpecialPedestal(choice, 4, "pagan"); // TEAR OF THE SEAS

            while (UsedNumberCheck(KeepTrack, choice) == true) { choice = Utility.RandomMinMax(1, most); }
            KeepTrack = KeepTrack + choice.ToString() + "_";
            CreateSpecialPedestal(choice, 0, "runes"); // VIRTUE CHEST
        }

        public static bool UsedNumberCheck(string info, int numb)
        {
            string number = "_" + numb.ToString() + "_";
            if (info.Contains(number)) { return true; }
            return false;
        }

        public static void CreateSpecialPedestal(int choice, int ped, string category)
        {
            Item specialPed = new FlamesBase("love"); specialPed.Delete();

            if (category == "shadowlord")
            {
                if (ped == 1) { specialPed = new FlamesBase("truth"); }
                else if (ped == 2) { specialPed = new FlamesBase("courage"); }
                else { specialPed = new FlamesBase("love"); }
            }
            else if (category == "serpent")
            {
                if (ped == 1) { specialPed = new BaneBase("ethicality"); }
                else if (ped == 2) { specialPed = new BaneBase("logic"); }
                else { specialPed = new BaneBase("discipline"); }
            }
            else if (category == "pagan")
            {
                if (ped == 1) { specialPed = new PaganBase("air"); }
                else if (ped == 2) { specialPed = new PaganBase("fire"); }
                else if (ped == 3) { specialPed = new PaganBase("earth"); }
                else { specialPed = new PaganBase("water"); }
            }
            else if (category == "runes")
            {
                specialPed = new RunesBase();
            }

            switch (choice)
            {
                case 1: specialPed.MoveToWorld(new Point3D(6295, 387, 5), Map.Lodor); break; // the Vault of the Black Knight
                case 2: specialPed.MoveToWorld(new Point3D(6441, 3808, 10), Map.Lodor); break; // the Undersea Pass
                case 3: specialPed.MoveToWorld(new Point3D(5820, 2785, 5), Map.Lodor); break; // the Crypts of Dracula
                case 4: specialPed.MoveToWorld(new Point3D(5596, 1874, 0), Map.Lodor); break; // the Lodoria Catacombs
                case 5: specialPed.MoveToWorld(new Point3D(5275, 675, 5), Map.Lodor); break; // Dungeon Deceit
                case 6: specialPed.MoveToWorld(new Point3D(5443, 2430, 40), Map.Lodor); break; // Dungeon Despise
                case 7: specialPed.MoveToWorld(new Point3D(5149, 843, 0), Map.Lodor); break; // Dungeon Destard
                case 8: specialPed.MoveToWorld(new Point3D(5826, 1418, 0), Map.Lodor); break; // the City of Embers
                case 9: specialPed.MoveToWorld(new Point3D(6085, 69, 27), Map.Lodor); break; // Dungeon Hythloth
                case 10: specialPed.MoveToWorld(new Point3D(5672, 311, 0), Map.Lodor); break; // the Ice Fiend Lair
                case 11: specialPed.MoveToWorld(new Point3D(5208, 1593, 0), Map.Lodor); break; // Terathan Keep
                case 12: specialPed.MoveToWorld(new Point3D(5378, 409, 0), Map.Lodor); break; // the Halls of Undermountain
                case 13: specialPed.MoveToWorld(new Point3D(5871, 3438, 0), Map.Lodor); break; // the Volcanic Cave
                case 14: specialPed.MoveToWorld(new Point3D(5527, 1352, 0), Map.Lodor); break; // Dungeon Wrong
                case 15: specialPed.MoveToWorld(new Point3D(6152, 2872, 0), Map.Lodor); break; // Stonegate Castle

                case 16: specialPed.MoveToWorld(new Point3D(6897, 2874, 50), Map.Vaelen); break; // Vordo's Castle
                case 17: specialPed.MoveToWorld(new Point3D(3882, 3281, 40), Map.Vaelen); break; // the Mausoleum
                case 18: specialPed.MoveToWorld(new Point3D(495, 3811, 78), Map.Vaelen); break; // the Tower of Brass
                case 19: specialPed.MoveToWorld(new Point3D(4734, 3682, 0), Map.Vaelen); break; // the Dragon's Maw
                case 20: specialPed.MoveToWorld(new Point3D(6966, 3848, 25), Map.Vaelen); break; // the Cave of the Zuluu
                case 21: specialPed.MoveToWorld(new Point3D(5333, 895, 0), Map.Vaelen); break; // the Ancient Pyramid
                case 22: specialPed.MoveToWorld(new Point3D(5939, 654, 0), Map.Vaelen); break; // Dungeon Exodus
                case 23: specialPed.MoveToWorld(new Point3D(5843, 1752, 0), Map.Vaelen); break; // the Caverns of Poseidon
                case 24: specialPed.MoveToWorld(new Point3D(5620, 2172, 0), Map.Vaelen); break; // Dungeon Clues
                case 25: specialPed.MoveToWorld(new Point3D(5622, 367, 0), Map.Vaelen); break; // Dardin's Pit
                case 26: specialPed.MoveToWorld(new Point3D(5242, 219, 0), Map.Vaelen); break; // Dungeon Doom
                case 27: specialPed.MoveToWorld(new Point3D(5528, 1246, 0), Map.Vaelen); break; // the Fires of Hell
                case 28: specialPed.MoveToWorld(new Point3D(5636, 1513, 0), Map.Vaelen); break; // the Mines of Morinia
                case 29: specialPed.MoveToWorld(new Point3D(5915, 462, 0), Map.Vaelen); break; // the Perinian Depths
                case 30: specialPed.MoveToWorld(new Point3D(5506, 818, 0), Map.Vaelen); break; // the Dungeon of Time Awaits

                case 31: specialPed.MoveToWorld(new Point3D(1961, 562, 0), Map.SerpentIsland); break; // the Ancient Prison
                case 32: specialPed.MoveToWorld(new Point3D(2134, 873, 0), Map.SerpentIsland); break; // the Cave of Fire
                case 33: specialPed.MoveToWorld(new Point3D(2449, 168, 0), Map.SerpentIsland); break; // the Cave of Souls
                case 34: specialPed.MoveToWorld(new Point3D(2085, 216, 0), Map.SerpentIsland); break; // Dungeon Ankh
                case 35: specialPed.MoveToWorld(new Point3D(1968, 180, 0), Map.SerpentIsland); break; // Dungeon Bane
                case 36: specialPed.MoveToWorld(new Point3D(2171, 520, 2), Map.SerpentIsland); break; // Dungeon Hate
                case 37: specialPed.MoveToWorld(new Point3D(2202, 832, 0), Map.SerpentIsland); break; // Dungeon Scorn
                case 38: specialPed.MoveToWorld(new Point3D(1935, 813, 0), Map.SerpentIsland); break; // Dungeon Torment
                case 39: specialPed.MoveToWorld(new Point3D(2329, 477, 0), Map.SerpentIsland); break; // Dungeon Vile
                case 40: specialPed.MoveToWorld(new Point3D(2205, 165, 2), Map.SerpentIsland); break; // Dungeon Wicked
                case 41: specialPed.MoveToWorld(new Point3D(2315, 893, 2), Map.SerpentIsland); break; // Dungeon Wrath
                case 42: specialPed.MoveToWorld(new Point3D(2473, 838, 0), Map.SerpentIsland); break; // the Flooded Temple
                case 43: specialPed.MoveToWorld(new Point3D(2083, 542, 0), Map.SerpentIsland); break; // the Gargoyle Crypts
                case 44: specialPed.MoveToWorld(new Point3D(2457, 471, 0), Map.SerpentIsland); break; // the Serpent Sanctum
                case 45: specialPed.MoveToWorld(new Point3D(2313, 168, 2), Map.SerpentIsland); break; // the Tomb of the Fallen Wizard

                case 46: specialPed.MoveToWorld(new Point3D(729, 2626, -28), Map.SavagedEmpire); break; // the Blood Temple
                case 47: specialPed.MoveToWorld(new Point3D(747, 1978, -28), Map.SavagedEmpire); break; // the Dungeon of the Mad Archmage
                case 48: specialPed.MoveToWorld(new Point3D(24, 2708, -28), Map.SavagedEmpire); break; // the Tombs
                case 49: specialPed.MoveToWorld(new Point3D(503, 2318, -1), Map.SavagedEmpire); break; // the Dungeon of the Lich King
                case 50: specialPed.MoveToWorld(new Point3D(47, 3252, 20), Map.SavagedEmpire); break; // the Forgotten Halls
                case 51: specialPed.MoveToWorld(new Point3D(424, 2827, 22), Map.SavagedEmpire); break; // the Ice Queen Fortress
                case 52: specialPed.MoveToWorld(new Point3D(937, 2336, -28), Map.SavagedEmpire); break; // the Halls of Ogrimar
                case 53: specialPed.MoveToWorld(new Point3D(662, 2208, -27), Map.SavagedEmpire); break; // Dungeon Rock
                case 54: specialPed.MoveToWorld(new Point3D(354, 3935, 20), Map.SavagedEmpire); break; // the Scurvy Reef
                case 55: specialPed.MoveToWorld(new Point3D(487, 3387, 0), Map.SavagedEmpire); break; // the Tomb of Kazibal
                case 56: specialPed.MoveToWorld(new Point3D(800, 3268, 0), Map.SavagedEmpire); break; // the Catacombs of Azerok
                case 57: specialPed.MoveToWorld(new Point3D(339, 3626, 3), Map.SavagedEmpire); break; // the Azure Castle
                case 58: specialPed.MoveToWorld(new Point3D(752, 4019, 0), Map.SavagedEmpire); break; // the Undersea Castle
                case 59: specialPed.MoveToWorld(new Point3D(203, 2629, -17), Map.SavagedEmpire); break; // the Altar of the Dragon King
                case 60: specialPed.MoveToWorld(new Point3D(865, 2177, -66), Map.SavagedEmpire); break; // the Ratmen Mines
                case 61: specialPed.MoveToWorld(new Point3D(1127, 2188, -28), Map.SavagedEmpire); break; // the Pixie Cave
                case 62: specialPed.MoveToWorld(new Point3D(461, 2617, -28), Map.SavagedEmpire); break; // the Spider Cave

                case 63: specialPed.MoveToWorld(new Point3D(237, 3486, 0), Map.Vaelen); break; // the Cave of Banished Mages
                case 64: specialPed.MoveToWorld(new Point3D(5765, 3248, 0), Map.Vaelen); break; // the City of the Dead
                case 65: specialPed.MoveToWorld(new Point3D(6495, 2877, 45), Map.Vaelen); break; // the Crypts of Kuldar
                case 66: specialPed.MoveToWorld(new Point3D(6340, 2831, 5), Map.Vaelen); break; // the Kuldara Sewers
                case 67: specialPed.MoveToWorld(new Point3D(5354, 53, 15), Map.Lodor); break; // the Mind Flayer City
                case 68: specialPed.MoveToWorld(new Point3D(1936, 1549, -7), Map.Underworld); break; // the Glacial Scar
                case 69: specialPed.MoveToWorld(new Point3D(1861, 1222, -42), Map.Underworld); break; // the Stygian Abyss
                case 70: specialPed.MoveToWorld(new Point3D(6185, 3645, -60), Map.Lodor); break; // the Temple of Osirus
                case 71: specialPed.MoveToWorld(new Point3D(6241, 2091, 34), Map.Lodor); break; // the Daemon's Crag
                case 72: specialPed.MoveToWorld(new Point3D(5755, 2516, 0), Map.Lodor); break; // Dungeon Covetous
                case 73: specialPed.MoveToWorld(new Point3D(6928, 1574, 0), Map.Lodor); break; // the Castle of Dracula
                case 74: specialPed.MoveToWorld(new Point3D(6897, 2337, 20), Map.Lodor); break; // the Zealan Tombs
                case 75: specialPed.MoveToWorld(new Point3D(6339, 1271, 1), Map.Lodor); break; // the Hall of the Mountain King
                case 76: specialPed.MoveToWorld(new Point3D(6768, 1038, 21), Map.Lodor); break; // Morgaelin's Inferno
                case 77: specialPed.MoveToWorld(new Point3D(5187, 2225, 10), Map.Lodor); break; // the Depths of Carthax Lake
                case 78: specialPed.MoveToWorld(new Point3D(6109, 698, 0), Map.Lodor); break; // Argentrock Castle
                case 79: specialPed.MoveToWorld(new Point3D(6216, 1343, 0), Map.Lodor); break; // the Sanctum of Saltmarsh
            }
        }
    }
}

namespace Server.Misc
{
    class Farms
    {
        public static Item GetPlant(string plant)
        {
            Item planted = new FarmableCabbage(); planted.Delete();

            if (plant == "garlic") { planted = new FarmableGarlic(); }
            else if (plant == "ginseng") { planted = new FarmableGinseng(); }
            else if (plant == "mandrake") { planted = new FarmableMandrakeRoot(); }
            else if (plant == "nightshade") { planted = new FarmableNightshade(); }
            else if (plant == "cabbage") { planted = new FarmableCabbage(); }
            else if (plant == "carrot") { planted = new FarmableCarrot(); }
            else if (plant == "corn") { planted = new FarmableCorn(); }
            else if (plant == "cotton") { planted = new FarmableCotton(); }
            else if (plant == "flax") { planted = new FarmableFlax(); }
            else if (plant == "lettuce") { planted = new FarmableLettuce(); }
            else if (plant == "onion") { planted = new FarmableOnion(); }
            else if (plant == "pumpkin")
            {
                int odds = Utility.RandomMinMax(1, 100);
                if (odds > 99) { planted = new FarmablePumpkinGiant(); }
                else if (odds > 96) { planted = new FarmablePumpkinLarge(); }
                else if (odds > 93) { planted = new FarmablePumpkinTall(); }
                else if (odds > 90) { planted = new FarmablePumpkinGreen(); }
                else { planted = new FarmablePumpkin(); }
            }
            else if (plant == "turnip") { planted = new FarmableTurnip(); }
            else if (plant == "wheat") { planted = new FarmableWheat(); }
            else if (plant == "watermelon") { planted = new FarmableWatermelon(); }
            else if (plant == "tomato") { planted = new FarmableTomato(); }
            else if (plant == "tailor")
            {
                switch (Utility.RandomMinMax(1, 2))
                {
                    case 1: planted = new FarmableFlax(); break;
                    case 2: planted = new FarmableCotton(); break;
                }
            }
            else if (plant == "mage")
            {
                switch (Utility.RandomMinMax(1, 4))
                {
                    case 1: planted = new FarmableGarlic(); break;
                    case 2: planted = new FarmableGinseng(); break;
                    case 3: planted = new FarmableMandrakeRoot(); break;
                    case 4: planted = new FarmableNightshade(); break;
                }
            }
            else
            {
                switch (Utility.RandomMinMax(1, 9))
                {
                    case 1: planted = new FarmableCabbage(); break;
                    case 2: planted = new FarmableCarrot(); break;
                    case 3: planted = new FarmableCorn(); break;
                    case 4: planted = new FarmableLettuce(); break;
                    case 5: planted = new FarmableOnion(); break;
                    case 6:
                        int odds = Utility.RandomMinMax(1, 100);
                        if (odds > 99) { planted = new FarmablePumpkinGiant(); }
                        else if (odds > 96) { planted = new FarmablePumpkinLarge(); }
                        else if (odds > 93) { planted = new FarmablePumpkinTall(); }
                        else if (odds > 90) { planted = new FarmablePumpkinGreen(); }
                        else { planted = new FarmablePumpkin(); }
                        break;
                    case 7: planted = new FarmableTurnip(); break;
                    case 8: planted = new FarmableWatermelon(); break;
                    case 9: planted = new FarmableTomato(); break;
                }
            }

            return planted;
        }

        public static string RandomCrop()
        {
            string randomCrop = "corn";
            switch (Utility.RandomMinMax(1, 9))
            {
                case 1: randomCrop = "cabbage"; break;
                case 2: randomCrop = "carrot"; break;
                case 3: randomCrop = "corn"; break;
                case 4: randomCrop = "lettuce"; break;
                case 5: randomCrop = "onion"; break;
                case 6: randomCrop = "pumpkin"; break;
                case 7: randomCrop = "turnip"; break;
                case 8: randomCrop = "watermelon"; break;
                case 9: randomCrop = "tomato"; break;
            }
            return randomCrop;
        }

        public static void PlantGardens()
        {
            int plantChance = 10;

            ArrayList RMtarg = new ArrayList();
            foreach (Item item in World.Items.Values)
                if (item is FarmableCrop)
                {
                    RMtarg.Add(item);
                }
            for (int i = 0; i < RMtarg.Count; ++i)
            {
                Item item = (Item)RMtarg[i];
                if (item != null) { item.Delete(); }
            }

            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant("tailor"); plant.MoveToWorld(new Point3D(941, 639, 0), Map.Vaelen); }
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant("wheat"); plant.MoveToWorld(new Point3D(2976, 1268, 0), Map.Vaelen); }
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant("tailor"); plant.MoveToWorld(new Point3D(6791, 1781, 20), Map.Vaelen); }
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant("food"); plant.MoveToWorld(new Point3D(6829, 1680, 1), Map.Vaelen); }
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant("wheat"); plant.MoveToWorld(new Point3D(922, 776, 0), Map.Vaelen); }
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant("cotton"); plant.MoveToWorld(new Point3D(1586, 1472, 2), Map.Vaelen); }
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant("wheat"); plant.MoveToWorld(new Point3D(2637, 521, 0), Map.Vaelen); }
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant("wheat"); plant.MoveToWorld(new Point3D(2693, 608, 0), Map.Vaelen); }

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            string thisCrop = RandomCrop();

            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(6607, 3207, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(6583, 3205, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(6609, 3220, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(6597, 3235, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(6585, 3229, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(6580, 3217, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2966, 1268, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(922, 768, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(928, 768, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2700, 603, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2700, 608, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2707, 603, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2704, 608, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2707, 608, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2707, 613, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2792, 605, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2799, 605, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2792, 608, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2799, 613, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2792, 613, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2799, 608, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2800, 613, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2800, 608, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2785, 918, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2785, 928, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2824, 989, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2834, 977, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2834, 987, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2832, 989, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2834, 992, 0), Map.Vaelen); }
            thisCrop = RandomCrop();
            if (plantChance >= Utility.RandomMinMax(1, 100)) { Item plant = GetPlant(thisCrop); plant.MoveToWorld(new Point3D(2832, 992, 0), Map.Vaelen); }
        }
    }
}

namespace Server.Misc
{
    class BuildQuests
    {
        public static void SearchCreate()
        {
            Item pedestal = new SearchBase(0);
            pedestal.Delete();

            Item prisoner = new Prisoner();
            prisoner.Delete();

            ArrayList SBtargets = new ArrayList();
            foreach (Item item in World.Items.Values)
                if (item is SearchBase || (item is Prisoner))
                {
                    SBtargets.Add(item);
                }
            for (int i = 0; i < SBtargets.Count; ++i)
            {
                Item item = (Item)SBtargets[i];
                item.Delete();
            }

            int dungeons = 90;
            int area = 0;

            while (dungeons > 0)
            {
                dungeons--;
                area++;

                bool success = false;
                int maxRetry = 10;
                int attempt = 0;
                while (!success && ++attempt < maxRetry)
                {
                    if (area == 1) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Mage Mansion
                    else if (area == 2) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Isle of the Lich
                    else if (area == 3) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Altar of the Blood God
                    else if (area == 4) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the City of the Dead
                    else if (area == 5) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Mausoleum
                    else if (area == 6) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Valley of Dark Druids
                    else if (area == 7) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Vordo's Castle
                    else if (area == 8) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Crypts of Kuldar
                    else if (area == 9) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Caverns of Poseidon
                    else if (area == 10) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Zealan Tombs
                    else if (area == 11) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Argentrock Castle
                    else if (area == 12) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Hall of the Mountain King
                    else if (area == 13) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Depths of Carthax Lake
                    else if (area == 14) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Morgaelin's Inferno
                    else if (area == 15) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Tower of Brass
                    else if (area == 16) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Kuldara Sewers
                    else if (area == 17) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Daemon's Crag
                    else if (area == 18) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Ratmen Lair
                    else if (area == 19) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Ancient Pyramid
                    else if (area == 20) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Exodus
                    else if (area == 21) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Cave of Banished Mages
                    else if (area == 22) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Clues
                    else if (area == 23) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dardin's Pit
                    else if (area == 24) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Doom
                    else if (area == 25) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Fires of Hell
                    else if (area == 26) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Mines of Morinia
                    else if (area == 27) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Perinian Depths
                    else if (area == 28) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Dungeon of Time Awaits
                    else if (area == 29) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Pirate Cave
                    else if (area == 30) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Dragon's Maw
                    else if (area == 31) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Cave of the Zuluu
                    else if (area == 32) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Vault of the Black Knight
                    else if (area == 33) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Undersea Pass
                    else if (area == 34) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Castle of Dracula
                    else if (area == 35) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Crypts of Dracula
                    else if (area == 36) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Lodoria Catacombs
                    else if (area == 37) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Covetous
                    else if (area == 38) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Deceit
                    else if (area == 39) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Despise
                    else if (area == 40) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Destard
                    else if (area == 41) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the City of Embers
                    else if (area == 42) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Hythloth
                    else if (area == 43) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Frozen Hells
                    else if (area == 44) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Ice Fiend Lair
                    else if (area == 45) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Halls of Undermountain
                    else if (area == 46) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Shame
                    else if (area == 47) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Terathan Keep
                    else if (area == 48) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Volcanic Cave
                    else if (area == 49) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Wrong
                    else if (area == 50) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Blood Temple
                    else if (area == 51) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Ice Queen Fortress
                    else if (area == 52) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon of the Mad Archmage
                    else if (area == 53) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon of the Lich King
                    else if (area == 54) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Halls of Ogrimar
                    else if (area == 55) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Ratmen Mines
                    else if (area == 56) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Rock
                    else if (area == 57) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Storm Giant Lair
                    else if (area == 58) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Corrupt Pass
                    else if (area == 59) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Tombs
                    else if (area == 60) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Ancient Prison
                    else if (area == 61) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Cave of Fire
                    else if (area == 62) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Cave of Souls
                    else if (area == 63) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Ankh
                    else if (area == 64) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Bane
                    else if (area == 65) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Hate
                    else if (area == 66) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Scorn
                    else if (area == 67) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Torment
                    else if (area == 68) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Vile
                    else if (area == 69) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Wicked
                    else if (area == 70) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Dungeon Wrath
                    else if (area == 71) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Flooded Temple
                    else if (area == 72) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Gargoyle Crypts
                    else if (area == 73) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Serpent Sanctum
                    else if (area == 74) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Tomb of the Fallen Wizard
                    else if (area == 75) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Vordo's Dungeon
                    else if (area == 76) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Forgotten Halls
                    else if (area == 77) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Ancient Elven Mine
                    else if (area == 78) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Tomb of Kazibal
                    else if (area == 79) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Scurvy Reef
                    else if (area == 80) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // Stonegate Castle
                    else if (area == 81) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Undersea Castle
                    else if (area == 82) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Catacombs of Azerok
                    else if (area == 83) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Azure Castle
                    else if (area == 84) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Glacial Scar
                    else if (area == 85) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Temple of Osirus
                    else if (area == 86) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Stygian Abyss
                    else if (area == 87) { pedestal = new SearchBase(0); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Sanctum of Saltmarsh
                    else if (area == 88) { pedestal = new SearchBase(1); success = MoveQuestPedestals(pedestal, area); prisoner = new Prisoner(); MoveQuestPedestals(prisoner, area); } // the Ancient Sky Ship
                }
            }
        }

        public static bool MoveQuestPedestals(Item item, int area)
        {
            Point3D loc = new Point3D(0, 0, 0);
            Map map = null;

            if (area == 1) { loc = new Point3D(792, 312, 66); map = Map.SavagedEmpire; }

            else if (area == 2) { loc = new Point3D(1057, 434, 88); map = Map.SavagedEmpire; }

            else if (area == 3) { loc = new Point3D(1143, 971, 75); map = Map.IslesDread; }

            else if (area == 4)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the City of the Dead"
                {
                    case 1: loc = new Point3D(5683, 3261, 0); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(5669, 3291, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(5758, 3331, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(5754, 3245, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 5)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Mausoleum"
                {
                    case 1: loc = new Point3D(3992, 3295, 20); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(3882, 3282, 40); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(3831, 3363, 40); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(3964, 3453, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 6)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Valley of Dark Druids"
                {
                    case 1: loc = new Point3D(6828, 200, 5); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(6790, 146, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(6783, 184, 50); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(6792, 209, 30); map = Map.Vaelen; break;
                }
            }
            else if (area == 7)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Vordo's Castle"
                {
                    case 1: loc = new Point3D(6893, 2866, 72); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(6896, 2909, 72); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(6910, 2907, 50); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(6905, 2858, 50); map = Map.Vaelen; break;
                }
            }
            else if (area == 8)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Crypts of Kuldar"
                {
                    case 1: loc = new Point3D(6508, 2940, 55); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(6493, 2879, 45); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(6547, 2842, 50); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(6591, 2884, 45); map = Map.Vaelen; break;
                }
            }
            else if (area == 9)
            {
                switch (Utility.RandomMinMax(1, 6)) // "the Caverns of Poseidon"
                {
                    case 1: loc = new Point3D(5629, 972, 0); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(5712, 1689, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(5839, 1743, 5); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(5996, 1723, 5); map = Map.Vaelen; break;
                    case 5: loc = new Point3D(5305, 2058, 0); map = Map.Vaelen; break;
                    case 6: loc = new Point3D(5366, 2073, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 10)
            {
                switch (Utility.RandomMinMax(1, 8)) // "the Zealan Tombs"
                {
                    case 1: loc = new Point3D(6493, 672, 10); map = Map.Lodor; break;
                    case 2: loc = new Point3D(6436, 753, -20); map = Map.Lodor; break;
                    case 3: loc = new Point3D(6963, 2431, 0); map = Map.Lodor; break;
                    case 4: loc = new Point3D(7010, 2463, 0); map = Map.Lodor; break;
                    case 5: loc = new Point3D(6971, 2528, 0); map = Map.Lodor; break;
                    case 6: loc = new Point3D(6700, 2395, 0); map = Map.Lodor; break;
                    case 7: loc = new Point3D(7025, 2253, 0); map = Map.Lodor; break;
                    case 8: loc = new Point3D(6804, 2332, 5); map = Map.Lodor; break;
                }
            }
            else if (area == 11)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Argentrock Castle"
                {
                    case 1: loc = new Point3D(6085, 475, 5); map = Map.Lodor; break;
                    case 2: loc = new Point3D(6014, 684, 0); map = Map.Lodor; break;
                    case 3: loc = new Point3D(6044, 675, 0); map = Map.Lodor; break;
                    case 4: loc = new Point3D(6225, 638, -20); map = Map.Lodor; break;
                }
            }
            else if (area == 12)
            {
                switch (Utility.RandomMinMax(1, 5)) // "the Hall of the Mountain King"
                {
                    case 1: loc = new Point3D(7006, 1804, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(6885, 1979, -45); map = Map.Lodor; break;
                    case 3: loc = new Point3D(6335, 920, 25); map = Map.Lodor; break;
                    case 4: loc = new Point3D(6413, 1261, 0); map = Map.Lodor; break;
                    case 5: loc = new Point3D(6481, 1246, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 13)
            {
                switch (Utility.RandomMinMax(1, 6)) // "the Depths of Carthax Lake"
                {
                    case 1: loc = new Point3D(5868, 1688, 45); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5936, 1653, -5); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5442, 2296, 0); map = Map.Lodor; break;
                    case 4: loc = new Point3D(5488, 2279, 0); map = Map.Lodor; break;
                    case 5: loc = new Point3D(5352, 2246, 5); map = Map.Lodor; break;
                    case 6: loc = new Point3D(5336, 2351, -10); map = Map.Lodor; break;
                }
            }
            else if (area == 14)
            {
                switch (Utility.RandomMinMax(1, 3)) // "Morgaelin's Inferno"
                {
                    case 1: loc = new Point3D(6917, 1084, 15); map = Map.Lodor; break;
                    case 2: loc = new Point3D(6902, 1180, -78); map = Map.Lodor; break;
                    case 3: loc = new Point3D(6037, 2143, 30); map = Map.Lodor; break;
                }
            }
            else if (area == 15)
            {
                switch (Utility.RandomMinMax(1, 14)) // "the Tower of Brass"
                {
                    case 1: loc = new Point3D(486, 3818, 56); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(6407, 3088, 5); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(6799, 3211, -20); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(6857, 3196, 5); map = Map.Vaelen; break;
                    case 5: loc = new Point3D(6779, 3122, 3); map = Map.Vaelen; break;
                    case 6: loc = new Point3D(6274, 3444, 30); map = Map.Vaelen; break;
                    case 7: loc = new Point3D(6570, 3381, 0); map = Map.Vaelen; break;
                    case 8: loc = new Point3D(6898, 3337, 40); map = Map.Vaelen; break;
                    case 9: loc = new Point3D(6527, 3573, 0); map = Map.Vaelen; break;
                    case 10: loc = new Point3D(6952, 3582, 0); map = Map.Vaelen; break;
                    case 11: loc = new Point3D(6937, 3536, 20); map = Map.Vaelen; break;
                    case 12: loc = new Point3D(6954, 3499, 40); map = Map.Vaelen; break;
                    case 13: loc = new Point3D(6831, 3856, 5); map = Map.Vaelen; break;
                    case 14: loc = new Point3D(6269, 3938, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 16)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Kuldara Sewers"
                {
                    case 1: loc = new Point3D(6226, 2902, 10); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(6207, 2991, 5); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(6252, 3011, -25); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(6333, 2826, 6); map = Map.Vaelen; break;
                }
            }
            else if (area == 17)
            {
                switch (Utility.RandomMinMax(1, 3)) // "the Daemon's Crag"
                {
                    case 1: loc = new Point3D(6376, 2091, -5); map = Map.Lodor; break;
                    case 2: loc = new Point3D(6241, 2177, -59); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5919, 2195, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 18)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Ratmen Lair"
                {
                    case 1: loc = new Point3D(2719, 3725, 0); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(2754, 3784, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(2796, 3753, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(2747, 3750, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 19)
            {
                switch (Utility.RandomMinMax(1, 7)) // "the Ancient Pyramid"
                {
                    case 1: loc = new Point3D(5359, 918, 0); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(5288, 901, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(5323, 955, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(5368, 767, 0); map = Map.Vaelen; break;
                    case 5: loc = new Point3D(5309, 784, 0); map = Map.Vaelen; break;
                    case 6: loc = new Point3D(5243, 761, 0); map = Map.Vaelen; break;
                    case 7: loc = new Point3D(5302, 724, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 20)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Exodus"
                {
                    case 1: loc = new Point3D(5937, 584, 0); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(5975, 611, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(5939, 696, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(5883, 597, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 21)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Cave of Banished Mages"
                {
                    case 1: loc = new Point3D(230, 3509, 20); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(231, 3486, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(124, 3767, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(124, 3462, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 22)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Clues"
                {
                    case 1: loc = new Point3D(5905, 2120, 0); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(5960, 2222, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(5608, 2204, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(5611, 2120, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 23)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dardin's Pit"
                {
                    case 1: loc = new Point3D(5660, 411, 0); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(5641, 382, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(5584, 419, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(5501, 420, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 24)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Doom"
                {
                    case 1: loc = new Point3D(5374, 297, 0); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(5321, 286, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(5276, 297, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(5263, 235, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 25)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Fires of Hell"
                {
                    case 1: loc = new Point3D(5609, 1240, 1); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(5482, 1233, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(5311, 1376, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(5255, 1400, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 26)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Mines of Morinia"
                {
                    case 1: loc = new Point3D(5685, 1462, 0); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(5705, 1538, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(5623, 1512, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(5633, 1614, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 27)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Perinian Depths"
                {
                    case 1: loc = new Point3D(5912, 475, 0); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(5975, 367, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(5894, 386, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(5859, 453, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 28)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Dungeon of Time Awaits"
                {
                    case 1: loc = new Point3D(5588, 897, 0); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(5537, 879, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(5498, 849, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(5566, 795, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 29)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Pirate Cave"
                {
                    case 1: loc = new Point3D(5459, 1676, 0); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(5476, 1675, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(5473, 1697, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(5436, 1676, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 30)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Dragon's Maw"
                {
                    case 1: loc = new Point3D(4839, 3852, 5); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(4755, 3692, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(4351, 3904, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(4476, 3935, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 31)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Cave of the Zuluu"
                {
                    case 1: loc = new Point3D(6982, 3840, 5); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(6997, 3913, 25); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(6654, 3660, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(6241, 3256, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 32)
            {
                switch (Utility.RandomMinMax(1, 6)) // "the Vault of the Black Knight"
                {
                    case 1: loc = new Point3D(6521, 537, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(6626, 340, 0); map = Map.Lodor; break;
                    case 3: loc = new Point3D(6587, 159, 20); map = Map.Lodor; break;
                    case 4: loc = new Point3D(6235, 260, 0); map = Map.Lodor; break;
                    case 5: loc = new Point3D(6270, 411, 40); map = Map.Lodor; break;
                    case 6: loc = new Point3D(6289, 561, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 33)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Undersea Pass"
                {
                    case 1: loc = new Point3D(6363, 3864, 10); map = Map.Lodor; break;
                    case 2: loc = new Point3D(6323, 3425, 10); map = Map.Lodor; break;
                    case 3: loc = new Point3D(6310, 3273, 10); map = Map.Lodor; break;
                    case 4: loc = new Point3D(6310, 3346, 10); map = Map.Lodor; break;
                }
            }
            else if (area == 34)
            {
                switch (Utility.RandomMinMax(1, 8)) // "the Castle of Dracula"
                {
                    case 1: loc = new Point3D(6852, 1588, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(6966, 1499, 0); map = Map.Lodor; break;
                    case 3: loc = new Point3D(6803, 1539, 0); map = Map.Lodor; break;
                    case 4: loc = new Point3D(6876, 1582, 0); map = Map.Lodor; break;
                    case 5: loc = new Point3D(6971, 1619, 0); map = Map.Lodor; break;
                    case 6: loc = new Point3D(6877, 1468, 0); map = Map.Lodor; break;
                    case 7: loc = new Point3D(6776, 1636, 0); map = Map.Lodor; break;
                    case 8: loc = new Point3D(6997, 1639, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 35)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Crypts of Dracula"
                {
                    case 1: loc = new Point3D(5737, 2835, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5734, 2792, 0); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5766, 2722, 0); map = Map.Lodor; break;
                    case 4: loc = new Point3D(5826, 2678, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 36)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Lodoria Catacombs"
                {
                    case 1: loc = new Point3D(5501, 1804, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5555, 1888, 0); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5450, 1824, 0); map = Map.Lodor; break;
                    case 4: loc = new Point3D(5608, 1832, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 37)
            {
                switch (Utility.RandomMinMax(1, 6)) // "Dungeon Covetous"
                {
                    case 1: loc = new Point3D(5543, 2032, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5510, 1981, 0); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5431, 2021, 0); map = Map.Lodor; break;
                    case 4: loc = new Point3D(5446, 1969, 0); map = Map.Lodor; break;
                    case 5: loc = new Point3D(5592, 2493, 40); map = Map.Lodor; break;
                    case 6: loc = new Point3D(5769, 2569, -20); map = Map.Lodor; break;
                }
            }
            else if (area == 38)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Deceit"
                {
                    case 1: loc = new Point3D(5309, 648, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5316, 735, 0); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5264, 664, 0); map = Map.Lodor; break;
                    case 4: loc = new Point3D(5144, 712, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 39)
            {
                switch (Utility.RandomMinMax(1, 6)) // "Dungeon Despise"
                {
                    case 1: loc = new Point3D(5432, 847, 45); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5514, 935, 20); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5553, 815, 47); map = Map.Lodor; break;
                    case 4: loc = new Point3D(5539, 916, 29); map = Map.Lodor; break;
                    case 5: loc = new Point3D(5500, 2426, 10); map = Map.Lodor; break;
                    case 6: loc = new Point3D(5175, 2428, 45); map = Map.Lodor; break;
                }
            }
            else if (area == 40)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Destard"
                {
                    case 1: loc = new Point3D(5192, 1007, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5133, 833, 0); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5253, 913, -23); map = Map.Lodor; break;
                    case 4: loc = new Point3D(5259, 790, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 41)
            {
                switch (Utility.RandomMinMax(1, 3)) // "the City of Embers"
                {
                    case 1: loc = new Point3D(5726, 1296, 2); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5649, 1406, 0); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5680, 1432, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 42)
            {
                switch (Utility.RandomMinMax(1, 2)) // "Dungeon Hythloth"
                {
                    case 1: loc = new Point3D(6112, 222, 22); map = Map.Lodor; break;
                    case 2: loc = new Point3D(6105, 32, 27); map = Map.Lodor; break;
                    case 3: loc = new Point3D(6048, 157, 0); map = Map.Lodor; break;
                    case 4: loc = new Point3D(5912, 233, 44); map = Map.Lodor; break;
                    case 5: loc = new Point3D(5964, 80, 0); map = Map.Lodor; break;
                    case 6: loc = new Point3D(6087, 168, 0); map = Map.Lodor; break;
                    case 7: loc = new Point3D(6111, 90, 0); map = Map.Lodor; break;
                    case 8: loc = new Point3D(6058, 51, 0); map = Map.Lodor; break;
                    case 9: loc = new Point3D(5958, 220, 22); map = Map.Lodor; break;
                    case 10: loc = new Point3D(5984, 149, 0); map = Map.Lodor; break;
                    case 11: loc = new Point3D(5997, 56, 22); map = Map.Lodor; break;
                    case 12: loc = new Point3D(5936, 96, 22); map = Map.Lodor; break;
                }
            }
            else if (area == 43)
            {
                switch (Utility.RandomMinMax(1, 2)) // "the Frozen Hells"
                {
                    case 1: loc = new Point3D(5705, 169, -4); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5672, 176, -6); map = Map.Lodor; break;
                }
            }
            else if (area == 44)
            {
                switch (Utility.RandomMinMax(1, 2)) // "the Ice Fiend Lair"
                {
                    case 1: loc = new Point3D(5681, 332, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5656, 302, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 45)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Halls of Undermountain"
                {
                    case 1: loc = new Point3D(5333, 472, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5325, 393, 0); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5245, 397, 0); map = Map.Lodor; break;
                    case 4: loc = new Point3D(5235, 439, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 46)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Shame"
                {
                    case 1: loc = new Point3D(5818, 77, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5851, 106, 10); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5661, 112, 11); map = Map.Lodor; break;
                    case 4: loc = new Point3D(5434, 180, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 47)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Terathan Keep"
                {
                    case 1: loc = new Point3D(5282, 1551, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5128, 1584, 0); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5144, 1713, 0); map = Map.Lodor; break;
                    case 4: loc = new Point3D(5338, 1770, -125); map = Map.Lodor; break;
                }
            }
            else if (area == 48)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Volcanic Cave"
                {
                    case 1: loc = new Point3D(5988, 3426, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5934, 3403, 0); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5862, 3424, 0); map = Map.Lodor; break;
                    case 4: loc = new Point3D(5996, 3511, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 49)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Wrong"
                {
                    case 1: loc = new Point3D(5547, 1294, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5419, 1311, 0); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5453, 1360, 5); map = Map.Lodor; break;
                    case 4: loc = new Point3D(5448, 1440, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 50)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Blood Temple"
                {
                    case 1: loc = new Point3D(758, 2526, -28); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(697, 2549, -28); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(779, 2571, -28); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(723, 2624, -28); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 51)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Ice Queen Fortress"
                {
                    case 1: loc = new Point3D(439, 2788, 22); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(267, 2762, 44); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(341, 2768, 22); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(296, 2851, 22); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 52)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon of the Mad Archmage"
                {
                    case 1: loc = new Point3D(768, 1921, -28); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(649, 1942, -28); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(612, 2003, -29); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(552, 2039, -28); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 53)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon of the Lich King"
                {
                    case 1: loc = new Point3D(346, 2143, -1); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(499, 2141, -1); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(510, 2337, 0); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(335, 2326, -1); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 54)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Halls of Ogrimar"
                {
                    case 1: loc = new Point3D(1155, 2360, -28); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(937, 2411, -28); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(972, 2342, -28); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(1083, 2361, 2); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 55)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Ratmen Mines"
                {
                    case 1: loc = new Point3D(988, 2185, -3); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(897, 2127, -28); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(898, 2187, -28); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(1030, 2140, 22); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 56)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Rock"
                {
                    case 1: loc = new Point3D(641, 2327, -32); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(609, 2223, -32); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(704, 2178, -32); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(743, 2307, -32); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 57)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Storm Giant Lair"
                {
                    case 1: loc = new Point3D(569, 2751, -28); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(619, 2742, -28); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(594, 2678, -28); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(621, 2717, -28); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 58)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Corrupt Pass"
                {
                    case 1: loc = new Point3D(64, 2421, -28); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(11, 2420, -28); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(51, 2328, -30); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(147, 2201, -30); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 59)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Tombs"
                {
                    case 1: loc = new Point3D(76, 2528, -28); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(124, 2560, -23); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(96, 2777, -28); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(24, 2669, -28); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 60)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Ancient Prison"
                {
                    case 1: loc = new Point3D(1980, 475, 0); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(2002, 554, 0); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(1945, 525, 0); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(1948, 387, 0); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 61)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Cave of Fire"
                {
                    case 1: loc = new Point3D(2039, 863, 0); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(2061, 918, 0); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(2106, 900, 0); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(2151, 870, 0); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 62)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Cave of Souls"
                {
                    case 1: loc = new Point3D(2443, 186, 0); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(2447, 156, 0); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(2503, 162, 0); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(2482, 87, 0); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 63)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Ankh"
                {
                    case 1: loc = new Point3D(2067, 179, 0); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(2078, 205, 0); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(2049, 202, 0); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(2058, 46, 0); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 64)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Bane"
                {
                    case 1: loc = new Point3D(1924, 188, 2); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(1970, 156, 2); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(1968, 222, 0); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(1909, 50, 0); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 65)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Hate"
                {
                    case 1: loc = new Point3D(2235, 506, 2); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(2160, 479, 2); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(2219, 398, 0); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(2197, 390, 0); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 66)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Scorn"
                {
                    case 1: loc = new Point3D(2214, 849, 0); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(2238, 862, 0); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(2234, 812, 0); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(2192, 843, 0); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 67)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Torment"
                {
                    case 1: loc = new Point3D(1978, 834, 0); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(1976, 810, 0); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(1933, 815, 0); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(1935, 853, 0); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 68)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Vile"
                {
                    case 1: loc = new Point3D(2315, 502, 20); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(2360, 498, 0); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(2360, 393, 0); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(2334, 403, 0); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 69)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Wicked"
                {
                    case 1: loc = new Point3D(2152, 167, 2); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(2177, 185, 0); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(2182, 239, 2); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(2205, 165, 2); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 70)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Dungeon Wrath"
                {
                    case 1: loc = new Point3D(2343, 839, 0); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(2299, 833, 2); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(2299, 892, 0); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(2332, 866, 2); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 71)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Flooded Temple"
                {
                    case 1: loc = new Point3D(2494, 838, 0); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(2484, 875, 0); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(2455, 874, 0); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(2428, 718, 2); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 72)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Gargoyle Crypts"
                {
                    case 1: loc = new Point3D(2089, 503, 0); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(2079, 475, 0); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(2108, 534, 0); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(2096, 535, 0); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 73)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Serpent Sanctum"
                {
                    case 1: loc = new Point3D(2500, 500, 0); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(2516, 462, 0); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(2448, 465, 0); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(2508, 569, 0); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 74)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Tomb of the Fallen Wizard"
                {
                    case 1: loc = new Point3D(2371, 240, 2); map = Map.SerpentIsland; break;
                    case 2: loc = new Point3D(2330, 207, 2); map = Map.SerpentIsland; break;
                    case 3: loc = new Point3D(2356, 42, 0); map = Map.SerpentIsland; break;
                    case 4: loc = new Point3D(2288, 74, 0); map = Map.SerpentIsland; break;
                }
            }
            else if (area == 75)
            {
                switch (Utility.RandomMinMax(1, 4)) // "Vordo's Dungeon"
                {
                    case 1: loc = new Point3D(6469, 713, 0); map = Map.Vaelen; break;
                    case 2: loc = new Point3D(6278, 468, 0); map = Map.Vaelen; break;
                    case 3: loc = new Point3D(6442, 451, 0); map = Map.Vaelen; break;
                    case 4: loc = new Point3D(6286, 510, 0); map = Map.Vaelen; break;
                }
            }
            else if (area == 76)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Forgotten Halls"
                {
                    case 1: loc = new Point3D(532, 3573, 0); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(169, 3364, 0); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(92, 3410, 0); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(56, 3251, 20); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 77)
            {
                loc = new Point3D(6818, 2691, 0); map = Map.Lodor; // "the Ancient Elven Mine"
            }
            else if (area == 78)
            {
                switch (Utility.RandomMinMax(1, 6)) // "the Tomb of Kazibal"
                {
                    case 1: loc = new Point3D(477, 3388, 0); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(490, 3310, 0); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(467, 3322, 0); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(415, 3319, 0); map = Map.SavagedEmpire; break;
                    case 5: loc = new Point3D(424, 3287, 35); map = Map.SavagedEmpire; break;
                    case 6: loc = new Point3D(424, 3281, 15); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 79)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Scurvy Reef"
                {
                    case 1: loc = new Point3D(396, 3905, 0); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(441, 3825, 5); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(400, 3969, 30); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(394, 4060, 0); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 80)
            {
                switch (Utility.RandomMinMax(1, 5)) // "Stonegate Castle"
                {
                    case 1: loc = new Point3D(6274, 2506, 0); map = Map.Lodor; break;
                    case 2: loc = new Point3D(6418, 2599, 0); map = Map.Lodor; break;
                    case 3: loc = new Point3D(6255, 2409, 0); map = Map.Lodor; break;
                    case 4: loc = new Point3D(6823, 2850, 0); map = Map.Lodor; break;
                    case 5: loc = new Point3D(6354, 2495, 60); map = Map.Lodor; break;
                }
            }
            else if (area == 81)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Undersea Castle"
                {
                    case 1: loc = new Point3D(692, 3814, -5); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(642, 3852, 39); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(681, 4063, -5); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(925, 3256, 40); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 82)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Catacombs of Azerok"
                {
                    case 1: loc = new Point3D(774, 3394, 20); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(750, 3436, 20); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(800, 3412, 20); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(782, 3285, 0); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 83)
            {
                switch (Utility.RandomMinMax(1, 3)) // "the Azure Castle"
                {
                    case 1: loc = new Point3D(225, 3642, 5); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(320, 3636, 0); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(271, 3616, 50); map = Map.SavagedEmpire; break;
                }
            }
            else if (area == 84)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Glacial Scar"
                {
                    case 1: loc = new Point3D(2183, 1451, 70); map = Map.Underworld; break;
                    case 2: loc = new Point3D(1950, 1524, -14); map = Map.Underworld; break;
                    case 3: loc = new Point3D(1719, 1520, 10); map = Map.Underworld; break;
                    case 4: loc = new Point3D(2181, 1295, 40); map = Map.Underworld; break;
                }
            }
            else if (area == 85)
            {
                switch (Utility.RandomMinMax(1, 8)) // "the Temple of Osirus"
                {
                    case 1: loc = new Point3D(6081, 3855, -40); map = Map.Lodor; break;
                    case 2: loc = new Point3D(6240, 3797, -5); map = Map.Lodor; break;
                    case 3: loc = new Point3D(6223, 3869, -5); map = Map.Lodor; break;
                    case 4: loc = new Point3D(6113, 950, 5); map = Map.Lodor; break;
                    case 5: loc = new Point3D(6084, 3965, 57); map = Map.Lodor; break;
                    case 6: loc = new Point3D(6146, 3662, -40); map = Map.Lodor; break;
                    case 7: loc = new Point3D(6130, 3651, -40); map = Map.Lodor; break;
                    case 8: loc = new Point3D(6138, 3638, -40); map = Map.Lodor; break;
                }
            }
            else if (area == 86)
            {
                switch (Utility.RandomMinMax(1, 5)) // "the Stygian Abyss"
                {
                    case 1: loc = new Point3D(2013, 1161, -8); map = Map.Underworld; break;
                    case 2: loc = new Point3D(1669, 1220, -43); map = Map.Underworld; break;
                    case 3: loc = new Point3D(1791, 1330, -42); map = Map.Underworld; break;
                    case 4: loc = new Point3D(1870, 1276, -37); map = Map.Underworld; break;
                    case 5: loc = new Point3D(1793, 1166, -42); map = Map.Underworld; break;
                }
            }
            else if (area == 87)
            {
                switch (Utility.RandomMinMax(1, 4)) // "the Sanctum of Saltmarsh"
                {
                    case 1: loc = new Point3D(6136, 1311, 10); map = Map.Lodor; break;
                    case 2: loc = new Point3D(5711, 2056, -60); map = Map.Lodor; break;
                    case 3: loc = new Point3D(5748, 2086, 0); map = Map.Lodor; break;
                    case 4: loc = new Point3D(6014, 1980, 0); map = Map.Lodor; break;
                }
            }
            else if (area == 88)
            {
                switch (Utility.RandomMinMax(1, 6)) // "the Ancient Sky Ship"
                {
                    case 1: loc = new Point3D(908, 4028, 0); map = Map.SavagedEmpire; break;
                    case 2: loc = new Point3D(1148, 3652, 0); map = Map.SavagedEmpire; break;
                    case 3: loc = new Point3D(1211, 3895, 0); map = Map.SavagedEmpire; break;
                    case 4: loc = new Point3D(1196, 4009, 0); map = Map.SavagedEmpire; break;
                    case 5: loc = new Point3D(1015, 3909, 0); map = Map.SavagedEmpire; break;
                    case 6: loc = new Point3D(1150, 3764, 0); map = Map.SavagedEmpire; break;
                }
            }

            int AlwaysAllow = 1;
            if (item is Prisoner) { AlwaysAllow = 0; }

            if (item != null && loc.X > 0 && loc.Y > 0 && map != null && CanUseSpot(loc, map, AlwaysAllow))
            {
                item.MoveToWorld(loc, map);
                return true;
            }
            else if (item != null)
            {
                item.Delete();
            }

            return false;
        }

        public static bool CanUseSpot(Point3D loc, Map map, int priority)
        {
            bool CanUse = true;

            if (Utility.Random(5) > 0 && priority != 1)
            {
                CanUse = false;
            }
            else if (loc.X > 0 && loc.Y > 0 && map != null)
            {
                IPooledEnumerable eable = map.GetItemsInRange(loc, 0);

                foreach (Item item in eable)
                {
                    CanUse = false;
                }

                eable.Free();
            }

            return CanUse;
        }
    }
}

namespace Server.Scripts.Commands
{
    public class BuildWorld
    {
        public static void Initialize()
        {
            CommandSystem.Register("BuildWorld", AccessLevel.Counselor, new CommandEventHandler(BuildWorlds));
        }

        [Usage("BuildWorld")]
        [Description("This cleans up the world and rebuilds it, leaving players intact.")]
        public static void BuildWorlds(CommandEventArgs e)
        {

            Server.Multis.BaseBoat.ClearShip(); // CLEAR THE NPC SHIPS

            int DungeonHomesDecorated = 0;

            if (MySettings.ConsoleLog) { Console.WriteLine("Delete Spawners, Cauldrons, and Pools..."); }
            ArrayList targets = new ArrayList();
            foreach (Item item in World.Items.Values)
                if (item is PremiumSpawner || item is BrewCauldron || item is PotionCauldron || item is MagicPool)
                {
                    targets.Add(item);
                }
                else if (item.Weight == -3.0) // DECORATE DUNGEON HOMES IF THEY ARE NOT ALREADY
                {
                    DungeonHomesDecorated++;
                }
            for (int i = 0; i < targets.Count; ++i)
            {
                Item item = (Item)targets[i];
                item.Delete();
            }

            if (MySettings.ConsoleLog) { Console.WriteLine("Delete Creatures and Citizens..."); }
            ArrayList beings = new ArrayList();
            foreach (Mobile being in World.Mobiles.Values)
                if (being is BaseCreature)
                {
                    BaseCreature bc = (BaseCreature)being;

                    if (bc.Home.X > 0 && !bc.IsStabled && !bc.Controlled && bc.ControlMaster == null)
                        beings.Add(being);

                    if (bc is Citizens)
                        beings.Add(being);
                }
            for (int i = 0; i < beings.Count; ++i)
            {
                Mobile being = (Mobile)beings[i];
                being.Delete();
            }

            //if (MySettings.ConsoleLog) { Console.WriteLine("Decorate Dungeon Homes..."); }
            Server.Commands.Decorate.Decorate_OnCommand(e);
            //if (DungeonHomesDecorated == 0) { Server.Commands.Monopoly.Monopoly_OnCommand(e); }

            if (MySettings.ConsoleLog) { Console.WriteLine("Spawn Animals..."); }
            Server.SpawnGenerator.Parse(e.Mobile, "animals.map");
            if (MySettings.ConsoleLog) { Console.WriteLine("Spawn Bastion..."); }
            Server.SpawnGenerator.Parse(e.Mobile, "bastion.map");

            if (MySettings.ConsoleLog) { Console.WriteLine("Respawn Regions..."); }
            Server.Regions.SpawnEntry.RespawnAllRegions_OnCommand(e);

            if (MySettings.ConsoleLog) { Console.WriteLine("Build Citizens..."); }
            Server.Mobiles.Citizens.PopulateCities();
            if (MySettings.ConsoleLog) { Console.WriteLine("Build Drinkers..."); }
            Server.Items.TavernTable.PopulateHomes();
            if (MySettings.ConsoleLog) { Console.WriteLine("Build Workers..."); }
            Server.Items.WorkingSpots.PopulateVillages();
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "Remove Stealables..." ); }
			Server.Items.StealableArtifactsSpawner.RemoveStealArties_OnCommand( e );
			if ( MySettings.ConsoleLog ){ Console.WriteLine( "Create Stealables..." ); }
			Server.Items.StealableArtifactsSpawner.GenStealArties_OnCommand( e );

            // CLEAR THESE OUT AT CREATION TIME BECAUSE THEY DUPLICATE FOR SOME REASON
            if (MySettings.ConsoleLog) { Console.WriteLine("Delete Spawners, Cauldrons, and Pools...Again..."); }
            ArrayList specials = new ArrayList();
            foreach (Item item in World.Items.Values)
                if (item is BrewCauldron || item is PotionCauldron || item is MagicPool)
                {
                    specials.Add(item);
                }
            for (int i = 0; i < specials.Count; ++i)
            {
                Item item = (Item)specials[i];
                item.Delete();
            }
            /*
            if (MySettings.ConsoleLog) { Console.WriteLine("Build Coffers..."); }
            Server.Items.Coffer.ConfigureAllThiefQuestItems();

            if (MySettings.ConsoleLog) { Console.WriteLine("Build Basement Doors..."); }
            Server.Items.BasementDoor.ConfigureBasementDoors();

            // DO INITIAL SETUP FOR MAGIC MIRRORS
            if (MySettings.ConsoleLog) { Console.WriteLine("Build Magic Mirrors..."); }
            Server.Items.MagicMirror.SetMirrors();

            if (MySettings.ConsoleLog) { Console.WriteLine("Rebuild ML Quest Spawners..."); }
            Engines.MLQuests.MLQuestSystem.MLQuestsClearSpawners_OnCommand(e);
            Engines.MLQuests.MLQuestSystem.MLQuestsGenerate_OnCommand(e);
            */
            if (MySettings.ConsoleLog) { Console.WriteLine("World Has Been Rebuilt!"); }
            e.Mobile.SendMessage("The world has been rebuilt.");
        }
    }
}
