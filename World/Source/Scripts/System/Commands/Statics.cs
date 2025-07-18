using System;
using System.IO;
using System.Collections;
using Server;
using Server.Gumps;
using Server.Items;
using Server.Commands;
using Server.Targeting;
using System.Collections.Generic;

namespace Server
{
	public class Statics
	{
		private static int m_ExportID = 0x0; // SET THIS TO A SPECIFIC ITEM ID YOU WANT FROZEN
		
		public static void Initialize()
		{
			CommandSystem.Register( "Freeze-SetId", AccessLevel.Administrator, new CommandEventHandler( FreezeSetId_OnCommand ) );
			CommandSystem.Register( "Freeze", AccessLevel.Administrator, new CommandEventHandler( Freeze_OnCommand ) );
			CommandSystem.Register( "FreezeMap", AccessLevel.Administrator, new CommandEventHandler( FreezeMap_OnCommand ) );
			CommandSystem.Register( "FreezeWorld", AccessLevel.Administrator, new CommandEventHandler( FreezeWorld_OnCommand ) );

			CommandSystem.Register( "Unfreeze", AccessLevel.Administrator, new CommandEventHandler( Unfreeze_OnCommand ) );
			CommandSystem.Register( "UnfreezeMap", AccessLevel.Administrator, new CommandEventHandler( UnfreezeMap_OnCommand ) );
			CommandSystem.Register( "UnfreezeWorld", AccessLevel.Administrator, new CommandEventHandler( UnfreezeWorld_OnCommand ) );
		}

		[Usage( "Freeze-SetId" )]
		[Description( "Sets a specific item ID to Freeze." )]
		public static void FreezeSetId_OnCommand( CommandEventArgs e )
        {
            if ( e.Arguments.Length != 1 )
            {
                e.Mobile.SendMessage("You must specify an item ID to Freeze or 0 to clear the current item ID.");
                return;
            }

            string itemIdArg = e.Arguments[0];
            int itemId;
            if (!int.TryParse(itemIdArg, out itemId))
            {
                e.Mobile.SendMessage("Item ID must be a valid number.");
                return;
            }

			m_ExportID = itemId;
			if ( itemId == 0 )
			{
				e.Mobile.SendMessage("The Freezable Item ID has been cleared.");
			}
			else
			{
				e.Mobile.SendMessage("The Freezable Item ID has been set to '{0}'.", itemId);
			}
		}

		private static Point3D NullP3D = new Point3D( int.MinValue, int.MinValue, int.MinValue );

		[Usage( "Freeze" )]
		[Description( "Makes a targeted area of dynamic items static." )]
		public static void Freeze_OnCommand( CommandEventArgs e )
		{
			BoundingBoxPicker.Begin( e.Mobile, new BoundingBoxCallback( FreezeBox_Callback ), null );
		}

		[Usage( "FreezeMap" )]
		[Description( "Makes every dynamic item in your map static." )]
		public static void FreezeMap_OnCommand( CommandEventArgs e )
		{
			Map map = e.Mobile.Map;

			if ( map != null && map != Map.Internal )
				SendWarning( e.Mobile, "You are about to freeze <u>all items in {0}</u>.", BaseFreezeWarning, map, NullP3D, NullP3D, new WarningGumpCallback( FreezeWarning_Callback ) );
		}

		[Usage( "FreezeWorld" )]
		[Description( "Makes every dynamic item on all maps static." )]
		public static void FreezeWorld_OnCommand( CommandEventArgs e )
		{
			SendWarning( e.Mobile, "You are about to freeze <u>every item on every map</u>.", BaseFreezeWarning, null, NullP3D, NullP3D, new WarningGumpCallback( FreezeWarning_Callback ) );
		}

		public static void SendWarning( Mobile m, string header, string baseWarning, Map map, Point3D start, Point3D end, WarningGumpCallback callback )
		{
			m.SendGump( new WarningGump( 1060635, 30720, String.Format( baseWarning, String.Format( header, map ) ), 0xFFC000, 420, 400, callback, new StateInfo( map, start, end ) ) );
		}

