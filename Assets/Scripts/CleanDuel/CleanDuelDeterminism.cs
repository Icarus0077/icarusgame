using System;
using System.Collections.Generic;

namespace IcarusGame.CleanDuel
{
    public sealed class CleanDuelEventRecord
    {
        public CleanDuelEventRecord(long sequence, string message)
        {
            Sequence = sequence;
            Message = message ?? string.Empty;
        }

        public long Sequence { get; private set; }
        public string Message { get; private set; }
    }

    public static class CleanDuelDeterminism
    {
        private const ulong FnvOffset = 14695981039346656037UL;
        private const ulong FnvPrime = 1099511628211UL;

        public static string ComputeStateHash(CleanDuelState state)
        {
            if (state == null)
            {
                return string.Empty;
            }

            Dictionary<int, int> stableLocations = BuildStableLocationMap(state);
            ulong hash = FnvOffset;
            Add(ref hash, state.RandomSeed);
            Add(ref hash, state.StateSequence);
            Add(ref hash, (int)state.TurnPlayer);
            Add(ref hash, (int)state.Phase);
            Add(ref hash, state.TurnNumber);
            Add(ref hash, state.GameOver);
            Add(ref hash, state.CinematicSequence);
            Add(ref hash, state.RivalryClimaxShown);
            AddPlayer(ref hash, state.Local, stableLocations);
            AddPlayer(ref hash, state.Rival, stableLocations);
            AddCards(ref hash, state.ExtraMonsters, stableLocations);
            AddPrompt(ref hash, state.PendingChoice);
            AddResult(ref hash, state.Result);
            return hash.ToString("X16");
        }

        private static Dictionary<int, int> BuildStableLocationMap(CleanDuelState state)
        {
            Dictionary<int, int> locations = new Dictionary<int, int>();
            int next = 1;
            AddLocations(locations, state.Local.Deck, ref next);
            AddLocations(locations, state.Local.ExtraDeck, ref next);
            AddLocations(locations, state.Local.Hand, ref next);
            AddLocations(locations, state.Local.Monsters, ref next);
            AddLocations(locations, state.Local.SpellsTraps, ref next);
            AddLocation(locations, state.Local.FieldSpell, ref next);
            AddLocations(locations, state.Local.Graveyard, ref next);
            AddLocations(locations, state.Local.Banished, ref next);
            AddLocations(locations, state.Rival.Deck, ref next);
            AddLocations(locations, state.Rival.ExtraDeck, ref next);
            AddLocations(locations, state.Rival.Hand, ref next);
            AddLocations(locations, state.Rival.Monsters, ref next);
            AddLocations(locations, state.Rival.SpellsTraps, ref next);
            AddLocation(locations, state.Rival.FieldSpell, ref next);
            AddLocations(locations, state.Rival.Graveyard, ref next);
            AddLocations(locations, state.Rival.Banished, ref next);
            AddLocations(locations, state.ExtraMonsters, ref next);
            return locations;
        }

        private static void AddPlayer(ref ulong hash, CleanPlayerState player, Dictionary<int, int> stableLocations)
        {
            Add(ref hash, (int)player.Id);
            Add(ref hash, player.Name);
            Add(ref hash, player.LifePoints);
            Add(ref hash, player.NormalSummonUsed);
            Add(ref hash, player.PendulumSummonUsed);
            Add(ref hash, player.MaxxCDrawActive);
            Add(ref hash, player.GatewayToChaosCounters);
            Add(ref hash, player.ExtraDeckDragonOnlyThisTurn);

            List<string> onceKeys = new List<string>(player.UsedOncePerTurn);
            onceKeys.Sort(StringComparer.Ordinal);
            Add(ref hash, onceKeys.Count);
            for (int i = 0; i < onceKeys.Count; i++)
            {
                Add(ref hash, onceKeys[i]);
            }

            AddCards(ref hash, player.Deck, stableLocations);
            AddCards(ref hash, player.ExtraDeck, stableLocations);
            AddCards(ref hash, player.Hand, stableLocations);
            AddCards(ref hash, player.Monsters, stableLocations);
            AddCards(ref hash, player.SpellsTraps, stableLocations);
            AddCard(ref hash, player.FieldSpell, stableLocations);
            AddCards(ref hash, player.Graveyard, stableLocations);
            AddCards(ref hash, player.Banished, stableLocations);
        }

        private static void AddPrompt(ref ulong hash, CleanChoicePrompt prompt)
        {
            Add(ref hash, prompt != null);
            if (prompt == null)
            {
                return;
            }

            Add(ref hash, prompt.SourceCardId);
            Add(ref hash, prompt.Title);
            Add(ref hash, prompt.Body);
            Add(ref hash, (int)prompt.Kind);
            Add(ref hash, prompt.CanCancel);
            Add(ref hash, prompt.Options.Count);
            for (int i = 0; i < prompt.Options.Count; i++)
            {
                CleanChoiceOption option = prompt.Options[i];
                Add(ref hash, option.Id);
                Add(ref hash, option.Label);
                Add(ref hash, option.Detail);
                Add(ref hash, option.IsAvailable);
                Add(ref hash, option.DisabledReason);
                Add(ref hash, option.CardDefinitionId);
            }
        }

