using System;
using System.Collections.Generic;
using System.Reflection;
using IcarusGame.CleanDuel;
using UnityEditor;
using UnityEngine;

namespace IcarusGame.EditorTools
{
    public static class CleanDuelLuaSmokeTest
    {
        [MenuItem("Tools/Icarus/Clean Duel/Run Lua Smoke Test")]
        public static void Run()
        {
            CleanDuelGame game = new CleanDuelGame();
            game.StartNewDuel();

            if (game.State.Local.Deck.Count != 36)
            {
                throw new InvalidOperationException("Screenshot deck test failed: expected 36 cards left after opening draw from a 41-card main deck.");
            }

            if (game.State.Local.ExtraDeck.Count != 10)
            {
                throw new InvalidOperationException("Screenshot deck test failed: expected 10 cards in the Extra Deck.");
            }

            AssertEveryCardHasCleanEffect(CleanCardLibrary.BuildDeck(CleanPlayerId.Local), "Main Deck");
            AssertEveryCardHasCleanEffect(CleanCardLibrary.BuildExtraDeck(CleanPlayerId.Local), "Extra Deck");
            AssertLuaScriptCardsGetGeneratedEffectKeys();
            AssertWishesSearchesBlueEyes();
            AssertMaidenFromHandPlacesTrueLight();
            AssertMaidenFromFieldPlacesTrueLight();
            AssertUltimateFusionSummonsFromExtraDeck();
            AssertRitualMonstersRequireRitualSpell();
            AssertTrueLightChoicePopupSummonsBlueEyes();
            AssertTrueLightChoicePopupSetsSpellTrap();
            AssertTrueLightProtectsBlueEyesFromTargeting();
            AssertNormalMonsterHasNoActivationAndUsesTributes();
            Debug.Log("CleanDuel screenshot deck smoke test passed. Main=41, Extra=10, clean activations, real fusion summons, ritual summons, targeting protection, and tribute rules verified.");
        }

