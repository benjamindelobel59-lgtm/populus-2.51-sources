using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Stump.DofusProtocol.D2oClasses.Tools.D2p;

namespace MapImporter
{
    /// <summary>
    /// Adds graphical elements to client maps (.dlm inside maps*.d2p) without touching anything else :
    /// the map is decompressed, decrypted, the element bytes are inserted in the right layer/cell,
    /// then the map is encrypted and compressed again.
    /// Format follows DofusProtocol/D2oClasses/Tools/Dlm (Map, Layer, Cell, Fixture, elements).
    /// </summary>
    internal static class DlmPatcher
    {
        const string EncryptionKey = "649ae451ca33ec53bbcbcc33becf15f4";
        const byte GraphicalElementType = 2;
        const byte SoundElementType = 33;

        /// <summary>
        /// Usage : MapImporter sparkle &lt;cells.tsv&gt; &lt;mapsFolder&gt; [apply]
        /// cells.tsv lines : mapId TAB cellId. Without "apply" nothing is written.
        /// </summary>
        public static int Run(string listFile, string mapsFolder, bool apply, uint elementId, int layerId)
        {
            var wanted = File.ReadAllLines(listFile)
                .Select(l => l.Split('\t'))
                .Where(p => p.Length >= 2)
                .GroupBy(p => uint.Parse(p[0]), p => int.Parse(p[1]))
                .ToDictionary(g => g.Key, g => g.Distinct().ToList());

            Console.WriteLine($"Maps to patch : {wanted.Count}, cells : {wanted.Values.Sum(x => x.Count)}, apply={apply}");

            var roots = Directory.GetFiles(mapsFolder, "*.d2p")
                .Where(f => !System.Text.RegularExpressions.Regex.IsMatch(Path.GetFileName(f), @"_\d+\.d2p$"))
                .ToList();

            int patched = 0, added = 0, failed = 0;
            var remaining = new HashSet<uint>(wanted.Keys);

            foreach (var root in roots)
            {
                using (var d2p = new D2pFile(root))
                {
                    var names = d2p.GetFilesName();
                    var changed = false;

                    foreach (var name in names)
                    {
                        if (!uint.TryParse(Path.GetFileNameWithoutExtension(name), out var mapId) || !remaining.Contains(mapId))
                            continue;

                        try
                        {
                            var raw = d2p.ReadFile(name);
                            var result = AddElements(raw, wanted[mapId], elementId, layerId, out var count);
                            remaining.Remove(mapId);
                            if (count == 0)
                                continue;

                            added += count;
                            patched++;
                            if (apply)
                            {
                                d2p.ModifyFile(name, result);
                                changed = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            Console.WriteLine($"  map {mapId} : {ex.Message}");
                        }
                    }

                    if (changed)
                    {
                        d2p.Save();
                        Console.WriteLine($"  saved {Path.GetFileName(root)}");
                    }
                }
            }

            Console.WriteLine($"Maps patched : {patched}, elements added : {added}, failed : {failed}, maps not found : {remaining.Count}");
            return failed == 0 ? 0 : 2;
        }

        public static byte[] AddElements(byte[] raw, List<int> cells, uint elementId, int layerId, out int added)
        {
            var plain = ZlibInflate(raw);
            var r = new Cursor(plain);

            if (r.U8() != 77)
                throw new FormatException("not a dlm file");

            var version = (sbyte)r.U8();
            r.Skip(4); // map id

            int dataStart = r.Pos;
            int dataLenPos = -1;
            bool encrypted = false;
            if (version >= 7)
            {
                encrypted = r.U8() != 0;
                r.Skip(1); // encryption version
                dataLenPos = r.Pos;
                var dataLen = r.I32();
                dataStart = r.Pos;
                if (dataStart + dataLen != plain.Length)
                    throw new FormatException("unexpected data length");
            }

            var data = plain.Skip(dataStart).ToArray();
            if (encrypted)
                Xor(data);

            var patchedData = InsertInData(data, version, cells, elementId, layerId, out added);
            if (added == 0)
                return raw;

            if (encrypted)
                Xor(patchedData);

            var header = plain.Take(dataStart).ToArray();
            if (dataLenPos >= 0)
                WriteI32(header, dataLenPos, patchedData.Length);

            return ZlibDeflate(header.Concat(patchedData).ToArray());
        }

        static byte[] InsertInData(byte[] data, sbyte version, List<int> cells, uint elementId, int layerId, out int added)
        {
            var r = new Cursor(data);
            r.Skip(4 + 1 + 4 + 16 + 4); // relativeId, mapType, subArea, neighbours, shadow
            if (version >= 9) r.Skip(8);
            else if (version >= 3) r.Skip(3);
            if (version >= 4) r.Skip(6);
            if (version > 10) r.Skip(4);
            r.Skip(1); // low pass
            if (r.U8() == 1) r.Skip(4); // reverb preset
            for (int f = 0; f < 2; f++)
            {
                var fixtures = (sbyte)r.U8();
                r.Skip(fixtures * 18);
            }
            r.Skip(8); // unknown int + ground crc

            int layersCountPos = r.Pos;
            int layersCount = (sbyte)r.U8();

            var layers = new List<LayerInfo>();
            for (int l = 0; l < layersCount; l++)
            {
                var layer = new LayerInfo { Start = r.Pos };
                layer.Id = version >= 9 ? (sbyte)r.U8() : r.I32();
                layer.CellsCountPos = r.Pos;
                int cellsCount = r.I16();
                for (int c = 0; c < cellsCount; c++)
                {
                    var cell = new CellInfo { Start = r.Pos, Id = r.I16(), CountPos = r.Pos };
                    int elements = r.I16();
                    cell.ElementsStart = r.Pos;
                    for (int e = 0; e < elements; e++)
                    {
                        var type = r.U8();
                        if (type == GraphicalElementType)
                        {
                            var id = (uint)r.I32();
                            r.Skip(6 + (version <= 4 ? 2 : 4) + 1 + 4);
                            if (id == elementId) cell.HasElement = true;
                        }
                        else if (type == SoundElementType)
                            r.Skip(18);
                        else
                            throw new FormatException($"unknown element type {type}");
                    }
                    cell.End = r.Pos;
                    layer.Cells.Add(cell);
                }
                layer.End = r.Pos;
                layers.Add(layer);
            }

            var element = BuildElement(version, elementId);
            var inserts = new List<(int pos, byte[] bytes)>();
            var counterChanges = new Dictionary<int, int>(); // position of a short counter -> delta
            added = 0;

            var target = layers.FirstOrDefault(x => x.Id == layerId);
            byte[] newLayer = null;

            foreach (var cellId in cells.OrderBy(x => x))
            {
                if (target == null)
                {
                    // the map has no such layer yet : build it and append it after the last layer
                    newLayer = newLayer ?? new byte[0];
                    var cellBytes = new List<byte>();
                    cellBytes.AddRange(BE16(cellId));
                    cellBytes.AddRange(BE16(1));
                    cellBytes.AddRange(element);
                    newLayer = newLayer.Concat(cellBytes).ToArray();
                    added++;
                    continue;
                }

                var cell = target.Cells.FirstOrDefault(x => x.Id == cellId);
                if (cell != null)
                {
                    if (cell.HasElement)
                        continue;
                    inserts.Add((cell.End, element));
                    Add(counterChanges, cell.CountPos, 1);
                }
                else
                {
                    var next = target.Cells.FirstOrDefault(x => x.Id > cellId);
                    var pos = next?.Start ?? target.End;
                    inserts.Add((pos, BE16(cellId).Concat(BE16(1)).Concat(element).ToArray()));
                    Add(counterChanges, target.CellsCountPos, 1);
                }
                added++;
            }

            var output = (byte[])data.Clone();
            foreach (var change in counterChanges)
            {
                var value = (short)((output[change.Key] << 8) | output[change.Key + 1]);
                WriteI16(output, change.Key, value + change.Value);
            }

            if (newLayer != null)
            {
                var cellCount = newLayer.Length / (4 + element.Length);
                var layerBytes = (version >= 9 ? new[] { (byte)layerId } : BE32(layerId))
                    .Concat(BE16(cellCount)).Concat(newLayer).ToArray();
                inserts.Add((layers.Count > 0 ? layers.Last().End : layersCountPos + 1, layerBytes));
                output[layersCountPos] = (byte)(layersCount + 1);
            }

            var result = new List<byte>(output.Length + inserts.Sum(x => x.bytes.Length));
            int last = 0;
            foreach (var insert in inserts.OrderBy(x => x.pos))
            {
                result.AddRange(output.Skip(last).Take(insert.pos - last));
                result.AddRange(insert.bytes);
                last = insert.pos;
            }
            result.AddRange(output.Skip(last));
            return result.ToArray();
        }

        static byte[] BuildElement(sbyte version, uint elementId)
        {
            var bytes = new List<byte> { GraphicalElementType };
            bytes.AddRange(BE32((int)elementId));
            bytes.AddRange(new byte[6]); // hue + shadow
            bytes.AddRange(version <= 4 ? new byte[2] : new byte[4]); // offset
            bytes.Add(0); // altitude
            bytes.AddRange(BE32(0)); // identifier : plain decoration
            return bytes.ToArray();
        }

        static void Add(Dictionary<int, int> dict, int key, int delta)
        {
            dict.TryGetValue(key, out var value);
            dict[key] = value + delta;
        }

        static void Xor(byte[] data)
        {
            var key = Encoding.UTF8.GetBytes(EncryptionKey);
            for (int i = 0; i < data.Length; i++)
                data[i] ^= key[i % key.Length];
        }

        static byte[] ZlibInflate(byte[] raw)
        {
            using (var input = new MemoryStream(raw, 2, raw.Length - 2))
            using (var inflate = new DeflateStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                inflate.CopyTo(output);
                return output.ToArray();
            }
        }

        static byte[] ZlibDeflate(byte[] plain)
        {
            using (var output = new MemoryStream())
            {
                output.WriteByte(0x78);
                output.WriteByte(0x9C);
                using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, true))
                    deflate.Write(plain, 0, plain.Length);

                uint a = 1, b = 0;
                foreach (var x in plain)
                {
                    a = (a + x) % 65521;
                    b = (b + a) % 65521;
                }
                output.Write(BE32((int)((b << 16) | a)), 0, 4);
                return output.ToArray();
            }
        }

