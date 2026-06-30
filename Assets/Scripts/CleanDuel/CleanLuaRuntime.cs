using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace IcarusGame.CleanDuel
{
    public sealed class CleanLuaScriptRepository
    {
        public const string DefaultScriptFolder = @"D:\unity\script";

        private static readonly Regex CardScriptNameRegex =
            new Regex(@"^c(?<id>\d+)\.lua$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex CategoryRegex =
            new Regex(@"\bCATEGORY_[A-Z0-9_]+", RegexOptions.Compiled);

        private static readonly Regex EventRegex =
            new Regex(@"\bEVENT_[A-Z0-9_]+", RegexOptions.Compiled);

        private static readonly Regex DuelCallRegex =
            new Regex(@"\bDuel\.(?<call>[A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);

        private static readonly Regex CardCallRegex =
            new Regex(@"\bCard\.(?<call>[A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);

        private static readonly Dictionary<int, string> sharedScriptPaths = new Dictionary<int, string>();
        private static bool sharedScriptPathsInitialized;

        public static bool HasCardScript(int cardId)
        {
            EnsureSharedScriptPaths();
            return sharedScriptPaths.ContainsKey(cardId);
        }

        private static void EnsureSharedScriptPaths()
        {
            if (sharedScriptPathsInitialized)
            {
                return;
            }

            sharedScriptPathsInitialized = true;
            sharedScriptPaths.Clear();
            if (!Directory.Exists(DefaultScriptFolder))
            {
                return;
            }

            try
            {
                string[] files = Directory.GetFiles(DefaultScriptFolder, "*.lua", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    string fileName = Path.GetFileName(files[i]);
                    Match match = CardScriptNameRegex.Match(fileName);
                    if (!match.Success)
                    {
                        continue;
                    }

                    int cardId;
                    if (int.TryParse(match.Groups["id"].Value, out cardId))
                    {
                        sharedScriptPaths[cardId] = files[i];
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Failed to scan Lua scripts for EffectKey generation: " + exception.Message);
            }
        }
        private readonly Dictionary<int, string> scriptPaths = new Dictionary<int, string>();
        private readonly Dictionary<int, CleanLuaScriptInfo> cache = new Dictionary<int, CleanLuaScriptInfo>();
        private bool initialized;
        private string status = "Lua script source has not been scanned yet.";

        public string Status
        {
            get
            {
                EnsureInitialized();
                return status;
            }
        }

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            scriptPaths.Clear();
            cache.Clear();

            if (!Directory.Exists(DefaultScriptFolder))
            {
                status = "Lua source missing: " + DefaultScriptFolder;
                return;
            }

            int luaFiles = 0;
            int cardScripts = 0;
            try
            {
                string[] files = Directory.GetFiles(DefaultScriptFolder, "*.lua", SearchOption.AllDirectories);
                luaFiles = files.Length;
                for (int i = 0; i < files.Length; i++)
                {
                    string fileName = Path.GetFileName(files[i]);
                    Match match = CardScriptNameRegex.Match(fileName);
                    if (!match.Success)
                    {
                        continue;
                    }

                    int cardId;
                    if (!int.TryParse(match.Groups["id"].Value, out cardId))
                    {
                        continue;
                    }

                    scriptPaths[cardId] = files[i];
                    cardScripts++;
                }
            }
            catch (Exception exception)
            {
                status = "Lua scan failed: " + exception.Message;
                return;
            }

            status = "Lua source loaded: " + cardScripts + " card scripts / " + luaFiles + " lua files from " + DefaultScriptFolder;
        }

        public bool TryGet(int cardId, out CleanLuaScriptInfo info)
        {
            EnsureInitialized();
            if (cache.TryGetValue(cardId, out info))
            {
                return info != null;
            }

            string path;
            if (!scriptPaths.TryGetValue(cardId, out path) || string.IsNullOrEmpty(path))
            {
                cache[cardId] = null;
                info = null;
                return false;
            }

            try
            {
                string text = File.ReadAllText(path, Encoding.UTF8);
                info = Analyze(cardId, path, text);
                cache[cardId] = info;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Failed to read Lua script " + path + ": " + exception.Message);
                cache[cardId] = null;
                info = null;
                return false;
            }
        }

        public string Describe(int cardId)
        {
            CleanLuaScriptInfo info;
            if (!TryGet(cardId, out info))
            {
                return "Lua: no c" + cardId + ".lua found";
            }

            return "Lua: " + info.FileName + " [" + info.FamilySummary + "]";
        }

        private void EnsureInitialized()
        {
            if (!initialized)
            {
                Initialize();
            }
        }

        private CleanLuaScriptInfo Analyze(int cardId, string path, string text)
        {
            CleanLuaScriptInfo info = new CleanLuaScriptInfo
            {
                CardId = cardId,
                FileName = Path.GetFileName(path),
                SourcePath = path,
                Text = text,
                CardName = ReadLeadingCardName(text),
                LineCount = CountLines(text),
                EffectCreateCount = CountOccurrences(text, "Effect.CreateEffect")
            };

            info.Categories = UniqueTokenMatches(CategoryRegex, text);
            info.Events = UniqueTokenMatches(EventRegex, text);
            info.DuelCalls = UniqueNamedMatches(DuelCallRegex, text, "call");
            info.CardCalls = UniqueNamedMatches(CardCallRegex, text, "call");
            info.EffectFamilies = ClassifyFamilies(info);
            return info;
        }

        private static string ReadLeadingCardName(string text)
        {
            using (StringReader reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string trimmed = line.Trim();
                    if (trimmed.Length == 0)
                    {
                        continue;
                    }

                    if (!trimmed.StartsWith("--", StringComparison.Ordinal))
                    {
                        return string.Empty;
                    }

                    return trimmed.Substring(2).Trim().TrimStart('-').Trim();
                }
            }

            return string.Empty;
        }

        private static List<string> ClassifyFamilies(CleanLuaScriptInfo info)
        {
            List<string> families = new List<string>();
            AddFamilyIfCategory(info, "CATEGORY_DRAW", families, "draw");
            AddFamilyIfCategory(info, "CATEGORY_DESTROY", families, "destroy");
            AddFamilyIfCategory(info, "CATEGORY_SPECIAL_SUMMON", families, "special_summon");
            AddFamilyIfCategory(info, "CATEGORY_FUSION_SUMMON", families, "fusion_summon");
            AddFamilyIfCategory(info, "CATEGORY_TOHAND", families, "search_or_recover");
            AddFamilyIfCategory(info, "CATEGORY_SEARCH", families, "search_or_recover");
            AddFamilyIfCategory(info, "CATEGORY_DAMAGE", families, "burn");
            AddFamilyIfCategory(info, "CATEGORY_RECOVER", families, "recover");
            AddFamilyIfCategory(info, "CATEGORY_ATKCHANGE", families, "stat_change");
            AddFamilyIfCategory(info, "CATEGORY_DEFCHANGE", families, "stat_change");
            AddFamilyIfCategory(info, "CATEGORY_TOGRAVE", families, "send_to_grave");
            AddFamilyIfCategory(info, "CATEGORY_GRAVE_ACTION", families, "graveyard_action");
            AddFamilyIfCategory(info, "CATEGORY_POSITION", families, "position");
            AddFamilyIfCategory(info, "CATEGORY_REMOVE", families, "banish");
            AddFamilyIfCategory(info, "CATEGORY_DECKDES", families, "mill");
            AddFamilyIfCategory(info, "CATEGORY_CONTROL", families, "control_change");

            if (families.Count == 0)
            {
                families.Add("loaded");
            }

            return families;
        }

        private static void AddFamilyIfCategory(CleanLuaScriptInfo info, string category, List<string> families, string family)
        {
            if (info.HasCategory(category) && !families.Contains(family))
            {
                families.Add(family);
            }
        }

        private static List<string> UniqueTokenMatches(Regex regex, string text)
        {
            SortedSet<string> values = new SortedSet<string>(StringComparer.Ordinal);
            foreach (Match match in regex.Matches(text))
            {
                values.Add(match.Value);
            }

            return new List<string>(values);
        }

        private static List<string> UniqueNamedMatches(Regex regex, string text, string groupName)
        {
            SortedSet<string> values = new SortedSet<string>(StringComparer.Ordinal);
            foreach (Match match in regex.Matches(text))
            {
                values.Add(match.Groups[groupName].Value);
            }

            return new List<string>(values);
        }

        private static int CountLines(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int count = 1;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountOccurrences(string text, string token)
        {
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += token.Length;
            }

            return count;
        }
    }

    public sealed class CleanLuaScriptInfo
    {
        private static readonly Regex SetTargetParamRegex =
            new Regex(@"SetTargetParam\s*\(\s*(?<amount>\d+)\s*\)", RegexOptions.Compiled);

        private static readonly Regex DamageRegex =
            new Regex(@"Duel\.(?:Damage|Recover)\s*\([^,]+,\s*(?<amount>\d+)", RegexOptions.Compiled);

        public int CardId;
        public string FileName;
        public string SourcePath;
        public string Text;
        public string CardName;
        public int LineCount;
        public int EffectCreateCount;
        public List<string> Categories = new List<string>();
        public List<string> Events = new List<string>();
        public List<string> DuelCalls = new List<string>();
        public List<string> CardCalls = new List<string>();
        public List<string> EffectFamilies = new List<string>();

        public string FamilySummary
        {
            get { return EffectFamilies == null || EffectFamilies.Count == 0 ? "loaded" : string.Join(", ", EffectFamilies.ToArray()); }
        }

        public bool HasCategory(string token)
        {
            return Contains(Categories, token);
        }

        public bool HasFamily(string token)
        {
            return Contains(EffectFamilies, token);
        }

        public bool HasDuelCall(string call)
        {
            return Contains(DuelCalls, call);
        }

        public bool ContainsText(string token)
        {
            return !string.IsNullOrEmpty(Text) && Text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public int EstimateDrawAmount(int fallback)
        {
            Match match = SetTargetParamRegex.Match(Text ?? string.Empty);
            if (match.Success)
            {
                int amount;
                if (int.TryParse(match.Groups["amount"].Value, out amount))
                {
                    return Mathf.Clamp(amount, 1, 5);
                }
            }

            return Mathf.Clamp(fallback, 1, 5);
        }

        public int EstimateEffectAmount(int fallback)
        {
            Match match = DamageRegex.Match(Text ?? string.Empty);
            if (match.Success)
            {
                int amount;
                if (int.TryParse(match.Groups["amount"].Value, out amount))
                {
                    return Mathf.Clamp(amount, 100, 5000);
                }
            }

            return fallback;
        }

        private static bool Contains(List<string> values, string token)
        {
            if (values == null)
            {
                return false;
            }

            for (int i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], token, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public sealed class CleanLuaEffectBridge
    {
        private readonly CleanLuaScriptRepository repository = new CleanLuaScriptRepository();

        public string Status
        {
            get { return repository.Status; }
        }

        public void Initialize()
        {
            repository.Initialize();
        }

        public string Describe(CleanCard card)
        {
            return card == null ? string.Empty : repository.Describe(card.Definition.Id);
        }

        public bool TryResolve(CleanDuelGame game, CleanPlayerState controller, CleanPlayerState rival, CleanCard card)
        {
            CleanLuaScriptInfo script;
            if (card == null || !repository.TryGet(card.Definition.Id, out script))
            {
                return false;
            }

            game.LuaLog("Lua " + script.FileName + " -> " + script.FamilySummary + ".");

            if (ResolveExactCard(game, controller, rival, card, script))
            {
                return true;
            }

            if (script.HasFamily("draw"))
            {
                game.LuaDraw(controller, script.EstimateDrawAmount(1));
                return true;
            }

            if (script.HasFamily("burn"))
            {
                game.LuaDamage(rival, script.EstimateEffectAmount(800));
                return true;
            }

            if (script.HasFamily("recover"))
            {
                game.LuaRecover(controller, script.EstimateEffectAmount(1000));
                return true;
            }

            if (script.HasFamily("destroy"))
            {
                if (script.HasDuelCall("GetMatchingGroup") && script.ContainsText("LOCATION_MZONE"))
                {
                    return game.LuaDestroyAllMonsters(rival) > 0;
                }

                return game.LuaDestroyStrongest(rival) > 0;
            }

            if (script.HasFamily("fusion_summon"))
            {
                if (game.LuaFusionSummonBestAvailable(controller, 0, false))
                {
                    return true;
                }
            }

            if (script.HasFamily("special_summon") || script.HasFamily("graveyard_action"))
            {
                if (script.ContainsText("LOCATION_GRAVE") && game.LuaReviveStrongest(controller))
                {
                    return true;
                }

                return game.LuaSpecialSummonBestAvailable(controller);
            }

            if (script.HasFamily("search_or_recover"))
            {
                return game.LuaSearchBestCard(controller);
            }

            if (script.HasFamily("send_to_grave") || script.HasFamily("mill"))
            {
                return game.LuaSendDeckMonsterToGrave(controller);
            }

            if (script.HasFamily("stat_change"))
            {
                game.LuaBoostTeam(controller, 500);
                return true;
            }

            if (script.HasFamily("position"))
            {
                return game.LuaChangeStrongestMonsterToDefense(rival);
            }

            game.LuaLog("Lua script loaded, but no CleanDuel operation is mapped for this family yet.");
            return false;
        }

        private bool ResolveExactCard(CleanDuelGame game, CleanPlayerState controller, CleanPlayerState rival, CleanCard card, CleanLuaScriptInfo script)
        {
            switch (card.Definition.Id)
            {
                case 17947697:
                    game.LuaPlaceTrueLight(controller);
                    return true;
                case 55144522:
                    game.LuaDraw(controller, 2);
                    return true;
                case 12580477:
                    return game.LuaDestroyAllMonsters(rival) > 0;
                case 71143015:
                    int fieldBlueEyesMaterialCount;
                    if (game.LuaFusionSummonBestAvailable(controller, 12381100, true, out fieldBlueEyesMaterialCount))
                    {
                        for (int i = 0; i < fieldBlueEyesMaterialCount; i++)
                        {
                            game.LuaDestroyStrongest(rival);
                        }

                        return true;
                    }

                    return false;
                case 62089826:
                    return game.LuaReviveStrongest(controller) || game.LuaSearchBestCard(controller);
                case 41620959:
                    return game.LuaSendDeckMonsterToGrave(controller);
                case 93437091:
                case 80326401:
                    return game.LuaSearchBestCard(controller);
                case 43845801:
                    game.LuaBoostTeam(controller, 1000);
                    return true;
                default:
                    return false;
            }
        }
    }
}
