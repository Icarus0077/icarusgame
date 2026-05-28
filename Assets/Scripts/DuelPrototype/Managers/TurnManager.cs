namespace IcarusGame.DuelPrototype
{
    public sealed class TurnManager
    {
        public bool ShouldDrawInDrawPhase(int turnNumber)
        {
            return turnNumber > 1;
        }

        public bool CanConductBattlePhase(int turnNumber)
        {
            return turnNumber > 1;
        }

        public DuelPlayerId OtherPlayer(DuelPlayerId id)
        {
            return id == DuelPlayerId.Player ? DuelPlayerId.Opponent : DuelPlayerId.Player;
        }

        public void PrepareTurn(DuelPlayerState player)
        {
            if (player != null)
            {
                player.NormalSummonUsed = false;
            }
        }
    }
}
