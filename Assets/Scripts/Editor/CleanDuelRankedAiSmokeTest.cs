using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using IcarusGame.CleanDuel;
using UnityEditor;
using UnityEngine;

namespace IcarusGame.EditorTools
{
    public static class CleanDuelRankedAiSmokeTest
    {
        [MenuItem("Tools/Icarus/Clean Duel/Test Ranked AI")]
        public static void Run()
        {
            AssertMonotonicProfiles();
            AssertPlannerBehavior();
            AssertAutomaticAndManualModes();
            AssertBattleInformationSafety();
            AssertAutonomousMasterTurn();
            Debug.Log("Ranked AI smoke test passed: 39 monotonic profiles, deterministic legal decisions, stronger high-rank choices, automatic/manual lifecycle, face-down information safety, autonomous Master turn and unchanged account wallet/rank.");
        }

        [MenuItem("Tools/Icarus/Clean Duel/Test Login Ranked and AI Suite")]
        public static void RunSuite()
        {
            CleanDuelAccessSmokeTest.Run();
            CleanDuelRankedSmokeTest.Run();
            Run();
        }

        private static void AssertMonotonicProfiles()
        {
            CleanDuelRankedAiProfile previous = null;
            for (int step = 0; step < 39; step++)
            {
                CleanDuelRankedAiProfile profile = CleanDuelRankedAiProfile.ForStep(step);
                Require(profile.LadderStep == step, "AI profile lost its authoritative ladder step.");
                CleanDuelAiDifficulty expected = step < 9 ? CleanDuelAiDifficulty.Beginner
                    : step < 22 ? CleanDuelAiDifficulty.Intermediate : CleanDuelAiDifficulty.Expert;
                Require(profile.Difficulty == expected, "AI difficulty family does not match the Greek rank tier.");
                Require(profile.RankLabel == CleanDuelRankCatalog.ForStep(step).Name + " " + CleanDuelRankCatalog.LevelForStep(step), "AI menu rank label mismatch.");
                if (previous != null)
                {
                    Require(profile.EarlyPassChance < previous.EarlyPassChance, "Every higher level must lower the early-pass chance.");
                    Require(profile.TopChoiceChance > previous.TopChoiceChance, "Every higher level must raise the best-choice chance.");
                    Require(profile.CandidateSlack < previous.CandidateSlack, "Every higher level must narrow the evaluation candidate range.");
                    Require(profile.EvaluationJitter < previous.EvaluationJitter, "Every higher level must reduce evaluation error.");
                }
                previous = profile;
            }
            Require(previous.EarlyPassChance == 0d && previous.TopChoiceChance == 1d && previous.EvaluationJitter == 0 && previous.CandidateSlack == 0,
                "Master 1 must select the best available heuristic action without deliberate mistakes or early passes.");
            Require(CleanDuelRankedAiProfile.FromSnapshot(null).LadderStep == 0, "Missing progression must use Bronze 4, not hidden MMR.");
            Require(CleanDuelRankedAiProfile.FromSnapshot(Snapshot(38, true)).LadderStep == 0, "A simulation snapshot must not set the real-account practice level.");
        }

