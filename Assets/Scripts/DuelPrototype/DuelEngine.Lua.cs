using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace IcarusGame.DuelPrototype
{
    public sealed partial class DuelEngine
    {
        private const string DuelLuaResourcePath = "DuelPrototype/Lua";

        private readonly DuelLuaRuntime duelLuaRuntime = new DuelLuaRuntime();

        private enum DuelLuaEvent
        {
            Activate,
            AttackTrap,
            Standby
        }

        private sealed class DuelLuaExecutionResult
        {
            public bool AttackStopped;
        }

        private bool HasLuaEffect(DuelCard card, DuelLuaEvent eventName)
        {
            return card != null && duelLuaRuntime.HasScript(card.Definition, eventName);
        }

        private bool TryResolveLuaEffect(
            DuelCard card,
            DuelPlayerState controller,
            DuelCard optionalTarget,
            DuelLuaEvent eventName,
            out DuelLuaExecutionResult result)
        {
            result = new DuelLuaExecutionResult();
            if (card == null || controller == null)
            {
                return false;
            }

            DuelLuaScript script = duelLuaRuntime.FindScript(card.Definition, eventName);
            if (script == null)
            {
                return false;
            }

            DuelLuaContext context = new DuelLuaContext(this, card, controller, optionalTarget, result);
            if (!duelLuaRuntime.Execute(script, eventName, context))
            {
                return false;
            }

            AddLog("Lua-script uitgevoerd: " + script.DisplayName + " -> " + LuaFunctionName(eventName) + ".");
            return true;
        }

        private static string LuaFunctionName(DuelLuaEvent eventName)
        {
            switch (eventName)
            {
                case DuelLuaEvent.AttackTrap:
                    return "on_attack_trap";
                case DuelLuaEvent.Standby:
                    return "on_standby";
                case DuelLuaEvent.Activate:
                default:
                    return "on_activate";
            }
        }

        private sealed class DuelLuaContext
        {
            private readonly DuelEngine engine;
            private readonly DuelCard card;
            private readonly DuelPlayerState controller;
            private readonly DuelCard optionalTarget;
            private readonly DuelLuaExecutionResult result;

            public DuelLuaContext(
                DuelEngine engine,
                DuelCard card,
                DuelPlayerState controller,
                DuelCard optionalTarget,
                DuelLuaExecutionResult result)
            {
                this.engine = engine;
                this.card = card;
                this.controller = controller;
                this.optionalTarget = optionalTarget;
                this.result = result;
            }

            public object ReadValue(string path)
            {
                string key = NormalizeMemberName(path);
                DuelPlayerState opponent = engine.GetOpponent(controller.Id);
                switch (key)
                {
                    case "effectvalue":
                        return card.Definition.EffectValue;
                    case "cardname":
                        return card.Definition.Name;
                    case "cardtype":
                        return card.Definition.CardTypeText;
                    case "cardarchetype":
                        return card.Definition.Archetype;
                    case "controller":
                        return "controller";
                    case "opponent":
                        return "opponent";
                    case "player":
                        return "player";
                    case "turn":
                    case "turnnumber":
                        return engine.TurnNumber;
                    case "phase":
                        return engine.CurrentPhase.ToString();
                    case "controllerlp":
                        return controller.LifePoints;
                    case "opponentlp":
                        return opponent.LifePoints;
                    case "controllerhandcount":
                        return controller.Hand.Count;
                    case "opponenthandcount":
                        return opponent.Hand.Count;
                    case "controllermonstercount":
                        return CountFaceUpAndFaceDownMonsters(controller);
                    case "opponentmonstercount":
                        return CountFaceUpAndFaceDownMonsters(opponent);
                    default:
                        return string.Empty;
                }
            }

            public bool Invoke(string methodName, IList<object> arguments)
            {
                string method = NormalizeMethodName(methodName);
                switch (method)
                {
                    case "log":
                        Log(GetString(arguments, 0, string.Empty));
                        return true;
                    case "draw":
                    case "drawcards":
                        Draw(
                            FirstArgumentIsNumber(arguments) ? controller : ResolvePlayer(arguments, 0, controller),
                            FirstArgumentIsNumber(arguments) ? GetInt(arguments, 0, 1) : GetInt(arguments, 1, 1));
                        return true;
                    case "gainlife":
                    case "heal":
                        GainLife(
                            FirstArgumentIsNumber(arguments) ? controller : ResolvePlayer(arguments, 0, controller),
                            FirstArgumentIsNumber(arguments) ? GetInt(arguments, 0, 0) : GetInt(arguments, 1, 0));
                        return true;
                    case "damage":
                    case "dealdamage":
                        DealDamage(
                            FirstArgumentIsNumber(arguments) ? engine.GetOpponent(controller.Id) : ResolvePlayer(arguments, 0, engine.GetOpponent(controller.Id)),
                            FirstArgumentIsNumber(arguments) ? GetInt(arguments, 0, 0) : GetInt(arguments, 1, 0));
                        return true;
                    case "boost":
                    case "boostfaceup":
                    case "boostownmonsters":
                        BoostFaceUp(
                            FirstArgumentIsNumber(arguments) ? controller : ResolvePlayer(arguments, 0, controller),
                            FirstArgumentIsNumber(arguments) ? GetInt(arguments, 0, 0) : GetInt(arguments, 1, 0));
                        return true;
                    case "destroyweakest":
                    case "destroyweakestmonster":
                    case "destroyweakestopponentmonster":
                        DestroyWeakest(ResolvePlayer(arguments, 0, engine.GetOpponent(controller.Id)));
                        return true;
                    case "destroytarget":
                        DestroyTarget();
                        return true;
                    case "destroyattacker":
                        DestroyAttacker();
                        return true;
                    case "negateattack":
                        NegateAttack();
                        return true;
                    case "searchdecktohand":
                    case "addfromdeck":
                        SearchDeckToHand(
                            ResolvePlayer(arguments, 0, controller),
                            GetString(arguments, 1, string.Empty),
                            GetInt(arguments, 2, 1));
                        return true;
                    case "specialsummonfromhand":
                    case "specialsummon":
                        SpecialSummonFromHand(
                            ResolvePlayer(arguments, 0, controller),
                            GetString(arguments, 1, string.Empty));
                        return true;
                    default:
                        engine.AddLog("Lua: onbekende functie '" + methodName + "'.");
                        return false;
                }
            }

            public void ReportError(string message)
            {
                engine.AddLog("Lua-fout bij " + card.Definition.Name + ": " + message);
            }

            private void Log(string message)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    engine.AddLog("Lua: " + message);
                }
            }

            private void Draw(DuelPlayerState player, int count)
            {
                int drawCount = Math.Max(0, count);
                for (int i = 0; i < drawCount; i++)
                {
                    engine.DrawCard(player, true);
                    if (engine.IsGameOver)
                    {
                        return;
                    }
                }
            }

            private void GainLife(DuelPlayerState player, int amount)
            {
                int value = Math.Max(0, amount);
                player.LifePoints += value;
                engine.AddLog(player.Name + " krijgt " + value + " LP via Lua.");
            }

            private void DealDamage(DuelPlayerState player, int amount)
            {
                int value = Math.Max(0, amount);
                player.LifePoints -= value;
                engine.AddLog(player.Name + " krijgt " + value + " effectschade via Lua.");
                engine.CheckLifePointWin();
            }

            private void BoostFaceUp(DuelPlayerState player, int amount)
            {
                engine.BoostMonsters(player, amount);
            }

            private void DestroyWeakest(DuelPlayerState player)
            {
                engine.DestroyWeakestMonster(player);
            }

            private void DestroyTarget()
            {
                if (!DestroyMonsterCard(optionalTarget))
                {
                    engine.AddLog("Lua: geen geldig monsterdoelwit om te vernietigen.");
                }
            }

            private void DestroyAttacker()
            {
                if (DestroyMonsterCard(optionalTarget))
                {
                    result.AttackStopped = true;
                    engine.AddLog("Lua: de aanval stopt en de aanvaller wordt vernietigd.");
                }
                else
                {
                    engine.AddLog("Lua: geen aanvaller gevonden om te vernietigen.");
                }
            }

            private void NegateAttack()
            {
                if (optionalTarget != null)
                {
                    optionalTarget.HasAttacked = true;
                }

                result.AttackStopped = true;
                engine.AddLog("Lua: de aanval wordt genegeerd.");
            }

            private void SearchDeckToHand(DuelPlayerState player, string query, int maxCount)
            {
                int remaining = Math.Max(1, maxCount);
                int found = 0;
                for (int i = 0; i < player.Deck.Count && remaining > 0; i++)
                {
                    DuelCardDefinition definition = player.Deck[i];
                    if (!MatchesSearch(definition, query))
                    {
                        continue;
                    }

                    player.Deck.RemoveAt(i);
                    player.Hand.Add(new DuelCard(definition, player.Id));
                    engine.AddLog(player.Name + " voegt " + definition.Name + " uit het deck toe aan de hand via Lua.");
                    found++;
                    remaining--;
                    i--;
                }

                if (found > 0)
                {
                    engine.Shuffle(player.Deck);
                    engine.AddLog(player.Name + " schudt het deck na een Lua search.");
                }
                else
                {
                    engine.AddLog("Lua: geen kaart gevonden in het deck voor '" + query + "'.");
                }
            }

            private void SpecialSummonFromHand(DuelPlayerState player, string query)
            {
                int zone = engine.FirstEmptyZone(player.MonsterZones);
                if (zone < 0)
                {
                    engine.AddLog("Lua: geen vrije Monster Zone voor een Special Summon.");
                    return;
                }

                for (int i = 0; i < player.Hand.Count; i++)
                {
                    DuelCard handCard = player.Hand[i];
                    if (!handCard.IsMonster || !MatchesSearch(handCard.Definition, query))
                    {
                        continue;
                    }

                    player.Hand.RemoveAt(i);
                    handCard.IsFaceDown = false;
                    handCard.Position = MonsterBattlePosition.Attack;
                    handCard.SetTurn = engine.TurnNumber;
                    handCard.SummonedTurn = engine.TurnNumber;
                    handCard.PositionChangedTurn = engine.TurnNumber;
                    handCard.Location = DuelLocation.MonsterZone;
                    handCard.ControllerId = player.Id;
                    handCard.SummonType = DuelSummonType.Special;
                    player.MonsterZones[zone] = handCard;
                    engine.AddLog(player.Name + " Special Summont " + handCard.Definition.Name + " via Lua.");
                    return;
                }

                engine.AddLog("Lua: geen monster in de hand gevonden voor '" + query + "'.");
            }

            private DuelPlayerState ResolvePlayer(IList<object> arguments, int index, DuelPlayerState fallback)
            {
                if (arguments == null || index >= arguments.Count || arguments[index] == null)
                {
                    return fallback;
                }

                string token = Convert.ToString(arguments[index], CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(token))
                {
                    return fallback;
                }

                token = NormalizeMethodName(token);
                if (token == "controller" || token == "self" || token == "me" || token == "own")
                {
                    return controller;
                }

                if (token == "opponent" || token == "enemy")
                {
                    return engine.GetOpponent(controller.Id);
                }

                if (token == "player" || token == "speler")
                {
                    return engine.Player;
                }

                if (token == "tegenstander")
                {
                    return engine.Opponent;
                }

                return fallback;
            }

            private bool DestroyMonsterCard(DuelCard target)
            {
                if (target == null)
                {
                    return false;
                }

                if (DestroyMonsterCard(engine.Player, target))
                {
                    return true;
                }

                return DestroyMonsterCard(engine.Opponent, target);
            }

            private bool DestroyMonsterCard(DuelPlayerState player, DuelCard target)
            {
                for (int zone = 0; zone < ZoneCount; zone++)
                {
                    if (!object.ReferenceEquals(player.MonsterZones[zone], target))
                    {
                        continue;
                    }

                    engine.DestroyMonster(player, zone);
                    return true;
                }

                return false;
            }

            private static int CountFaceUpAndFaceDownMonsters(DuelPlayerState player)
            {
                int count = 0;
                for (int zone = 0; zone < ZoneCount; zone++)
                {
                    if (player.MonsterZones[zone] != null)
                    {
                        count++;
                    }
                }

                return count;
            }

            private static bool MatchesSearch(DuelCardDefinition definition, string query)
            {
                if (definition == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(query))
                {
                    return true;
                }

                return ContainsIgnoreCase(definition.Name, query) ||
                       ContainsIgnoreCase(definition.Archetype, query) ||
                       ContainsIgnoreCase(definition.CardTypeText, query) ||
                       ContainsIgnoreCase(definition.RulesText, query);
            }

            private static bool ContainsIgnoreCase(string source, string value)
            {
                return !string.IsNullOrEmpty(source) &&
                       source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            private static int GetInt(IList<object> arguments, int index, int fallback)
            {
                if (arguments == null || index >= arguments.Count || arguments[index] == null)
                {
                    return fallback;
                }

                object value = arguments[index];
                if (value is int)
                {
                    return (int)value;
                }

                int parsed;
                return int.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed)
                    ? parsed
                    : fallback;
            }

            private static bool FirstArgumentIsNumber(IList<object> arguments)
            {
                if (arguments == null || arguments.Count == 0 || arguments[0] == null)
                {
                    return false;
                }

                if (arguments[0] is int)
                {
                    return true;
                }

                int ignored;
                return int.TryParse(Convert.ToString(arguments[0], CultureInfo.InvariantCulture), NumberStyles.Integer, CultureInfo.InvariantCulture, out ignored);
            }

            private static string GetString(IList<object> arguments, int index, string fallback)
            {
                if (arguments == null || index >= arguments.Count || arguments[index] == null)
                {
                    return fallback;
                }

                return Convert.ToString(arguments[index], CultureInfo.InvariantCulture);
            }
        }

        private sealed class DuelLuaRuntime
        {
            private readonly Dictionary<string, DuelLuaScript> scriptsByCard = new Dictionary<string, DuelLuaScript>();
            private readonly Dictionary<string, DuelLuaScript> scriptsByResourceName = new Dictionary<string, DuelLuaScript>();
            private readonly Dictionary<DuelEffectKind, DuelLuaScript> scriptsByEffect = new Dictionary<DuelEffectKind, DuelLuaScript>();
            private bool loaded;

            public bool HasScript(DuelCardDefinition definition, DuelLuaEvent eventName)
            {
                return FindScript(definition, eventName) != null;
            }

            public DuelLuaScript FindScript(DuelCardDefinition definition, DuelLuaEvent eventName)
            {
                EnsureLoaded();
                if (definition == null)
                {
                    return null;
                }

                string functionName = LuaFunctionName(eventName);
                DuelLuaScript script;
                string cardKey = NormalizeLookupKey(definition.Name);
                if (scriptsByCard.TryGetValue(cardKey, out script) && script.HasFunction(functionName))
                {
                    return script;
                }

                if (scriptsByResourceName.TryGetValue(cardKey, out script) && script.HasFunction(functionName))
                {
                    return script;
                }

                if (definition.DatabaseId != 0)
                {
                    string idKey = NormalizeLookupKey(definition.DatabaseId.ToString(CultureInfo.InvariantCulture));
                    string cIdKey = NormalizeLookupKey("c" + definition.DatabaseId.ToString(CultureInfo.InvariantCulture));
                    if (scriptsByCard.TryGetValue(idKey, out script) && script.HasFunction(functionName))
                    {
                        return script;
                    }

                    if (scriptsByCard.TryGetValue(cIdKey, out script) && script.HasFunction(functionName))
                    {
                        return script;
                    }

                    if (scriptsByResourceName.TryGetValue(idKey, out script) && script.HasFunction(functionName))
                    {
                        return script;
                    }

                    if (scriptsByResourceName.TryGetValue(cIdKey, out script) && script.HasFunction(functionName))
                    {
                        return script;
                    }
                }

                if (definition.Effect != DuelEffectKind.None &&
                    scriptsByEffect.TryGetValue(definition.Effect, out script) &&
                    script.HasFunction(functionName))
                {
                    return script;
                }

                return null;
            }

            public bool Execute(DuelLuaScript script, DuelLuaEvent eventName, DuelLuaContext context)
            {
                string functionBody = script.GetFunctionBody(LuaFunctionName(eventName));
                if (string.IsNullOrEmpty(functionBody))
                {
                    return false;
                }

                Dictionary<string, object> variables = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                string[] lines = NormalizeNewLines(functionBody).Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] statements = SplitStatements(lines[i]);
                    for (int statementIndex = 0; statementIndex < statements.Length; statementIndex++)
                    {
                        bool shouldReturn;
                        if (!ExecuteStatement(statements[statementIndex], variables, context, out shouldReturn))
                        {
                            return false;
                        }

                        if (shouldReturn)
                        {
                            return true;
                        }
                    }
                }

                return true;
            }

            private void EnsureLoaded()
            {
                if (loaded)
                {
                    return;
                }

                loaded = true;
                TextAsset[] assets = Resources.LoadAll<TextAsset>(DuelLuaResourcePath);
                for (int i = 0; i < assets.Length; i++)
                {
                    TextAsset asset = assets[i];
                    if (asset == null)
                    {
                        continue;
                    }

                    DuelLuaScript script = new DuelLuaScript(asset.name, asset.text);
                    scriptsByResourceName[NormalizeScriptAssetName(asset.name)] = script;

                    string cardName = ReadMetadata(asset.text, "card");
                    if (!string.IsNullOrEmpty(cardName))
                    {
                        scriptsByCard[NormalizeLookupKey(cardName)] = script;
                    }

                    string cardId = ReadMetadata(asset.text, "id");
                    if (!string.IsNullOrEmpty(cardId))
                    {
                        scriptsByCard[NormalizeLookupKey(cardId)] = script;
                        scriptsByCard[NormalizeLookupKey("c" + cardId)] = script;
                    }

                    string effectName = ReadMetadata(asset.text, "effect");
                    DuelEffectKind effectKind;
                    if (!string.IsNullOrEmpty(effectName) && TryParseEffectKind(effectName, out effectKind))
                    {
                        scriptsByEffect[effectKind] = script;
                    }
                }
            }

            private bool ExecuteStatement(
                string rawStatement,
                Dictionary<string, object> variables,
                DuelLuaContext context,
                out bool shouldReturn)
            {
                shouldReturn = false;
                string statement = StripLineComment(rawStatement).Trim();
                if (statement.Length == 0)
                {
                    return true;
                }

                if (statement.EndsWith(";", StringComparison.Ordinal))
                {
                    statement = statement.Substring(0, statement.Length - 1).Trim();
                }

                if (statement.StartsWith("return", StringComparison.Ordinal))
                {
                    shouldReturn = true;
                    return true;
                }

                if (statement.StartsWith("local ", StringComparison.Ordinal))
                {
                    return ExecuteLocalAssignment(statement.Substring(6).Trim(), variables, context);
                }

                int openIndex = statement.IndexOf('(');
                int closeIndex = statement.LastIndexOf(')');
                if (openIndex <= 0 || closeIndex < openIndex)
                {
                    context.ReportError("kan statement niet lezen: " + statement);
                    return false;
                }

                string callTarget = statement.Substring(0, openIndex).Trim();
                string methodName = callTarget;
                if (callTarget.StartsWith("ctx:", StringComparison.Ordinal))
                {
                    methodName = callTarget.Substring(4);
                }
                else if (callTarget.StartsWith("ctx.", StringComparison.Ordinal))
                {
                    methodName = callTarget.Substring(4);
                }

                string argumentText = statement.Substring(openIndex + 1, closeIndex - openIndex - 1);
                string[] rawArguments = SplitArguments(argumentText);
                List<object> arguments = new List<object>(rawArguments.Length);
                for (int i = 0; i < rawArguments.Length; i++)
                {
                    if (rawArguments[i].Trim().Length == 0)
                    {
                        continue;
                    }

                    arguments.Add(EvaluateValue(rawArguments[i], variables, context));
                }

                return context.Invoke(methodName, arguments);
            }

            private bool ExecuteLocalAssignment(
                string assignment,
                Dictionary<string, object> variables,
                DuelLuaContext context)
            {
                int equalsIndex = FindTopLevelEquals(assignment);
                if (equalsIndex <= 0)
                {
                    context.ReportError("kan local assignment niet lezen: " + assignment);
                    return false;
                }

                string name = assignment.Substring(0, equalsIndex).Trim();
                string expression = assignment.Substring(equalsIndex + 1).Trim();
                if (string.IsNullOrEmpty(name))
                {
                    context.ReportError("local assignment zonder naam.");
                    return false;
                }

                variables[name] = EvaluateValue(expression, variables, context);
                return true;
            }

            private object EvaluateValue(
                string rawValue,
                Dictionary<string, object> variables,
                DuelLuaContext context)
            {
                string value = rawValue.Trim();
                if (value.Length == 0)
                {
                    return string.Empty;
                }

                if (value.Length >= 2 &&
                    ((value[0] == '"' && value[value.Length - 1] == '"') ||
                     (value[0] == '\'' && value[value.Length - 1] == '\'')))
                {
                    return UnescapeLuaString(value.Substring(1, value.Length - 2));
                }

                int parsedInt;
                if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedInt))
                {
                    return parsedInt;
                }

                if (value.StartsWith("ctx.", StringComparison.Ordinal))
                {
                    return context.ReadValue(value.Substring(4));
                }

                object variableValue;
                if (variables.TryGetValue(value, out variableValue))
                {
                    return variableValue;
                }

                if (string.Equals(value, "nil", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                return value;
            }

            private static string ReadMetadata(string source, string key)
            {
                string prefix = key + ":";
                string[] lines = NormalizeNewLines(source).Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();
                    if (!line.StartsWith("--", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    string comment = line.Substring(2).Trim();
                    if (comment.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return comment.Substring(prefix.Length).Trim();
                    }
                }

                return string.Empty;
            }

            private static bool TryParseEffectKind(string value, out DuelEffectKind effectKind)
            {
                try
                {
                    effectKind = (DuelEffectKind)Enum.Parse(typeof(DuelEffectKind), value.Trim(), true);
                    return true;
                }
                catch (ArgumentException)
                {
                    effectKind = DuelEffectKind.None;
                    return false;
                }
            }
        }

        private sealed class DuelLuaScript
        {
            private readonly Dictionary<string, string> functionBodies = new Dictionary<string, string>(StringComparer.Ordinal);

            public DuelLuaScript(string displayName, string source)
            {
                DisplayName = displayName;
                Source = source ?? string.Empty;
            }

            public string DisplayName { get; private set; }
            public string Source { get; private set; }

            public bool HasFunction(string functionName)
            {
                return GetFunctionBody(functionName) != null;
            }

            public string GetFunctionBody(string functionName)
            {
                string body;
                if (functionBodies.TryGetValue(functionName, out body))
                {
                    return body;
                }

                body = ExtractFunctionBody(Source, functionName);
                functionBodies[functionName] = body;
                return body;
            }

            private static string ExtractFunctionBody(string source, string functionName)
            {
                string[] lines = NormalizeNewLines(source).Split('\n');
                bool insideFunction = false;
                int depth = 0;
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < lines.Length; i++)
                {
                    string trimmed = StripLineComment(lines[i]).Trim();
                    if (!insideFunction)
                    {
                        if (!IsFunctionDeclaration(trimmed, functionName))
                        {
                            continue;
                        }

                        insideFunction = true;
                        depth = 1;
                        continue;
                    }

                    if (StartsBlock(trimmed))
                    {
                        depth++;
                    }

                    if (IsEndStatement(trimmed))
                    {
                        depth--;
                        if (depth <= 0)
                        {
                            return builder.ToString();
                        }
                    }

                    builder.AppendLine(lines[i]);
                }

                return null;
            }

            private static bool IsFunctionDeclaration(string line, string functionName)
            {
                return IsNamedFunctionDeclaration(line, "function ", functionName) ||
                       IsNamedFunctionDeclaration(line, "local function ", functionName);
            }

            private static bool IsNamedFunctionDeclaration(string line, string prefix, string functionName)
            {
                if (!line.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return false;
                }

                string rest = line.Substring(prefix.Length).TrimStart();
                if (!rest.StartsWith(functionName, StringComparison.Ordinal))
                {
                    return false;
                }

                int nextIndex = functionName.Length;
                while (rest.Length > nextIndex && char.IsWhiteSpace(rest[nextIndex]))
                {
                    nextIndex++;
                }

                return rest.Length > nextIndex && rest[nextIndex] == '(';
            }

            private static bool StartsBlock(string line)
            {
                return line.StartsWith("if ", StringComparison.Ordinal) ||
                       line.StartsWith("for ", StringComparison.Ordinal) ||
                       line.StartsWith("while ", StringComparison.Ordinal) ||
                       line.StartsWith("function ", StringComparison.Ordinal) ||
                       line.StartsWith("local function ", StringComparison.Ordinal);
            }

            private static bool IsEndStatement(string line)
            {
                return line == "end" || line.StartsWith("end ", StringComparison.Ordinal);
            }
        }

        private static string[] SplitStatements(string line)
        {
            List<string> statements = new List<string>();
            StringBuilder builder = new StringBuilder();
            bool inString = false;
            char stringQuote = '\0';
            int depth = 0;

            for (int i = 0; i < line.Length; i++)
            {
                char current = line[i];
                if (inString)
                {
                    builder.Append(current);
                    if (current == '\\' && i + 1 < line.Length)
                    {
                        i++;
                        builder.Append(line[i]);
                        continue;
                    }

                    if (current == stringQuote)
                    {
                        inString = false;
                    }

                    continue;
                }

                if (current == '"' || current == '\'')
                {
                    inString = true;
                    stringQuote = current;
                    builder.Append(current);
                    continue;
                }

                if (current == '(')
                {
                    depth++;
                }
                else if (current == ')' && depth > 0)
                {
                    depth--;
                }

                if (current == ';' && depth == 0)
                {
                    statements.Add(builder.ToString());
                    builder.Length = 0;
                    continue;
                }

                builder.Append(current);
            }

            statements.Add(builder.ToString());
            return statements.ToArray();
        }

        private static string[] SplitArguments(string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments))
            {
                return new string[0];
            }

            List<string> result = new List<string>();
            StringBuilder builder = new StringBuilder();
            bool inString = false;
            char stringQuote = '\0';
            int depth = 0;

            for (int i = 0; i < arguments.Length; i++)
            {
                char current = arguments[i];
                if (inString)
                {
                    builder.Append(current);
                    if (current == '\\' && i + 1 < arguments.Length)
                    {
                        i++;
                        builder.Append(arguments[i]);
                        continue;
                    }

                    if (current == stringQuote)
                    {
                        inString = false;
                    }

                    continue;
                }

                if (current == '"' || current == '\'')
                {
                    inString = true;
                    stringQuote = current;
                    builder.Append(current);
                    continue;
                }

                if (current == '(')
                {
                    depth++;
                }
                else if (current == ')' && depth > 0)
                {
                    depth--;
                }

                if (current == ',' && depth == 0)
                {
                    result.Add(builder.ToString());
                    builder.Length = 0;
                    continue;
                }

                builder.Append(current);
            }

            result.Add(builder.ToString());
            return result.ToArray();
        }

        private static int FindTopLevelEquals(string text)
        {
            bool inString = false;
            char stringQuote = '\0';
            for (int i = 0; i < text.Length; i++)
            {
                char current = text[i];
                if (inString)
                {
                    if (current == '\\')
                    {
                        i++;
                        continue;
                    }

                    if (current == stringQuote)
                    {
                        inString = false;
                    }

                    continue;
                }

                if (current == '"' || current == '\'')
                {
                    inString = true;
                    stringQuote = current;
                    continue;
                }

                if (current == '=')
                {
                    return i;
                }
            }

            return -1;
        }

        private static string StripLineComment(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return string.Empty;
            }

            bool inString = false;
            char stringQuote = '\0';
            for (int i = 0; i < line.Length - 1; i++)
            {
                char current = line[i];
                if (inString)
                {
                    if (current == '\\')
                    {
                        i++;
                        continue;
                    }

                    if (current == stringQuote)
                    {
                        inString = false;
                    }

                    continue;
                }

                if (current == '"' || current == '\'')
                {
                    inString = true;
                    stringQuote = current;
                    continue;
                }

                if (current == '-' && line[i + 1] == '-')
                {
                    return line.Substring(0, i);
                }
            }

            return line;
        }

        private static string NormalizeNewLines(string value)
        {
            return (value ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n");
        }

        private static string NormalizeScriptAssetName(string value)
        {
            if (!string.IsNullOrEmpty(value) && value.EndsWith(".lua", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(0, value.Length - 4);
            }

            return NormalizeLookupKey(value);
        }

        private static string NormalizeLookupKey(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(value.Length);
            bool lastWasSeparator = false;
            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];
                if (char.IsLetterOrDigit(current))
                {
                    builder.Append(char.ToLowerInvariant(current));
                    lastWasSeparator = false;
                }
                else if (!lastWasSeparator)
                {
                    builder.Append('_');
                    lastWasSeparator = true;
                }
            }

            return builder.ToString().Trim('_');
        }

        private static string NormalizeMethodName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                char current = value[i];
                if (char.IsLetterOrDigit(current))
                {
                    builder.Append(char.ToLowerInvariant(current));
                }
            }

            return builder.ToString();
        }

        private static string NormalizeMemberName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return NormalizeMethodName(value.Replace("_", string.Empty).Replace(".", string.Empty));
        }

        private static string UnescapeLuaString(string value)
        {
            return value
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t")
                .Replace("\\\"", "\"")
                .Replace("\\'", "'")
                .Replace("\\\\", "\\");
        }
    }
}
