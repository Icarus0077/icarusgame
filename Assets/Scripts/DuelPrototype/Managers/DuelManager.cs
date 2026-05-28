using System.Collections.Generic;

namespace IcarusGame.DuelPrototype
{
    public sealed class DeckValidationResult
    {
        public readonly List<string> Warnings = new List<string>();

        public bool IsLegal
        {
            get { return Warnings.Count == 0; }
        }

        public void AddWarning(string warning)
        {
            if (!string.IsNullOrEmpty(warning))
            {
                Warnings.Add(warning);
            }
        }
    }

    public sealed class DuelManager
    {
        public const int StartingLifePoints = 8000;
        public const int StartingHandSize = 5;
        public const int MainDeckMinimum = 40;
        public const int MainDeckMaximum = 60;
        public const int ExtraDeckMaximum = 15;
        public const int DefaultCopyLimit = 3;
        public const int EndPhaseHandLimit = 6;

        public void ApplyDuelSetup(DuelPlayerState player)
        {
            if (player != null)
            {
                player.LifePoints = StartingLifePoints;
            }
        }

        public bool IsLegalMainDeckSize(int count)
        {
            return count >= MainDeckMinimum && count <= MainDeckMaximum;
        }

        public bool IsLegalExtraDeckSize(int count)
        {
            return count >= 0 && count <= ExtraDeckMaximum;
        }

        public bool HasLostByDeckOut(DuelPlayerState player)
        {
            return player != null && player.Deck.Count <= 0;
        }

        public string FirstCopyLimitViolation(IList<DuelCardDefinition> deck)
        {
            if (deck == null)
            {
                return string.Empty;
            }

            Dictionary<string, int> copiesByName = new Dictionary<string, int>();
            for (int i = 0; i < deck.Count; i++)
            {
                DuelCardDefinition definition = deck[i];
                if (definition == null || string.IsNullOrEmpty(definition.Name))
                {
                    continue;
                }

                int copies;
                copiesByName.TryGetValue(definition.Name, out copies);
                copies++;
                copiesByName[definition.Name] = copies;
                if (copies > DefaultCopyLimit)
                {
                    return definition.Name;
                }
            }

            return string.Empty;
        }

        public DeckValidationResult ValidateDeck(
            IList<DuelCardDefinition> mainDeck,
            IList<DuelCardDefinition> extraDeck,
            IDictionary<string, int> copyLimits)
        {
            DeckValidationResult result = new DeckValidationResult();

            int mainCount = mainDeck == null ? 0 : mainDeck.Count;
            int extraCount = extraDeck == null ? 0 : extraDeck.Count;
            if (!IsLegalMainDeckSize(mainCount))
            {
                result.AddWarning("Main Deck moet " + MainDeckMinimum + "-" + MainDeckMaximum + " kaarten hebben.");
            }

            if (!IsLegalExtraDeckSize(extraCount))
            {
                result.AddWarning("Extra Deck mag maximaal " + ExtraDeckMaximum + " kaarten hebben.");
            }

            ValidateMainDeckContents(mainDeck, result);
            ValidateExtraDeckContents(extraDeck, result);
            ValidateCopyLimits(mainDeck, extraDeck, copyLimits, result);
            return result;
        }

        private void ValidateMainDeckContents(IList<DuelCardDefinition> mainDeck, DeckValidationResult result)
        {
            if (mainDeck == null)
            {
                return;
            }

            for (int i = 0; i < mainDeck.Count; i++)
            {
                DuelCardDefinition card = mainDeck[i];
                if (card != null && !card.CanBeInMainDeck)
                {
                    result.AddWarning(card.Name + " hoort niet in het Main Deck.");
                    return;
                }
            }
        }

        private void ValidateExtraDeckContents(IList<DuelCardDefinition> extraDeck, DeckValidationResult result)
        {
            if (extraDeck == null)
            {
                return;
            }

            for (int i = 0; i < extraDeck.Count; i++)
            {
                DuelCardDefinition card = extraDeck[i];
                if (card != null && !card.CanBeInExtraDeck)
                {
                    result.AddWarning(card.Name + " hoort niet in het Extra Deck.");
                    return;
                }
            }
        }

        private void ValidateCopyLimits(
            IList<DuelCardDefinition> mainDeck,
            IList<DuelCardDefinition> extraDeck,
            IDictionary<string, int> copyLimits,
            DeckValidationResult result)
        {
            Dictionary<string, int> copiesByName = new Dictionary<string, int>();
            CountCopies(mainDeck, copiesByName);
            CountCopies(extraDeck, copiesByName);

            foreach (KeyValuePair<string, int> pair in copiesByName)
            {
                int allowed = AllowedCopies(pair.Key, copyLimits);
                if (pair.Value > allowed)
                {
                    result.AddWarning(pair.Key + " heeft " + pair.Value + " kopieen; limiet is " + allowed + ".");
                    return;
                }
            }
        }

        private void CountCopies(IList<DuelCardDefinition> deck, Dictionary<string, int> copiesByName)
        {
            if (deck == null)
            {
                return;
            }

            for (int i = 0; i < deck.Count; i++)
            {
                DuelCardDefinition definition = deck[i];
                if (definition == null || string.IsNullOrEmpty(definition.Name))
                {
                    continue;
                }

                int copies;
                copiesByName.TryGetValue(definition.Name, out copies);
                copiesByName[definition.Name] = copies + 1;
            }
        }

        private int AllowedCopies(string cardName, IDictionary<string, int> copyLimits)
        {
            if (copyLimits == null || string.IsNullOrEmpty(cardName))
            {
                return DefaultCopyLimit;
            }

            int limit;
            return copyLimits.TryGetValue(cardName, out limit) ? limit : DefaultCopyLimit;
        }
    }
}
