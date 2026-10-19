using System;

namespace IcarusGame.CleanDuel
{
    public sealed class CleanDuelRankedAiProfile
    {
        private CleanDuelRankedAiProfile(int ladderStep)
        {
            LadderStep = ladderStep;
            CleanDuelRankDefinition rank = CleanDuelRankCatalog.ForStep(ladderStep);
            RankLabel = rank.Name + " " + CleanDuelRankCatalog.LevelForStep(ladderStep);
            Difficulty = ladderStep < 9 ? CleanDuelAiDifficulty.Beginner
                : ladderStep < 22 ? CleanDuelAiDifficulty.Intermediate : CleanDuelAiDifficulty.Expert;
            double strength = ladderStep / (double)(CleanDuelRankCatalog.StepCount - 1);
            EarlyPassChance = 0.22d * (1d - strength);
            TopChoiceChance = 0.15d + 0.85d * strength;
            CandidateSlack = (int)Math.Round(5000d * (1d - strength));
            EvaluationJitter = (int)Math.Round(900d * (1d - strength));
        }

        public int LadderStep { get; private set; }
        public string RankLabel { get; private set; }
        public CleanDuelAiDifficulty Difficulty { get; private set; }
        public double EarlyPassChance { get; private set; }
        public double TopChoiceChance { get; private set; }
        public int CandidateSlack { get; private set; }
        public int EvaluationJitter { get; private set; }

        public static CleanDuelRankedAiProfile ForStep(int ladderStep)
        {
            if (ladderStep < 0 || ladderStep >= CleanDuelRankCatalog.StepCount) throw new ArgumentOutOfRangeException(nameof(ladderStep));
            return new CleanDuelRankedAiProfile(ladderStep);
        }

        public static CleanDuelRankedAiProfile FromSnapshot(CleanDuelRankedSnapshot snapshot)
        {
            // An old server or disabled ranked access starts practice at the safe beginner level.
            return ForStep(snapshot != null && !snapshot.IsSimulation ? snapshot.LadderStep : 0);
        }
    }
}