		private const string BaseFreezeWarning =	"{0}  " +
													"Those items <u>will be removed from the world</u> and placed into the server data files.  " +
													"Other players <u>will not see the changes</u> unless you distribute your data files to them.<br><br>" +
													"This operation may not complete unless the server and client are using different data files.  " +
													"If you receive a message stating 'output data files could not be opened,' then you are probably sharing data files.  " +
													"Create a new directory for the world data files (statics*.mul and staidx*.mul) and add that to Scritps/Misc/DataPath.cs.<br><br>" +
													"The change will be in effect immediately on the server, however, you must restart your client and update it's data files for the changes to become visible.  " +
													"It is strongly recommended that you make backup of the data files mentioned above.  " +
													"Do you wish to proceed?";

		private static void FreezeBox_Callback( Mobile from, Map map, Point3D start, Point3D end, object state )
		{
			SendWarning( from, "You are about to freeze a section of items.", BaseFreezeWarning, map, start, end, new WarningGumpCallback( FreezeWarning_Callback ) );
		}

		private static void FreezeWarning_Callback( Mobile from, bool okay, object state )
		{
			if ( !okay )
				return;

			StateInfo si = (StateInfo)state;

			Freeze( from, si.m_Map, si.m_Start, si.m_End );
		}

		public static void Freeze( Mobile from, Map targetMap, Point3D start3d, Point3D end3d )
		{
			Hashtable mapTable = new Hashtable();
			
			var id = m_ExportID;
			if ( id != 0 )
			{
				from.SendMessage("The '[Freeze-SetId {0}' command has been used. Only items with this ID will be frozen.", id);
			}

			if ( start3d == NullP3D && end3d == NullP3D )
			{
				if ( targetMap == null )
					CommandLogging.WriteLine( from, "{0} {1} invoking freeze for every item in every map", from.AccessLevel, CommandLogging.Format( from ) );
				else
					CommandLogging.WriteLine( from, "{0} {1} invoking freeze for every item in {0}", from.AccessLevel, CommandLogging.Format( from ), targetMap );

				foreach ( Item item in World.Items.Values )
				{
					if ( id == 0 || id == item.ItemID )
					{
						if ( targetMap != null && item.Map != targetMap )
							continue;

						if ( item.Parent != null )
							continue;

						if ( id > 0 || item is Static || item is BaseFloor || item is BaseWall || item is AddonComponent ) //
						{
							Map itemMap = item.Map;

							if ( itemMap == null || itemMap == Map.Internal )
								continue;

							Hashtable table = (Hashtable)mapTable[itemMap];

							if ( table == null )
								mapTable[itemMap] = table = new Hashtable();

							Point2D p = new Point2D( item.X >> 3, item.Y >> 3 );

							DeltaState state = (DeltaState)table[p];

							if ( state == null )
								table[p] = state = new DeltaState( p );

							state.m_List.Add( item );
						}
					}
				}
			}
			else if ( targetMap != null )
			{
				Point2D start = targetMap.Bound( new Point2D( start3d ) ), end = targetMap.Bound( new Point2D( end3d ) );

				CommandLogging.WriteLine( from, "{0} {1} invoking freeze from {2} to {3} in {4}", from.AccessLevel, CommandLogging.Format( from ), start, end, targetMap );

				IPooledEnumerable eable = targetMap.GetItemsInBounds( new Rectangle2D( start.X, start.Y, end.X - start.X + 1, end.Y - start.Y + 1 ) );

				foreach ( Item item in eable )
				{
					if ( id == 0 || id == item.ItemID )
					{
						if ( id > 0 || item is Static || item is BaseFloor || item is BaseWall || item is AddonComponent )
						{
							Map itemMap = item.Map;

							if ( itemMap == null || itemMap == Map.Internal )
								continue;

							Hashtable table = (Hashtable)mapTable[itemMap];

							if ( table == null )
								mapTable[itemMap] = table = new Hashtable();

							Point2D p = new Point2D( item.X >> 3, item.Y >> 3 );

							DeltaState state = (DeltaState)table[p];

							if ( state == null )
								table[p] = state = new DeltaState( p );

							state.m_List.Add( item );
						}
					}
				}

				eable.Free();
			}

			if ( mapTable.Count == 0 )
			{
				from.SendGump( new NoticeGump( 1060637, 30720, "No freezable items were found.  Only the following item types are frozen:<br> - Static<br> - BaseFloor<br> - BaseWall", 0xFFC000, 320, 240, null, null ) );
				return;
			}

			bool badDataFile = false;

			int totalFrozen = 0;

			foreach ( DictionaryEntry de in mapTable )
			{
				Map map = (Map)de.Key;
				Hashtable table = (Hashtable)de.Value;

				TileMatrix matrix = map.Tiles;

				using ( FileStream idxStream = OpenWrite( matrix.IndexStream ) )
				{
					using ( FileStream mulStream = OpenWrite( matrix.DataStream ) )
					{
						if ( idxStream == null || mulStream == null )
						{
							badDataFile = true;
							continue;
						}

						BinaryReader idxReader = new BinaryReader( idxStream );

						BinaryWriter idxWriter = new BinaryWriter( idxStream );
						BinaryWriter mulWriter = new BinaryWriter( mulStream );

						foreach ( DeltaState state in table.Values )
						{
							int oldTileCount;
							StaticTile[] oldTiles = ReadStaticBlock( idxReader, mulStream, state.m_X, state.m_Y, matrix.BlockWidth, matrix.BlockHeight, out oldTileCount );

							if ( oldTileCount < 0 )
								continue;

							int newTileCount = 0;
							StaticTile[] newTiles = new StaticTile[state.m_List.Count];

							for ( int i = 0; i < state.m_List.Count; ++i )
							{
								Item item = state.m_List[i];

								int xOffset = item.X - (state.m_X * 8);
								int yOffset = item.Y - (state.m_Y * 8);

								if ( xOffset < 0 || xOffset >= 8 || yOffset < 0 || yOffset >= 8 )
									continue;

								StaticTile newTile = new StaticTile( (ushort)item.ItemID, (byte)xOffset, (byte)yOffset, (sbyte)item.Z, (short)item.Hue );

								newTiles[newTileCount++] = newTile;

								item.Delete();

								++totalFrozen;
							}

							int mulPos = -1;
							int length = -1;
							int extra = 0;

							if ( (oldTileCount + newTileCount) > 0 )
							{
								mulWriter.Seek( 0, SeekOrigin.End );

								mulPos = (int)mulWriter.BaseStream.Position;
								length = (oldTileCount + newTileCount) * 7;
								extra = 1;

								for ( int i = 0; i < oldTileCount; ++i )
								{
									StaticTile toWrite = oldTiles[i];

									mulWriter.Write( (ushort) toWrite.ID );
									mulWriter.Write( (byte) toWrite.X );
									mulWriter.Write( (byte) toWrite.Y );
									mulWriter.Write( (sbyte) toWrite.Z );
									mulWriter.Write( (short) toWrite.Hue );
								}

								for ( int i = 0; i < newTileCount; ++i )
								{
									StaticTile toWrite = newTiles[i];

									mulWriter.Write( (ushort) toWrite.ID );
									mulWriter.Write( (byte) toWrite.X );
									mulWriter.Write( (byte) toWrite.Y );
									mulWriter.Write( (sbyte) toWrite.Z );
									mulWriter.Write( (short) toWrite.Hue );
								}

								mulWriter.Flush();
							}

							int idxPos = ((state.m_X * matrix.BlockHeight) + state.m_Y) * 12;

							idxWriter.Seek( idxPos, SeekOrigin.Begin );
							idxWriter.Write( mulPos );
							idxWriter.Write( length );
							idxWriter.Write( extra );

							idxWriter.Flush();

							matrix.SetStaticBlock( state.m_X, state.m_Y, null );
						}
					}
				}
			}

			if ( totalFrozen == 0 && badDataFile )
				from.SendGump( new NoticeGump( 1060637, 30720, "Output data files could not be opened and the freeze operation has been aborted.<br><br>This probably means your server and client are using the same data files.  Instructions on how to resolve this can be found in the first warning window.", 0xFFC000, 320, 240, null, null ) );
			else
				from.SendGump( new NoticeGump( 1060637, 30720, String.Format( "Freeze operation completed successfully.<br><br>{0} item{1} frozen.<br><br>You must restart your client and update it's data files to see the changes.", totalFrozen, totalFrozen != 1 ? "s were" : " was" ), 0xFFC000, 320, 240, null, null ) );
		}

