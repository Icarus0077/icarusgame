using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace IcarusGame.MasterDuel
{
    public sealed class YgoCardRecord
    {
        internal YgoCardRecord(
            int id,
            int ot,
            int alias,
            string setCodeHex,
            int type,
            int attack,
            int defense,
            int level,
            int race,
            int attribute,
            long category,
            string name,
            string description)
        {
            Id = id;
            Ot = ot;
            Alias = alias;
            SetCodeHex = setCodeHex;
            Type = type;
            Attack = attack;
            Defense = defense;
            Level = level;
            Race = race;
            Attribute = attribute;
            Category = category;
            Name = name;
            Description = description;
        }

        public int Id { get; private set; }
        public int Ot { get; private set; }
        public int Alias { get; private set; }
        public string SetCodeHex { get; private set; }
        public int Type { get; private set; }
        public int Attack { get; private set; }
        public int Defense { get; private set; }
        public int Level { get; private set; }
        public int Race { get; private set; }
        public int Attribute { get; private set; }
        public long Category { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
    }

    public sealed class YgoCardDatabase
    {
        public const string StreamingAssetsRelativePath = "CardDatabase/cards.cdb";

        private const int TypeMonster = 0x1;
        private const int TypeSpell = 0x2;
        private const int TypeTrap = 0x4;
        private const int TypeNormal = 0x10;
        private const int TypeEffect = 0x20;
        private const int TypeFusion = 0x40;
        private const int TypeRitual = 0x80;
        private const int TypeSpirit = 0x200;
        private const int TypeUnion = 0x400;
        private const int TypeGemini = 0x800;
        private const int TypeTuner = 0x1000;
        private const int TypeSynchro = 0x2000;
        private const int TypeToken = 0x4000;
        private const int TypeQuickPlay = 0x10000;
        private const int TypeContinuous = 0x20000;
        private const int TypeEquip = 0x40000;
        private const int TypeField = 0x80000;
        private const int TypeCounter = 0x100000;
        private const int TypeFlip = 0x200000;
        private const int TypeToon = 0x400000;
        private const int TypeXyz = 0x800000;
        private const int TypePendulum = 0x1000000;
        private const int TypeSpecialSummon = 0x2000000;
        private const int TypeLink = 0x4000000;

        private static YgoCardDatabase cachedDefault;

        private readonly Dictionary<int, YgoCardRecord> recordsById;
        private readonly List<YgoCardRecord> records;

        internal YgoCardDatabase(Dictionary<int, YgoCardRecord> recordsById)
        {
            this.recordsById = recordsById;
            records = new List<YgoCardRecord>(recordsById.Values);
            records.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        }

        public int Count
        {
            get { return recordsById.Count; }
        }

        public static bool TryLoadDefault(out YgoCardDatabase database, out string error)
        {
            if (cachedDefault != null)
            {
                database = cachedDefault;
                error = string.Empty;
                return true;
            }

            try
            {
                database = LoadFromStreamingAssets();
                cachedDefault = database;
                error = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                database = null;
                error = exception.Message;
                return false;
            }
        }

        public static YgoCardDatabase LoadFromStreamingAssets()
        {
            string path = Path.Combine(Application.streamingAssetsPath, StreamingAssetsRelativePath);
            byte[] bytes = ReadStreamingAssetBytes(path);
            return Load(bytes);
        }

        public static YgoCardDatabase LoadFromFile(string path)
        {
            return Load(File.ReadAllBytes(path));
        }

        public static YgoCardDatabase Load(byte[] bytes)
        {
            return new YgoCdbReader(bytes).ReadDatabase();
        }

        public bool TryGetRecord(int id, out YgoCardRecord record)
        {
            return recordsById.TryGetValue(id, out record);
        }

        public bool TryCreateCardData(int id, string archetype, string effectId, out CardData card)
        {
            YgoCardRecord record;
            if (!TryGetRecord(id, out record))
            {
                card = null;
                return false;
            }

            card = CreateCardData(record, archetype, effectId);
            return true;
        }

        public List<YgoCardRecord> SearchByName(string query, int maxResults)
        {
            List<YgoCardRecord> matches = new List<YgoCardRecord>();
            if (string.IsNullOrEmpty(query) || maxResults <= 0)
            {
                return matches;
            }

            for (int i = 0; i < records.Count && matches.Count < maxResults; i++)
            {
                if (records[i].Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matches.Add(records[i]);
                }
            }

            return matches;
        }

        public static CardData CreateCardData(YgoCardRecord record, string archetype, string effectId)
        {
            CardKind kind = KindFromType(record.Type);
            MonsterFrame frame = FrameFromType(record.Type);
            int level = record.Level & 0xff;
            int attack = NormalizeStat(record.Attack);
            int defense = HasType(record.Type, TypeLink) ? 0 : NormalizeStat(record.Defense);
            string typeLine = BuildTypeLine(record);

            return new CardData(
                record.Id,
                record.Name,
                kind,
                frame,
                level,
                attack,
                defense,
                typeLine,
                record.Description,
                archetype ?? string.Empty,
                effectId ?? string.Empty);
        }

        private static byte[] ReadStreamingAssetBytes(string path)
        {
            if (path.IndexOf("://", StringComparison.Ordinal) < 0)
            {
                return File.ReadAllBytes(path);
            }

            using (UnityWebRequest request = UnityWebRequest.Get(path))
            {
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new IOException("Unable to load " + path + ": " + request.error);
                }

                return request.downloadHandler.data;
            }
        }

        private static CardKind KindFromType(int type)
        {
            if (HasType(type, TypeSpell))
            {
                return CardKind.Spell;
            }

            if (HasType(type, TypeTrap))
            {
                return CardKind.Trap;
            }

            return CardKind.Monster;
        }

        private static MonsterFrame FrameFromType(int type)
        {
            if (HasType(type, TypeLink))
            {
                return MonsterFrame.Link;
            }

            if (HasType(type, TypeXyz))
            {
                return MonsterFrame.Xyz;
            }

            if (HasType(type, TypeSynchro))
            {
                return MonsterFrame.Synchro;
            }

            if (HasType(type, TypeFusion))
            {
                return MonsterFrame.Fusion;
            }

            if (HasType(type, TypeRitual))
            {
                return MonsterFrame.Ritual;
            }

            return HasType(type, TypeEffect) ? MonsterFrame.Effect : MonsterFrame.Normal;
        }

        private static string BuildTypeLine(YgoCardRecord record)
        {
            if (HasType(record.Type, TypeSpell))
            {
                return SpellTrapTypeLine(record.Type, "Spell");
            }

            if (HasType(record.Type, TypeTrap))
            {
                return SpellTrapTypeLine(record.Type, "Trap");
            }

            List<string> parts = new List<string>();
            parts.Add(RaceLabel(record.Race));
            AddMonsterSubtype(parts, record.Type, TypePendulum, "Pendulum");
            AddMonsterSubtype(parts, record.Type, TypeRitual, "Ritual");
            AddMonsterSubtype(parts, record.Type, TypeFusion, "Fusion");
            AddMonsterSubtype(parts, record.Type, TypeSynchro, "Synchro");
            AddMonsterSubtype(parts, record.Type, TypeXyz, "Xyz");
            AddMonsterSubtype(parts, record.Type, TypeLink, "Link");
            AddMonsterSubtype(parts, record.Type, TypeTuner, "Tuner");
            AddMonsterSubtype(parts, record.Type, TypeSpirit, "Spirit");
            AddMonsterSubtype(parts, record.Type, TypeUnion, "Union");
            AddMonsterSubtype(parts, record.Type, TypeGemini, "Gemini");
            AddMonsterSubtype(parts, record.Type, TypeFlip, "Flip");
            AddMonsterSubtype(parts, record.Type, TypeToon, "Toon");
            AddMonsterSubtype(parts, record.Type, TypeSpecialSummon, "Special Summon");
            AddMonsterSubtype(parts, record.Type, TypeToken, "Token");
            AddMonsterSubtype(parts, record.Type, TypeNormal, "Normal");
            AddMonsterSubtype(parts, record.Type, TypeEffect, "Effect");
            return string.Join(" / ", parts.ToArray());
        }

        private static string SpellTrapTypeLine(int type, string suffix)
        {
            string subtype = "Normal";
            if (HasType(type, TypeQuickPlay))
            {
                subtype = "Quick-Play";
            }
            else if (HasType(type, TypeContinuous))
            {
                subtype = "Continuous";
            }
            else if (HasType(type, TypeEquip))
            {
                subtype = "Equip";
            }
            else if (HasType(type, TypeField))
            {
                subtype = "Field";
            }
            else if (HasType(type, TypeCounter))
            {
                subtype = "Counter";
            }
            else if (HasType(type, TypeRitual))
            {
                subtype = "Ritual";
            }

            return subtype + " " + suffix;
        }

        private static void AddMonsterSubtype(List<string> parts, int type, int flag, string label)
        {
            if (HasType(type, flag))
            {
                parts.Add(label);
            }
        }

        private static bool HasType(int type, int flag)
        {
            return (type & flag) != 0;
        }

        private static int NormalizeStat(int value)
        {
            return value < 0 ? 0 : value;
        }

        private static string RaceLabel(int race)
        {
            switch (race)
            {
                case 1: return "Warrior";
                case 2: return "Spellcaster";
                case 4: return "Fairy";
                case 8: return "Fiend";
                case 16: return "Zombie";
                case 32: return "Machine";
                case 64: return "Aqua";
                case 128: return "Pyro";
                case 256: return "Rock";
                case 512: return "Winged Beast";
                case 1024: return "Plant";
                case 2048: return "Insect";
                case 4096: return "Thunder";
                case 8192: return "Dragon";
                case 16384: return "Beast";
                case 32768: return "Beast-Warrior";
                case 65536: return "Dinosaur";
                case 131072: return "Fish";
                case 262144: return "Sea Serpent";
                case 524288: return "Reptile";
                case 1048576: return "Psychic";
                case 2097152: return "Divine-Beast";
                case 4194304: return "Creator God";
                case 8388608: return "Wyrm";
                case 16777216: return "Cyberse";
                case 33554432: return "Illusion";
                default: return "Unknown";
            }
        }

        private sealed class YgoCdbReader
        {
            private readonly byte[] data;
            private int pageSize;
            private int usableSize;
            private Encoding textEncoding;

            public YgoCdbReader(byte[] data)
            {
                this.data = data;
            }

            public YgoCardDatabase ReadDatabase()
            {
                ReadHeader();
                Dictionary<string, int> roots = ReadTableRoots();
                int datasRoot;
                int textsRoot;
                if (!roots.TryGetValue("datas", out datasRoot) || !roots.TryGetValue("texts", out textsRoot))
                {
                    throw new InvalidDataException("cards.cdb is missing required datas/texts tables.");
                }

                Dictionary<int, DataRow> datas = ReadDatas(datasRoot);
                Dictionary<int, TextRow> texts = ReadTexts(textsRoot);
                Dictionary<int, YgoCardRecord> cards = new Dictionary<int, YgoCardRecord>();

                foreach (KeyValuePair<int, DataRow> pair in datas)
                {
                    TextRow text;
                    if (!texts.TryGetValue(pair.Key, out text))
                    {
                        continue;
                    }

                    DataRow row = pair.Value;
                    cards[pair.Key] = new YgoCardRecord(
                        row.Id,
                        row.Ot,
                        row.Alias,
                        row.SetCodeHex,
                        row.Type,
                        row.Attack,
                        row.Defense,
                        row.Level,
                        row.Race,
                        row.Attribute,
                        row.Category,
                        text.Name,
                        text.Description);
                }

                return new YgoCardDatabase(cards);
            }

            private void ReadHeader()
            {
                byte[] signature = Encoding.ASCII.GetBytes("SQLite format 3");
                for (int i = 0; i < signature.Length; i++)
                {
                    if (data.Length <= i || data[i] != signature[i])
                    {
                        throw new InvalidDataException("cards.cdb is not a SQLite 3 database.");
                    }
                }

                pageSize = ReadUInt16(16);
                if (pageSize == 1)
                {
                    pageSize = 65536;
                }

                int reservedBytes = data[20];
                usableSize = pageSize - reservedBytes;
                int encoding = ReadInt32(56);
                if (encoding == 2)
                {
                    textEncoding = Encoding.Unicode;
                }
                else if (encoding == 3)
                {
                    textEncoding = Encoding.BigEndianUnicode;
                }
                else
                {
                    textEncoding = Encoding.UTF8;
                }
            }

            private Dictionary<string, int> ReadTableRoots()
            {
                Dictionary<string, int> roots = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                List<SqliteRow> rows = ReadTableRows(1);
                for (int i = 0; i < rows.Count; i++)
                {
                    object[] fields = ReadRecord(rows[i].Payload);
                    if (fields.Length < 5 || !string.Equals(FieldString(fields, 0), "table", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string name = FieldString(fields, 1);
                    int rootPage = FieldInt(fields, 3);
                    roots[name] = rootPage;
                }

                return roots;
            }

            private Dictionary<int, DataRow> ReadDatas(int rootPage)
            {
                Dictionary<int, DataRow> rows = new Dictionary<int, DataRow>();
                List<SqliteRow> tableRows = ReadTableRows(rootPage);
                for (int i = 0; i < tableRows.Count; i++)
                {
                    object[] fields = ReadRecord(tableRows[i].Payload);
                    if (fields.Length < 11)
                    {
                        continue;
                    }

                    DataRow row = new DataRow
                    {
                        Id = FieldInt(fields, 0),
                        Ot = FieldInt(fields, 1),
                        Alias = FieldInt(fields, 2),
                        SetCodeHex = FieldHex(fields, 3),
                        Type = FieldInt(fields, 4),
                        Attack = FieldInt(fields, 5),
                        Defense = FieldInt(fields, 6),
                        Level = FieldInt(fields, 7),
                        Race = FieldInt(fields, 8),
                        Attribute = FieldInt(fields, 9),
                        Category = FieldLong(fields, 10)
                    };
                    rows[row.Id] = row;
                }

                return rows;
            }

            private Dictionary<int, TextRow> ReadTexts(int rootPage)
            {
                Dictionary<int, TextRow> rows = new Dictionary<int, TextRow>();
                List<SqliteRow> tableRows = ReadTableRows(rootPage);
                for (int i = 0; i < tableRows.Count; i++)
                {
                    object[] fields = ReadRecord(tableRows[i].Payload);
                    if (fields.Length < 3)
                    {
                        continue;
                    }

                    TextRow row = new TextRow
                    {
                        Id = FieldInt(fields, 0),
                        Name = FieldString(fields, 1),
                        Description = FieldString(fields, 2)
                    };
                    rows[row.Id] = row;
                }

                return rows;
            }

            private List<SqliteRow> ReadTableRows(int rootPage)
            {
                List<SqliteRow> rows = new List<SqliteRow>();
                ReadTablePage(rootPage, rows);
                return rows;
            }

            private void ReadTablePage(int pageNumber, List<SqliteRow> rows)
            {
                int pageStart = PageOffset(pageNumber);
                int header = pageStart + (pageNumber == 1 ? 100 : 0);
                byte pageType = data[header];
                int cellCount = ReadUInt16(header + 3);

                if (pageType == 0x0d)
                {
                    int cellPointers = header + 8;
                    for (int i = 0; i < cellCount; i++)
                    {
                        int cell = pageStart + ReadUInt16(cellPointers + i * 2);
                        rows.Add(ReadLeafTableCell(cell));
                    }

                    return;
                }

                if (pageType == 0x05)
                {
                    int rightMost = ReadInt32(header + 8);
                    int cellPointers = header + 12;
                    for (int i = 0; i < cellCount; i++)
                    {
                        int cell = pageStart + ReadUInt16(cellPointers + i * 2);
                        int childPage = ReadInt32(cell);
                        ReadTablePage(childPage, rows);
                    }

                    ReadTablePage(rightMost, rows);
                    return;
                }

                throw new InvalidDataException("Unsupported SQLite page type " + pageType + ".");
            }

            private SqliteRow ReadLeafTableCell(int cell)
            {
                int offset;
                long payloadSize = ReadVarint(data, cell, out offset);
                long rowId = ReadVarint(data, offset, out offset);
                byte[] payload = ReadPayload(offset, (int)payloadSize);
                return new SqliteRow(rowId, payload);
            }

            private byte[] ReadPayload(int payloadOffset, int payloadSize)
            {
                int local = LocalPayloadLength(payloadSize);
                byte[] payload = new byte[payloadSize];
                Buffer.BlockCopy(data, payloadOffset, payload, 0, local);

                if (local == payloadSize)
                {
                    return payload;
                }

                int overflowPage = ReadInt32(payloadOffset + local);
                int copied = local;
                while (overflowPage != 0 && copied < payloadSize)
                {
                    int pageOffset = PageOffset(overflowPage);
                    int nextOverflow = ReadInt32(pageOffset);
                    int bytesToCopy = Math.Min(usableSize - 4, payloadSize - copied);
                    Buffer.BlockCopy(data, pageOffset + 4, payload, copied, bytesToCopy);
                    copied += bytesToCopy;
                    overflowPage = nextOverflow;
                }

                if (copied != payloadSize)
                {
                    throw new InvalidDataException("SQLite overflow chain ended before payload was complete.");
                }

                return payload;
            }

            private int LocalPayloadLength(int payloadSize)
            {
                int maxLocal = usableSize - 35;
                if (payloadSize <= maxLocal)
                {
                    return payloadSize;
                }

                int minLocal = ((usableSize - 12) * 32 / 255) - 23;
                int local = minLocal + ((payloadSize - minLocal) % (usableSize - 4));
                if (local > maxLocal)
                {
                    local = minLocal;
                }

                return local;
            }

            private object[] ReadRecord(byte[] payload)
            {
                int offset;
                long headerSize = ReadVarint(payload, 0, out offset);
                List<long> serialTypes = new List<long>();
                while (offset < headerSize)
                {
                    serialTypes.Add(ReadVarint(payload, offset, out offset));
                }

                object[] fields = new object[serialTypes.Count];
                int dataOffset = (int)headerSize;
                for (int i = 0; i < serialTypes.Count; i++)
                {
                    long serialType = serialTypes[i];
                    int length = SerialTypeLength(serialType);
                    fields[i] = ReadField(payload, dataOffset, serialType, length);
                    dataOffset += length;
                }

                return fields;
            }

            private object ReadField(byte[] payload, int offset, long serialType, int length)
            {
                if (serialType == 0)
                {
                    return null;
                }

                if (serialType >= 1 && serialType <= 6)
                {
                    return ReadSignedInteger(payload, offset, length);
                }

                if (serialType == 7)
                {
                    byte[] number = new byte[8];
                    for (int i = 0; i < 8; i++)
                    {
                        number[BitConverter.IsLittleEndian ? 7 - i : i] = payload[offset + i];
                    }

                    return BitConverter.ToDouble(number, 0);
                }

                if (serialType == 8)
                {
                    return 0L;
                }

                if (serialType == 9)
                {
                    return 1L;
                }

                if (serialType >= 12 && serialType % 2 == 0)
                {
                    byte[] blob = new byte[length];
                    Buffer.BlockCopy(payload, offset, blob, 0, length);
                    return blob;
                }

                if (serialType >= 13 && serialType % 2 == 1)
                {
                    return textEncoding.GetString(payload, offset, length);
                }

                return null;
            }

            private static int SerialTypeLength(long serialType)
            {
                switch (serialType)
                {
                    case 0:
                    case 8:
                    case 9:
                        return 0;
                    case 1:
                        return 1;
                    case 2:
                        return 2;
                    case 3:
                        return 3;
                    case 4:
                        return 4;
                    case 5:
                        return 6;
                    case 6:
                    case 7:
                        return 8;
                    default:
                        if (serialType >= 12)
                        {
                            return (int)((serialType - 12) / 2);
                        }

                        throw new InvalidDataException("Unsupported SQLite serial type " + serialType + ".");
                }
            }

            private static long ReadSignedInteger(byte[] source, int offset, int length)
            {
                long value = 0L;
                for (int i = 0; i < length; i++)
                {
                    value = (value << 8) | source[offset + i];
                }

                int shift = (8 - length) * 8;
                return (value << shift) >> shift;
            }

            private int PageOffset(int pageNumber)
            {
                if (pageNumber <= 0)
                {
                    throw new InvalidDataException("Invalid SQLite page number " + pageNumber + ".");
                }

                int offset = (pageNumber - 1) * pageSize;
                if (offset < 0 || offset >= data.Length)
                {
                    throw new InvalidDataException("SQLite page " + pageNumber + " is outside the database file.");
                }

                return offset;
            }

            private int ReadUInt16(int offset)
            {
                return (data[offset] << 8) | data[offset + 1];
            }

            private int ReadInt32(int offset)
            {
                return (data[offset] << 24) |
                       (data[offset + 1] << 16) |
                       (data[offset + 2] << 8) |
                       data[offset + 3];
            }

            private static long ReadVarint(byte[] source, int offset, out int nextOffset)
            {
                long value = 0L;
                for (int i = 0; i < 9; i++)
                {
                    byte current = source[offset + i];
                    if (i == 8)
                    {
                        value = (value << 8) | current;
                        nextOffset = offset + 9;
                        return value;
                    }

                    value = (value << 7) | (uint)(current & 0x7f);
                    if ((current & 0x80) == 0)
                    {
                        nextOffset = offset + i + 1;
                        return value;
                    }
                }

                nextOffset = offset + 9;
                return value;
            }

            private static int FieldInt(object[] fields, int index)
            {
                return (int)FieldLong(fields, index);
            }

            private static long FieldLong(object[] fields, int index)
            {
                object value = fields[index];
                if (value == null)
                {
                    return 0L;
                }

                if (value is long)
                {
                    return (long)value;
                }

                if (value is int)
                {
                    return (int)value;
                }

                return Convert.ToInt64(value);
            }

            private static string FieldString(object[] fields, int index)
            {
                object value = fields[index];
                return value == null ? string.Empty : value.ToString();
            }

            private static string FieldHex(object[] fields, int index)
            {
                byte[] blob = fields[index] as byte[];
                if (blob == null || blob.Length == 0)
                {
                    return string.Empty;
                }

                StringBuilder builder = new StringBuilder(blob.Length * 2);
                for (int i = 0; i < blob.Length; i++)
                {
                    builder.Append(blob[i].ToString("x2"));
                }

                return builder.ToString();
            }

            private sealed class DataRow
            {
                public int Id;
                public int Ot;
                public int Alias;
                public string SetCodeHex;
                public int Type;
                public int Attack;
                public int Defense;
                public int Level;
                public int Race;
                public int Attribute;
                public long Category;
            }

            private sealed class TextRow
            {
                public int Id;
                public string Name;
                public string Description;
            }

            private sealed class SqliteRow
            {
                public SqliteRow(long rowId, byte[] payload)
                {
                    RowId = rowId;
                    Payload = payload;
                }

                public long RowId { get; private set; }
                public byte[] Payload { get; private set; }
            }
        }
    }
}
