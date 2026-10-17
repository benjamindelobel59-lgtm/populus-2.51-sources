using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using D2pReader;
using D2pReader.MapInformations;
using MySql.Data.MySqlClient;
using ZipHelper = Stump.Core.IO.ZipHelper;
using Stump.DofusProtocol.D2oClasses;
using Stump.DofusProtocol.D2oClasses.Tools.D2o;

namespace MapImporter
{
    /// <summary>
    /// Imports the client maps (content/maps/*.d2p) that are missing from world_maps / world_maps_positions.
    /// Usage:
    ///   MapImporter verify [count]   compare client conversion with maps already in the database
    ///   MapImporter report           list missing maps and teleport skills pointing to them
    ///   MapImporter import           insert the missing maps (and their positions)
    /// </summary>
    internal static class Program
    {
        static string MapsFolder;

        const string ConnectionString = "Server=127.0.0.1;Port=3306;Database=world;Uid=root;Pwd=;SslMode=None;";
        const string ClientPath = @"..\..\..\client 2.51\client 2.51\app";

        static int Main(string[] args)
        {
            var mode = args.Length > 0 ? args[0] : "report";
            var clientPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..", ClientPath));
            if (args.Length > 2)
                clientPath = args[2];

            var mapsFolder = Path.Combine(clientPath, "content", "maps");
            MapsFolder = mapsFolder;
            var positionsFile = Path.Combine(clientPath, "data", "common", "MapPositions.d2o");

            Console.WriteLine($"Client maps : {mapsFolder}");
            if (!Directory.Exists(mapsFolder))
            {
                Console.WriteLine("Maps folder not found. Pass the client 'app' folder as 3rd argument.");
                return 1;
            }

            if (mode == "sparkle")
                return DlmPatcher.Run(args[1], mapsFolder, args.Contains("apply"), 30991, 2);

            if (mode == "ele")
            {
                var ele = new Stump.DofusProtocol.D2oClasses.Tools.Ele.EleReader(Path.Combine(mapsFolder, "elements.ele")).ReadElements();
                foreach (var id in args[1].Split(',').Select(int.Parse))
                {
                    if (!ele.GraphicalDatas.TryGetValue(id, out var data))
                    {
                        Console.WriteLine($"{id}: not found");
                        continue;
                    }
                    var props = data.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0)
                        .Select(p => $"{p.Name}={p.GetValue(data)}");
                    Console.WriteLine($"{id}: {data.GetType().Name} {string.Join(" ", props)}");
                }
                return 0;
            }

            var manager = new MapManager(mapsFolder);

            if (mode == "cells")
                return DumpCells(manager, uint.Parse(args[1]));

            Console.WriteLine("Parsing all client maps...");
            var clientMaps = manager.ParseAllMap(manager);
            Console.WriteLine($"Client maps parsed : {clientMaps.Count}");

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var dbMaps = ReadIds(connection, "SELECT Id FROM world_maps");
                Console.WriteLine($"Database maps : {dbMaps.Count}");