		private const string BaseUnfreezeWarning =	"{0}  " +
													"Those items <u>will be removed from the static files</u> and exchanged with unmovable dynamic items.  " +
													"Other players <u>will not see the changes</u> unless you distribute your data files to them.<br><br>" +
													"This operation may not complete unless the server and client are using different data files.  " +
													"If you receive a message stating 'output data files could not be opened,' then you are probably sharing data files.  " +
													"Create a new directory for the world data files (statics*.mul and staidx*.mul) and add that to Scritps/Misc/DataPath.cs.<br><br>" +
													"The change will be in effect immediately on the server, however, you must restart your client and update it's data files for the changes to become visible.  " +
													"It is strongly recommended that you make backup of the data files mentioned above.  " +
													"Do you wish to proceed?";

		[Usage( "Unfreeze" )]
		[Description( "Makes a targeted area of static items dynamic." )]
		public static void Unfreeze_OnCommand( CommandEventArgs e )
		{
			BoundingBoxPicker.Begin( e.Mobile, new BoundingBoxCallback( UnfreezeBox_Callback ), null );
		}

		[Usage( "UnfreezeMap" )]
		[Description( "Makes every static item in your map dynamic." )]
		public static void UnfreezeMap_OnCommand( CommandEventArgs e )
		{
			Map map = e.Mobile.Map;

			if ( map != null && map != Map.Internal )
				SendWarning( e.Mobile, "You are about to unfreeze <u>all items in {0}</u>.", BaseUnfreezeWarning, map, NullP3D, NullP3D, new WarningGumpCallback( UnfreezeWarning_Callback ) );
		}