        private static void AssertPlannerBehavior()
        {
            CleanDuelRankedAiProfile bronze = CleanDuelRankedAiProfile.ForStep(0);
            CleanDuelRankedAiProfile master = CleanDuelRankedAiProfile.ForStep(38);
            int bronzeErrors = 0;
            int bronzePasses = 0;
            CleanChoicePrompt prompt = new CleanChoicePrompt(1, "AI test", "Only available choices may be chosen.", new List<CleanChoiceOption>
            {
                new CleanChoiceOption(1, "Normal monster", "ATK 100", true, "", 1),
                new CleanChoiceOption(2, "Add to hand", "Search from deck", true, "", 2),
                new CleanChoiceOption(3, "Special Summon", "From your deck ATK 3000", true, "", 3),
                new CleanChoiceOption(4, "Special Summon", "From your deck ATK 999999", false, "Unavailable", 4)
            });
            for (int seed = 0; seed < 256; seed++)
            {
                int low = PickMain(bronze, seed);
                int high = PickMain(master, seed);
                Require(low == PickMain(bronze, seed) && high == PickMain(master, seed), "Seeded ranked main actions are not deterministic.");
                Require(high == 2, "Master 1 must always choose the highest-scored legal main action.");
                Require(low >= -1 && low <= 2, "AI selected an action outside its legal candidates.");
                if (low != 2) bronzeErrors++;
                if (low == -1) bronzePasses++;
                int choice = PickChoice(prompt, bronze, seed);
                Require(choice >= 0 && choice < 3, "Lower-rank AI must still resolve an available legal choice, never early-pass a required prompt.");
                Require(choice == PickChoice(prompt, bronze, seed), "Seeded ranked choices are not deterministic.");
                Require(PickChoice(prompt, master, seed) == 2, "Master must choose the best available prompt option, not a stronger unavailable option.");
                Require(PickMain(null, seed, CleanDuelAiDifficulty.Expert) == 2, "Manual Expert behavior must remain unchanged.");
            }
            Require(bronzeErrors > 20 && bronzePasses > 10, "Bronze and Master profiles must produce meaningfully different decisions, not only different labels.");
        }

        private static void AssertAutomaticAndManualModes()
        {
            CleanDuelRankedSnapshot snapshot = Snapshot(12);
            CleanDuelGame game = new CleanDuelGame();
            game.ConfigureRankedOpponent(snapshot);
            game.StartNewDuel(43019);
            Require(game.RivalAiFollowsRank && game.RivalAiDifficulty == CleanDuelAiDifficulty.Intermediate && game.RivalRankedAiProfile.LadderStep == 12,
                "Login progression must activate automatic Gold 3 AI.");
            Require(game.RivalAiModeLabel == "Auto (Gold 3)", "Automatic mode must expose the real level in the duel menu.");
            game.CycleRivalAiMode();
            Require(!game.RivalAiFollowsRank && game.RivalAiDifficulty == CleanDuelAiDifficulty.Beginner, "Auto must cycle to manual Beginner.");
            game.CycleRivalAiMode();
            Require(game.RivalAiDifficulty == CleanDuelAiDifficulty.Intermediate && !game.RivalAiFollowsRank, "Manual Intermediate override failed.");
            game.CycleRivalAiMode();
            Require(game.RivalAiDifficulty == CleanDuelAiDifficulty.Expert && !game.RivalAiFollowsRank, "Manual Expert override failed.");
            game.CycleRivalAiMode();
            Require(game.RivalAiFollowsRank && game.RivalRankedAiProfile.LadderStep == 12, "Returning to Auto must restore the login rank, not the last manual enum.");

            game.State.TurnPlayer = CleanPlayerId.Rival;
            game.CycleRivalAiMode();
            game.ConfigureRankedOpponent(Snapshot(38));
            Require(game.RivalAiFollowsRank && game.RivalRankedAiProfile.LadderStep == 12, "AI mode must not reset in the middle of a rival turn.");
            game.State.TurnPlayer = CleanPlayerId.Local;
            game.State.PendingChoice = new CleanChoicePrompt(1, "Pending choice", "Finish first", new List<CleanChoiceOption>());
            game.CycleRivalAiMode();
            Require(game.RivalAiFollowsRank, "AI mode must not change during a pending choice.");
            game.State.PendingChoice = null;
            game.StartNewDuel(43019);
            Require(game.RivalAiFollowsRank && game.RivalRankedAiProfile.LadderStep == 12, "Restarting practice must preserve the automatic profile.");
            game.Surrender(CleanPlayerId.Local);
            game.CycleRivalAiMode();
            Require(game.RivalAiFollowsRank && snapshot.LadderStep == 12 && snapshot.Diamonds == 1400, "Practice or AI settings must never mutate account rank or wallet.");

            CleanDuelGame fallback = new CleanDuelGame();
            fallback.ConfigureRankedOpponent(null);
            Require(fallback.RivalAiFollowsRank && fallback.RivalAiDifficulty == CleanDuelAiDifficulty.Beginner && fallback.RivalRankedAiProfile.LadderStep == 0,
                "Missing or disabled ranked progression must start practice in automatic Bronze 4.");
            fallback.SetRivalAiDifficulty(CleanDuelAiDifficulty.Beginner);
            fallback.CycleRivalAiDifficulty();
            Require(fallback.RivalAiDifficulty == CleanDuelAiDifficulty.Intermediate, "Existing manual Beginner cycle changed.");
            fallback.CycleRivalAiDifficulty();
            Require(fallback.RivalAiDifficulty == CleanDuelAiDifficulty.Expert, "Existing manual Intermediate cycle changed.");
            fallback.CycleRivalAiDifficulty();
            Require(fallback.RivalAiDifficulty == CleanDuelAiDifficulty.Beginner && !fallback.RivalAiFollowsRank, "Existing three-way manual cycle must remain compatible.");
        }

