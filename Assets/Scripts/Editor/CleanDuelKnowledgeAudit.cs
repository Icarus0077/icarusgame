using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IcarusGame.CleanDuel;
using IcarusGame.MasterDuel;
using UnityEditor;
using UnityEngine;

namespace IcarusGame.EditorTools
{
    public static class CleanDuelKnowledgeAudit
    {
        private const string OutputFolder = "Assets/StreamingAssets/CleanDuelKnowledge";
        private const string OutputFileName = "blue_eyes_knowledge_audit.md";
        private static readonly int[] SupportedRuleCardIds =
        {
            70551291
        };

        [MenuItem("Tools/Icarus/Clean Duel/Build Knowledge Audit")]
        public static void Build()
        {
            Directory.CreateDirectory(OutputFolder);
            string outputPath = Path.Combine(OutputFolder, OutputFileName);
            File.WriteAllText(outputPath, BuildReport(), Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log("CleanDuel knowledge audit written to " + outputPath);
        }

        public static string BuildReport()
        {
            string databaseError;
            YgoCardDatabase database;
            bool hasDatabase = YgoCardDatabase.TryLoadDefault(out database, out databaseError);

            CleanLuaScriptRepository luaRepository = new CleanLuaScriptRepository();
            luaRepository.Initialize();

            List<CleanCard> mainDeck = CleanCardLibrary.BuildDeck(CleanPlayerId.Local);
            List<CleanCard> extraDeck = CleanCardLibrary.BuildExtraDeck(CleanPlayerId.Local);
            Dictionary<int, int> mainCounts = CountCards(mainDeck);
            Dictionary<int, int> extraCounts = CountCards(extraDeck);
            List<int> cardIds = UniqueCardIds(mainCounts, extraCounts, SupportedRuleCardIds);
            CleanDuelGame ruleGame = new CleanDuelGame();

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("# CleanDuel Yu-Gi-Oh Knowledge Audit");
            builder.AppendLine();
            builder.AppendLine("Generated UTC: " + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            builder.AppendLine("Unity deck source: CleanCardLibrary local Blue-Eyes recipe");
            builder.AppendLine("Card database source: " + (hasDatabase ? YgoCardDatabase.StreamingAssetsRelativePath + " (" + database.Count + " records)" : "missing: " + databaseError));
            builder.AppendLine("Lua source: " + CleanLuaScriptRepository.DefaultScriptFolder);
            builder.AppendLine("Official reference priority: Konami Rulebook + Yu-Gi-Oh! Neuron/Card Database, then local cards.cdb, then local c*.lua scripts.");
            builder.AppendLine();
            builder.AppendLine("## Baseline Rules To Keep");
            builder.AppendLine();
            builder.AppendLine("- Do not expose Activate on Normal Monsters just because they have card art or text.");
            builder.AppendLine("- Ritual Monsters require a Ritual Spell/effect and valid Level/material payment.");
            builder.AppendLine("- Fusion Monsters require a Fusion Summon/effect unless their own text allows Special Summon.");
            builder.AppendLine("- Synchro Summons require at least 1 Tuner and at least 1 non-Tuner with the exact required Level total.");
            builder.AppendLine("- Link Monsters use Link Materials and should not be treated as Level monsters.");
            builder.AppendLine("- Field Spells stay in the Field Spell Zone after activation unless replaced or removed.");
            builder.AppendLine("- Quick Effects are Spell Speed 2 monster effects and can be activated during the opponent's turn when their condition is legal.");
            builder.AppendLine("- Continuous Effects do not activate; they apply only while the card is face-up and not negated, then stop when that condition ends.");
            builder.AppendLine("- Trigger Effects activate from specific events such as Summon, attack target, destruction, Graveyard timing, or End Phase; do not expose them as generic Activate buttons.");
            builder.AppendLine("- Flip Effects are Trigger Effects that start with FLIP and trigger when a face-down monster is flipped face-up by Flip Summon, battle, or card effect.");
            builder.AppendLine("- Link Monsters use printed Link Materials and Link Rating; Extra Deck Link Summons use the Extra Monster Zone first, then linked Main Monster Zones opened by Link Arrows.");
            builder.AppendLine("- Link Arrows mark linked zones/monsters; two Link Monsters pointing at each other are co-linked.");
            builder.AppendLine("- Pendulum Monsters can be activated as Spells only in the left/right Pendulum Zones; Pendulum Summon is once per turn, uses open scales, and face-up Pendulum Monsters from the Extra Deck must use the Extra Monster Zone or a linked Main Monster Zone.");
            builder.AppendLine("- Pendulum Monsters on the field go face-up to the Extra Deck instead of the GY when they would be sent to the GY.");
            builder.AppendLine("- If a card text says choose/add/send/target, prefer a player choice prompt over automatic best-card selection.");
            builder.AppendLine();
            builder.AppendLine("## Blue-Eyes Deck Audit");
            builder.AppendLine();
            builder.AppendLine("Cards with Main/Extra count 0 are not in the current recipe, but are tracked because CleanDuel has rules that search or resolve them.");
            builder.AppendLine();
            builder.AppendLine("| Main | Extra | Id | Card | Kind | Type Line | Effect Key | Timing | Speed | Lua Families | Issues |");
            builder.AppendLine("| ---: | ---: | ---: | --- | --- | --- | --- | --- | ---: | --- | --- |");

            int issueCount = 0;
            for (int i = 0; i < cardIds.Count; i++)
            {
                int cardId = cardIds[i];
                CleanCardDefinition clean = CleanCardLibrary.Get(cardId);
                CleanCard auditCard = new CleanCard(clean, CleanPlayerId.Local);
                CleanEffectTiming timing = ruleGame.GetEffectTiming(auditCard);
                int spellSpeed = ruleGame.GetSpellSpeed(auditCard);
                List<string> issues = new List<string>();
                string officialName = clean.Name;
                string officialKind = clean.Kind.ToString();
                string officialTypeLine = clean.TypeLine;

                if (hasDatabase)
                {
                    YgoCardRecord record;
                    if (database.TryGetRecord(cardId, out record))
                    {
                        CardData official = YgoCardDatabase.CreateCardData(record, string.Empty, clean.EffectKey);
                        officialName = official.Name;
                        officialKind = official.Kind.ToString();
                        officialTypeLine = official.TypeLine;
                        if (!string.Equals(clean.Name, official.Name, StringComparison.Ordinal))
                        {
                            issues.Add("Clean name differs from cdb");
                        }

                        if (!string.Equals(clean.Kind.ToString(), official.Kind.ToString(), StringComparison.Ordinal))
                        {
                            issues.Add("Clean kind differs from cdb");
                        }

                        if (clean.Level != official.Level || clean.Attack != official.Attack || clean.Defense != official.Defense)
                        {
                            issues.Add("Stats differ from cdb");
                        }
                    }
                    else
                    {
                        issues.Add("Missing from cards.cdb");
                    }
                }

                CleanLuaScriptInfo lua;
                string families = "no c" + cardId + ".lua";
                if (luaRepository.TryGet(cardId, out lua))
                {
                    families = lua.FamilySummary;
                    if (clean.Kind != CleanCardKind.Monster && lua.HasFamily("special_summon") && string.IsNullOrEmpty(clean.EffectKey))
                    {
                        issues.Add("Lua has summon family but no clean effect key");
                    }
                }
                else if (!string.IsNullOrEmpty(clean.EffectKey))
                {
                    issues.Add("Clean effect key has no local Lua script");
                }

                if (clean.Kind == CleanCardKind.Monster && IsNormalTypeLine(clean.TypeLine) && !string.IsNullOrEmpty(clean.EffectKey))
                {
                    issues.Add("Normal Monster should usually not have activate effect key");
                }

                if (issues.Count > 0)
                {
                    issueCount += issues.Count;
                }

                builder.Append("| ");
                builder.Append(GetCount(mainCounts, cardId));
                builder.Append(" | ");
                builder.Append(GetCount(extraCounts, cardId));
                builder.Append(" | ");
                builder.Append(cardId);
                builder.Append(" | ");
                builder.Append(EscapeMarkdown(officialName));
                builder.Append(" | ");
                builder.Append(EscapeMarkdown(officialKind));
                builder.Append(" | ");
                builder.Append(EscapeMarkdown(officialTypeLine));
                builder.Append(" | ");
                builder.Append(EscapeMarkdown(string.IsNullOrEmpty(clean.EffectKey) ? "none" : clean.EffectKey));
                builder.Append(" | ");
                builder.Append(timing);
                builder.Append(" | ");
                builder.Append(spellSpeed);
                builder.Append(" | ");
                builder.Append(EscapeMarkdown(families));
                builder.Append(" | ");
                builder.Append(EscapeMarkdown(issues.Count == 0 ? "OK" : string.Join("; ", issues.ToArray())));
                builder.AppendLine(" |");
            }

            builder.AppendLine();
            builder.AppendLine("## Summary");
            builder.AppendLine();
            builder.AppendLine("- Main Deck cards: " + mainDeck.Count);
            builder.AppendLine("- Extra Deck cards: " + extraDeck.Count);
            builder.AppendLine("- Unique cards audited: " + cardIds.Count);
            builder.AppendLine("- Audit issues: " + issueCount);
            builder.AppendLine("- Lua status: " + luaRepository.Status);
            return builder.ToString();
        }

        private static Dictionary<int, int> CountCards(List<CleanCard> cards)
        {
            Dictionary<int, int> counts = new Dictionary<int, int>();
            for (int i = 0; i < cards.Count; i++)
            {
                int id = cards[i].Definition.Id;
                counts[id] = GetCount(counts, id) + 1;
            }

            return counts;
        }

        private static List<int> UniqueCardIds(Dictionary<int, int> mainCounts, Dictionary<int, int> extraCounts, int[] supportedRuleCardIds)
        {
            SortedSet<int> ids = new SortedSet<int>();
            foreach (int id in mainCounts.Keys)
            {
                ids.Add(id);
            }

            foreach (int id in extraCounts.Keys)
            {
                ids.Add(id);
            }

            for (int i = 0; i < supportedRuleCardIds.Length; i++)
            {
                ids.Add(supportedRuleCardIds[i]);
            }

            return new List<int>(ids);
        }

        private static int GetCount(Dictionary<int, int> counts, int id)
        {
            int count;
            return counts.TryGetValue(id, out count) ? count : 0;
        }

        private static bool IsNormalTypeLine(string typeLine)
        {
            return !string.IsNullOrEmpty(typeLine) &&
                   typeLine.IndexOf("Normal", StringComparison.OrdinalIgnoreCase) >= 0 &&
                   typeLine.IndexOf("Effect", StringComparison.OrdinalIgnoreCase) < 0;
        }

        private static string EscapeMarkdown(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
        }
    }
}