		[Usage( "UnfreezeWorld" )]
		[Description( "Makes every static item on all maps dynamic." )]
		public static void UnfreezeWorld_OnCommand( CommandEventArgs e )
		{
			SendWarning( e.Mobile, "You are about to unfreeze <u>every item on every map</u>.", BaseUnfreezeWarning, null, NullP3D, NullP3D, new WarningGumpCallback( UnfreezeWarning_Callback ) );
		}

		private static void UnfreezeBox_Callback( Mobile from, Map map, Point3D start, Point3D end, object state )
		{
			SendWarning( from, "You are about to unfreeze a section of items.", BaseUnfreezeWarning, map, start, end, new WarningGumpCallback( UnfreezeWarning_Callback ) );
		}

		private static void UnfreezeWarning_Callback( Mobile from, bool okay, object state )
		{
			if ( !okay )
				return;

			StateInfo si = (StateInfo)state;

			Unfreeze( from, si.m_Map, si.m_Start, si.m_End );
		}

		private static void DoUnfreeze( Map map, Point2D start, Point2D end, ref bool badDataFile, ref int totalUnfrozen )
		{
			start = map.Bound( start );
			end = map.Bound( end );

			int xStartBlock = start.X >> 3;
			int yStartBlock = start.Y >> 3;
			int xEndBlock = end.X >> 3;
			int yEndBlock = end.Y >> 3;

			int xTileStart = start.X, yTileStart = start.Y;
			int xTileWidth = end.X - start.X + 1, yTileHeight = end.Y - start.Y + 1;

			TileMatrix matrix = map.Tiles;

			using ( FileStream idxStream = OpenWrite( matrix.IndexStream ) )
			{
				using ( FileStream mulStream = OpenWrite( matrix.DataStream ) )
				{
					if ( idxStream == null || mulStream == null )
					{
						badDataFile = true;
						return;
					}

					BinaryReader idxReader = new BinaryReader( idxStream );

					BinaryWriter idxWriter = new BinaryWriter( idxStream );
					BinaryWriter mulWriter = new BinaryWriter( mulStream );

					for ( int x = xStartBlock; x <= xEndBlock; ++x )
					{
						for ( int y = yStartBlock; y <= yEndBlock; ++y )
						{
							int oldTileCount;
							StaticTile[] oldTiles = ReadStaticBlock( idxReader, mulStream, x, y, matrix.BlockWidth, matrix.BlockHeight, out oldTileCount );

							if ( oldTileCount < 0 )
								continue;

							int newTileCount = 0;
							StaticTile[] newTiles = new StaticTile[oldTileCount];

							int baseX = (x << 3) - xTileStart, baseY = (y << 3) - yTileStart;

							for ( int i = 0; i < oldTileCount; ++i )
							{
								StaticTile oldTile = oldTiles[i];

								int px = baseX + oldTile.X;
								int py = baseY + oldTile.Y;

								if ( px < 0 || px >= xTileWidth || py < 0 || py >= yTileHeight )
								{
									newTiles[newTileCount++] = oldTile;
								}
								else
								{
									++totalUnfrozen;

									Item item = new Static( oldTile.ID );

									item.Hue = oldTile.Hue;

									item.MoveToWorld( new Point3D( px + xTileStart, py + yTileStart, oldTile.Z ), map );
								}
							}

							int mulPos = -1;
							int length = -1;
							int extra = 0;

							if ( newTileCount > 0 )
							{
								mulWriter.Seek( 0, SeekOrigin.End );

								mulPos = (int)mulWriter.BaseStream.Position;
								length = newTileCount * 7;
								extra = 1;

								for ( int i = 0; i < newTileCount; ++i )
								{
									StaticTile toWrite = newTiles[i];

									mulWriter.Write( (ushort) toWrite.ID );
									mulWriter.Write( (byte) toWrite.X );
									mulWriter.Write( (byte) toWrite.Y );
									mulWriter.Write( (sbyte) toWrite.Z );
									mulWriter.Write( (short) toWrite.Hue );
								}

								mulWriter.Flush();
							}

							int idxPos = ((x * matrix.BlockHeight) + y) * 12;

							idxWriter.Seek( idxPos, SeekOrigin.Begin );
							idxWriter.Write( mulPos );
							idxWriter.Write( length );
							idxWriter.Write( extra );

							idxWriter.Flush();

							matrix.SetStaticBlock( x, y, null );
						}
					}
				}
			}
		}