                switch (mode)
                {
                    case "verify":
                        return Verify(connection, clientMaps, args.Length > 1 ? int.Parse(args[1]) : 200);
                    case "report":
                        return Report(connection, clientMaps, dbMaps);
                    case "import":
                        return Import(connection, clientMaps, dbMaps, positionsFile);
                    case "exitstats":
                        return ExitStats(connection, clientMaps);
                    default:
                        Console.WriteLine("Unknown mode: " + mode);
                        return 1;
                }
            }
        }

        /// <summary>
        /// Lists the graphical elements of every cell of one map : layer, cell, element id (ele), identifier.
        /// </summary>
        static int DumpCells(MapManager manager, uint mapId)
        {
            // MapManager.GetMap builds a manager with an empty folder, so go through the full parse instead.
            manager.ParseAllMap(manager).TryGetValue(mapId, out var map);
            if (map == null)
            {
                Console.WriteLine($"Map {mapId} not found in client");
                return 1;
            }

            foreach (var layer in map.Layers)
                foreach (var cell in layer.Cells)
                    foreach (var element in cell.Elements.OfType<D2pReader.MapInformations.Elements.GraphicalElement>())
                        Console.WriteLine($"layer={layer.LayerId}\tcell={cell.CellId}\telement={element.ElementId}\tidentifier={element.Identifier}\toffset={element.PixelOffsetX},{element.PixelOffsetY}\talt={element.Altitude}");
            return 0;
        }

        /// <summary>
        /// Finds the graphical elements that sit on known exit cells of indoor maps far more often than elsewhere.
        /// </summary>
        static int ExitStats(MySqlConnection connection, Dictionary<uint, Map> clientMaps)
        {
            var exits = new List<(int map, int cell)>();
            using (var cmd = new MySqlCommand(@"SELECT t.MapId, t.CellId FROM world_maps_triggers t
                JOIN world_maps_positions p ON p.Id = t.MapId AND p.Outdoor = 0", connection))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    exits.Add((reader.GetInt32(0), reader.GetInt32(1)));

            var indoor = ReadIds(connection, "SELECT Id FROM world_maps_positions WHERE Outdoor = 0");
            var onExit = new Dictionary<uint, int>();
            var everywhere = new Dictionary<uint, int>();
            var exitSet = new HashSet<(int, int)>(exits);

            foreach (var map in clientMaps.Values.Where(m => indoor.Contains((int)m.Id)))
            {
                var seen = new HashSet<(int, uint)>();
                foreach (var layer in map.Layers)
                    foreach (var cell in layer.Cells)
                        foreach (var element in cell.Elements.OfType<D2pReader.MapInformations.Elements.GraphicalElement>())
                        {
                            if (!seen.Add((cell.CellId, element.ElementId)))
                                continue;
                            everywhere.TryGetValue(element.ElementId, out var all);
                            everywhere[element.ElementId] = all + 1;
                            if (exitSet.Contains(((int)map.Id, cell.CellId)))
                            {
                                onExit.TryGetValue(element.ElementId, out var hits);
                                onExit[element.ElementId] = hits + 1;
                            }
                        }
            }

            // every animated entity element using the exit sparkle looks (3507, 3518) is an exit marker
            var ele = new Stump.DofusProtocol.D2oClasses.Tools.Ele.EleReader(Path.Combine(MapsFolder, "elements.ele")).ReadElements();
            var markerIds = new HashSet<uint>(ele.GraphicalDatas.Values
                .OfType<Stump.DofusProtocol.D2oClasses.Tools.Ele.Datas.EntityGraphicalElementData>()
                .Where(x => x.EntityLook == "{3507}" || x.EntityLook == "{3518}")
                .Select(x => (uint)x.Id));
            Console.WriteLine($"Exit marker elements : {string.Join(",", markerIds.OrderBy(x => x))}");

            var sparkles = new List<(int map, int cell)>();
            foreach (var map in clientMaps.Values.Where(m => indoor.Contains((int)m.Id)))
                foreach (var layer in map.Layers)
                    foreach (var cell in layer.Cells)
                        if (cell.Elements.OfType<D2pReader.MapInformations.Elements.GraphicalElement>().Any(e => markerIds.Contains(e.ElementId)))
                            sparkles.Add(((int)map.Id, cell.CellId));
            var sparkleSet = new HashSet<(int, int)>(sparkles);

            // which other elements sit on the same cell as a marker (the ground "trigger" plate), and on which layer
            var companions = new Dictionary<(uint, int), int>();
            foreach (var map in clientMaps.Values.Where(m => indoor.Contains((int)m.Id)))
                foreach (var layer in map.Layers)
                    foreach (var cell in layer.Cells)
                        if (sparkleSet.Contains(((int)map.Id, cell.CellId)))
                            foreach (var element in cell.Elements.OfType<D2pReader.MapInformations.Elements.GraphicalElement>())
                                if (!markerIds.Contains(element.ElementId))
                                {
                                    companions.TryGetValue((element.ElementId, layer.LayerId), out var n);
                                    companions[(element.ElementId, layer.LayerId)] = n + 1;
                                }
            foreach (var c in companions.OrderByDescending(x => x.Value).Take(12))
                Console.WriteLine($"companion element {c.Key.Item1} layer {c.Key.Item2} : {c.Value} marker cells");
            Console.WriteLine($"Sparkle cells : {sparkleSet.Count}, without trigger : {sparkleSet.Count(s => !exitSet.Contains(s))}");
            Console.WriteLine($"Indoor triggers without sparkle : {exitSet.Count(e => !sparkleSet.Contains(e))}");
            File.WriteAllLines("sparkles_without_trigger.tsv", sparkleSet.Where(s => !exitSet.Contains(s)).Select(s => $"{s.Item1}\t{s.Item2}"));
            File.WriteAllLines("sparkles_with_trigger.tsv", sparkleSet.Where(s => exitSet.Contains(s)).Select(s => $"{s.Item1}\t{s.Item2}"));
            File.WriteAllLines("triggers_without_sparkle.tsv", exitSet.Where(e => !sparkleSet.Contains(e)).Select(e => $"{e.Item1}\t{e.Item2}"));

            Console.WriteLine($"Known indoor exit cells : {exits.Count}");
            foreach (var entry in onExit.Where(x => x.Value >= 10)
                         .OrderByDescending(x => (double)x.Value / everywhere[x.Key]).Take(15))
                Console.WriteLine($"element {entry.Key}\ton exits {entry.Value}\teverywhere {everywhere[entry.Key]}\tratio {(double)entry.Value / everywhere[entry.Key]:P0}");
            return 0;
        }

        static HashSet<int> ReadIds(MySqlConnection connection, string sql)
        {
            var ids = new HashSet<int>();
            using (var cmd = new MySqlCommand(sql, connection))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    ids.Add(reader.GetInt32(0));
            return ids;
        }

        static byte[] SerializeCells(Map map)
        {
            var data = new byte[map.Cells.Count * 11];
            for (var i = 0; i < map.Cells.Count; i++)
            {
                var cell = map.Cells[i];
                var id = (short)cell.Id;
                var floor = (short)cell.Floor;
                var moveZone = cell.MoveZone;
                var o = i * 11;
                data[o] = (byte)(id >> 8);
                data[o + 1] = (byte)(id & 255);
                data[o + 2] = (byte)(floor >> 8);
                data[o + 3] = (byte)(floor & 255);
                data[o + 4] = (byte)cell.Losmov;
                data[o + 5] = (byte)cell.MapChangeData;
                data[o + 6] = (byte)cell.Speed;
                data[o + 7] = (byte)(moveZone >> 24);
                data[o + 8] = (byte)(moveZone >> 16);
                data[o + 9] = (byte)(moveZone >> 8);
                data[o + 10] = (byte)(moveZone & 255u);
            }
            return data;
        }

        static string CellsCsv(Map map, Func<D2pReader.MapInformations.CellData, bool> predicate)
        {
            var cells = map.Cells.Where(predicate).Select(c => c.Id.ToString(CultureInfo.InvariantCulture)).ToArray();
            return cells.Length == 0 ? null : string.Join(",", cells);
        }

        static int Verify(MySqlConnection connection, Dictionary<uint, Map> clientMaps, int count)
        {
            int checkedMaps = 0, cellsEqual = 0, headerEqual = 0, csvEqual = 0;
            var diffs = new Dictionary<string, int>();

            using (var cmd = new MySqlCommand($"SELECT Id, Version, RelativeId, MapType, SubAreaId, TopNeighbourId, BottomNeighbourId, LeftNeighbourId, RightNeighbourId, ClientTopNeighbourId, ShadowBonusOnEntities, UseLowpassFilter, UseReverb, PresetId, BlueCellsCSV, RedCellsCSV, CompressedCells FROM world_maps ORDER BY RAND() LIMIT {count}", connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = (uint)reader.GetInt32(0);
                    if (!clientMaps.TryGetValue(id, out var map))
                    {
                        Count(diffs, "not in client");
                        continue;
                    }
                    checkedMaps++;

                    var header = true;
                    void Check(string name, object db, object client)
                    {
                        if (!Equals(Convert.ToString(db, CultureInfo.InvariantCulture), Convert.ToString(client, CultureInfo.InvariantCulture)))
                        {
                            header = false;
                            Count(diffs, name);
                        }
                    }

                    Check("Version", reader.GetUInt32(1), map.MapVersion);
                    Check("RelativeId", reader.GetUInt32(2), map.RelativeId);
                    Check("MapType", reader.GetInt32(3), map.MapType);
                    Check("SubAreaId", reader.GetInt32(4), map.SubareaId);
                    Check("TopNeighbourId", reader.GetInt32(5), map.TopNeighbourId);
                    Check("BottomNeighbourId", reader.GetInt32(6), map.BottomNeighbourId);
                    Check("LeftNeighbourId", reader.GetInt32(7), map.LeftNeighbourId);
                    Check("RightNeighbourId", reader.GetInt32(8), map.RightNeighbourId);
                    Check("ClientTopNeighbourId", reader.GetInt32(9), map.TopNeighbourId);
                    Check("ShadowBonus", reader.GetUInt32(10), map.ShadowBonusOnEntities);
                    Check("UseLowpassFilter", reader.GetBoolean(11), map.UseLowPassFilter);
                    Check("UseReverb", reader.GetBoolean(12), map.UseReverb);
                    Check("PresetId", reader.GetInt32(13), map.PresetId);
                    if (header) headerEqual++;

                    var blue = reader.IsDBNull(14) ? null : reader.GetString(14);
                    var red = reader.IsDBNull(15) ? null : reader.GetString(15);
                    if ((blue ?? "") == (CellsCsv(map, c => c.Blue) ?? "") && (red ?? "") == (CellsCsv(map, c => c.Red) ?? ""))
                        csvEqual++;
                    else
                        Count(diffs, "Blue/Red CSV");

                    var dbCells = ZipHelper.Uncompress((byte[])reader["CompressedCells"]);
                    var clientCells = SerializeCells(map);
                    if (dbCells.SequenceEqual(clientCells))
                        cellsEqual++;
                    else
                    {
                        Count(diffs, "Cells");
                        var firstDiff = Enumerable.Range(0, Math.Min(dbCells.Length, clientCells.Length)).FirstOrDefault(i => dbCells[i] != clientCells[i]);
                        Count(diffs, $"Cells diff byte offset%11={firstDiff % 11} (len db={dbCells.Length} client={clientCells.Length})");
                    }
                }
            }

            Console.WriteLine($"Checked {checkedMaps} maps : header identical {headerEqual}, cells identical {cellsEqual}, fight cells identical {csvEqual}");
            foreach (var diff in diffs.OrderByDescending(x => x.Value))
                Console.WriteLine($"  {diff.Value,5} x {diff.Key}");
            return 0;
        }

        static void Count(Dictionary<string, int> dict, string key)
        {
            dict.TryGetValue(key, out var value);
            dict[key] = value + 1;
        }

        static int Report(MySqlConnection connection, Dictionary<uint, Map> clientMaps, HashSet<int> dbMaps)
        {
            var missing = clientMaps.Keys.Where(id => !dbMaps.Contains((int)id)).ToList();
            Console.WriteLine($"Maps in client but not in database : {missing.Count}");

            var targets = new List<int>();
            using (var cmd = new MySqlCommand("SELECT DISTINCT CAST(Parameter0 AS SIGNED) FROM interactives_skills WHERE Type='Teleport'", connection))
            using (var reader = cmd.ExecuteReader())
                while (reader.Read())
                    if (!reader.IsDBNull(0)) targets.Add(Convert.ToInt32(reader.GetValue(0)));

            var brokenTargets = targets.Where(t => !dbMaps.Contains(t)).ToList();
            var fixable = brokenTargets.Count(t => clientMaps.ContainsKey((uint)t));
            Console.WriteLine($"Teleport destinations missing from database : {brokenTargets.Count} (found in client: {fixable}, not in client: {brokenTargets.Count - fixable})");
            foreach (var t in brokenTargets.Where(t => !clientMaps.ContainsKey((uint)t)).Take(20))
                Console.WriteLine($"  not in client: {t}");
            return 0;
        }

        static int Import(MySqlConnection connection, Dictionary<uint, Map> clientMaps, HashSet<int> dbMaps, string positionsFile)
        {
            var positions = new Dictionary<int, MapPosition>();
            var d2o = new D2OReader(positionsFile);
            foreach (var position in d2o.ReadObjects<MapPosition>().Values)
                positions[(int)position.id] = position;
            Console.WriteLine($"MapPositions.d2o entries : {positions.Count}");

            var dbPositions = ReadIds(connection, "SELECT Id FROM world_maps_positions");
            var missing = clientMaps.Values.Where(m => !dbMaps.Contains((int)m.Id)).ToList();
            int inserted = 0, withoutPosition = 0;

            using (var transaction = connection.BeginTransaction())
            {
                foreach (var map in missing)
                {
                    positions.TryGetValue((int)map.Id, out var position);
                    if (position == null)
                    {
                        withoutPosition++;
                        continue;
                    }

                    using (var cmd = new MySqlCommand(@"INSERT INTO world_maps (Id, Version, RelativeId, MapType, SubAreaId,
                        TopNeighbourId, BottomNeighbourId, LeftNeighbourId, RightNeighbourId,
                        TopNeighbourCellId, BottomNeighbourCellId, LeftNeighbourCellId, RightNeighbourCellId,
                        ClientTopNeighbourId, ClientBottomNeighbourId, ClientLeftNeighbourId, ClientRightNeighbourId,
                        ShadowBonusOnEntities, UseLowpassFilter, UseReverb, PresetId, BlueCellsCSV, RedCellsCSV,
                        CompressedCells, SpawnDisabled, IsInstantiated)
                        VALUES (@id, @version, @relativeId, @mapType, @subAreaId, @top, @bottom, @left, @right,
                        NULL, NULL, NULL, NULL, @top, @bottom, @left, @right,
                        @shadow, @lowpass, @reverb, @preset, @blue, @red, @cells, 0, 0)", connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@id", (int)map.Id);
                        cmd.Parameters.AddWithValue("@version", map.MapVersion);
                        cmd.Parameters.AddWithValue("@relativeId", map.RelativeId);
                        cmd.Parameters.AddWithValue("@mapType", map.MapType);
                        cmd.Parameters.AddWithValue("@subAreaId", map.SubareaId);
                        cmd.Parameters.AddWithValue("@top", map.TopNeighbourId);
                        cmd.Parameters.AddWithValue("@bottom", map.BottomNeighbourId);
                        cmd.Parameters.AddWithValue("@left", map.LeftNeighbourId);
                        cmd.Parameters.AddWithValue("@right", map.RightNeighbourId);
                        cmd.Parameters.AddWithValue("@shadow", map.ShadowBonusOnEntities);
                        cmd.Parameters.AddWithValue("@lowpass", map.UseLowPassFilter);
                        cmd.Parameters.AddWithValue("@reverb", map.UseReverb);
                        cmd.Parameters.AddWithValue("@preset", map.PresetId);
                        cmd.Parameters.AddWithValue("@blue", (object)CellsCsv(map, c => c.Blue) ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@red", (object)CellsCsv(map, c => c.Red) ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@cells", ZipHelper.Compress(SerializeCells(map)));
                        cmd.ExecuteNonQuery();
                    }

                    if (!dbPositions.Contains((int)map.Id))
                    {
                        using (var cmd = new MySqlCommand(@"INSERT INTO world_maps_positions (Id, PosX, PosY, Outdoor, SubAreaId, Capabilities, WorldMap, NameId, HasPriorityOnWorldmap)
                            VALUES (@id, @x, @y, @outdoor, @subArea, @capabilities, @worldMap, @nameId, @priority)", connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@id", (int)map.Id);
                            cmd.Parameters.AddWithValue("@x", position.posX);
                            cmd.Parameters.AddWithValue("@y", position.posY);
                            cmd.Parameters.AddWithValue("@outdoor", position.outdoor);
                            cmd.Parameters.AddWithValue("@subArea", position.subAreaId);
                            cmd.Parameters.AddWithValue("@capabilities", position.capabilities);
                            cmd.Parameters.AddWithValue("@worldMap", position.worldMap);
                            cmd.Parameters.AddWithValue("@nameId", position.nameId);
                            cmd.Parameters.AddWithValue("@priority", position.hasPriorityOnWorldmap);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    inserted++;
                }

                transaction.Commit();
            }

            Console.WriteLine($"Inserted {inserted} maps ({withoutPosition} skipped: no entry in MapPositions.d2o)");
            return 0;
        }
    }
}
