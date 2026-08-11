using System.Collections.Generic;

namespace IcarusGame.CleanDuel
{
    public static class CleanDuelAnimationCatalog
    {
        private static readonly HashSet<int> ownSummonCardIds = new HashSet<int>
        {
            89631139, // Blue-Eyes White Dragon
            46986414, // Dark Magician
            30576089, // Blue-Eyes Jet Dragon
            55410871, // Blue-Eyes Chaos MAX Dragon
            12381100, // Dragon Master Magia
            89604813  // Blue-Eyes Ultimate Spirit Dragon
        };

        private static readonly HashSet<int> effectAnimationCardIds = new HashSet<int>
        {
            97268402, // Effect Veiler
            14558127, // Ash Blossom & Joyous Spring
            83764719, // Monster Reborn
            12580477  // Raigeki
        };

        private static readonly HashSet<int> finalBlowCardIds = new HashSet<int>
        {
            89631139, // Blue-Eyes White Dragon
            46986414  // Dark Magician
        };

        public static bool HasOwnSummonAnimation(int cardId)
        {
            return ownSummonCardIds.Contains(cardId);
        }

        public static bool HasEffectAnimation(int cardId)
        {
            return effectAnimationCardIds.Contains(cardId);
        }

        public static bool HasFinalBlowAnimation(int cardId)
        {
            return finalBlowCardIds.Contains(cardId);
        }

        public static bool HasGenericSummonAnimation(CleanSummonPresentationKind summonKind)
        {
            return summonKind != CleanSummonPresentationKind.None;
        }
    }
}
