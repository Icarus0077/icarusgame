namespace IcarusGame.DuelPrototype
{
    public sealed class PhaseManager
    {
        public static readonly DuelPhase[] OrderedPhases =
        {
            DuelPhase.Draw,
            DuelPhase.Standby,
            DuelPhase.Main1,
            DuelPhase.Battle,
            DuelPhase.Main2,
            DuelPhase.End
        };

        public bool IsMainPhase(DuelPhase phase)
        {
            return phase == DuelPhase.Main1 || phase == DuelPhase.Main2;
        }

        public bool IsAutomaticPlayerPhaseCandidate(DuelPhase phase)
        {
            return phase == DuelPhase.Draw || phase == DuelPhase.Standby || phase == DuelPhase.End;
        }

        public DuelPhase NextPhase(DuelPhase currentPhase, int turnNumber, TurnManager turnManager)
        {
            switch (currentPhase)
            {
                case DuelPhase.Draw:
                    return DuelPhase.Standby;
                case DuelPhase.Standby:
                    return DuelPhase.Main1;
                case DuelPhase.Main1:
                    return turnManager != null && turnManager.CanConductBattlePhase(turnNumber)
                        ? DuelPhase.Battle
                        : DuelPhase.End;
                case DuelPhase.Battle:
                    return DuelPhase.Main2;
                case DuelPhase.Main2:
                    return DuelPhase.End;
                default:
                    return DuelPhase.Draw;
            }
        }

        public string PhaseName(DuelPhase phase)
        {
            switch (phase)
            {
                case DuelPhase.Draw:
                    return "Draw Phase";
                case DuelPhase.Standby:
                    return "Standby Phase";
                case DuelPhase.Main1:
                    return "Main Phase 1";
                case DuelPhase.Battle:
                    return "Battle Phase";
                case DuelPhase.Main2:
                    return "Main Phase 2";
                case DuelPhase.End:
                    return "End Phase";
                default:
                    return phase.ToString();
            }
        }

        public string PhaseShortLabel(DuelPhase phase)
        {
            switch (phase)
            {
                case DuelPhase.Draw:
                    return "DP";
                case DuelPhase.Standby:
                    return "SP";
                case DuelPhase.Main1:
                    return "M1";
                case DuelPhase.Battle:
                    return "BP";
                case DuelPhase.Main2:
                    return "M2";
                case DuelPhase.End:
                    return "EP";
                default:
                    return phase.ToString();
            }
        }
    }
}