		public static void DoUnfreeze( Map map, ref bool badDataFile, ref int totalUnfrozen )
		{
			DoUnfreeze( map, Point2D.Zero, new Point2D( map.Width - 1, map.Height - 1 ), ref badDataFile, ref totalUnfrozen );
		}

		public static void Unfreeze( Mobile from, Map map, Point3D start, Point3D end )
		{
			int totalUnfrozen = 0;
			bool badDataFile = false;

			if ( map == null )
			{
				CommandLogging.WriteLine( from, "{0} {1} invoking unfreeze for every item in every map", from.AccessLevel, CommandLogging.Format( from ) );

				DoUnfreeze( Map.Lodor, ref badDataFile, ref totalUnfrozen );
				DoUnfreeze( Map.Sosaria, ref badDataFile, ref totalUnfrozen );
				DoUnfreeze( Map.Underworld, ref badDataFile, ref totalUnfrozen );
				DoUnfreeze( Map.SerpentIsland, ref badDataFile, ref totalUnfrozen );
				DoUnfreeze( Map.IslesDread, ref badDataFile, ref totalUnfrozen );
				DoUnfreeze( Map.SavagedEmpire, ref badDataFile, ref totalUnfrozen );
				DoUnfreeze( Map.Atlantis, ref badDataFile, ref totalUnfrozen );
			}
			else if ( start == NullP3D && end == NullP3D )
			{
				CommandLogging.WriteLine( from, "{0} {1} invoking unfreeze for every item in {2}", from.AccessLevel, CommandLogging.Format( from ), map );

				DoUnfreeze( map, ref badDataFile, ref totalUnfrozen );
			}
			else
			{
				CommandLogging.WriteLine( from, "{0} {1} invoking unfreeze from {2} to {3} in {4}", from.AccessLevel, CommandLogging.Format( from ), new Point2D( start ), new Point2D( end ), map );

				DoUnfreeze( map, new Point2D( start ), new Point2D( end ), ref badDataFile, ref totalUnfrozen );
			}

			if ( totalUnfrozen == 0 && badDataFile )
				from.SendGump( new NoticeGump( 1060637, 30720, "Output data files could not be opened and the unfreeze operation has been aborted.<br><br>This probably means your server and client are using the same data files.  Instructions on how to resolve this can be found in the first warning window.", 0xFFC000, 320, 240, null, null ) );
			else
				from.SendGump( new NoticeGump( 1060637, 30720, String.Format( "Unfreeze operation completed successfully.<br><br>{0} item{1} unfrozen.<br><br>You must restart your client and update it's data files to see the changes.", totalUnfrozen, totalUnfrozen != 1 ? "s were" : " was" ), 0xFFC000, 320, 240, null, null ) );
		}