        static byte[] BE16(int v) => new[] { (byte)(v >> 8), (byte)v };
        static byte[] BE32(int v) => new[] { (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v };
        static void WriteI16(byte[] b, int pos, int v) { b[pos] = (byte)(v >> 8); b[pos + 1] = (byte)v; }
        static void WriteI32(byte[] b, int pos, int v) { b[pos] = (byte)(v >> 24); b[pos + 1] = (byte)(v >> 16); b[pos + 2] = (byte)(v >> 8); b[pos + 3] = (byte)v; }

        sealed class LayerInfo
        {
            public int Start, End, Id, CellsCountPos;
            public readonly List<CellInfo> Cells = new List<CellInfo>();
        }

        sealed class CellInfo
        {
            public int Start, End, Id, CountPos, ElementsStart;
            public bool HasElement;
        }

        sealed class Cursor
        {
            readonly byte[] m_data;
            public int Pos;
            public Cursor(byte[] data) { m_data = data; }
            public byte U8() => m_data[Pos++];
            public short I16() { var v = (short)((m_data[Pos] << 8) | m_data[Pos + 1]); Pos += 2; return v; }
            public int I32() { var v = (m_data[Pos] << 24) | (m_data[Pos + 1] << 16) | (m_data[Pos + 2] << 8) | m_data[Pos + 3]; Pos += 4; return v; }
            public void Skip(int n) { Pos += n; }
        }
    }
}
