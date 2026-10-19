using System;
using System.Collections.Generic;

namespace IcarusGame.CleanDuel
{
    public enum CleanDuelAiDifficulty
    {
        Beginner,
        Intermediate,
        Expert
    }

    internal enum CleanDuelAiActionKind
    {
        ActivateHand,
        ActivateMonster,
        ActivateExtraMonster,
        ActivateSetCard,
        NormalSummon,
        SetMonster,
        FlipSummon,
        SetSpellTrap,
        SynchroSummon,
        XyzSummon,
        LinkSummon,
        AttackTarget,
        HoldAttack
    }

    internal sealed class CleanDuelAiAction
    {
        public CleanDuelAiAction(CleanDuelAiActionKind kind, int index, int score, string label)
        {
            Kind = kind;
            Index = index;
            Score = score;
            Label = label ?? string.Empty;
        }

        public CleanDuelAiActionKind Kind { get; private set; }
        public int Index { get; private set; }
        public int Score { get; private set; }
        public string Label { get; private set; }
    }

    internal sealed class CleanDuelAiPlanner
    {
        public CleanDuelAiAction SelectMainAction(
            List<CleanDuelAiAction> legalActions,
            CleanDuelAiDifficulty difficulty,
            Random random,
            CleanDuelRankedAiProfile rankedProfile = null)
        {
            if (legalActions == null || legalActions.Count == 0)
            {
                return null;
            }

            if (rankedProfile != null)
            {
                return SelectRankedAction(legalActions, rankedProfile, random, true);
            }

            if (difficulty == CleanDuelAiDifficulty.Beginner)
            {
                // A beginner occasionally passes early and otherwise chooses a legal move
                // without looking far enough ahead to preserve the strongest play.
                if (random.NextDouble() < 0.16d)
                {
                    return null;
                }

                return legalActions[random.Next(legalActions.Count)];
            }

            legalActions.Sort(CompareActions);
            if (difficulty == CleanDuelAiDifficulty.Expert || legalActions.Count == 1)
            {
                return legalActions[0];
            }

            // Intermediate play sees the strongest candidates, but is allowed a small
            // evaluation error. Keeping this deterministic makes replays reproducible.
            int bestScore = legalActions[0].Score;
            int candidateCount = 1;
            while (candidateCount < legalActions.Count &&
                   candidateCount < 3 &&
                   legalActions[candidateCount].Score >= bestScore - 350)
            {
                candidateCount++;
            }

            return legalActions[random.Next(candidateCount)];
        }

        public int SelectChoiceIndex(
            CleanChoicePrompt prompt,
            CleanDuelAiDifficulty difficulty,
            Random random,
            CleanDuelRankedAiProfile rankedProfile = null)
        {
            if (prompt == null || prompt.Options == null)
            {
                return -1;
            }

            List<int> available = new List<int>();
            for (int i = 0; i < prompt.Options.Count; i++)
            {
                if (prompt.Options[i].IsAvailable)
                {
                    available.Add(i);
                }
            }

            if (available.Count == 0)
            {
                return -1;
            }

            if (rankedProfile != null)
            {
                List<CleanDuelAiAction> choices = new List<CleanDuelAiAction>(available.Count);
                for (int i = 0; i < available.Count; i++)
                {
                    int optionIndex = available[i];
                    choices.Add(new CleanDuelAiAction(CleanDuelAiActionKind.ActivateHand, optionIndex,
                        ScoreChoice(prompt.Options[optionIndex]), prompt.Options[optionIndex].Label));
                }
                return SelectRankedAction(choices, rankedProfile, random, false).Index;
            }

            if (difficulty == CleanDuelAiDifficulty.Beginner)
            {
                return available[random.Next(available.Count)];
            }

            int bestIndex = available[0];
            int bestScore = int.MinValue;
            for (int i = 0; i < available.Count; i++)
            {
                int optionIndex = available[i];
                int score = ScoreChoice(prompt.Options[optionIndex]);
                if (difficulty == CleanDuelAiDifficulty.Intermediate)
                {
                    score += random.Next(-80, 81);
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = optionIndex;
                }
            }

            return bestIndex;
        }

        private static CleanDuelAiAction SelectRankedAction(List<CleanDuelAiAction> legalActions,
            CleanDuelRankedAiProfile profile, Random random, bool allowEarlyPass)
        {
            if (allowEarlyPass && profile.EarlyPassChance > 0d && random.NextDouble() < profile.EarlyPassChance)
            {
                return null;
            }

            legalActions.Sort(CompareActions);
            if (legalActions.Count == 1 || profile.TopChoiceChance >= 1d || random.NextDouble() < profile.TopChoiceChance)
            {
                return legalActions[0];
            }

            int candidateCount = 1;
            while (candidateCount < legalActions.Count &&
                   (long)legalActions[candidateCount].Score >= (long)legalActions[0].Score - profile.CandidateSlack)
            {
                candidateCount++;
            }
            if (candidateCount == 1) return legalActions[0];

            // Lower ranks consider a wider range of legal moves and mis-evaluate them more.
            // Only the supplied legal action scores are visible here, never either player's hidden cards.
            CleanDuelAiAction first = legalActions[random.Next(candidateCount)];
            CleanDuelAiAction second = legalActions[random.Next(candidateCount)];
            long firstScore = (long)first.Score + random.Next(-profile.EvaluationJitter, profile.EvaluationJitter + 1);
            long secondScore = (long)second.Score + random.Next(-profile.EvaluationJitter, profile.EvaluationJitter + 1);
            return firstScore >= secondScore ? first : second;
        }

        private static int CompareActions(CleanDuelAiAction left, CleanDuelAiAction right)
        {
            int score = right.Score.CompareTo(left.Score);
            if (score != 0)
            {
                return score;
            }

            int kind = left.Kind.CompareTo(right.Kind);
            return kind != 0 ? kind : left.Index.CompareTo(right.Index);
        }

        private static int ScoreChoice(CleanChoiceOption option)
        {
            string text = (option.Label + " " + option.Detail).ToLowerInvariant();
            int score = option.CardDefinitionId != 0 ? 100 : 0;
            score += ParseStatAfter(text, "atk ") / 10;

            if (text.Contains("special summon"))
            {
                score += 500;
            }
            if (text.Contains("from deck") || text.Contains("from your deck"))
            {
                score += 260;
            }
            if (text.Contains("destroy") || text.Contains("banish"))
            {
                score += 220;
            }
            if (text.Contains("add to hand") || text.Contains("search"))
            {
                score += 180;
            }
            if (text.Contains("skip") || text.Contains("finish") || text.Contains("do not") || text.Contains("decline"))
            {
                score -= 800;
            }

            return score;
        }

        private static int ParseStatAfter(string text, string marker)
        {
            int markerIndex = text.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex < 0)
            {
                return 0;
            }

            int value = 0;
            bool found = false;
            for (int i = markerIndex + marker.Length; i < text.Length && char.IsDigit(text[i]); i++)
            {
                found = true;
                value = value * 10 + (text[i] - '0');
            }

            return found ? value : 0;
        }
    }
}