        private static void AssertLuaScriptCardsGetGeneratedEffectKeys()
        {
            CleanCardDefinition potOfGreed = CleanCardLibrary.Get(55144522);
            if (potOfGreed.Kind != CleanCardKind.Spell || string.IsNullOrEmpty(potOfGreed.EffectKey))
            {
                throw new InvalidOperationException("Lua EffectKey generation failed: Pot of Greed should receive an EffectKey because c55144522.lua exists.");
            }

            if (!string.Equals(potOfGreed.EffectKey, "lua_55144522", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Lua EffectKey generation failed: unexpected key " + potOfGreed.EffectKey + ".");
            }

            CleanCardDefinition darkMagician = CleanCardLibrary.Get(46986414);
            if (!string.IsNullOrEmpty(darkMagician.EffectKey))
            {
                throw new InvalidOperationException("Lua EffectKey generation failed: Dark Magician should not receive an EffectKey without a Lua script.");
            }
        }

        private static void AssertWishesSearchesBlueEyes()
        {
            CleanDuelGame game = ReadyCleanGame();
            game.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(80326401), CleanPlayerId.Local));
            game.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));

            if (!game.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Wishes for Eyes of Blue activation failed: Activate was not available.");
            }

            game.ActivateHandCard(0);
            if (!game.State.Local.Hand.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Wishes for Eyes of Blue activation failed: Blue-Eyes was not added to hand.");
            }
        }

        private static void AssertMaidenFromHandPlacesTrueLight()
        {
            CleanDuelGame game = ReadyCleanGame();
            game.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local));
            game.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local));

            if (!game.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Maiden of White hand activation failed: Activate was not available.");
            }

            game.ActivateHandCard(0);
            AssertMaidenPlacedTrueLight(game, "hand");
        }

        private static void AssertMaidenFromFieldPlacesTrueLight()
        {
            CleanDuelGame game = ReadyCleanGame();
            game.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local);
            game.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local));

            if (!game.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Maiden of White field activation failed: Activate was not available.");
            }

            game.ActivateMonsterZoneCard(0);
            AssertMaidenPlacedTrueLight(game, "field");
        }

        private static void AssertUltimateFusionSummonsFromExtraDeck()
        {
            CleanDuelGame blocked = ReadyCleanGame();
            blocked.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(71143015), CleanPlayerId.Local));
            blocked.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(23995346), CleanPlayerId.Local));
            if (blocked.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Ultimate Fusion material rule failed: Activate should be disabled without fusion materials.");
            }

            CleanDuelGame game = ReadyCleanGame();
            game.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(71143015), CleanPlayerId.Local));
            game.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            game.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            game.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            game.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(12381100), CleanPlayerId.Local));
            game.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(23995346), CleanPlayerId.Local));

            if (!game.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Ultimate Fusion activation failed: Activate was not available with 3 Blue-Eyes materials.");
            }

            game.ActivateHandCard(0);
            if (game.State.Local.Monsters[0] == null || game.State.Local.Monsters[0].Definition.Id != 23995346)
            {
                throw new InvalidOperationException("Ultimate Fusion activation failed: Blue-Eyes Ultimate Dragon was not Fusion Summoned from Extra Deck.");
            }

            if (game.State.Local.Hand.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Ultimate Fusion material rule failed: Blue-Eyes materials stayed in hand.");
            }

            if (game.State.Local.Deck.FindAll(card => card.Definition.Id == 89631139).Count != 3)
            {
                throw new InvalidOperationException("Ultimate Fusion material rule failed: materials were not shuffled back into the deck.");
            }

            if (!game.State.Local.Graveyard.Exists(card => card.Definition.Id == 71143015))
            {
                throw new InvalidOperationException("Ultimate Fusion activation failed: activated spell was not sent to graveyard.");
            }
        }

        private static void AssertRitualMonstersRequireRitualSpell()
        {
            CleanDuelGame blocked = ReadyCleanGame();
            blocked.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(55410871), CleanPlayerId.Local));
            blocked.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            blocked.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local);
            blocked.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local);

            if (blocked.CanNormalSummon(0) || blocked.CanSetMonster(0) || blocked.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Ritual rule failed: Blue-Eyes Chaos MAX Dragon should wait for a Ritual Spell, not Summon/Set/Activate itself.");
            }

            CleanDuelGame genericSpecial = ReadyCleanGame();
            genericSpecial.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(17725109), CleanPlayerId.Local));
            genericSpecial.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(55410871), CleanPlayerId.Local));
            genericSpecial.ActivateHandCard(0);
            if (genericSpecial.State.Local.Monsters[0] != null || genericSpecial.State.Local.Deck.Count != 1)
            {
                throw new InvalidOperationException("Ritual rule failed: generic Blue-Eyes special summon should not summon a Ritual Monster.");
            }

            CleanDuelGame noMaterial = ReadyCleanGame();
            noMaterial.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(21082832), CleanPlayerId.Local));
            noMaterial.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(55410871), CleanPlayerId.Local));
            if (noMaterial.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Chaos Form rule failed: Activate should be disabled without exact-level ritual tribute material.");
            }

            CleanDuelGame ritual = ReadyCleanGame();
            ritual.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(21082832), CleanPlayerId.Local));
            ritual.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(55410871), CleanPlayerId.Local));
            ritual.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));

            if (!ritual.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Chaos Form rule failed: Activate was not available with Ritual Monster and exact Level 8 material.");
            }

            ritual.ActivateHandCard(0);
            if (ritual.State.Local.Monsters[0] == null || ritual.State.Local.Monsters[0].Definition.Id != 55410871)
            {
                throw new InvalidOperationException("Chaos Form rule failed: Blue-Eyes Chaos MAX Dragon was not Ritual Summoned.");
            }

            if (ritual.State.Local.Hand.Exists(card => card.Definition.Id == 55410871 || card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Chaos Form rule failed: ritual monster or tribute material stayed in hand.");
            }

            if (!ritual.State.Local.Graveyard.Exists(card => card.Definition.Id == 21082832) || !ritual.State.Local.Graveyard.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Chaos Form rule failed: Ritual Spell and tribute material were not sent to graveyard.");
            }
        }
        private static void AssertTrueLightChoicePopupSummonsBlueEyes()
        {
            CleanDuelGame game = ReadyCleanGame();
            game.State.Local.SpellsTraps[0] = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local);
            game.State.Local.SpellsTraps[0].FaceDown = true;
            game.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            game.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(71143015), CleanPlayerId.Local));

            if (!game.CanActivateSetCard(0))
            {
                throw new InvalidOperationException("True Light activation failed: Flip/Activate was not available.");
            }

            game.ActivateSetCard(0);
            if (game.State.PendingChoice == null || game.State.PendingChoice.Options.Count != 2)
            {
                throw new InvalidOperationException("True Light activation failed: choice popup with two options was not opened.");
            }

            if (!game.State.PendingChoice.Options[0].IsAvailable || !game.State.PendingChoice.Options[1].IsAvailable)
            {
                throw new InvalidOperationException("True Light activation failed: both choices should be valid in the hand/deck setup.");
            }

            game.ChoosePendingOption(0);
            if (game.State.PendingChoice != null)
            {
                throw new InvalidOperationException("True Light summon choice failed: popup did not close.");
            }

            if (game.State.Local.SpellsTraps[0] == null || game.State.Local.SpellsTraps[0].Definition.Id != 62089826 || game.State.Local.SpellsTraps[0].FaceDown)
            {
                throw new InvalidOperationException("True Light summon choice failed: True Light should stay face-up on the field.");
            }

            if (game.State.Local.Monsters[0] == null || game.State.Local.Monsters[0].Definition.Id != 89631139)
            {
                throw new InvalidOperationException("True Light summon choice failed: Blue-Eyes White Dragon was not summoned.");
            }
        }

        private static void AssertTrueLightChoicePopupSetsSpellTrap()
        {
            CleanDuelGame game = ReadyCleanGame();
            game.State.Local.SpellsTraps[0] = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local);
            game.State.Local.SpellsTraps[0].FaceDown = true;
            game.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(71143015), CleanPlayerId.Local));

            if (!game.CanActivateSetCard(0))
            {
                throw new InvalidOperationException("True Light set choice failed: Flip/Activate was not available.");
            }

            game.ActivateSetCard(0);
            if (game.State.PendingChoice == null || game.State.PendingChoice.Options.Count != 2)
            {
                throw new InvalidOperationException("True Light set choice failed: choice popup with two options was not opened.");
            }

            if (game.State.PendingChoice.Options[0].IsAvailable)
            {
                throw new InvalidOperationException("True Light set choice failed: summon option should be disabled without Blue-Eyes in hand or GY.");
            }

            if (!game.State.PendingChoice.Options[1].IsAvailable)
            {
                throw new InvalidOperationException("True Light set choice failed: set option should be available with Ultimate Fusion in deck.");
            }

            game.ChoosePendingOption(1);
            if (game.State.PendingChoice != null)
            {
                throw new InvalidOperationException("True Light set choice failed: popup did not close.");
            }

            if (game.State.Local.SpellsTraps[1] == null || game.State.Local.SpellsTraps[1].Definition.Id != 71143015 || !game.State.Local.SpellsTraps[1].FaceDown)
            {
                throw new InvalidOperationException("True Light set choice failed: Ultimate Fusion was not set from deck.");
            }
        }
        private static void AssertTrueLightProtectsBlueEyesFromTargeting()
        {
            CleanDuelGame game = ReadyCleanGame();
            game.State.Local.SpellsTraps[0] = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local);
            game.State.Local.SpellsTraps[0].FaceDown = false;
            game.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            game.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local);

            int firstResult = InvokeLuaDestroyStrongest(game, game.State.Local);
            if (firstResult != 1)
            {
                throw new InvalidOperationException("True Light targeting protection failed: another valid monster should have been targeted.");
            }

            if (game.State.Local.Monsters[0] == null || game.State.Local.Monsters[0].Definition.Id != 89631139)
            {
                throw new InvalidOperationException("True Light targeting protection failed: Blue-Eyes White Dragon was destroyed by a targeted effect.");
            }

            if (game.State.Local.Monsters[1] != null || !game.State.Local.Graveyard.Exists(card => card.Definition.Id == 17947697))
            {
                throw new InvalidOperationException("True Light targeting protection failed: the unprotected monster was not destroyed instead.");
            }

            game = ReadyCleanGame();
            game.State.Local.SpellsTraps[0] = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local);
            game.State.Local.SpellsTraps[0].FaceDown = false;
            game.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);

            int secondResult = InvokeLuaDestroyStrongest(game, game.State.Local);
            if (secondResult != 0)
            {
                throw new InvalidOperationException("True Light targeting protection failed: protected Blue-Eyes should leave no valid target.");
            }

            if (game.State.Local.Monsters[0] == null || game.State.Local.Graveyard.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("True Light targeting protection failed: lone protected Blue-Eyes did not stay on the field.");
            }
        }

        private static int InvokeLuaDestroyStrongest(CleanDuelGame game, CleanPlayerState player)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("LuaDestroyStrongest", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find LuaDestroyStrongest for targeting protection test.");
            }

            return (int)method.Invoke(game, new object[] { player });
        }
        private static void AssertNormalMonsterHasNoActivationAndUsesTributes()
        {
            CleanDuelGame game = ReadyCleanGame();
            game.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));

            if (game.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Normal monster rule failed: Blue-Eyes White Dragon should not activate from hand.");
            }

            if (game.CanNormalSummon(0) || game.CanSetMonster(0))
            {
                throw new InvalidOperationException("Tribute rule failed: Level 8 Blue-Eyes should not be summonable/settable without tributes.");
            }

            game.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local);
            if (game.CanNormalSummon(0) || game.CanSetMonster(0))
            {
                throw new InvalidOperationException("Tribute rule failed: Level 8 Blue-Eyes should require two tributes, not one.");
            }

            game.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local);
            if (!game.CanNormalSummon(0) || !game.CanSetMonster(0))
            {
                throw new InvalidOperationException("Tribute rule failed: Level 8 Blue-Eyes should be summonable/settable with two tributes.");
            }

            game.NormalSummon(0);
            if (game.State.Local.Monsters[0] == null || game.State.Local.Monsters[0].Definition.Id != 89631139 || game.State.Local.Monsters[0].Stance != CleanMonsterStance.Attack)
            {
                throw new InvalidOperationException("Tribute summon failed: Blue-Eyes was not normal summoned in attack position.");
            }

            if (game.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Normal monster rule failed: face-up Blue-Eyes should not have an Activate action.");
            }

            if (!game.State.Local.Graveyard.Exists(card => card.Definition.Id == 17947697) || !game.State.Local.Graveyard.Exists(card => card.Definition.Id == 8240199))
            {
                throw new InvalidOperationException("Tribute summon failed: tribute monsters were not sent to the GY.");
            }

            game = ReadyCleanGame();
            game.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            game.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local);
            game.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local);
            game.SetMonster(0);

            if (game.State.Local.Monsters[0] == null || game.State.Local.Monsters[0].Definition.Id != 89631139 || !game.State.Local.Monsters[0].FaceDown || game.State.Local.Monsters[0].Stance != CleanMonsterStance.Defense)
            {
                throw new InvalidOperationException("Tribute set failed: Blue-Eyes was not set face-down in defense position.");
            }
        }
        private static CleanDuelGame ReadyCleanGame()
        {
            CleanDuelGame game = new CleanDuelGame();
            game.StartNewDuel();
            game.AdvancePhase();
            game.State.Local.Hand.Clear();
            game.State.Local.Deck.Clear();
            game.State.Local.ExtraDeck.Clear();
            game.State.Local.Graveyard.Clear();
            Array.Clear(game.State.Local.Monsters, 0, game.State.Local.Monsters.Length);
            Array.Clear(game.State.Local.SpellsTraps, 0, game.State.Local.SpellsTraps.Length);
            return game;
        }

        private static void AssertMaidenPlacedTrueLight(CleanDuelGame game, string source)
        {
            if (game.State.Local.SpellsTraps[0] == null || game.State.Local.SpellsTraps[0].Definition.Id != 62089826)
            {
                throw new InvalidOperationException("Maiden of White " + source + " activation failed: True Light was not placed.");
            }

            if (game.State.Local.SpellsTraps[0].FaceDown)
            {
                throw new InvalidOperationException("Maiden of White " + source + " activation failed: True Light should be face-up.");
            }

            if (!game.State.Local.Graveyard.Exists(card => card.Definition.Id == 17947697))
            {
                throw new InvalidOperationException("Maiden of White " + source + " activation failed: Maiden was not sent to graveyard.");
            }
        }

        private static void AssertEveryCardHasCleanEffect(List<CleanCard> cards, string label)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (IsNormalMonsterWithoutActivation(cards[i]))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(cards[i].Definition.EffectKey))
                {
                    throw new InvalidOperationException(label + " card has no clean activation: " + cards[i].Definition.Name);
                }
            }
        }

        private static bool IsNormalMonsterWithoutActivation(CleanCard card)
        {
            if (card == null || card.Definition.Kind != CleanCardKind.Monster)
            {
                return false;
            }

            if (card.Definition.Id == 89631139 || card.Definition.Id == 23995346)
            {
                return true;
            }

            return card.Definition.TypeLine != null &&
                   card.Definition.TypeLine.IndexOf("Normal", StringComparison.OrdinalIgnoreCase) >= 0 &&
                   card.Definition.TypeLine.IndexOf("Effect", StringComparison.OrdinalIgnoreCase) < 0;
        }
    }
}
