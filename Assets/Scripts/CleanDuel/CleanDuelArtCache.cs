using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace IcarusGame.CleanDuel
{
    public sealed class CleanDuelArtCache
    {
        private static readonly string[] CardExtensions = { ".jpg", ".jpeg", ".png" };
        private static readonly string[] FieldExtensions = { ".png", ".jpg", ".jpeg" };

        private readonly Dictionary<int, Texture2D> textures = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Texture2D> fieldTextures = new Dictionary<int, Texture2D>();
        private readonly Dictionary<int, Sprite> sprites = new Dictionary<int, Sprite>();
        private Texture2D defaultFieldTexture;
        private string[] fieldImagePaths;

        public Texture2D GetTexture(int cardId)
        {
            Texture2D texture;
            if (textures.TryGetValue(cardId, out texture))
            {
                return texture;
            }

            string path = FindImagePathForId(CardImageDirectory, cardId, CardExtensions);
            if (string.IsNullOrEmpty(path))
            {
                textures[cardId] = null;
                return null;
            }

            texture = LoadTexture(path, "CardArt_" + cardId);
            textures[cardId] = texture;
            return texture;
        }

        public Texture2D GetFieldTexture(int fieldId)
        {
            Texture2D texture;
            if (fieldTextures.TryGetValue(fieldId, out texture))
            {
                return texture;
            }

            string path = FindImagePathForId(FieldImageDirectory, fieldId, FieldExtensions);
            if (string.IsNullOrEmpty(path))
            {
                fieldTextures[fieldId] = null;
                return null;
            }

            texture = LoadTexture(path, "FieldArt_" + fieldId);
            fieldTextures[fieldId] = texture;
            return texture;
        }

        public Texture2D GetFirstFieldTexture()
        {
            if (defaultFieldTexture != null)
            {
                return defaultFieldTexture;
            }

            EnsureFieldImagePaths();
            if (fieldImagePaths.Length == 0)
            {
                return null;
            }

            defaultFieldTexture = LoadTexture(fieldImagePaths[0], "FieldArt_Default");
            return defaultFieldTexture;
        }

        public Sprite GetSprite(int cardId)
        {
            Sprite sprite;
            if (sprites.TryGetValue(cardId, out sprite))
            {
                return sprite;
            }

            Texture2D texture = GetTexture(cardId);
            if (texture == null)
            {
                sprites[cardId] = null;
                return null;
            }

            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0u,
                SpriteMeshType.FullRect);
            sprites[cardId] = sprite;
            return sprite;
        }

        public Color CardColor(CleanCard card)
        {
            if (card == null)
            {
                return new Color(0.07f, 0.09f, 0.13f, 1f);
            }

            if (card.FaceDown)
            {
                return new Color(0.09f, 0.05f, 0.03f, 1f);
            }

            switch (card.Definition.Kind)
            {
                case CleanCardKind.Spell:
                    return new Color(0.06f, 0.42f, 0.34f, 1f);
                case CleanCardKind.Trap:
                    return new Color(0.55f, 0.12f, 0.38f, 1f);
                default:
                    if (card.Definition.Level >= 7)
                    {
                        return new Color(0.67f, 0.36f, 0.12f, 1f);
                    }

                    return new Color(0.46f, 0.23f, 0.09f, 1f);
            }
        }

        private static string CardImageDirectory
        {
            get { return Path.Combine(Application.dataPath, "YGOProDeck", "CardImages"); }
        }

        private static string FieldImageDirectory
        {
            get { return Path.Combine(Application.dataPath, "YGOProDeck", "FieldImages"); }
        }

        private static string FindImagePathForId(string directory, int imageId, string[] extensions)
        {
            string rawId = imageId.ToString();
            string path = FindImagePath(directory, rawId, extensions);
            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }

            string paddedId = imageId.ToString("D8");
            return paddedId == rawId ? string.Empty : FindImagePath(directory, paddedId, extensions);
        }

        private static string FindImagePath(string directory, string fileStem, string[] extensions)
        {
            if (!Directory.Exists(directory))
            {
                return string.Empty;
            }

            for (int i = 0; i < extensions.Length; i++)
            {
                string path = Path.Combine(directory, fileStem + extensions[i]);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return string.Empty;
        }

        private static Texture2D LoadTexture(string path, string textureName)
        {
            byte[] bytes = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.name = textureName;
            if (!ImageConversion.LoadImage(texture, bytes, false))
            {
                Object.Destroy(texture);
                return null;
            }

            return texture;
        }

        private void EnsureFieldImagePaths()
        {
            if (fieldImagePaths != null)
            {
                return;
            }

            if (!Directory.Exists(FieldImageDirectory))
            {
                fieldImagePaths = new string[0];
                return;
            }

            List<string> paths = new List<string>();
            for (int i = 0; i < FieldExtensions.Length; i++)
            {
                paths.AddRange(Directory.GetFiles(FieldImageDirectory, "*" + FieldExtensions[i], SearchOption.TopDirectoryOnly));
            }

            fieldImagePaths = paths.ToArray();
            System.Array.Sort(fieldImagePaths, System.StringComparer.OrdinalIgnoreCase);
        }
    }
}