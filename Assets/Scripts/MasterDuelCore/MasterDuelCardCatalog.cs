using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace IcarusGame.MasterDuel
{
    public enum MasterDuelStatKind
    {
        NotApplicable,
        Fixed,
        Variable
    }

    public enum MasterDuelImplementationStatus
    {
        Discovered,
        StaticDataValidated,
        TextLicensed,
        EffectsModeled,
        RulesTested,
        IntegrationTested,
        Playable
    }

    public struct MasterDuelStat
    {
        public MasterDuelStat(MasterDuelStatKind kind, int value)
        {
            Kind = kind;
            Value = value;
        }

        public MasterDuelStatKind Kind { get; private set; }
        public int Value { get; private set; }

        public bool HasFixedValue
        {
            get { return Kind == MasterDuelStatKind.Fixed; }
        }
    }

    /// <summary>
    /// Immutable static card facts imported from the versioned Master Duel reference.
    /// Executable effect support is represented separately by EffectHandlerId and
    /// ImplementationStatus; HasEffect only describes the source card metadata.
    /// </summary>
    public sealed class MasterDuelCardDefinition
    {
        internal MasterDuelCardDefinition(
            string databaseId,
            long numericDatabaseId,
            long? konamiCardId,
            string name,
            string cardClass,
            string frame,
            string attribute,
            string raceOrType,
            string aliasDatabaseId,
            string setCodes,
            string treatedAs,
            string localizationKey,
            string textLocale,
            int? level,
            int? rank,
            int? linkRating,
            MasterDuelStat attack,
            MasterDuelStat defense,
            int? pendulumScale,
            string linkMarkers,
            string archetype,
            string masterDuelRarity,
            bool hasEffect,
            string officialUrl,
            string officialText,
            string effectHandlerId,
            MasterDuelImplementationStatus implementationStatus)
        {
            DatabaseId = databaseId;
            NumericDatabaseId = numericDatabaseId;
            KonamiCardId = konamiCardId;
            Name = name;
            CardClass = cardClass;
            Frame = frame;
            Attribute = attribute;
            RaceOrType = raceOrType;
            AliasDatabaseId = aliasDatabaseId;
            SetCodes = setCodes;
            TreatedAs = treatedAs;
            LocalizationKey = localizationKey;
            TextLocale = textLocale;
            Level = level;
            Rank = rank;
            LinkRating = linkRating;
            Attack = attack;
            Defense = defense;
            PendulumScale = pendulumScale;
            LinkMarkers = linkMarkers;
            Archetype = archetype;
            MasterDuelRarity = masterDuelRarity;
            HasEffect = hasEffect;
            OfficialUrl = officialUrl;
            OfficialText = officialText;
            EffectHandlerId = effectHandlerId;
            ImplementationStatus = implementationStatus;
        }

        public string DatabaseId { get; private set; }
        public long NumericDatabaseId { get; private set; }
        public long? KonamiCardId { get; private set; }
        public string Name { get; private set; }
        public string CardClass { get; private set; }
        public string Frame { get; private set; }
        public string Attribute { get; private set; }
        public string RaceOrType { get; private set; }
        public string AliasDatabaseId { get; private set; }
        public string SetCodes { get; private set; }
        public string TreatedAs { get; private set; }
        public string LocalizationKey { get; private set; }
        public string TextLocale { get; private set; }
        public int? Level { get; private set; }
        public int? Rank { get; private set; }
        public int? LinkRating { get; private set; }
        public MasterDuelStat Attack { get; private set; }
        public MasterDuelStat Defense { get; private set; }
        public int? PendulumScale { get; private set; }
        public string LinkMarkers { get; private set; }
        public string Archetype { get; private set; }
        public string MasterDuelRarity { get; private set; }
        public bool HasEffect { get; private set; }
        public string OfficialUrl { get; private set; }
        public string OfficialText { get; private set; }
        public string EffectHandlerId { get; private set; }
        public MasterDuelImplementationStatus ImplementationStatus { get; private set; }

        public bool HasExecutableEffect
        {
            get
            {
                return !string.IsNullOrEmpty(EffectHandlerId) &&
                       ImplementationStatus >= MasterDuelImplementationStatus.EffectsModeled;
            }
        }

        public bool IsPlayable
        {
            get { return ImplementationStatus == MasterDuelImplementationStatus.Playable; }
        }
    }

    public sealed class MasterDuelCardCatalog
    {
        public const string StreamingAssetsRelativePath = "CardDatabase/master_duel_catalog.tsv";

        private static MasterDuelCardCatalog cachedDefault;
        private readonly Dictionary<long, MasterDuelCardDefinition> definitionsById;
        private readonly List<MasterDuelCardDefinition> definitions;

        private MasterDuelCardCatalog(Dictionary<long, MasterDuelCardDefinition> definitionsById)
        {
            this.definitionsById = definitionsById;
            definitions = new List<MasterDuelCardDefinition>(definitionsById.Values);
            definitions.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
        }

        public int Count
        {
            get { return definitionsById.Count; }
        }

        public IList<MasterDuelCardDefinition> Definitions
        {
            get { return definitions.AsReadOnly(); }
        }

        public static bool TryLoadDefault(out MasterDuelCardCatalog catalog, out string error)
        {
            if (cachedDefault != null)
            {
                catalog = cachedDefault;
                error = string.Empty;
                return true;
            }

            try
            {
                string path = Path.Combine(Application.streamingAssetsPath, StreamingAssetsRelativePath);
                cachedDefault = LoadFromFile(path);
                catalog = cachedDefault;
                error = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                catalog = null;
                error = exception.Message;
                return false;
            }
        }

        public static MasterDuelCardCatalog LoadFromFile(string path)
        {
            return Load(File.ReadAllText(path));
        }

        public static MasterDuelCardCatalog Load(string tsv)
        {
            if (string.IsNullOrEmpty(tsv))
            {
                throw new InvalidDataException("The Master Duel catalog is empty.");
            }

            string[] lines = tsv.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            if (lines.Length < 2)
            {
                throw new InvalidDataException("The Master Duel catalog has no card rows.");
            }

            string[] headers = lines[0].Split('\t');
            Dictionary<string, int> columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Length; i++)
            {
                columns[headers[i]] = i;
            }

            RequireColumn(columns, "databaseId");
            RequireColumn(columns, "name");
            RequireColumn(columns, "cardClass");
            RequireColumn(columns, "frame");

            Dictionary<long, MasterDuelCardDefinition> byId = new Dictionary<long, MasterDuelCardDefinition>();
            for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
            {
                if (string.IsNullOrWhiteSpace(lines[lineIndex]))
                {
                    continue;
                }

                string[] cells = lines[lineIndex].Split('\t');
                string databaseId = Cell(cells, columns, "databaseId");
                long numericId;
                if (!long.TryParse(databaseId, NumberStyles.None, CultureInfo.InvariantCulture, out numericId))
                {
                    throw new InvalidDataException("Invalid databaseId at catalog line " + (lineIndex + 1) + ": " + databaseId);
                }

                if (byId.ContainsKey(numericId))
                {
                    throw new InvalidDataException("Duplicate databaseId in catalog: " + databaseId);
                }

                MasterDuelCardDefinition definition = new MasterDuelCardDefinition(
                    databaseId,
                    numericId,
                    ParseNullableLong(Cell(cells, columns, "cid")),
                    Unescape(Cell(cells, columns, "name")),
                    Unescape(Cell(cells, columns, "cardClass")),
                    Unescape(Cell(cells, columns, "frame")),
                    Unescape(Cell(cells, columns, "attribute")),
                    Unescape(Cell(cells, columns, "raceOrType")),
                    Unescape(Cell(cells, columns, "aliasDatabaseId")),
                    Unescape(Cell(cells, columns, "setCodes")),
                    Unescape(Cell(cells, columns, "treatedAs")),
                    Unescape(Cell(cells, columns, "localizationKey")),
                    Unescape(Cell(cells, columns, "textLocale")),
                    ParseNullableInt(Cell(cells, columns, "level")),
                    ParseNullableInt(Cell(cells, columns, "rank")),
                    ParseNullableInt(Cell(cells, columns, "linkRating")),
                    ParseStat(Cell(cells, columns, "attackKind"), Cell(cells, columns, "attackValue")),
                    ParseStat(Cell(cells, columns, "defenseKind"), Cell(cells, columns, "defenseValue")),
                    ParseNullableInt(Cell(cells, columns, "pendulumScale")),
                    Unescape(Cell(cells, columns, "linkMarkers")),
                    Unescape(Cell(cells, columns, "archetype")),
                    Unescape(Cell(cells, columns, "rarity")),
                    string.Equals(Cell(cells, columns, "hasEffect"), "true", StringComparison.OrdinalIgnoreCase),
                    Unescape(Cell(cells, columns, "officialUrl")),
                    Unescape(Cell(cells, columns, "officialText")),
                    Unescape(Cell(cells, columns, "effectHandlerId")),
                    ParseStatus(Cell(cells, columns, "implementationStatus")));
                byId.Add(numericId, definition);
            }

            return new MasterDuelCardCatalog(byId);
        }

        public bool TryGet(long databaseId, out MasterDuelCardDefinition definition)
        {
            return definitionsById.TryGetValue(databaseId, out definition);
        }

        public List<MasterDuelCardDefinition> SearchByName(string query, int maxResults)
        {
            List<MasterDuelCardDefinition> matches = new List<MasterDuelCardDefinition>();
            if (string.IsNullOrWhiteSpace(query) || maxResults <= 0)
            {
                return matches;
            }

            for (int i = 0; i < definitions.Count && matches.Count < maxResults; i++)
            {
                if (definitions[i].Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matches.Add(definitions[i]);
                }
            }

            return matches;
        }

        private static void RequireColumn(Dictionary<string, int> columns, string name)
        {
            if (!columns.ContainsKey(name))
            {
                throw new InvalidDataException("The Master Duel catalog is missing required column '" + name + "'.");
            }
        }

        private static string Cell(string[] cells, Dictionary<string, int> columns, string name)
        {
            int index;
            return columns.TryGetValue(name, out index) && index >= 0 && index < cells.Length ? cells[index] : string.Empty;
        }

        private static int? ParseNullableInt(string value)
        {
            int parsed;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? parsed : (int?)null;
        }

        private static long? ParseNullableLong(string value)
        {
            long parsed;
            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? parsed : (long?)null;
        }

        private static MasterDuelStat ParseStat(string kindText, string valueText)
        {
            MasterDuelStatKind kind;
            if (!Enum.TryParse(kindText, true, out kind))
            {
                kind = MasterDuelStatKind.NotApplicable;
            }

            int value;
            int.TryParse(valueText, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
            return new MasterDuelStat(kind, value);
        }

        private static MasterDuelImplementationStatus ParseStatus(string value)
        {
            MasterDuelImplementationStatus status;
            return Enum.TryParse(value, true, out status) ? status : MasterDuelImplementationStatus.StaticDataValidated;
        }

        private static string Unescape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] != '\\' || i + 1 >= value.Length)
                {
                    builder.Append(value[i]);
                    continue;
                }

                char escaped = value[++i];
                switch (escaped)
                {
                    case 't':
                        builder.Append('\t');
                        break;
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'r':
                        builder.Append('\r');
                        break;
                    case '\\':
                        builder.Append('\\');
                        break;
                    default:
                        builder.Append('\\');
                        builder.Append(escaped);
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
