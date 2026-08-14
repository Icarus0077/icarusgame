using System;
using System.Collections.Generic;

namespace IcarusGame.CleanDuel
{
    [Serializable]
    public sealed class CleanDuelAccessRequest
    {
        public string user;
        public string accessCode;
        public string gameId;
        public string environment;
        public string clientVersion;
    }

    [Serializable]
    public sealed class CleanDuelAccessResponse
    {
        public string requestId;
        public string error;
        public CleanDuelAccessDecision decision;
        public CleanDuelAccessUser user;
        public CleanDuelAccessSettings settings;
    }

    [Serializable]
    public sealed class CleanDuelAccessDecision
    {
        public bool allowed;
        public string code;
        public string reason;
    }

    [Serializable]
    public sealed class CleanDuelAccessUser
    {
        public string id;
        public string displayName;
        public string email;
        public string role;
        public string accessGroup;
    }

    [Serializable]
    public sealed class CleanDuelAccessSettings
    {
        public string matchmakingRegion;
        public bool rankedEnabled;
        public int deckSlots;
        public string contentAudience;
        public string defaultCardAccess;
        public CleanDuelCardAccessOverride[] cardOverrides;
    }

    [Serializable]
    public sealed class CleanDuelCardAccessOverride
    {
        public string cardId;
        public string cardName;
        public bool allowed;
        public string availability;
        public string audience;
        public string regulation;
    }

    public sealed class CleanDuelAccessSession
    {
        private readonly HashSet<int> unavailableCardIds = new HashSet<int>();

        private CleanDuelAccessSession()
        {
        }

        public string UserId { get; private set; }
        public string DisplayName { get; private set; }
        public string Email { get; private set; }
        public string Role { get; private set; }
        public string AccessGroup { get; private set; }
        public string MatchmakingRegion { get; private set; }
        public bool RankedEnabled { get; private set; }
        public int DeckSlots { get; private set; }
        public IEnumerable<int> UnavailableCardIds { get { return unavailableCardIds; } }

        public static bool TryCreate(CleanDuelAccessResponse response, out CleanDuelAccessSession session, out string error)
        {
            session = null;
            error = string.Empty;
            if (response == null || response.decision == null)
            {
                error = "De server stuurde geen geldige toegangsbeslissing.";
                return false;
            }

            if (!response.decision.allowed)
            {
                error = string.IsNullOrWhiteSpace(response.decision.reason)
                    ? "Je hebt geen toegang tot deze game."
                    : response.decision.reason;
                return false;
            }

            if (response.user == null || string.IsNullOrWhiteSpace(response.user.id))
            {
                error = "De server stuurde geen geldige gebruiker.";
                return false;
            }

            CleanDuelAccessSettings settings = response.settings ?? new CleanDuelAccessSettings();
            CleanDuelAccessSession created = new CleanDuelAccessSession
            {
                UserId = response.user.id,
                DisplayName = string.IsNullOrWhiteSpace(response.user.displayName) ? response.user.email : response.user.displayName,
                Email = response.user.email ?? string.Empty,
                Role = response.user.role ?? string.Empty,
                AccessGroup = response.user.accessGroup ?? string.Empty,
                MatchmakingRegion = settings.matchmakingRegion ?? string.Empty,
                RankedEnabled = settings.rankedEnabled,
                DeckSlots = settings.deckSlots
            };

            CleanDuelCardAccessOverride[] overrides = settings.cardOverrides ?? Array.Empty<CleanDuelCardAccessOverride>();
            for (int index = 0; index < overrides.Length; index++)
            {
                CleanDuelCardAccessOverride cardOverride = overrides[index];
                int cardId;
                if (cardOverride != null && !cardOverride.allowed && int.TryParse(cardOverride.cardId, out cardId))
                {
                    created.unavailableCardIds.Add(cardId);
                }
            }

            session = created;
            return true;
        }
    }

    public sealed class CleanDuelAccessResult
    {
        public bool Success;
        public string Message;
        public CleanDuelAccessResponse Response;
    }
}
