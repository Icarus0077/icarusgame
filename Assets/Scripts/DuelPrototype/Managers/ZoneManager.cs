namespace IcarusGame.DuelPrototype
{
    public sealed class ZoneManager
    {
        public const int ExtraMonsterZoneCount = 2;

        public bool IsValidZoneIndex(int zoneIndex)
        {
            return zoneIndex >= 0 && zoneIndex < DuelPlayerState.ZoneCount;
        }

        public bool IsValidExtraMonsterZoneIndex(int zoneIndex)
        {
            return zoneIndex >= 0 && zoneIndex < ExtraMonsterZoneCount;
        }

        public int FirstEmptyZone(DuelCard[] zones)
        {
            if (zones == null)
            {
                return -1;
            }

            for (int i = 0; i < zones.Length; i++)
            {
                if (zones[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }

        public int CountMonsters(DuelPlayerState player)
        {
            if (player == null)
            {
                return 0;
            }

            int count = 0;
            for (int zone = 0; zone < DuelPlayerState.ZoneCount; zone++)
            {
                if (player.MonsterZones[zone] != null)
                {
                    count++;
                }
            }

            return count;
        }

        public bool HasEmptyMonsterZone(DuelPlayerState player)
        {
            return player != null && FirstEmptyZone(player.MonsterZones) >= 0;
        }

        public bool HasEmptySpellTrapZone(DuelPlayerState player)
        {
            return player != null && FirstEmptyZone(player.SpellTrapZones) >= 0;
        }
    }
}
