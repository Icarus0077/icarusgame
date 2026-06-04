using System;
using System.Collections.Generic;
using UnityEngine;

namespace IcarusGame.MasterDuel
{
    [Serializable]
    public sealed class CardArtworkEntry
    {
        public int DatabaseId;
        public Texture2D Texture;
    }

    [CreateAssetMenu(fileName = "CardArtworkCatalog", menuName = "Icarus/Card Artwork Catalog")]
    public sealed class CardArtworkCatalog : ScriptableObject
    {
        [SerializeField] private List<CardArtworkEntry> entries = new List<CardArtworkEntry>();

        private Dictionary<int, Texture2D> texturesById;

        public bool TryGetTexture(int databaseId, out Texture2D texture)
        {
            EnsureLookup();
            return texturesById.TryGetValue(databaseId, out texture) && texture != null;
        }

        private void OnEnable()
        {
            texturesById = null;
        }

        private void EnsureLookup()
        {
            if (texturesById != null)
            {
                return;
            }

            texturesById = new Dictionary<int, Texture2D>();
            for (int i = 0; i < entries.Count; i++)
            {
                CardArtworkEntry entry = entries[i];
                if (entry != null && entry.Texture != null)
                {
                    texturesById[entry.DatabaseId] = entry.Texture;
                }
            }
        }
    }
}