        private static void AssertBattleInformationSafety()
        {
            CleanDuelGame game = new CleanDuelGame();
            game.ConfigureRankedOpponent(Snapshot(38));
            game.StartNewDuel(8801);
            Array.Clear(game.State.Local.Monsters, 0, game.State.Local.Monsters.Length);
            CleanCard first = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local) { FaceDown = true, DefenseOverride = 1 };
            CleanCard second = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local) { FaceDown = true, DefenseOverride = 999999 };
            game.State.Local.Monsters[0] = first;
            game.State.Local.Monsters[1] = second;
            CleanCard attacker = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Rival);
            int target = BattleTarget(game, attacker);
            first.DefenseOverride = 999999;
            second.DefenseOverride = 1;
            game.State.Local.Monsters[0] = second;
            game.State.Local.Monsters[1] = first;
            Require(target == BattleTarget(game, attacker), "Automatic battle selection must not inspect hidden defender identity or DEF.");

            Array.Clear(game.State.Local.Monsters, 0, game.State.Local.Monsters.Length);
            game.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            CleanCard weakAttacker = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Rival);
            Require(BattleTarget(game, weakAttacker) == -1, "Master must hold an unfavorable visible attack rather than receive combat bonuses.");
            Require(game.State.Local.LifePoints == 8000 && game.State.Rival.LifePoints == 8000, "AI target planning must not alter life points.");
        }

        private static void AssertAutonomousMasterTurn()
        {
            CleanDuelGame game = new CleanDuelGame();
            game.ConfigureRankedOpponent(Snapshot(38));
            game.StartNewDuel(7719);
            game.State.Local.Hand.Clear();
            game.State.Local.Deck.Clear();
            game.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local));
            game.State.Local.Graveyard.Clear();
            Array.Clear(game.State.Local.Monsters, 0, game.State.Local.Monsters.Length);
            Array.Clear(game.State.Local.SpellsTraps, 0, game.State.Local.SpellsTraps.Length);
            game.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            game.State.Rival.Hand.Clear();
            game.State.Rival.Deck.Clear();
            game.State.Rival.ExtraDeck.Clear();
            game.State.Rival.Graveyard.Clear();
            game.State.Rival.Banished.Clear();
            game.State.Rival.FieldSpell = null;
            game.State.Rival.NormalSummonUsed = false;
            game.State.Rival.UsedOncePerTurn.Clear();
            Array.Clear(game.State.Rival.Monsters, 0, game.State.Rival.Monsters.Length);
            Array.Clear(game.State.Rival.SpellsTraps, 0, game.State.Rival.SpellsTraps.Length);
            game.State.Rival.Hand.Add(new CleanCard(CleanCardLibrary.Get(12580477), CleanPlayerId.Rival));
            game.State.Rival.Hand.Add(new CleanCard(CleanCardLibrary.Get(71039903), CleanPlayerId.Rival));
            game.State.Rival.Hand.Add(new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Rival));
            game.State.TurnPlayer = CleanPlayerId.Rival;
            game.State.TurnNumber = 2;
            game.State.Phase = CleanDuelPhase.Main;
            game.State.PendingChoice = null;
            typeof(CleanDuelGame).GetMethod("ContinueRivalTurnAfterQuickWindow", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, null);
            Require(game.State.Local.Monsters[0] == null, "Master main planner should prioritize legal Raigeki removal.");
            Require(game.State.Rival.Monsters[0] != null, "Master must perform a legal Normal Summon without player help.");
            Require(game.State.Rival.SpellsTraps[0] != null && !game.State.Rival.SpellsTraps[0].FaceDown, "Master must set and activate True Light through normal response/choice logic.");
            Require(game.State.PendingChoice == null && game.State.TurnPlayer == CleanPlayerId.Local, "Autonomous Master turn must finish or return control without a stuck prompt.");
        }

        private static int BattleTarget(CleanDuelGame game, CleanCard attacker)
        {
            return (int)typeof(CleanDuelGame).GetMethod("SelectRivalBattleTarget", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(game, new object[] { attacker });
        }

        private static int PickMain(CleanDuelRankedAiProfile profile, int seed, CleanDuelAiDifficulty manual = CleanDuelAiDifficulty.Beginner)
        {
            Assembly assembly = typeof(CleanDuelGame).Assembly;
            Type plannerType = assembly.GetType("IcarusGame.CleanDuel.CleanDuelAiPlanner", true);
            Type actionType = assembly.GetType("IcarusGame.CleanDuel.CleanDuelAiAction", true);
            Type kindType = assembly.GetType("IcarusGame.CleanDuel.CleanDuelAiActionKind", true);
            IList actions = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(actionType));
            for (int index = 0; index < 3; index++) actions.Add(Activator.CreateInstance(actionType,
                new object[] { Enum.Parse(kindType, "NormalSummon"), index, index * 1000, "legal test action " + index }));
            object selected = plannerType.GetMethod("SelectMainAction").Invoke(Activator.CreateInstance(plannerType, true),
                new object[] { actions, profile != null ? profile.Difficulty : manual, new System.Random(seed), profile });
            return selected == null ? -1 : (int)actionType.GetProperty("Index").GetValue(selected);
        }

        private static int PickChoice(CleanChoicePrompt prompt, CleanDuelRankedAiProfile profile, int seed)
        {
            Type plannerType = typeof(CleanDuelGame).Assembly.GetType("IcarusGame.CleanDuel.CleanDuelAiPlanner", true);
            return (int)plannerType.GetMethod("SelectChoiceIndex").Invoke(Activator.CreateInstance(plannerType, true),
                new object[] { prompt, profile.Difficulty, new System.Random(seed), profile });
        }

        private static CleanDuelRankedSnapshot Snapshot(int step, bool simulation = false)
        {
            CleanDuelRankDefinition rank = CleanDuelRankCatalog.ForStep(step);
            CleanDuelRankedSnapshot result = CleanDuelRankedSnapshot.FromServer(new CleanDuelRankedData
            {
                seasonId = simulation ? "simulation-v1" : "ranked-v1", progressionVersion = 1, ladderStep = step,
                level = CleanDuelRankCatalog.LevelForStep(step), tierLevels = rank.Levels,
                rankId = rank.Id, rankName = rank.Name, rankPatronName = rank.PatronName,
                highestStep = step, winsRequired = 4, lossesRequired = 3, diamonds = 1400,
                rating = 1000, isSimulation = simulation
            });
            Require(result != null, "Invalid ranked AI test snapshot.");
            return result;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
