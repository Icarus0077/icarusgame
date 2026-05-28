namespace IcarusGame.DuelPrototype
{
    public struct BattleResult
    {
        public bool DestroyAttacker;
        public bool DestroyDefender;
        public int AttackerDamage;
        public int DefenderDamage;
        public string LogMessage;
    }

    public sealed class BattleSystem
    {
        public bool CanDeclareAttack(DuelCard attacker, DuelPhase phase, int turnNumber, TurnManager turnManager)
        {
            return attacker != null &&
                   attacker.Position == MonsterBattlePosition.Attack &&
                   !attacker.HasAttacked &&
                   phase == DuelPhase.Battle &&
                   turnManager != null &&
                   turnManager.CanConductBattlePhase(turnNumber);
        }

        public BattleResult ResolveAttackAgainstAttack(DuelCard attacker, DuelCard defender)
        {
            BattleResult result = new BattleResult();
            int difference = attacker.CurrentAttack - defender.CurrentAttack;

            if (difference > 0)
            {
                result.DestroyDefender = true;
                result.DefenderDamage = difference;
                result.LogMessage = "defender_damage";
            }
            else if (difference < 0)
            {
                result.DestroyAttacker = true;
                result.AttackerDamage = -difference;
                result.LogMessage = "attacker_damage";
            }
            else if (attacker.CurrentAttack == 0 && defender.CurrentAttack == 0)
            {
                result.LogMessage = "zero_attack_draw";
            }
            else
            {
                result.DestroyAttacker = true;
                result.DestroyDefender = true;
                result.LogMessage = "equal_attack_destroy_both";
            }

            return result;
        }

        public BattleResult ResolveAttackAgainstDefense(DuelCard attacker, DuelCard defender)
        {
            BattleResult result = new BattleResult();
            int difference = attacker.CurrentAttack - defender.CurrentDefense;

            if (difference > 0)
            {
                result.DestroyDefender = true;
                result.LogMessage = "defender_destroyed";
            }
            else if (difference < 0)
            {
                result.AttackerDamage = -difference;
                result.LogMessage = "attacker_defense_damage";
            }
            else
            {
                result.LogMessage = "equal_attack_defense";
            }

            return result;
        }

        public int DirectAttackDamage(DuelCard attacker)
        {
            return attacker == null || attacker.CurrentAttack < 0 ? 0 : attacker.CurrentAttack;
        }
    }
}