		private static FileStream OpenWrite( FileStream orig )
		{
			if ( orig == null )
				return null;

			try{ return new FileStream( orig.Name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite ); }
			catch{ return null; }
		}

		private static byte[] m_Buffer;

		private static StaticTile[] m_TileBuffer = new StaticTile[128];

		private static StaticTile[] ReadStaticBlock( BinaryReader idxReader, FileStream mulStream, int x, int y, int width, int height, out int count )
		{
			try
			{
				if ( x < 0 || x >= width || y < 0 || y >= height )
				{
					count = -1;
					return m_TileBuffer;
				}

				idxReader.BaseStream.Seek( ((x * height) + y) * 12, SeekOrigin.Begin );

				int lookup = idxReader.ReadInt32();
				int length = idxReader.ReadInt32();

				if ( lookup < 0 || length <= 0 )
				{
					count = 0;
				}
				else
				{
					count = length / 7;

					mulStream.Seek( lookup, SeekOrigin.Begin );

					if ( m_TileBuffer.Length < count )
						m_TileBuffer = new StaticTile[count];

					StaticTile[] staTiles = m_TileBuffer;

					if ( m_Buffer == null || length > m_Buffer.Length )
						m_Buffer = new byte[length];

					mulStream.Read( m_Buffer, 0, length );

					int index = 0;

					for ( int i = 0; i < count; ++i )
					{
						staTiles[i].Set((ushort)(m_Buffer[index++] | (m_Buffer[index++] << 8)),
								(byte)m_Buffer[index++], (byte)m_Buffer[index++], (sbyte)m_Buffer[index++],
								(short)(m_Buffer[index++] | (m_Buffer[index++] << 8)));
					}
				}
			}
			catch
			{
				count = -1;
			}

			return m_TileBuffer;
		}

		private class DeltaState
		{
			public int m_X, m_Y;
			public List<Item> m_List;

			public DeltaState( Point2D p )
			{
				m_X = p.X;
				m_Y = p.Y;
				m_List = new List<Item>();
			}
		}

		private class StateInfo
		{
			public Map m_Map;
			public Point3D m_Start, m_End;

			public StateInfo( Map map, Point3D start, Point3D end )
			{
				m_Map = map;
				m_Start = start;
				m_End = end;
			}
		}
	}
}