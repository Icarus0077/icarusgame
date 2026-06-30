using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace IcarusGame.EditorTools
{
    public static class LuaScriptCatalogBuilder
    {
        private const string DefaultLuaFolder = @"D:\unity\script";
        private const string DefaultOutputFolder = "Assets/StreamingAssets/LuaScriptCatalog";
        private const string CatalogJsonFileName = "lua_script_catalog.json";
        private const string CatalogCsvFileName = "lua_script_catalog.csv";
        private const string SummaryFileName = "lua_script_catalog_summary.txt";

        private static readonly Regex CardFileNameRegex =
            new Regex(@"^c(?<id>\d+)\.lua$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex FunctionRegex =
            new Regex(@"\bfunction\s+(?<name>c\d+\.[A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);

        private static readonly Regex AnyFunctionRegex =
            new Regex(@"\bfunction\s+(?<name>[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*)", RegexOptions.Compiled);

        private static readonly Regex AssignedFunctionRegex =
            new Regex(@"(?<![A-Za-z0-9_.])(?<name>[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*)\s*=\s*function\b", RegexOptions.Compiled);

        private static readonly Regex GlobalAssignmentRegex =
            new Regex(@"(?m)^\s*(?<name>[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)*)\s*=", RegexOptions.Compiled);

        private static readonly Regex EffectCreateRegex =
            new Regex(@"\bEffect\.CreateEffect\s*\(", RegexOptions.Compiled);

        private static readonly Regex SetCodeRegex =
            new Regex(@"\bSetCode\s*\(\s*(?<value>[^)]*)\)", RegexOptions.Compiled);

        private static readonly Regex SetTypeRegex =
            new Regex(@"\bSetType\s*\(\s*(?<value>[^)]*)\)", RegexOptions.Compiled);

        private static readonly Regex SetCategoryRegex =
            new Regex(@"\bSetCategory\s*\(\s*(?<value>[^)]*)\)", RegexOptions.Compiled);

        private static readonly Regex CategoryRegex =
            new Regex(@"\bCATEGORY_[A-Z0-9_]+", RegexOptions.Compiled);

        private static readonly Regex EventRegex =
            new Regex(@"\bEVENT_[A-Z0-9_]+", RegexOptions.Compiled);

        private static readonly Regex EffectRegex =
            new Regex(@"\bEFFECT_[A-Z0-9_]+", RegexOptions.Compiled);

        private static readonly Regex PhaseRegex =
            new Regex(@"\bPHASE_[A-Z0-9_]+", RegexOptions.Compiled);

        private static readonly Regex LocationRegex =
            new Regex(@"\bLOCATION_[A-Z0-9_]+", RegexOptions.Compiled);

        private static readonly Regex RaceRegex =
            new Regex(@"\bRACE_[A-Z0-9_]+", RegexOptions.Compiled);

        private static readonly Regex AttributeRegex =
            new Regex(@"\bATTRIBUTE_[A-Z0-9_]+", RegexOptions.Compiled);

        private static readonly Regex TypeRegex =
            new Regex(@"(?<![A-Z0-9_])TYPE_[A-Z0-9_]+", RegexOptions.Compiled);

        private static readonly Regex DuelCallRegex =
            new Regex(@"\bDuel\.(?<call>[A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);

        private static readonly Regex CardCallRegex =
            new Regex(@"\bCard\.(?<call>[A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);

        private static readonly Regex AuxCallRegex =
            new Regex(@"\b(?:aux|Auxiliary)\.(?<call>[A-Za-z_][A-Za-z0-9_]*)", RegexOptions.Compiled);

        [MenuItem("Tools/Icarus/Lua Scripts/Build Catalog From D:\\unity\\script")]
        public static void BuildDefaultCatalog()
        {
            BuildCatalog(DefaultLuaFolder, DefaultOutputFolder);
        }

        public static LuaScriptCatalog BuildCatalog(string luaFolder, string outputFolder)
        {
            if (string.IsNullOrWhiteSpace(luaFolder) || !Directory.Exists(luaFolder))
            {
                throw new DirectoryNotFoundException("Lua folder does not exist: " + luaFolder);
            }

            string outputAbsolutePath = ToAbsoluteProjectPath(outputFolder);
            if (!IsUnderAssets(outputAbsolutePath))
            {
                throw new InvalidOperationException("Output folder must be under this project's Assets folder: " + outputFolder);
            }

            Directory.CreateDirectory(outputAbsolutePath);

            string luaRoot = Path.GetFullPath(luaFolder);
            List<string> files = new List<string>(Directory.GetFiles(luaRoot, "*.lua", SearchOption.AllDirectories));
            files.Sort(StringComparer.OrdinalIgnoreCase);

            LuaScriptCatalog catalog = new LuaScriptCatalog
            {
                generatedAtUtc = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                sourceFolder = luaRoot,
                outputFolder = ToProjectRelativePath(outputAbsolutePath),
                totalFiles = files.Count
            };

            Dictionary<string, int> categoryCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, int> eventCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, int> duelCallCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, int> cardCallCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, int> auxCallCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, int> familyCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            Dictionary<string, int> scriptKindCounts = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                if (!Application.isBatchMode && i % 250 == 0)
                {
                    EditorUtility.DisplayProgressBar(
                        "Lua script catalog",
                        "Scanning " + Path.GetFileName(file) + " (" + (i + 1) + "/" + files.Count + ")",
                        files.Count == 0 ? 1f : (float)i / files.Count);
                }

                LuaScriptEntry entry = AnalyzeScript(luaRoot, file);
                catalog.entries.Add(entry);
                if (entry.scriptKind == "card")
                {
                    catalog.cardScriptCount++;
                }
                else
                {
                    catalog.libraryScriptCount++;
                    catalog.libraryFunctionCount += entry.functions.Count;
                }

                if (!string.IsNullOrEmpty(entry.cardId))
                {
                    catalog.scriptsWithCardId++;
                }

                if (!string.IsNullOrEmpty(entry.cardName))
                {
                    catalog.scriptsWithCardName++;
                }

                catalog.totalEffectCreates += entry.effectCreateCount;
                AddCounts(categoryCounts, entry.categories);
                AddCounts(eventCounts, entry.events);
                AddCounts(duelCallCounts, entry.duelCalls);
                AddCounts(cardCallCounts, entry.cardCalls);
                AddCounts(auxCallCounts, entry.auxCalls);
                AddCounts(familyCounts, entry.effectFamilies);
                AddCount(scriptKindCounts, entry.scriptKind);
            }

            catalog.categoryCounts = ToFeatureCounts(categoryCounts);
            catalog.eventCounts = ToFeatureCounts(eventCounts);
            catalog.duelCallCounts = ToFeatureCounts(duelCallCounts);
            catalog.cardCallCounts = ToFeatureCounts(cardCallCounts);
            catalog.auxCallCounts = ToFeatureCounts(auxCallCounts);
            catalog.effectFamilyCounts = ToFeatureCounts(familyCounts);
            catalog.scriptKindCounts = ToFeatureCounts(scriptKindCounts);

            File.WriteAllText(Path.Combine(outputAbsolutePath, CatalogJsonFileName), JsonUtility.ToJson(catalog, true), Encoding.UTF8);
            File.WriteAllText(Path.Combine(outputAbsolutePath, CatalogCsvFileName), BuildCsv(catalog), Encoding.UTF8);
            File.WriteAllText(Path.Combine(outputAbsolutePath, SummaryFileName), BuildSummary(catalog), Encoding.UTF8);

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
            Debug.Log("Lua script catalog built. Files: " + catalog.totalFiles + ", output: " + catalog.outputFolder);
            return catalog;
        }

        private static LuaScriptEntry AnalyzeScript(string luaRoot, string file)
        {
            string text = File.ReadAllText(file, Encoding.UTF8);
            string fileName = Path.GetFileName(file);
            LuaScriptEntry entry = new LuaScriptEntry
            {
                fileName = fileName,
                sourcePath = GetRelativePath(luaRoot, file).Replace('\\', '/'),
                byteLength = new FileInfo(file).Length,
                lineCount = CountLines(text),
                cardName = ReadLeadingCardName(text)
            };

            Match cardIdMatch = CardFileNameRegex.Match(fileName);
            if (cardIdMatch.Success)
            {
                entry.cardId = cardIdMatch.Groups["id"].Value;
            }

            entry.scriptKind = DetermineScriptKind(fileName, entry.cardId);
            entry.moduleName = DetermineModuleName(fileName, entry.cardId);
            entry.functions = UniqueFunctionDeclarations(text);
            if (entry.functions.Count == 0 && entry.scriptKind == "card")
            {
                entry.functions = UniqueMatches(FunctionRegex, text, "name");
            }

            entry.declaredGlobals = UniqueMatches(GlobalAssignmentRegex, text, "name");
            entry.effectCreateCount = EffectCreateRegex.Matches(text).Count;
            entry.setCodes = UniqueArgumentMatches(SetCodeRegex, text);
            entry.setTypes = UniqueArgumentMatches(SetTypeRegex, text);
            entry.setCategories = UniqueArgumentMatches(SetCategoryRegex, text);
            entry.categories = UniqueTokenMatches(CategoryRegex, text);
            entry.events = UniqueTokenMatches(EventRegex, text);
            entry.effects = UniqueTokenMatches(EffectRegex, text);
            entry.phases = UniqueTokenMatches(PhaseRegex, text);
            entry.locations = UniqueTokenMatches(LocationRegex, text);
            entry.races = UniqueTokenMatches(RaceRegex, text);
            entry.attributes = UniqueTokenMatches(AttributeRegex, text);
            entry.cardTypes = UniqueTokenMatches(TypeRegex, text);
            entry.duelCalls = UniqueMatches(DuelCallRegex, text, "call");
            entry.cardCalls = UniqueMatches(CardCallRegex, text, "call");
            entry.auxCalls = UniqueMatches(AuxCallRegex, text, "call");

            entry.hasCost = text.IndexOf(":SetCost(", StringComparison.Ordinal) >= 0;
            entry.hasTarget = text.IndexOf(":SetTarget(", StringComparison.Ordinal) >= 0;
            entry.hasCondition = text.IndexOf(":SetCondition(", StringComparison.Ordinal) >= 0;
            entry.hasOperation = text.IndexOf(":SetOperation(", StringComparison.Ordinal) >= 0;
            entry.effectFamilies = ClassifyFamilies(entry);
            entry.complexityScore = EstimateComplexity(entry);
            entry.complexityBand = ComplexityBand(entry.complexityScore);
            return entry;
        }

        private static List<string> ClassifyFamilies(LuaScriptEntry entry)
        {
            SortedSet<string> families = new SortedSet<string>(StringComparer.Ordinal);
            AddLibraryFamilies(entry, families);

            AddFamilyIfToken(entry.categories, "CATEGORY_SPECIAL_SUMMON", families, "special_summon");
            AddFamilyIfToken(entry.categories, "CATEGORY_TOHAND", families, "search_or_recover");
            AddFamilyIfToken(entry.categories, "CATEGORY_SEARCH", families, "search_or_recover");
            AddFamilyIfToken(entry.categories, "CATEGORY_DRAW", families, "draw");
            AddFamilyIfToken(entry.categories, "CATEGORY_DAMAGE", families, "burn");
            AddFamilyIfToken(entry.categories, "CATEGORY_RECOVER", families, "recover");
            AddFamilyIfToken(entry.categories, "CATEGORY_ATKCHANGE", families, "stat_change");
            AddFamilyIfToken(entry.categories, "CATEGORY_EQUIP", families, "equip");
            AddFamilyIfToken(entry.categories, "CATEGORY_DESTROY", families, "destroy");
            AddFamilyIfToken(entry.categories, "CATEGORY_REMOVE", families, "banish");
            AddFamilyIfToken(entry.categories, "CATEGORY_TOGRAVE", families, "send_to_grave");
            AddFamilyIfToken(entry.categories, "CATEGORY_DECKDES", families, "mill");
            AddFamilyIfToken(entry.categories, "CATEGORY_POSITION", families, "position");
            AddFamilyIfToken(entry.categories, "CATEGORY_CONTROL", families, "control_change");
            AddFamilyIfToken(entry.categories, "CATEGORY_COUNTER", families, "counter");
            AddFamilyIfToken(entry.events, "EVENT_PHASE", families, "phase_trigger");
            AddFamilyIfToken(entry.events, "EVENT_ATTACK_ANNOUNCE", families, "attack_trigger");
            AddFamilyIfToken(entry.events, "EVENT_SUMMON_SUCCESS", families, "summon_trigger");
            AddFamilyIfToken(entry.events, "EVENT_SPSUMMON_SUCCESS", families, "summon_trigger");
            AddFamilyIfToken(entry.events, "EVENT_TO_GRAVE", families, "graveyard_trigger");
            AddFamilyIfToken(entry.events, "EVENT_DESTROYED", families, "destroy_trigger");

            if (families.Count == 0)
            {
                families.Add("unclassified");
            }

            return new List<string>(families);
        }

        private static void AddLibraryFamilies(LuaScriptEntry entry, SortedSet<string> families)
        {
            if (entry.scriptKind == "procedure_library")
            {
                families.Add("procedure_library");
                AddFamilyIfFunctionContains(entry.functions, "Fusion", families, "procedure_fusion");
                AddFamilyIfFunctionContains(entry.functions, "Link", families, "procedure_link");
                AddFamilyIfFunctionContains(entry.functions, "Pendulum", families, "procedure_pendulum");
                AddFamilyIfFunctionContains(entry.functions, "Ritual", families, "procedure_ritual");
                AddFamilyIfFunctionContains(entry.functions, "Synchro", families, "procedure_synchro");
                AddFamilyIfFunctionContains(entry.functions, "Xyz", families, "procedure_xyz");
            }
            else if (entry.scriptKind == "utility_library")
            {
                families.Add("utility_helper");
            }
            else if (entry.scriptKind == "rush_duel_library")
            {
                families.Add("rush_duel_helper");
                families.Add("rush_duel_" + ToSnakeCase(entry.moduleName.Substring(2)));
            }
            else if (entry.scriptKind == "library")
            {
                families.Add("library_helper");
            }
        }

        private static int EstimateComplexity(LuaScriptEntry entry)
        {
            int score = entry.effectCreateCount * 3;
            score += entry.functions.Count;
            score += entry.declaredGlobals.Count / 2;
            score += entry.categories.Count;
            score += entry.events.Count;
            score += entry.duelCalls.Count * 2;
            score += entry.hasCost ? 2 : 0;
            score += entry.hasTarget ? 1 : 0;
            score += entry.hasCondition ? 1 : 0;
            score += entry.lineCount / 80;
            return score;
        }

        private static string ComplexityBand(int score)
        {
            if (score <= 12)
            {
                return "simple";
            }

            if (score <= 28)
            {
                return "medium";
            }

            if (score <= 55)
            {
                return "complex";
            }

            return "very_complex";
        }

        private static string BuildCsv(LuaScriptCatalog catalog)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("script_kind,module_name,card_id,card_name,file_name,source_path,line_count,byte_length,effect_count,function_count,global_assignment_count,complexity_score,complexity_band,families,categories,events,duel_calls,card_calls,aux_calls");
            for (int i = 0; i < catalog.entries.Count; i++)
            {
                LuaScriptEntry entry = catalog.entries[i];
                AppendCsv(builder, entry.scriptKind);
                AppendCsv(builder, entry.moduleName);
                AppendCsv(builder, entry.cardId);
                AppendCsv(builder, entry.cardName);
                AppendCsv(builder, entry.fileName);
                AppendCsv(builder, entry.sourcePath);
                AppendCsv(builder, entry.lineCount.ToString());
                AppendCsv(builder, entry.byteLength.ToString());
                AppendCsv(builder, entry.effectCreateCount.ToString());
                AppendCsv(builder, entry.functions.Count.ToString());
                AppendCsv(builder, entry.declaredGlobals.Count.ToString());
                AppendCsv(builder, entry.complexityScore.ToString());
                AppendCsv(builder, entry.complexityBand);
                AppendCsv(builder, string.Join("|", entry.effectFamilies.ToArray()));
                AppendCsv(builder, string.Join("|", entry.categories.ToArray()));
                AppendCsv(builder, string.Join("|", entry.events.ToArray()));
                AppendCsv(builder, string.Join("|", entry.duelCalls.ToArray()));
                AppendCsv(builder, string.Join("|", entry.cardCalls.ToArray()));
                AppendCsv(builder, string.Join("|", entry.auxCalls.ToArray()), true);
            }

            return builder.ToString();
        }

        private static string BuildSummary(LuaScriptCatalog catalog)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Lua Script Catalog");
            builder.AppendLine("==================");
            builder.AppendLine("Generated UTC: " + catalog.generatedAtUtc);
            builder.AppendLine("Source folder: " + catalog.sourceFolder);
            builder.AppendLine("Output folder: " + catalog.outputFolder);
            builder.AppendLine("Total Lua files: " + catalog.totalFiles);
            builder.AppendLine("Card scripts: " + catalog.cardScriptCount);
            builder.AppendLine("Library/procedure scripts: " + catalog.libraryScriptCount);
            builder.AppendLine("Scripts with card id: " + catalog.scriptsWithCardId);
            builder.AppendLine("Scripts with leading card name: " + catalog.scriptsWithCardName);
            builder.AppendLine("Effect.CreateEffect calls: " + catalog.totalEffectCreates);
            builder.AppendLine("Library/procedure function declarations: " + catalog.libraryFunctionCount);
            builder.AppendLine();
            AppendTopCounts(builder, "Script kinds", catalog.scriptKindCounts, 20);
            AppendTopCounts(builder, "Effect families", catalog.effectFamilyCounts, 20);
            AppendTopCounts(builder, "Categories", catalog.categoryCounts, 30);
            AppendTopCounts(builder, "Events", catalog.eventCounts, 30);
            AppendTopCounts(builder, "Duel calls", catalog.duelCallCounts, 40);
            AppendTopCounts(builder, "Card calls", catalog.cardCallCounts, 40);
            AppendTopCounts(builder, "Auxiliary calls", catalog.auxCallCounts, 40);
            AppendLibraryScripts(builder, catalog);
            return builder.ToString();
        }

        private static void AppendLibraryScripts(StringBuilder builder, LuaScriptCatalog catalog)
        {
            builder.AppendLine("Library/procedure scripts:");
            for (int i = 0; i < catalog.entries.Count; i++)
            {
                LuaScriptEntry entry = catalog.entries[i];
                if (entry.scriptKind == "card")
                {
                    continue;
                }

                builder.AppendLine(
                    "  " + entry.sourcePath +
                    " [" + entry.scriptKind + "]" +
                    " functions=" + entry.functions.Count +
                    ", globals=" + entry.declaredGlobals.Count +
                    ", families=" + string.Join("|", entry.effectFamilies.ToArray()));
            }

            builder.AppendLine();
        }

        private static void AppendTopCounts(StringBuilder builder, string title, List<LuaScriptFeatureCount> counts, int max)
        {
            builder.AppendLine(title + ":");
            int limit = Mathf.Min(max, counts.Count);
            for (int i = 0; i < limit; i++)
            {
                builder.AppendLine("  " + counts[i].name + ": " + counts[i].count);
            }

            builder.AppendLine();
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

                    string comment = trimmed.Substring(2).Trim().TrimStart('-').Trim();
                    return comment;
                }
            }

            return string.Empty;
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

        private static List<string> UniqueMatches(Regex regex, string text, string groupName)
        {
            SortedSet<string> values = new SortedSet<string>(StringComparer.Ordinal);
            foreach (Match match in regex.Matches(text))
            {
                values.Add(match.Groups[groupName].Value);
            }

            return new List<string>(values);
        }

        private static List<string> UniqueFunctionDeclarations(string text)
        {
            SortedSet<string> values = new SortedSet<string>(StringComparer.Ordinal);
            AddMatches(values, AnyFunctionRegex, text, "name");
            AddMatches(values, AssignedFunctionRegex, text, "name");
            return new List<string>(values);
        }

        private static void AddMatches(SortedSet<string> values, Regex regex, string text, string groupName)
        {
            foreach (Match match in regex.Matches(text))
            {
                values.Add(match.Groups[groupName].Value);
            }
        }

        private static List<string> UniqueArgumentMatches(Regex regex, string text)
        {
            SortedSet<string> values = new SortedSet<string>(StringComparer.Ordinal);
            foreach (Match match in regex.Matches(text))
            {
                string value = NormalizeLuaExpression(match.Groups["value"].Value);
                if (!string.IsNullOrEmpty(value))
                {
                    values.Add(value);
                }
            }

            return new List<string>(values);
        }

        private static string NormalizeLuaExpression(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : Regex.Replace(value.Trim(), @"\s+", string.Empty);
        }

        private static void AddFamilyIfToken(List<string> tokens, string token, SortedSet<string> families, string family)
        {
            if (tokens.Contains(token))
            {
                families.Add(family);
            }
        }

        private static void AddFamilyIfFunctionContains(List<string> functions, string token, SortedSet<string> families, string family)
        {
            for (int i = 0; i < functions.Count; i++)
            {
                if (functions[i].IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    families.Add(family);
                    return;
                }
            }
        }

        private static string DetermineScriptKind(string fileName, string cardId)
        {
            if (!string.IsNullOrEmpty(cardId))
            {
                return "card";
            }

            if (string.Equals(fileName, "procedure.lua", StringComparison.OrdinalIgnoreCase))
            {
                return "procedure_library";
            }

            if (string.Equals(fileName, "utility.lua", StringComparison.OrdinalIgnoreCase))
            {
                return "utility_library";
            }

            if (fileName.StartsWith("RD", StringComparison.OrdinalIgnoreCase))
            {
                return "rush_duel_library";
            }

            return "library";
        }

        private static string DetermineModuleName(string fileName, string cardId)
        {
            if (!string.IsNullOrEmpty(cardId))
            {
                return "c" + cardId;
            }

            return Path.GetFileNameWithoutExtension(fileName);
        }

        private static string ToSnakeCase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "unknown";
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (char.IsUpper(c) && i > 0)
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(c));
            }

            return builder.ToString();
        }

        private static void AddCounts(Dictionary<string, int> counts, List<string> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                AddCount(counts, values[i]);
            }
        }

        private static void AddCount(Dictionary<string, int> counts, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            counts[value] = counts.TryGetValue(value, out int count) ? count + 1 : 1;
        }

        private static List<LuaScriptFeatureCount> ToFeatureCounts(Dictionary<string, int> counts)
        {
            List<LuaScriptFeatureCount> result = new List<LuaScriptFeatureCount>();
            foreach (KeyValuePair<string, int> pair in counts)
            {
                result.Add(new LuaScriptFeatureCount { name = pair.Key, count = pair.Value });
            }

            result.Sort((a, b) =>
            {
                int countComparison = b.count.CompareTo(a.count);
                return countComparison != 0 ? countComparison : string.Compare(a.name, b.name, StringComparison.Ordinal);
            });
            return result;
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

        private static void AppendCsv(StringBuilder builder, string value, bool last = false)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            builder.Append('"');
            builder.Append(value.Replace("\"", "\"\""));
            builder.Append('"');
            builder.Append(last ? '\n' : ',');
        }

        private static string ToAbsoluteProjectPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return Path.GetFullPath(path);
            }

            return Path.GetFullPath(Path.Combine(ProjectRoot, path.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static string ToProjectRelativePath(string absolutePath)
        {
            string fullPath = Path.GetFullPath(absolutePath);
            string root = ProjectRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string rootWithSeparator = root + Path.DirectorySeparatorChar;
            return fullPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase)
                ? fullPath.Substring(rootWithSeparator.Length).Replace('\\', '/')
                : fullPath;
        }

        private static bool IsUnderAssets(string absolutePath)
        {
            string assetsPath = Path.GetFullPath(Application.dataPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string target = Path.GetFullPath(absolutePath);
            return target.Equals(assetsPath, StringComparison.OrdinalIgnoreCase) ||
                   target.StartsWith(assetsPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetRelativePath(string root, string file)
        {
            Uri rootUri = new Uri(Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar);
            Uri fileUri = new Uri(Path.GetFullPath(file));
            return Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString()).Replace('/', Path.DirectorySeparatorChar);
        }

        private static string ProjectRoot
        {
            get { return Path.GetFullPath(Path.Combine(Application.dataPath, "..")); }
        }
    }

    [Serializable]
    public sealed class LuaScriptCatalog
    {
        public string generatedAtUtc;
        public string sourceFolder;
        public string outputFolder;
        public int totalFiles;
        public int cardScriptCount;
        public int libraryScriptCount;
        public int scriptsWithCardId;
        public int scriptsWithCardName;
        public int totalEffectCreates;
        public int libraryFunctionCount;
        public List<LuaScriptEntry> entries = new List<LuaScriptEntry>();
        public List<LuaScriptFeatureCount> categoryCounts = new List<LuaScriptFeatureCount>();
        public List<LuaScriptFeatureCount> eventCounts = new List<LuaScriptFeatureCount>();
        public List<LuaScriptFeatureCount> duelCallCounts = new List<LuaScriptFeatureCount>();
        public List<LuaScriptFeatureCount> cardCallCounts = new List<LuaScriptFeatureCount>();
        public List<LuaScriptFeatureCount> auxCallCounts = new List<LuaScriptFeatureCount>();
        public List<LuaScriptFeatureCount> effectFamilyCounts = new List<LuaScriptFeatureCount>();
        public List<LuaScriptFeatureCount> scriptKindCounts = new List<LuaScriptFeatureCount>();
    }

    [Serializable]
    public sealed class LuaScriptEntry
    {
        public string cardId;
        public string cardName;
        public string scriptKind;
        public string moduleName;
        public string fileName;
        public string sourcePath;
        public long byteLength;
        public int lineCount;
        public int effectCreateCount;
        public int complexityScore;
        public string complexityBand;
        public bool hasCost;
        public bool hasTarget;
        public bool hasCondition;
        public bool hasOperation;
        public List<string> effectFamilies = new List<string>();
        public List<string> functions = new List<string>();
        public List<string> declaredGlobals = new List<string>();
        public List<string> setCodes = new List<string>();
        public List<string> setTypes = new List<string>();
        public List<string> setCategories = new List<string>();
        public List<string> categories = new List<string>();
        public List<string> events = new List<string>();
        public List<string> effects = new List<string>();
        public List<string> phases = new List<string>();
        public List<string> locations = new List<string>();
        public List<string> races = new List<string>();
        public List<string> attributes = new List<string>();
        public List<string> cardTypes = new List<string>();
        public List<string> duelCalls = new List<string>();
        public List<string> cardCalls = new List<string>();
        public List<string> auxCalls = new List<string>();
    }

    [Serializable]
    public sealed class LuaScriptFeatureCount
    {
        public string name;
        public int count;
    }
}
