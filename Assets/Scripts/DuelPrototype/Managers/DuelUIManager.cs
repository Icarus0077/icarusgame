using System.Text;

namespace IcarusGame.DuelPrototype
{
    public sealed class DuelUIManager
    {
        private readonly PhaseManager phaseManager;

        public DuelUIManager(PhaseManager phaseManager)
        {
            this.phaseManager = phaseManager ?? new PhaseManager();
        }

        public string PhaseLabel(DuelPhase phase)
        {
            return phaseManager.PhaseName(phase);
        }

        public string PhaseShortLabel(DuelPhase phase)
        {
            return phaseManager.PhaseShortLabel(phase);
        }

        public string BuildZoneInfo(DuelPlayerState player)
        {
            if (player == null)
            {
                return string.Empty;
            }

            return "Deck " + player.Deck.Count +
                   "   Extra " + player.ExtraDeck.Count +
                   "   Hand " + player.Hand.Count +
                   "   GY " + player.Graveyard.Count +
                   "   Banished " + player.Banished.Count +
                   "   Field " + (player.FieldZone == null ? "0" : "1");
        }

        public string BuildDuelOverviewText(DuelEngine engine)
        {
            if (engine == null)
            {
                return string.Empty;
            }

            string lastEvent = engine.Log.Count > 0 ? engine.Log[engine.Log.Count - 1] : "Duel gestart.";
            return "Fase: " + PhaseLabel(engine.CurrentPhase) +
                   "\nActieve speler: " + engine.GetActivePlayer().Name +
                   "\nLaatste actie: " + lastEvent;
        }

        public string BuildTributeRuleText(DuelCardDefinition definition)
        {
            if (definition == null || definition.Kind != DuelCardKind.Monster)
            {
                return string.Empty;
            }

            if (definition.Level <= 4)
            {
                return "Summonregel: Level 1-4 heeft geen tribute nodig.";
            }

            if (definition.Level <= 6)
            {
                return "Summonregel: Level 5-6 vereist 1 tribute, tenzij de kaarttekst dit specifiek overschrijft.";
            }

            if (definition.Level <= 8)
            {
                return "Summonregel: Level 7-8 vereist 2 tributes, tenzij de kaarttekst dit specifiek overschrijft.";
            }

            return "Summonregel: Level 9 of hoger vereist 3 tributes in deze engine, tenzij de kaarttekst dit specifiek overschrijft.";
        }

        public string BuildPrototypeExtraEffectText(DuelCardDefinition definition)
        {
            if (definition == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            if (definition.IsQuickPlaySpell)
            {
                builder.Append("Prototype extra effect: bij activatie krijgen je eigen face-up monsters +500 ATK tot de End Phase.");
            }

            if (definition.IsContinuousCard)
            {
                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append("Prototype extra effect: zolang deze kaart face-up ligt, krijg je in je Standby Phase +300 LP en je eigen face-up monsters +200 ATK tot de End Phase.");
            }

            return builder.ToString();
        }
    }
}