        private static void AddResult(ref ulong hash, CleanDuelResult result)
        {
            Add(ref hash, result != null);
            if (result == null)
            {
                return;
            }

            Add(ref hash, (int)result.Reason);
            Add(ref hash, result.HasWinner);
            Add(ref hash, (int)result.Winner);
            Add(ref hash, (int)result.Loser);
            Add(ref hash, result.TurnNumber);
            Add(ref hash, result.Summary);
        }

        private static void AddCards(ref ulong hash, IList<CleanCard> cards, Dictionary<int, int> stableLocations)
        {
            Add(ref hash, cards == null ? -1 : cards.Count);
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                AddCard(ref hash, cards[i], stableLocations);
            }
        }

        private static void AddCard(ref ulong hash, CleanCard card, Dictionary<int, int> stableLocations)
        {
            Add(ref hash, card != null);
            if (card == null)
            {
                return;
            }

            Add(ref hash, card.Definition.Id);
            Add(ref hash, (int)card.Owner);
            Add(ref hash, (int)card.Controller);
            Add(ref hash, card.FaceDown);
            Add(ref hash, card.SetThisTurn);
            Add(ref hash, card.HasBattled);
            Add(ref hash, card.SummonedThisTurn);
            Add(ref hash, card.PositionChangedThisTurn);
            Add(ref hash, (int)card.Stance);
            Add(ref hash, card.AttackBonus);
            Add(ref hash, card.EquipAttackBonus);
            Add(ref hash, card.DefenseBonus);
            Add(ref hash, card.AttackOverride);
            Add(ref hash, card.DefenseOverride);
            Add(ref hash, card.CannotAttack);
            Add(ref hash, card.EffectsNegated);
            Add(ref hash, card.EffectsNegatedUntilEndOfTurn);
            Add(ref hash, card.PiercingThisTurn);
            Add(ref hash, card.RitualSummonedUsingBlueEyesWhiteDragon);
            Add(ref hash, card.SentToGraveThisTurn);
            Add(ref hash, card.FusionSummoned);
            Add(ref hash, card.SynchroSummoned);
            Add(ref hash, card.LinkSummoned);
            Add(ref hash, card.XyzSummoned);
            Add(ref hash, card.FaceUpExtraDeck);
            Add(ref hash, card.DestroyDuringEndPhase);
            Add(ref hash, card.ProperlySummoned);

            int equippedLocation;
            Add(ref hash, card.EquippedToRuntimeId != 0 && stableLocations.TryGetValue(card.EquippedToRuntimeId, out equippedLocation) ? equippedLocation : 0);
            AddCards(ref hash, card.XyzMaterials, stableLocations);
        }

        private static void AddLocations(Dictionary<int, int> locations, IList<CleanCard> cards, ref int next)
        {
            if (cards == null)
            {
                return;
            }

            for (int i = 0; i < cards.Count; i++)
            {
                AddLocation(locations, cards[i], ref next);
            }
        }

        private static void AddLocation(Dictionary<int, int> locations, CleanCard card, ref int next)
        {
            if (card == null)
            {
                next++;
                return;
            }

            locations[card.RuntimeId] = next++;
            for (int i = 0; i < card.XyzMaterials.Count; i++)
            {
                AddLocation(locations, card.XyzMaterials[i], ref next);
            }
        }

        private static void Add(ref ulong hash, bool value)
        {
            Add(ref hash, value ? 1 : 0);
        }

        private static void Add(ref ulong hash, int value)
        {
            unchecked
            {
                AddByte(ref hash, (byte)value);
                AddByte(ref hash, (byte)(value >> 8));
                AddByte(ref hash, (byte)(value >> 16));
                AddByte(ref hash, (byte)(value >> 24));
            }
        }

        private static void Add(ref ulong hash, long value)
        {
            unchecked
            {
                Add(ref hash, (int)value);
                Add(ref hash, (int)(value >> 32));
            }
        }

        private static void Add(ref ulong hash, string value)
        {
            string safe = value ?? string.Empty;
            Add(ref hash, safe.Length);
            for (int i = 0; i < safe.Length; i++)
            {
                char character = safe[i];
                AddByte(ref hash, (byte)character);
                AddByte(ref hash, (byte)(character >> 8));
            }
        }

        private static void AddByte(ref ulong hash, byte value)
        {
            hash ^= value;
            hash *= FnvPrime;
        }
    }
}
