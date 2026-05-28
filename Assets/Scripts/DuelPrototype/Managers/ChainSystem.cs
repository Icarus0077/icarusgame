namespace IcarusGame.DuelPrototype
{
    public enum DuelDamageStep
    {
        None,
        StartStep,
        BattleStep,
        DamageStep,
        DamageCalculation,
        EndStep
    }

    public sealed class ChainLink
    {
        public ChainLink(int index, DuelCard card, DuelPlayerId controllerId, int spellSpeed)
        {
            Index = index;
            Card = card;
            ControllerId = controllerId;
            SpellSpeed = spellSpeed;
        }

        public int Index { get; private set; }
        public DuelCard Card { get; private set; }
        public DuelPlayerId ControllerId { get; private set; }
        public int SpellSpeed { get; private set; }
    }

    public sealed class ChainSystem
    {
        private readonly System.Collections.Generic.List<ChainLink> chainLinks = new System.Collections.Generic.List<ChainLink>();

        public int ChainCount
        {
            get { return chainLinks.Count; }
        }

        public void Clear()
        {
            chainLinks.Clear();
        }

        public ChainLink AddLink(DuelCard card, DuelPlayerId controllerId)
        {
            ChainLink link = new ChainLink(chainLinks.Count + 1, card, controllerId, SpellSpeed(card));
            chainLinks.Add(link);
            return link;
        }

        public ChainLink LastLink()
        {
            return chainLinks.Count == 0 ? null : chainLinks[chainLinks.Count - 1];
        }

        public int SpellSpeed(DuelCard card)
        {
            if (card == null)
            {
                return 0;
            }

            if (card.Definition.IsCounterTrap)
            {
                return 3;
            }

            if (card.Definition.Kind == DuelCardKind.Trap || card.Definition.IsQuickPlaySpell)
            {
                return 2;
            }

            return 1;
        }

        public bool CanRespondToChain(int currentChainSpeed, DuelCard responseCard)
        {
            int responseSpeed = SpellSpeed(responseCard);
            return responseSpeed > 0 && responseSpeed >= currentChainSpeed;
        }

        public bool CanRespondToCurrentChain(DuelCard responseCard)
        {
            ChainLink lastLink = LastLink();
            return lastLink == null || CanRespondToChain(lastLink.SpellSpeed, responseCard);
        }

        public bool IsAllowedInDamageStep(DuelCard card, DuelDamageStep damageStep)
        {
            if (damageStep == DuelDamageStep.None)
            {
                return true;
            }

            if (card == null)
            {
                return false;
            }

            return card.Definition.IsCounterTrap ||
                   card.Definition.Effect == DuelEffectKind.NegateAttack ||
                   card.Definition.Effect == DuelEffectKind.DestroyAttacker ||
                   card.Definition.Effect == DuelEffectKind.BoostOwnMonsters;
        }

        public bool CanActivateSpellInPhase(DuelCard card, DuelPhase phase, PhaseManager phaseManager)
        {
            if (card == null || card.Definition.Kind != DuelCardKind.Spell)
            {
                return false;
            }

            if (card.Definition.IsQuickPlaySpell)
            {
                return true;
            }

            return phaseManager != null && phaseManager.IsMainPhase(phase);
        }

        public bool CanActivateSetSpellTrap(DuelCard card, DuelPhase phase, int setTurn, int currentTurn, PhaseManager phaseManager)
        {
            if (card == null || !card.IsFaceDown)
            {
                return false;
            }

            if (card.Definition.Kind == DuelCardKind.Trap && setTurn >= currentTurn)
            {
                return false;
            }

            if (card.Definition.IsQuickPlaySpell && setTurn >= currentTurn)
            {
                return false;
            }

            if (card.Definition.Kind == DuelCardKind.Spell)
            {
                return CanActivateSpellInPhase(card, phase, phaseManager);
            }

            return card.Definition.Kind == DuelCardKind.Trap &&
                   card.Definition.IsContinuousCard &&
                   phaseManager != null &&
                   phaseManager.IsMainPhase(phase);
        }
    }
}
