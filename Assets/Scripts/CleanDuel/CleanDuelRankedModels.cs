using System;
using System.Globalization;

namespace IcarusGame.CleanDuel
{
    [Serializable]
    public sealed class CleanDuelRankedData
    {
        public string seasonId;
        public int rating;
        public string rankId;
        public string rankName;
        public int games;
        public int wins;
        public int losses;
        public int draws;
        public int placementGamesRemaining;
        public bool isSimulation;
        public int progressionVersion;
        public int ladderStep;
        public int level;
        public int tierLevels;
        public string rankPatronName;
        public int levelWins;
        public int levelLosses;
        public int winsRequired;
        public int lossesRequired;
        public long diamonds;
        public int highestStep;
        public string lastRankedAt;
        public string nextDecayAt;
    }

    // A read-only projection of the server's ladder. Local duel outcomes never mutate it.
    public sealed class CleanDuelRankedSnapshot
    {
        private CleanDuelRankedSnapshot(CleanDuelRankedData data, DateTimeOffset? lastRankedAt, DateTimeOffset? nextDecayAt)
        {
            SeasonId = data.seasonId;
            Rating = data.rating;
            Games = data.games;
            Wins = data.wins;
            Losses = data.losses;
            Draws = data.draws;
            IsSimulation = data.isSimulation;
            LadderStep = data.ladderStep;
            Level = data.level;
            LevelWins = data.levelWins;
            LevelLosses = data.levelLosses;
            Diamonds = data.diamonds;
            HighestStep = data.highestStep;
            LastRankedAt = lastRankedAt;
            NextDecayAt = nextDecayAt;
        }

        public string SeasonId { get; private set; }
        public int Rating { get; private set; }
        public int Games { get; private set; }
        public int Wins { get; private set; }
        public int Losses { get; private set; }
        public int Draws { get; private set; }
        public bool IsSimulation { get; private set; }
        public int LadderStep { get; private set; }
        public int Level { get; private set; }
        public int LevelWins { get; private set; }
        public int LevelLosses { get; private set; }
        public long Diamonds { get; private set; }
        public int HighestStep { get; private set; }
        public DateTimeOffset? LastRankedAt { get; private set; }
        public DateTimeOffset? NextDecayAt { get; private set; }
        public int WinsRequired { get { return 4; } }
        public int LossesRequired { get { return 3; } }
        public CleanDuelRankDefinition Rank { get { return CleanDuelRankCatalog.ForStep(LadderStep); } }
        public string DisplayName { get { return Rank.Name + " " + Level + " — " + Rank.PatronName; } }

        public static CleanDuelRankedSnapshot FromServer(CleanDuelRankedData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.seasonId) || data.seasonId.Length > 80 ||
                data.progressionVersion != 1 || data.ladderStep < 0 || data.ladderStep >= CleanDuelRankCatalog.StepCount ||
                data.rating < 0 || data.games < 0 || data.wins < 0 || data.losses < 0 || data.draws < 0 ||
                (long)data.wins + data.losses + data.draws != data.games ||
                data.levelWins < 0 || data.levelWins >= 4 || data.levelLosses < 0 || data.levelLosses >= 3 ||
                data.winsRequired != 4 || data.lossesRequired != 3 ||
                data.wins < data.levelWins || data.losses < data.levelLosses ||
                data.diamonds < 0 || data.diamonds > 9007199254740991L ||
                data.highestStep < data.ladderStep || data.highestStep >= CleanDuelRankCatalog.StepCount)
            {
                return null;
            }

            CleanDuelRankDefinition rank = CleanDuelRankCatalog.ForStep(data.ladderStep);
            if (data.rankId != rank.Id || data.rankName != rank.Name || data.rankPatronName != rank.PatronName ||
                data.level != CleanDuelRankCatalog.LevelForStep(data.ladderStep) || data.tierLevels != rank.Levels)
            {
                return null;
            }

            DateTimeOffset? lastRankedAt;
            DateTimeOffset? nextDecayAt;
            if (!TryTimestamp(data.lastRankedAt, out lastRankedAt) || !TryTimestamp(data.nextDecayAt, out nextDecayAt) ||
                lastRankedAt.HasValue != nextDecayAt.HasValue ||
                (lastRankedAt.HasValue && nextDecayAt.Value < lastRankedAt.Value))
            {
                return null;
            }

            return new CleanDuelRankedSnapshot(data, lastRankedAt, nextDecayAt);
        }

        private static bool TryTimestamp(string value, out DateTimeOffset? timestamp)
        {
            timestamp = null;
            if (value == null) return true;
            DateTimeOffset parsed;
            if (value.Length > 40 || !value.EndsWith("Z", StringComparison.Ordinal) || value.IndexOf('T') < 0 ||
                !DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out parsed))
            {
                return false;
            }
            timestamp = parsed.ToUniversalTime();
            return true;
        }
    }

    public sealed class CleanDuelRankDefinition
    {
        internal CleanDuelRankDefinition(string id, string name, string patronName, int levels, int firstStep, string motif)
        {
            Id = id;
            Name = name;
            PatronName = patronName;
            Levels = levels;
            FirstStep = firstStep;
            Motif = motif;
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string PatronName { get; private set; }
        public string DisplayName { get { return Name + " — " + PatronName; } }
        public int Levels { get; private set; }
        public int FirstStep { get; private set; }
        public int LastStep { get { return FirstStep + Levels - 1; } }
        public string Motif { get; private set; }
    }

    public static class CleanDuelRankCatalog
    {
        private static readonly CleanDuelRankDefinition[] ranks =
        {
            new CleanDuelRankDefinition("icarus", "Bronze", "Icarus", 4, 0, "De eerste vlucht"),
            new CleanDuelRankDefinition("hermes", "Silver", "Hermes", 5, 4, "Snelheid en inzicht"),
            new CleanDuelRankDefinition("athena", "Gold", "Athena", 6, 9, "Strategie en wijsheid"),
            new CleanDuelRankDefinition("apollo", "Platinum", "Apollo", 7, 15, "Licht en precisie"),
            new CleanDuelRankDefinition("poseidon", "Diamond", "Poseidon", 8, 22, "Meester van de diepte"),
            new CleanDuelRankDefinition("zeus", "Master", "Zeus", 9, 30, "De top van Olympus")
        };

        public static int Count { get { return ranks.Length; } }
        public static int StepCount { get { return 39; } }

        public static CleanDuelRankDefinition At(int index)
        {
            if (index < 0 || index >= ranks.Length) throw new ArgumentOutOfRangeException(nameof(index));
            return ranks[index];
        }

        public static CleanDuelRankDefinition ForStep(int step)
        {
            if (step < 0 || step >= StepCount) throw new ArgumentOutOfRangeException(nameof(step));
            for (int index = ranks.Length - 1; index >= 0; index--)
            {
                if (step >= ranks[index].FirstStep) return ranks[index];
            }
            return ranks[0];
        }

        public static int LevelForStep(int step)
        {
            CleanDuelRankDefinition rank = ForStep(step);
            return rank.Levels - (step - rank.FirstStep);
        }

        public static string DisplayForStep(int step)
        {
            CleanDuelRankDefinition rank = ForStep(step);
            return rank.Name + " " + LevelForStep(step) + " — " + rank.PatronName;
        }
    }
}
