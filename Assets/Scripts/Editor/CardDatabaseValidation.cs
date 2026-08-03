using System;
using IcarusGame.MasterDuel;
using UnityEditor;
using UnityEngine;

namespace IcarusGame.EditorTools
{
    public static class CardDatabaseValidation
    {
        [MenuItem("Tools/Icarus/Card Database/Validate CDB")]
        public static void Validate()
        {
            YgoCardDatabase database;
            string error;
            if (!YgoCardDatabase.TryLoadDefault(out database, out error))
            {
                throw new InvalidOperationException("Unable to load cards.cdb: " + error);
            }

            Require(database.Count >= 14000, "Expected at least 14000 cards, got " + database.Count + ".");
            RequireCard(database, 89631139, "Blue-Eyes White Dragon", CardKind.Monster, MonsterFrame.Normal);
            RequireCard(database, 71143015, "Ultimate Fusion", CardKind.Spell, MonsterFrame.Normal);
            RequireCard(database, 63767246, "Number 38: Hope Harbinger Dragon Titanic Galaxy", CardKind.Monster, MonsterFrame.Xyz);
            RequireCard(database, 20515672, "S-Force Nightchaser", CardKind.Monster, MonsterFrame.Link);

            Debug.Log("cards.cdb validation passed. Loaded " + database.Count + " cards.");
        }

        private static void RequireCard(YgoCardDatabase database, int id, string expectedName, CardKind kind, MonsterFrame frame)
        {
            CardData card;
            if (!database.TryCreateCardData(id, string.Empty, string.Empty, out card))
            {
                throw new InvalidOperationException("Missing card id " + id + ".");
            }

            Require(card.Name == expectedName, "Unexpected name for " + id + ": " + card.Name + ".");
            Require(card.Kind == kind, "Unexpected kind for " + card.Name + ": " + card.Kind + ".");
            Require(card.Frame == frame, "Unexpected frame for " + card.Name + ": " + card.Frame + ".");
            Require(card.HasCdbData, card.Name + " did not retain CDB metadata.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
