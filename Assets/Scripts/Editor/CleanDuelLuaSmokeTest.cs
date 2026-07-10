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

            AssertScreenshotDeckListFromSummary();
            AssertEveryCardHasCleanEffect(CleanCardLibrary.BuildDeck(CleanPlayerId.Local), "Main Deck");
            AssertEveryCardHasCleanEffect(CleanCardLibrary.BuildExtraDeck(CleanPlayerId.Local), "Extra Deck");
            AssertLuaScriptCardsGetGeneratedEffectKeys();
            AssertLuaCardsOnlyActivateWhenResolvable();
            AssertLuaDestroyTargetChoiceAllowsManualTarget();
            AssertWishesSearchesBlueEyes();
            AssertDocumentSpellEffects();
            AssertDocumentMonsterEffects();
            AssertGatewayAndSpiritScreenshotRules();
            AssertKnowledgeAuditBuilds();
            AssertEffectTimingRules();
            AssertFlipAndLinkRules();
            AssertPendulumRules();
            AssertDocumentTriggeredEffects();
            AssertMaidenFromHandPlacesTrueLight();
            AssertMaidenFromFieldPlacesTrueLight();
            AssertMaidenRevivesAfterTrueLightSummonsBlueEyes();
            AssertUltimateFusionSummonsFromExtraDeck();
            AssertSynchroRequiresTunerAndNonTuner();
            AssertBlueEyesSpiritDragonEffects();
            AssertBlueEyesUltimateSpiritDragonEffects();
            AssertRitualMonstersRequireRitualSpell();
            AssertTrueLightChoicePopupSummonsBlueEyes();
            AssertTrueLightChoicePopupSetsSpellTrap();
            AssertTrueLightProtectsBlueEyesFromTargeting();
            AssertTrueLightLeavingFieldDestroysOwnMonsters();
            AssertPhaseFlow();
            AssertDeckOutRules();
            AssertNormalMonsterHasNoActivationAndUsesTributes();
            Debug.Log("CleanDuel screenshot deck smoke test passed. Main=41, Extra=10, clean activations, real fusion summons, ritual summons, Flip/Trigger rules, Link Summon rules, Pendulum rules, True Light rules, phase flow, deck-out, targeting protection, and tribute rules verified.");
        }

        private static void AssertScreenshotDeckListFromSummary()
        {
            List<CleanCard> main = CleanCardLibrary.BuildDeck(CleanPlayerId.Local);
            AssertCardCount(main, 89631139, 3, "Blue-Eyes White Dragon");
            AssertCardCount(main, 88241506, 3, "Maiden with Eyes of Blue");
            AssertCardCount(main, 8240199, 3, "Sage with Eyes of Blue");
            AssertCardCount(main, 71039903, 3, "The White Stone of Ancients");
            AssertCardCount(main, 17947697, 2, "Maiden of White");
            AssertCardCount(main, 23434538, 1, "Maxx C");
            AssertCardCount(main, 97268402, 1, "Effect Veiler");
            AssertCardCount(main, 14558127, 1, "Ash Blossom");
            AssertCardCount(main, 59438930, 1, "Ghost Ogre");
            AssertCardCount(main, 66961194, 1, "Dictator of D.");
            AssertCardCount(main, 30576089, 2, "Blue-Eyes Jet Dragon");
            AssertCardCount(main, 45467446, 1, "Dragon Spirit of White");
            AssertCardCount(main, 55410871, 1, "Blue-Eyes Chaos MAX Dragon");
            AssertCardCount(main, 20654247, 1, "Blue-Eyes Chaos Dragon");
            AssertCardCount(main, 24382602, 1, "Mausoleum of White");
            AssertCardCount(main, 93437091, 1, "Bingo Machine, Go!!!");
            AssertCardCount(main, 80326401, 2, "Wishes for Eyes of Blue");
            AssertCardCount(main, 17725109, 3, "Roar of the Blue-Eyed Dragons");
            AssertCardCount(main, 40089744, 1, "Gateway to Chaos");
            AssertCardCount(main, 2783661, 1, "Majesty with Eyes of Blue");
            AssertCardCount(main, 21082832, 1, "Chaos Form");
            AssertCardCount(main, 41620959, 1, "Dragon Shrine");
            AssertCardCount(main, 71143015, 3, "Ultimate Fusion");
            AssertCardType(main, 71143015, CleanCardKind.Spell, "Quick-Play", "Ultimate Fusion");
            AssertCardCount(main, 62089826, 3, "True Light");

            List<CleanCard> extra = CleanCardLibrary.BuildExtraDeck(CleanPlayerId.Local);
            AssertCardCount(extra, 23995346, 1, "Blue-Eyes Ultimate Dragon");
            AssertCardCount(extra, 12381100, 3, "Dragon Master Magia");
            AssertCardCount(extra, 59822133, 2, "Blue-Eyes Spirit Dragon");
            AssertCardCount(extra, 89604813, 1, "Blue-Eyes Ultimate Spirit Dragon");
            AssertCardCount(extra, 40908371, 0, "Azure-Eyes Silver Dragon");
            AssertCardCount(extra, 42097666, 3, "Spirit with Eyes of Blue");
        }

        private static void AssertDocumentSpellEffects()
        {
            CleanDuelGame shrine = ReadyCleanGame();
            shrine.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(41620959), CleanPlayerId.Local));
            shrine.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            shrine.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            if (!shrine.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Dragon Shrine failed: activation was not available with Dragon monsters in deck.");
            }

            shrine.ActivateHandCard(0);
            if (shrine.State.PendingChoice == null || IndexOfChoiceOption(shrine, 30576089) < 0)
            {
                throw new InvalidOperationException("Dragon Shrine failed: it should offer Dragon monsters from deck as manual choices.");
            }

            shrine.ChoosePendingOption(IndexOfChoiceOption(shrine, 30576089));
            if (shrine.State.PendingChoice != null || !shrine.State.Local.Graveyard.Exists(card => card.Definition.Id == 30576089) || !shrine.State.Local.Deck.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Dragon Shrine failed: chosen non-Normal Dragon should go to GY and unchosen Blue-Eyes should stay in deck.");
            }

            CleanDuelGame shrineNormal = ReadyCleanGame();
            shrineNormal.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(41620959), CleanPlayerId.Local));
            shrineNormal.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            shrineNormal.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            shrineNormal.ActivateHandCard(0);
            shrineNormal.ChoosePendingOption(IndexOfChoiceOption(shrineNormal, 89631139));
            if (shrineNormal.State.PendingChoice == null || IndexOfChoiceOption(shrineNormal, 30576089) < 0)
            {
                throw new InvalidOperationException("Dragon Shrine failed: choosing a Normal Dragon should open the extra Dragon choice.");
            }

            shrineNormal.ChoosePendingOption(IndexOfChoiceOption(shrineNormal, 30576089));
            if (shrineNormal.State.PendingChoice != null || !shrineNormal.State.Local.Graveyard.Exists(card => card.Definition.Id == 89631139) || !shrineNormal.State.Local.Graveyard.Exists(card => card.Definition.Id == 30576089))
            {
                throw new InvalidOperationException("Dragon Shrine failed: chosen Normal Dragon and chosen extra Dragon should both go to the GY.");
            }

            CleanDuelGame mausoleumField = ReadyCleanGame();
            mausoleumField.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(24382602), CleanPlayerId.Local));
            if (!mausoleumField.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Mausoleum of White failed: Field Spell activation from hand was not available.");
            }

            mausoleumField.ActivateHandCard(0);
            if (mausoleumField.State.Local.FieldSpell == null || mausoleumField.State.Local.FieldSpell.Definition.Id != 24382602 || mausoleumField.State.Local.FieldSpell.FaceDown || mausoleumField.State.Local.Graveyard.Exists(card => card.Definition.Id == 24382602))
            {
                throw new InvalidOperationException("Mausoleum of White failed: Field Spell should stay face-up on the field, not go to the GY.");
            }

            mausoleumField.State.Local.NormalSummonUsed = true;
            mausoleumField.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local));
            if (!mausoleumField.CanNormalSummon(0))
            {
                throw new InvalidOperationException("Mausoleum of White failed: extra Normal Summon for Level 1 LIGHT Tuner was not available.");
            }

            mausoleumField.NormalSummon(0);
            if (mausoleumField.State.Local.Monsters[0] == null || mausoleumField.State.Local.Monsters[0].Definition.Id != 8240199)
            {
                throw new InvalidOperationException("Mausoleum of White failed: extra Normal Summon did not summon Sage.");
            }

            CleanDuelGame mausoleumBoost = ReadyCleanGame();
            mausoleumBoost.State.Local.FieldSpell = new CleanCard(CleanCardLibrary.Get(24382602), CleanPlayerId.Local);
            mausoleumBoost.State.Local.FieldSpell.FaceDown = false;
            mausoleumBoost.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local);
            mausoleumBoost.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (!mausoleumBoost.CanActivateSetCard(CleanDuelGame.FieldSpellZoneIndex))
            {
                throw new InvalidOperationException("Mausoleum of White failed: face-up boost effect was not available.");
            }

            mausoleumBoost.ActivateSetCard(CleanDuelGame.FieldSpellZoneIndex);
            if (mausoleumBoost.State.PendingChoice == null || IndexOfChoiceOption(mausoleumBoost, 0) < 0)
            {
                throw new InvalidOperationException("Mausoleum of White failed: target choice did not open.");
            }

            mausoleumBoost.ChoosePendingOption(IndexOfChoiceOption(mausoleumBoost, 0));
            if (mausoleumBoost.State.PendingChoice == null || IndexOfChoiceOption(mausoleumBoost, 89631139) < 0)
            {
                throw new InvalidOperationException("Mausoleum of White failed: Normal Monster choice did not open.");
            }

            mausoleumBoost.ChoosePendingOption(IndexOfChoiceOption(mausoleumBoost, 89631139));
            if (mausoleumBoost.State.Local.Monsters[0].CurrentAttack != 800 || mausoleumBoost.State.Local.Monsters[0].CurrentDefense != 2300 || !mausoleumBoost.State.Local.Graveyard.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Mausoleum of White failed: chosen Normal Monster should go to GY and give Level x100 ATK/DEF.");
            }
            CleanDuelGame roar = ReadyCleanGame();
            roar.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(17725109), CleanPlayerId.Local));
            roar.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            roar.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (!roar.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Roar failed: activation was not available with Blue-Eyes White Dragon in deck.");
            }

            roar.ActivateHandCard(0);
            if (roar.State.Local.Monsters[0] == null || roar.State.Local.Monsters[0].Definition.Id != 89631139 || roar.State.Local.Monsters[0].Stance != CleanMonsterStance.Defense)
            {
                throw new InvalidOperationException("Roar failed: without controlling Blue-Eyes White Dragon it must summon Blue-Eyes White Dragon in Defense Position.");
            }

            CleanDuelGame gateway = ReadyCleanGame();
            gateway.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(40089744), CleanPlayerId.Local));
            gateway.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(70551291), CleanPlayerId.Local));
            if (!gateway.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Gateway to Chaos failed: activation was not available with a Black Luster Soldier Ritual in deck.");
            }

            gateway.ActivateHandCard(0);
            if (gateway.State.PendingChoice == null || IndexOfChoiceOption(gateway, 70551291) < 0)
            {
                throw new InvalidOperationException("Gateway to Chaos failed: Black Luster Soldier Ritual was not offered as a search choice.");
            }

            gateway.ChoosePendingOption(IndexOfChoiceOption(gateway, 70551291));
            if (!gateway.State.Local.Hand.Exists(card => card.Definition.Id == 70551291))
            {
                throw new InvalidOperationException("Gateway to Chaos failed: chosen Black Luster Soldier Ritual was not added to hand.");
            }

            CleanDuelGame majesty = ReadyCleanGame();
            majesty.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(2783661), CleanPlayerId.Local));
            majesty.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            majesty.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            if (!majesty.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Majesty failed: activation was not available with Blue-Eyes in deck and a face-up target.");
            }

            majesty.ActivateHandCard(0);
            if (!majesty.State.Local.Graveyard.Exists(card => card.Definition.Id == 30576089) || !majesty.State.Rival.Monsters[0].CannotAttack)
            {
                throw new InvalidOperationException("Majesty failed: it should send a Blue-Eyes monster to GY and prevent the target from attacking.");
            }
        }

        private static void AssertDocumentMonsterEffects()
        {
            CleanDuelGame maxx = ReadyCleanGame();
            maxx.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(23434538), CleanPlayerId.Local));
            maxx.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (!maxx.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Maxx C failed: activation was not available with a deck to draw from.");
            }

            maxx.ActivateHandCard(0);
            if (!maxx.State.Local.Hand.Exists(card => card.Definition.Id == 89631139) || !maxx.State.Local.Graveyard.Exists(card => card.Definition.Id == 23434538))
            {
                throw new InvalidOperationException("Maxx C failed: it should go to GY and draw a card in the clean model.");
            }

            CleanDuelGame ghost = ReadyCleanGame();
            ghost.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(59438930), CleanPlayerId.Local));
            ghost.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            if (!ghost.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Ghost Ogre failed: activation was not available against a face-up monster.");
            }

            ghost.ActivateHandCard(0);
            if (ghost.State.Rival.Monsters[0] != null || !ghost.State.Rival.Graveyard.Exists(card => card.Definition.Id == 66961194) || !ghost.State.Local.Graveyard.Exists(card => card.Definition.Id == 59438930))
            {
                throw new InvalidOperationException("Ghost Ogre failed: it should send itself to GY and destroy the activated face-up card.");
            }

            CleanDuelGame spirit = ReadyCleanGame();
            spirit.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(45467446), CleanPlayerId.Local);
            spirit.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            spirit.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            spirit.State.Rival.SpellsTraps[0] = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Rival);
            spirit.State.Rival.SpellsTraps[0].FaceDown = false;
            if (!spirit.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Dragon Spirit of White failed: monster-zone activation was not available.");
            }

            spirit.ActivateMonsterZoneCard(0);
            if (spirit.State.Rival.SpellsTraps[0] != null || !spirit.State.Rival.Banished.Exists(card => card.Definition.Id == 62089826) || spirit.State.Local.Monsters[0] == null || spirit.State.Local.Monsters[0].Definition.Id != 89631139 || !spirit.State.Local.Graveyard.Exists(card => card.Definition.Id == 45467446))
            {
                throw new InvalidOperationException("Dragon Spirit of White failed: it should banish a Spell/Trap, tribute itself, and summon Blue-Eyes White Dragon.");
            }

            CleanDuelGame spiritField = ReadyCleanGame();
            spiritField.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(45467446), CleanPlayerId.Local);
            spiritField.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            spiritField.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            spiritField.State.Rival.FieldSpell = new CleanCard(CleanCardLibrary.Get(24382602), CleanPlayerId.Rival);
            spiritField.State.Rival.FieldSpell.FaceDown = false;
            if (!spiritField.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Dragon Spirit field spell banish failed: monster-zone activation was not available.");
            }

            spiritField.ActivateMonsterZoneCard(0);
            if (spiritField.State.Rival.FieldSpell != null || !spiritField.State.Rival.Banished.Exists(card => card.Definition.Id == 24382602))
            {
                throw new InvalidOperationException("Dragon Spirit field spell banish failed: Field Spell should move to the Banished zone.");
            }

            CleanDuelGame sage = ReadyCleanGame();
            sage.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local));
            sage.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(71039903), CleanPlayerId.Local));
            if (!sage.CanNormalSummon(0))
            {
                throw new InvalidOperationException("Sage failed: it should be normal summonable.");
            }

            sage.NormalSummon(0);
            if (sage.State.PendingChoice == null || IndexOfChoiceOption(sage, 71039903) < 0)
            {
                throw new InvalidOperationException("Sage failed: normal summon should open a Level 1 LIGHT Tuner choice.");
            }

            sage.ChoosePendingOption(IndexOfChoiceOption(sage, 71039903));
            if (!sage.State.Local.Hand.Exists(card => card.Definition.Id == 71039903))
            {
                throw new InvalidOperationException("Sage failed: chosen Level 1 LIGHT Tuner was not added to hand.");
            }

            CleanDuelGame sageHand = ReadyCleanGame();
            sageHand.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local));
            sageHand.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local);
            sageHand.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            if (!sageHand.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Sage hand effect failed: activation was not available with an Effect Monster target and Blue-Eyes in Deck.");
            }

            sageHand.ActivateHandCard(0);
            if (sageHand.State.PendingChoice == null || IndexOfChoiceOption(sageHand, 0) < 0)
            {
                throw new InvalidOperationException("Sage hand effect failed: target choice did not open.");
            }

            sageHand.ChoosePendingOption(IndexOfChoiceOption(sageHand, 0));
            if (sageHand.State.PendingChoice == null || IndexOfChoiceOption(sageHand, 30576089) < 0)
            {
                throw new InvalidOperationException("Sage hand effect failed: Blue-Eyes Deck choice did not open.");
            }

            sageHand.ChoosePendingOption(IndexOfChoiceOption(sageHand, 30576089));
            if (sageHand.State.Local.Monsters[0] == null || sageHand.State.Local.Monsters[0].Definition.Id != 30576089 || !sageHand.State.Local.Graveyard.Exists(card => card.Definition.Id == 8240199) || !sageHand.State.Local.Graveyard.Exists(card => card.Definition.Id == 17947697))
            {
                throw new InvalidOperationException("Sage hand effect failed: Sage and target should go to GY, then chosen Blue-Eyes should be Special Summoned from Deck.");
            }

            CleanDuelGame jet = ReadyCleanGame();
            jet.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            if (jet.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Blue-Eyes Jet Dragon failed: hand summon should require Blue-Eyes White Dragon on field or in GY.");
            }

            jet.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (!jet.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Blue-Eyes Jet Dragon failed: hand summon was not available with Blue-Eyes White Dragon in GY.");
            }

            jet.ActivateHandCard(0);
            if (jet.State.Local.Monsters[0] == null || jet.State.Local.Monsters[0].Definition.Id != 30576089)
            {
                throw new InvalidOperationException("Blue-Eyes Jet Dragon failed: it did not Special Summon itself from hand.");
            }

            CleanDuelGame jetProtection = ReadyCleanGame();
            jetProtection.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            jetProtection.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local);
            InvokeDestroyStrongestFaceUpCard(jetProtection, jetProtection.State.Local);
            if (jetProtection.State.Local.Monsters[0] == null || jetProtection.State.Local.Monsters[0].Definition.Id != 89631139)
            {
                throw new InvalidOperationException("Blue-Eyes Jet Dragon failed: it should protect other controlled cards from card-effect destruction.");
            }

            CleanDuelGame jetTrigger = ReadyCleanGame();
            jetTrigger.State.Phase = CleanDuelPhase.Battle;
            jetTrigger.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            jetTrigger.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            jetTrigger.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(55410871), CleanPlayerId.Rival);
            jetTrigger.Attack(0, 0);
            if (jetTrigger.State.Local.Monsters[0] == null || jetTrigger.State.Local.Monsters[0].Definition.Id != 30576089)
            {
                throw new InvalidOperationException("Blue-Eyes Jet Dragon failed: it should Special Summon itself after a field card is destroyed by battle.");
            }

            CleanDuelGame jetBounce = ReadyCleanGame();
            jetBounce.State.Phase = CleanDuelPhase.Battle;
            jetBounce.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local);
            jetBounce.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Rival);
            jetBounce.Attack(0, 0);
            if (jetBounce.State.Rival.Monsters[0] != null || !jetBounce.State.Rival.Hand.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Blue-Eyes Jet Dragon failed: battle trigger should return an opponent card to hand.");
            }
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

        private static void AssertLuaCardsOnlyActivateWhenResolvable()
        {
            CleanDuelGame game = ReadyCleanGame();
            game.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(55144522), CleanPlayerId.Local));

            if (game.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Activation gate failed: Pot of Greed should not activate with an empty Deck.");
            }

            game.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (game.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Activation gate failed: Pot of Greed should not activate unless two cards can be drawn.");
            }

            game.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            if (!game.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Activation gate failed: a resolvable Lua draw card should get an Activate action.");
            }

            game.State.Rival.Hand.Clear();
            game.State.Rival.Deck.Clear();
            game.State.Rival.Hand.Add(new CleanCard(CleanCardLibrary.Get(55144522), CleanPlayerId.Rival));
            MethodInfo method = typeof(CleanDuelGame).GetMethod("FirstPlayableRivalSpell", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find FirstPlayableRivalSpell for activation gate test.");
            }

            if ((int)method.Invoke(game, null) != -1)
            {
                throw new InvalidOperationException("Activation gate failed: rival AI should not auto-activate unresolved Lua spells.");
            }

            game.State.Rival.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Rival));
            if ((int)method.Invoke(game, null) != -1)
            {
                throw new InvalidOperationException("Activation gate failed: rival AI should not auto-activate draw spells without enough cards to draw.");
            }

            game.State.Rival.Deck.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Rival));
            if ((int)method.Invoke(game, null) != 0)
            {
                throw new InvalidOperationException("Activation gate failed: rival AI should activate resolvable Lua spells.");
            }
        }

        private static void AssertWishesSearchesBlueEyes()
        {
            CleanDuelGame game = ReadyCleanGame();
            game.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(80326401), CleanPlayerId.Local));
            game.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            game.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local));
            game.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(17725109), CleanPlayerId.Local));

            if (!game.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Wishes for Eyes of Blue activation failed: Activate was not available with discard cost and both search targets.");
            }

            game.ActivateHandCard(0);
            if (game.State.PendingChoice == null || IndexOfChoiceOption(game, 89631139) < 0)
            {
                throw new InvalidOperationException("Wishes for Eyes of Blue activation failed: discard choice did not open.");
            }

            game.ChoosePendingOption(IndexOfChoiceOption(game, 89631139));
            if (!game.State.Local.Graveyard.Exists(card => card.Definition.Id == 89631139) || game.State.PendingChoice == null || IndexOfChoiceOption(game, 8240199) < 0)
            {
                throw new InvalidOperationException("Wishes for Eyes of Blue activation failed: discard was not paid or Tuner choice did not open.");
            }

            game.ChoosePendingOption(IndexOfChoiceOption(game, 8240199));
            if (!game.State.Local.Hand.Exists(card => card.Definition.Id == 8240199) || game.State.PendingChoice == null || IndexOfChoiceOption(game, 17725109) < 0)
            {
                throw new InvalidOperationException("Wishes for Eyes of Blue activation failed: chosen Tuner was not added or Spell/Trap choice did not open.");
            }

            game.ChoosePendingOption(IndexOfChoiceOption(game, 17725109));
            if (game.State.PendingChoice != null || !game.State.Local.Hand.Exists(card => card.Definition.Id == 17725109))
            {
                throw new InvalidOperationException("Wishes for Eyes of Blue activation failed: chosen Spell/Trap was not added to hand.");
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

        private static void AssertMaidenRevivesAfterTrueLightSummonsBlueEyes()
        {
            CleanDuelGame game = ReadyCleanGame();
            game.State.Local.SpellsTraps[0] = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local);
            game.State.Local.SpellsTraps[0].FaceDown = false;
            game.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local));
            game.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));

            if (!game.CanActivateSetCard(0))
            {
                throw new InvalidOperationException("Maiden of White revive failed: True Light activation was not available.");
            }

            game.ActivateSetCard(0);
            game.ChoosePendingOption(0);

            if (game.State.Local.Monsters[0] == null || game.State.Local.Monsters[0].Definition.Id != 89631139)
            {
                throw new InvalidOperationException("Maiden of White revive failed: True Light did not Special Summon Blue-Eyes White Dragon.");
            }

            if (game.State.Local.Monsters[1] == null || game.State.Local.Monsters[1].Definition.Id != 17947697)
            {
                throw new InvalidOperationException("Maiden of White revive failed: Maiden did not Special Summon itself from the GY.");
            }

            if (game.State.Local.Graveyard.Exists(card => card.Definition.Id == 17947697))
            {
                throw new InvalidOperationException("Maiden of White revive failed: Maiden stayed in the GY after its trigger.");
            }
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

            CleanDuelGame unrelatedFusion = ReadyCleanGame();
            unrelatedFusion.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(71143015), CleanPlayerId.Local));
            unrelatedFusion.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            unrelatedFusion.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            CleanCardDefinition unrelatedDefinition = new CleanCardDefinition(
                99900001,
                "Unrelated Fusion Dragon",
                CleanCardKind.Monster,
                8,
                5000,
                5000,
                "Dragon / Fusion / Effect",
                string.Empty,
                string.Empty);
            unrelatedFusion.State.Local.ExtraDeck.Add(new CleanCard(unrelatedDefinition, CleanPlayerId.Local));
            if (unrelatedFusion.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Ultimate Fusion restriction failed: an unrelated Fusion Monster was accepted.");
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

            CleanDuelGame magia = ReadyCleanGame();
            magia.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(71143015), CleanPlayerId.Local));
            magia.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            magia.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            magia.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            magia.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(55410871), CleanPlayerId.Local));
            magia.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(12381100), CleanPlayerId.Local));
            if (!magia.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Ultimate Fusion restriction failed: Dragon Master Magia should be a valid target.");
            }

            magia.ActivateHandCard(0);
            if (magia.State.Local.Monsters[0] == null || magia.State.Local.Monsters[0].Definition.Id != 12381100)
            {
                throw new InvalidOperationException("Ultimate Fusion activation failed: Dragon Master Magia was not Fusion Summoned.");
            }
        }

        private static void AssertSynchroRequiresTunerAndNonTuner()
        {
            CleanDuelGame allTuners = ReadyCleanGame();
            allTuners.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(59822133), CleanPlayerId.Local));
            allTuners.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local);
            allTuners.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(14558127), CleanPlayerId.Local);
            allTuners.State.Local.Monsters[2] = new CleanCard(CleanCardLibrary.Get(59438930), CleanPlayerId.Local);
            allTuners.State.Local.Monsters[3] = new CleanCard(CleanCardLibrary.Get(97268402), CleanPlayerId.Local);
            allTuners.State.Local.Monsters[4] = new CleanCard(CleanCardLibrary.Get(71039903), CleanPlayerId.Local);
            if (InvokeLuaCanSynchroSummon(allTuners, allTuners.State.Local, 59822133))
            {
                throw new InvalidOperationException("Synchro rule failed: a Synchro Summon must not be available with only Tuner monsters, even when Levels match.");
            }

            CleanDuelGame noTuner = ReadyCleanGame();
            noTuner.State.Local.ExtraDeck.Add(TestMonster(99000001, "Generic Level 9 Synchro", 9, "Dragon / Synchro / Effect"));
            noTuner.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            noTuner.State.Local.Monsters[1] = TestMonster(99000002, "Level 1 non-Tuner", 1, "Warrior / Effect");
            if (InvokeLuaCanSynchroSummon(noTuner, noTuner.State.Local, 99000001))
            {
                throw new InvalidOperationException("Synchro rule failed: a Synchro Summon must not be available with only non-Tuner monsters, even when Levels match.");
            }

            CleanDuelGame valid = ReadyCleanGame();
            valid.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(59822133), CleanPlayerId.Local));
            valid.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local);
            valid.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            if (!InvokeLuaCanSynchroSummon(valid, valid.State.Local, 59822133) || !InvokeLuaSynchroSummon(valid, valid.State.Local, 59822133))
            {
                throw new InvalidOperationException("Synchro rule failed: Blue-Eyes Spirit Dragon should be summonable with 1 Tuner plus 1 non-Tuner Blue-Eyes monster at Level 9 total.");
            }

            if (valid.State.Local.Monsters[0] == null || valid.State.Local.Monsters[0].Definition.Id != 59822133 || !valid.State.Local.Monsters[0].SynchroSummoned)
            {
                throw new InvalidOperationException("Synchro summon failed: Blue-Eyes Spirit Dragon was not marked as Synchro Summoned on the field.");
            }

            if (!valid.State.Local.Graveyard.Exists(card => card.Definition.Id == 8240199) || !valid.State.Local.Graveyard.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Synchro summon failed: Tuner and non-Tuner materials were not sent from field to GY.");
            }

            CleanDuelGame ultimateOneTuner = ReadyCleanGame();
            ultimateOneTuner.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(89604813), CleanPlayerId.Local));
            ultimateOneTuner.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            ultimateOneTuner.State.Local.Monsters[1] = TestMonster(99000003, "Level 4 Tuner", 4, "Spellcaster / Tuner / Effect");
            if (InvokeLuaCanSynchroSummon(ultimateOneTuner, ultimateOneTuner.State.Local, 89604813))
            {
                throw new InvalidOperationException("Synchro rule failed: Blue-Eyes Ultimate Spirit Dragon should require at least 2 Tuners.");
            }

            CleanDuelGame ultimateValid = ReadyCleanGame();
            ultimateValid.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(89604813), CleanPlayerId.Local));
            ultimateValid.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            ultimateValid.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local);
            ultimateValid.State.Local.Monsters[2] = new CleanCard(CleanCardLibrary.Get(59438930), CleanPlayerId.Local);
            if (!InvokeLuaCanSynchroSummon(ultimateValid, ultimateValid.State.Local, 89604813) || !InvokeLuaSynchroSummon(ultimateValid, ultimateValid.State.Local, 89604813))
            {
                throw new InvalidOperationException("Synchro rule failed: Blue-Eyes Ultimate Spirit Dragon should be summonable with 1 Blue-Eyes non-Tuner plus 2 Tuners at Level 12 total.");
            }

            if (ultimateValid.State.Local.Monsters[0] == null || ultimateValid.State.Local.Monsters[0].Definition.Id != 89604813 || !ultimateValid.State.Local.Monsters[0].SynchroSummoned)
            {
                throw new InvalidOperationException("Synchro summon failed: Blue-Eyes Ultimate Spirit Dragon was not marked as Synchro Summoned on the field.");
            }
        }

        private static void AssertBlueEyesSpiritDragonEffects()
        {
            CleanDuelGame blocked = ReadyCleanGame();
            blocked.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(59822133), CleanPlayerId.Local);
            blocked.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(40908371), CleanPlayerId.Local));
            if (blocked.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Blue-Eyes Spirit Dragon failed: tag-out should require a properly Synchro Summoned copy.");
            }

            CleanDuelGame tagOut = ReadyCleanGame();
            CleanCard spirit = new CleanCard(CleanCardLibrary.Get(59822133), CleanPlayerId.Local);
            spirit.SynchroSummoned = true;
            tagOut.State.Local.Monsters[0] = spirit;
            tagOut.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(40908371), CleanPlayerId.Local));
            if (!tagOut.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Blue-Eyes Spirit Dragon failed: tag-out should be available with Azure-Eyes in the Extra Deck.");
            }

            tagOut.ActivateMonsterZoneCard(0);
            if (tagOut.State.Local.Monsters[0] == null || tagOut.State.Local.Monsters[0].Definition.Id != 40908371 || tagOut.State.Local.Monsters[0].Stance != CleanMonsterStance.Defense)
            {
                throw new InvalidOperationException("Blue-Eyes Spirit Dragon failed: it should Special Summon Azure-Eyes Silver Dragon from the Extra Deck in Defense Position.");
            }

            if (!tagOut.State.Local.Monsters[0].DestroyDuringEndPhase || !tagOut.State.Local.Graveyard.Exists(card => card.Definition.Id == 59822133))
            {
                throw new InvalidOperationException("Blue-Eyes Spirit Dragon failed: Spirit should go to GY and the summoned Synchro should be marked for End Phase destruction.");
            }

            InvokeResolveEndPhaseTriggers(tagOut, tagOut.State.Local);
            if (tagOut.State.Local.Monsters[0] != null || !tagOut.State.Local.Graveyard.Exists(card => card.Definition.Id == 40908371))
            {
                throw new InvalidOperationException("Blue-Eyes Spirit Dragon failed: the summoned Synchro should be destroyed during the End Phase.");
            }

            CleanDuelGame negate = ReadyCleanGame();
            CleanCard negatingSpirit = new CleanCard(CleanCardLibrary.Get(59822133), CleanPlayerId.Local);
            negatingSpirit.SynchroSummoned = true;
            negate.State.Local.Monsters[0] = negatingSpirit;
            negate.State.Rival.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Rival));
            if (InvokeLuaReviveStrongest(negate, negate.State.Rival))
            {
                throw new InvalidOperationException("Blue-Eyes Spirit Dragon failed: the first opposing GY effect should be negated once per turn.");
            }

            if (negate.State.Rival.Monsters[0] != null || !negate.State.Rival.Graveyard.Exists(card => card.Definition.Id == 30576089))
            {
                throw new InvalidOperationException("Blue-Eyes Spirit Dragon failed: a negated GY effect should not move its target.");
            }

            if (!InvokeLuaReviveStrongest(negate, negate.State.Rival) || negate.State.Rival.Monsters[0] == null || negate.State.Rival.Monsters[0].Definition.Id != 30576089)
            {
                throw new InvalidOperationException("Blue-Eyes Spirit Dragon failed: only one opposing GY effect should be negated per turn.");
            }
        }

        private static void AssertLuaDestroyTargetChoiceAllowsManualTarget()
        {
            CleanDuelGame game = ReadyCleanGame();
            game.State.Rival.Hand.Clear();
            game.State.Rival.Deck.Clear();
            game.State.Rival.ExtraDeck.Clear();
            game.State.Rival.Graveyard.Clear();
            game.State.Rival.Banished.Clear();
            game.State.Rival.FieldSpell = null;
            Array.Clear(game.State.Rival.Monsters, 0, game.State.Rival.Monsters.Length);
            Array.Clear(game.State.Rival.SpellsTraps, 0, game.State.Rival.SpellsTraps.Length);
            game.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            game.State.Rival.Monsters[1] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Rival);

            int result = InvokeLuaDestroyStrongest(game, game.State.Rival);
            if (result != 1 || game.State.PendingChoice == null || IndexOfChoiceOption(game, 0) < 0 || IndexOfChoiceOption(game, 1) < 0)
            {
                throw new InvalidOperationException("Lua destroy target choice failed: a local destroy effect should open manual target choices for rival monsters.");
            }

            game.ChoosePendingOption(IndexOfChoiceOption(game, 0));
            if (game.State.PendingChoice != null)
            {
                throw new InvalidOperationException("Lua destroy target choice failed: single-target choice should close after resolving.");
            }

            if (game.State.Rival.Monsters[0] != null || !game.State.Rival.Graveyard.Exists(card => card.Definition.Id == 66961194))
            {
                throw new InvalidOperationException("Lua destroy target choice failed: the chosen weaker monster should be destroyed.");
            }

            if (game.State.Rival.Monsters[1] == null || game.State.Rival.Monsters[1].Definition.Id != 89631139)
            {
                throw new InvalidOperationException("Lua destroy target choice failed: the stronger unchosen monster should stay on the field.");
            }
        }

        private static void AssertBlueEyesUltimateSpiritDragonEffects()
        {
            CleanDuelGame negate = ReadyCleanGame();
            negate.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89604813), CleanPlayerId.Local);
            negate.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            if (!negate.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Blue-Eyes Ultimate Spirit Dragon failed: field-effect negate should be available against a face-up rival effect monster.");
            }

            negate.ActivateMonsterZoneCard(0);
            if (negate.State.Local.Monsters[0].AttackBonus != 1000 || !negate.State.Rival.Monsters[0].EffectsNegated)
            {
                throw new InvalidOperationException("Blue-Eyes Ultimate Spirit Dragon failed: negate should stop the rival field effect and give itself 1000 ATK.");
            }

            if (negate.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Blue-Eyes Ultimate Spirit Dragon failed: negate should be once per turn.");
            }

            CleanDuelGame destroyed = ReadyCleanGame();
            destroyed.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89604813), CleanPlayerId.Local);
            destroyed.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (InvokeLuaDestroyStrongest(destroyed, destroyed.State.Local) != 1)
            {
                throw new InvalidOperationException("Blue-Eyes Ultimate Spirit Dragon failed: destruction test could not destroy the monster.");
            }

            if (destroyed.State.Local.Monsters[0] == null || destroyed.State.Local.Monsters[0].Definition.Id != 89631139 || destroyed.State.Local.Graveyard.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Blue-Eyes Ultimate Spirit Dragon failed: when destroyed by effect it should Special Summon a LIGHT Dragon from the GY.");
            }
        }

        private static bool InvokeLuaCanSynchroSummon(CleanDuelGame game, CleanPlayerState player, int preferredId)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("LuaCanSynchroSummonBestAvailable", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find LuaCanSynchroSummonBestAvailable for Synchro material tests.");
            }

            return (bool)method.Invoke(game, new object[] { player, preferredId });
        }

        private static bool InvokeLuaSynchroSummon(CleanDuelGame game, CleanPlayerState player, int preferredId)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("LuaSynchroSummonBestAvailable", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find LuaSynchroSummonBestAvailable for Synchro material tests.");
            }

            return (bool)method.Invoke(game, new object[] { player, preferredId });
        }

        private static bool InvokeLuaReviveStrongest(CleanDuelGame game, CleanPlayerState player)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("LuaReviveStrongest", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find LuaReviveStrongest for Blue-Eyes Spirit Dragon GY negate tests.");
            }

            return (bool)method.Invoke(game, new object[] { player });
        }

        private static CleanCard TestMonster(int id, string name, int level, string typeLine)
        {
            return new CleanCard(new CleanCardDefinition(id, name, CleanCardKind.Monster, level, 100, 100, typeLine, string.Empty, string.Empty), CleanPlayerId.Local);
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

            CleanDuelGame blackLusterBlocked = ReadyCleanGame();
            blackLusterBlocked.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(70551291), CleanPlayerId.Local));
            blackLusterBlocked.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (blackLusterBlocked.CanNormalSummon(0) || blackLusterBlocked.CanSetMonster(0) || blackLusterBlocked.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Ritual rule failed: Black Luster Soldier - Legendary Swordsman should wait for Chaos Form, not Summon/Set/Activate itself.");
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

            CleanDuelGame graveyardMaterial = ReadyCleanGame();
            graveyardMaterial.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(21082832), CleanPlayerId.Local));
            graveyardMaterial.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(20654247), CleanPlayerId.Local));
            graveyardMaterial.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (!graveyardMaterial.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Chaos Form rule failed: Activate was not available using Blue-Eyes White Dragon from the GY as exact material.");
            }

            graveyardMaterial.ActivateHandCard(0);
            if (graveyardMaterial.State.Local.Monsters[0] == null || graveyardMaterial.State.Local.Monsters[0].Definition.Id != 20654247 || !graveyardMaterial.State.Local.Monsters[0].RitualSummonedUsingBlueEyesWhiteDragon)
            {
                throw new InvalidOperationException("Chaos Form rule failed: Blue-Eyes Chaos Dragon was not Ritual Summoned with Blue-Eyes White Dragon material remembered.");
            }

            if (graveyardMaterial.State.Local.Graveyard.Exists(card => card.Definition.Id == 89631139) || !graveyardMaterial.State.Local.Banished.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Chaos Form rule failed: Blue-Eyes White Dragon from GY should be banished, not sent back to GY.");
            }

            CleanDuelGame blackLusterRitual = ReadyCleanGame();
            blackLusterRitual.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(21082832), CleanPlayerId.Local));
            blackLusterRitual.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(70551291), CleanPlayerId.Local));
            blackLusterRitual.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (!blackLusterRitual.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Chaos Form rule failed: Activate was not available for Black Luster Soldier - Legendary Swordsman with exact Level 8 material.");
            }

            blackLusterRitual.ActivateHandCard(0);
            if (blackLusterRitual.State.Local.Monsters[0] == null || blackLusterRitual.State.Local.Monsters[0].Definition.Id != 70551291)
            {
                throw new InvalidOperationException("Chaos Form rule failed: Black Luster Soldier - Legendary Swordsman was not Ritual Summoned.");
            }

            if (blackLusterRitual.State.Local.Hand.Exists(card => card.Definition.Id == 70551291 || card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Chaos Form rule failed: Black Luster Soldier ritual monster or tribute material stayed in hand.");
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
            game.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(17725109), CleanPlayerId.Local));

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
                throw new InvalidOperationException("True Light set choice failed: set option should be available with listed Spell/Trap cards in deck.");
            }

            game.ChoosePendingOption(1);
            if (game.State.PendingChoice == null || game.State.PendingChoice.Options.Count != 2)
            {
                throw new InvalidOperationException("True Light set choice failed: Spell/Trap selection popup did not open with both valid cards.");
            }

            int roarOption = IndexOfChoiceOption(game, 17725109);
            if (roarOption < 0)
            {
                throw new InvalidOperationException("True Light set choice failed: Roar of the Blue-Eyed Dragons was not offered as a manual choice.");
            }

            game.ChoosePendingOption(roarOption);
            if (game.State.PendingChoice != null)
            {
                throw new InvalidOperationException("True Light set choice failed: Spell/Trap selection popup did not close.");
            }

            if (game.State.Local.SpellsTraps[1] == null || game.State.Local.SpellsTraps[1].Definition.Id != 17725109 || !game.State.Local.SpellsTraps[1].FaceDown)
            {
                throw new InvalidOperationException("True Light set choice failed: chosen Roar of the Blue-Eyed Dragons was not set from deck.");
            }

            if (!game.State.Local.Deck.Exists(card => card.Definition.Id == 71143015))
            {
                throw new InvalidOperationException("True Light set choice failed: unchosen Ultimate Fusion should have stayed in deck.");
            }
        }

        private static void AssertGatewayAndSpiritScreenshotRules()
        {
            CleanDuelGame invalidGateway = ReadyCleanGame();
            invalidGateway.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(40089744), CleanPlayerId.Local));
            invalidGateway.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(21082832), CleanPlayerId.Local));
            if (invalidGateway.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Gateway to Chaos rule failed: a Ritual Spell is not a valid activation search target.");
            }

            CleanDuelGame gaiaGateway = ReadyCleanGame();
            gaiaGateway.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(40089744), CleanPlayerId.Local));
            CleanCardDefinition gaiaDefinition = new CleanCardDefinition(
                99900002,
                "Gaia The Fierce Knight Origin",
                CleanCardKind.Monster,
                4,
                1600,
                1200,
                "Warrior / Effect",
                string.Empty,
                string.Empty);
            gaiaGateway.State.Local.Deck.Add(new CleanCard(gaiaDefinition, CleanPlayerId.Local));
            if (!gaiaGateway.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Gateway to Chaos rule failed: a Gaia The Fierce Knight monster should be searchable.");
            }

            CleanDuelGame choiceGateway = ReadyCleanGame();
            choiceGateway.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(40089744), CleanPlayerId.Local));
            choiceGateway.State.Local.Deck.Add(new CleanCard(gaiaDefinition, CleanPlayerId.Local));
            choiceGateway.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(70551291), CleanPlayerId.Local));
            choiceGateway.ActivateHandCard(0);
            if (choiceGateway.State.PendingChoice == null || IndexOfChoiceOption(choiceGateway, 99900002) < 0 || IndexOfChoiceOption(choiceGateway, 70551291) < 0)
            {
                throw new InvalidOperationException("Gateway to Chaos rule failed: activation must let the player choose between Gaia and Black Luster Soldier targets.");
            }

            choiceGateway.ChoosePendingOption(IndexOfChoiceOption(choiceGateway, 70551291));
            if (!choiceGateway.State.Local.Hand.Exists(card => card.Definition.Id == 70551291) || !choiceGateway.State.Local.Deck.Exists(card => card.Definition.Id == 99900002) || choiceGateway.State.Local.Graveyard.Exists(card => card.Definition.Id == 40089744))
            {
                throw new InvalidOperationException("Gateway to Chaos rule failed: chosen Black Luster Soldier should go to hand while Gateway stays on the field and Gaia stays in Deck.");
            }

            CleanDuelGame dragonRestriction = ReadyCleanGame();
            dragonRestriction.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(42097666), CleanPlayerId.Local);
            dragonRestriction.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Local));
            dragonRestriction.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (dragonRestriction.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Spirit with Eyes of Blue rule failed: it must prevent Special Summoning non-Dragon monsters such as Dictator of D.");
            }

            CleanDuelGame spiritGame = ReadyCleanGame();
            CleanCard spirit = new CleanCard(CleanCardLibrary.Get(42097666), CleanPlayerId.Local) { LinkSummoned = true };
            spiritGame.State.Local.Monsters[0] = spirit;
            spiritGame.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(24382602), CleanPlayerId.Local));
            spiritGame.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            if (!spiritGame.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Spirit with Eyes of Blue rule failed: both screenshot effects should be available.");
            }

            spiritGame.ActivateMonsterZoneCard(0);
            int mausoleumEffect = IndexOfChoiceOption(spiritGame, 1);
            if (mausoleumEffect < 0)
            {
                throw new InvalidOperationException("Spirit with Eyes of Blue rule failed: the Mausoleum effect choice is missing.");
            }

            spiritGame.ChoosePendingOption(mausoleumEffect);
            int sendToGrave = IndexOfChoiceOption(spiritGame, 2);
            if (sendToGrave < 0)
            {
                throw new InvalidOperationException("Spirit with Eyes of Blue rule failed: Send Mausoleum to GY choice is missing.");
            }

            spiritGame.ChoosePendingOption(sendToGrave);
            if (!spiritGame.State.Local.Graveyard.Exists(card => card.Definition.Id == 24382602) || spiritGame.State.Local.Hand.Exists(card => card.Definition.Id == 24382602))
            {
                throw new InvalidOperationException("Spirit with Eyes of Blue rule failed: Mausoleum should be sent to the chosen GY destination.");
            }

            spiritGame.ActivateMonsterZoneCard(0);
            int jetFromGrave = IndexOfChoiceOption(spiritGame, 30576089);
            if (jetFromGrave < 0)
            {
                throw new InvalidOperationException("Spirit with Eyes of Blue rule failed: Blue-Eyes Jet Dragon in the GY is not selectable.");
            }

            spiritGame.ChoosePendingOption(jetFromGrave);
            CleanCard summonedJet = spiritGame.State.Local.Monsters[0];
            if (summonedJet == null || summonedJet.Definition.Id != 30576089 || !summonedJet.EffectsNegated || !summonedJet.CannotAttack ||
                !spiritGame.State.Local.Graveyard.Exists(card => card.Definition.Id == 42097666))
            {
                throw new InvalidOperationException("Spirit with Eyes of Blue rule failed: GY Effect Monster restrictions or tribute cost are incorrect.");
            }

            CleanDuelGame handSummon = ReadyCleanGame();
            handSummon.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(42097666), CleanPlayerId.Local);
            handSummon.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            handSummon.ActivateMonsterZoneCard(0);
            int blueEyesFromHand = IndexOfChoiceOption(handSummon, -89631139);
            if (blueEyesFromHand < 0)
            {
                throw new InvalidOperationException("Spirit with Eyes of Blue rule failed: Blue-Eyes White Dragon in hand is not selectable.");
            }

            handSummon.ChoosePendingOption(blueEyesFromHand);
            if (handSummon.State.Local.Monsters[0] == null || handSummon.State.Local.Monsters[0].Definition.Id != 89631139 || handSummon.State.Local.Monsters[0].CannotAttack)
            {
                throw new InvalidOperationException("Spirit with Eyes of Blue rule failed: a monster summoned from hand must not receive the GY restriction.");
            }
        }
        private static void AssertDocumentTriggeredEffects()
        {
            CleanDuelGame stone = ReadyCleanGame();
            stone.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(71039903), CleanPlayerId.Local) { SentToGraveThisTurn = true });
            stone.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            InvokeResolveEndPhaseTriggers(stone, stone.State.Local);
            if (stone.State.Local.Monsters[0] == null || stone.State.Local.Monsters[0].Definition.Id != 30576089 || stone.State.Local.Deck.Count != 0)
            {
                throw new InvalidOperationException("White Stone of Ancients failed: End Phase trigger should Special Summon a Blue-Eyes monster from Deck.");
            }

            CleanDuelGame gateway = ReadyCleanGame();
            gateway.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(40089744), CleanPlayerId.Local));
            gateway.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(70551291), CleanPlayerId.Local));
            gateway.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(21082832), CleanPlayerId.Local));
            gateway.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(21082832), CleanPlayerId.Local));
            gateway.ActivateHandCard(0);
            if (gateway.State.PendingChoice == null || IndexOfChoiceOption(gateway, 70551291) < 0)
            {
                throw new InvalidOperationException("Gateway to Chaos failed: it should offer Black Luster Soldier/Gaia as the activation search.");
            }

            gateway.ChoosePendingOption(IndexOfChoiceOption(gateway, 70551291));
            if (gateway.State.Local.FieldSpell == null || gateway.State.Local.FieldSpell.Definition.Id != 40089744 || !gateway.State.Local.Hand.Exists(card => card.Definition.Id == 70551291) || gateway.State.Local.Graveyard.Exists(card => card.Definition.Id == 40089744))
            {
                throw new InvalidOperationException("Gateway to Chaos failed: it should stay as a Field Spell and add the chosen Black Luster Soldier/Gaia target.");
            }

            for (int i = 0; i < 7; i++)
            {
                gateway.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local);
                InvokeMoveMonsterToGrave(gateway, gateway.State.Local, 0);
            }

            if (gateway.State.Local.GatewayToChaosCounters != 6 || !gateway.CanActivateSetCard(CleanDuelGame.FieldSpellZoneIndex))
            {
                throw new InvalidOperationException("Gateway to Chaos failed: counters should cap at 6 and unlock the Ritual Spell search at 3.");
            }

            gateway.ActivateSetCard(CleanDuelGame.FieldSpellZoneIndex);
            if (gateway.State.Local.GatewayToChaosCounters != 3 || !gateway.State.Local.Hand.Exists(card => card.Definition.Id == 21082832) || gateway.CanActivateSetCard(CleanDuelGame.FieldSpellZoneIndex))
            {
                throw new InvalidOperationException("Gateway to Chaos failed: remove exactly 3 counters and allow the Ritual Spell search only once per turn.");
            }

            gateway.EndTurn();
            if (gateway.State.Local.GatewayToChaosCounters != 3)
            {
                throw new InvalidOperationException("Gateway to Chaos failed: Spell Counters must remain on the face-up Field Spell after the turn ends.");
            }

            CleanDuelGame roar = ReadyCleanGame();
            roar.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(17725109), CleanPlayerId.Local));
            roar.State.Local.Banished.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (!roar.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Roar failed: activation should be available with Blue-Eyes White Dragon banished.");
            }

            roar.ActivateHandCard(0);
            if (roar.State.Local.Monsters[0] == null || roar.State.Local.Monsters[0].Definition.Id != 89631139 || roar.State.Local.Monsters[0].Stance != CleanMonsterStance.Defense || roar.State.Local.Banished.Count != 0)
            {
                throw new InvalidOperationException("Roar failed: it should Special Summon Blue-Eyes White Dragon from the banished pile in Defense Position.");
            }

            CleanDuelGame maiden = ReadyCleanGame();
            maiden.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(88241506), CleanPlayerId.Local);
            maiden.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            maiden.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            InvokeResolveBattle(maiden, maiden.State.Rival, maiden.State.Local, 0, 0);
            if (maiden.State.Local.Monsters[0] == null || maiden.State.Local.Monsters[0].Definition.Id != 88241506 || maiden.State.Local.Monsters[1] == null || maiden.State.Local.Monsters[1].Definition.Id != 89631139 || !maiden.State.Rival.Monsters[0].HasBattled)
            {
                throw new InvalidOperationException("Maiden with Eyes of Blue failed: attack target trigger should negate the attack and summon Blue-Eyes White Dragon.");
            }

            CleanDuelGame maidenWhite = ReadyCleanGame();
            maidenWhite.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local);
            maidenWhite.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local));
            maidenWhite.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            InvokeResolveBattle(maidenWhite, maidenWhite.State.Rival, maidenWhite.State.Local, 0, 0);
            if (maidenWhite.State.Local.Monsters[0] == null || maidenWhite.State.Local.Monsters[0].Definition.Id != 17947697 || maidenWhite.State.Local.Monsters[1] == null || maidenWhite.State.Local.Monsters[1].Definition.Id != 8240199 || maidenWhite.State.Local.Graveyard.Exists(card => card.Definition.Id == 8240199))
            {
                throw new InvalidOperationException("Maiden of White failed: attack target trigger should negate the attack and revive Blue-Eyes White Dragon or a Level 1 LIGHT Tuner from the GY.");
            }
        }

        private static void AssertEffectTimingRules()
        {
            CleanDuelGame speed = ReadyCleanGame();
            CleanCard veiler = new CleanCard(CleanCardLibrary.Get(97268402), CleanPlayerId.Local);
            CleanCard sage = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local);
            CleanCard maiden = new CleanCard(CleanCardLibrary.Get(88241506), CleanPlayerId.Local);
            CleanCard jet = new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local);
            if (speed.GetEffectTiming(veiler) != CleanEffectTiming.Quick || speed.GetSpellSpeed(veiler) != 2)
            {
                throw new InvalidOperationException("Effect timing failed: Effect Veiler should be a Spell Speed 2 Quick Effect.");
            }

            if (speed.GetEffectTiming(sage) != CleanEffectTiming.Ignition || speed.GetSpellSpeed(sage) != 1)
            {
                throw new InvalidOperationException("Effect timing failed: Sage hand effect should be a Spell Speed 1 activated monster effect in the clean model.");
            }

            if (speed.GetEffectTiming(maiden) != CleanEffectTiming.Trigger || speed.GetSpellSpeed(maiden) != 1)
            {
                throw new InvalidOperationException("Effect timing failed: Maiden with Eyes of Blue should be treated as a Trigger Effect, not a free Activate button.");
            }

            if (!speed.HasContinuousEffect(jet) || speed.GetEffectTiming(jet) != CleanEffectTiming.Continuous || speed.GetSpellSpeed(jet) != 0)
            {
                throw new InvalidOperationException("Effect timing failed: Blue-Eyes Jet Dragon's protection should be continuous, not an activated Spell Speed effect.");
            }

            CleanDuelGame quick = ReadyCleanGame();
            quick.State.TurnPlayer = CleanPlayerId.Rival;
            quick.State.Phase = CleanDuelPhase.Main;
            quick.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(97268402), CleanPlayerId.Local));
            quick.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            if (quick.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Quick Effect rule failed: normal hand activation should stay blocked during the opponent's turn.");
            }

            if (!quick.CanActivateLocalQuickEffectHandCard(0))
            {
                throw new InvalidOperationException("Quick Effect rule failed: a legal Spell Speed 2 monster effect should be usable during the opponent's turn.");
            }

            quick.ActivateLocalQuickEffectHandCard(0);
            if (!quick.State.Local.Graveyard.Exists(card => card.Definition.Id == 97268402) || !quick.State.Rival.Monsters[0].EffectsNegated)
            {
                throw new InvalidOperationException("Quick Effect rule failed: Effect Veiler should resolve from hand and negate the rival monster.");
            }

            CleanDuelGame quickPrompt = ReadyCleanGame();
            quickPrompt.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(23434538), CleanPlayerId.Local));
            quickPrompt.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            quickPrompt.State.Rival.Hand.Clear();
            quickPrompt.State.Rival.Deck.Clear();
            quickPrompt.State.Rival.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Rival));
            quickPrompt.EndTurn();
            if (quickPrompt.State.PendingChoice == null || quickPrompt.State.PendingChoice.Title != "Quick Effect")
            {
                throw new InvalidOperationException("Quick Effect rule failed: opponent turn should pause for an available Spell Speed 2 monster effect.");
            }

            CleanDuelGame trigger = ReadyCleanGame();
            trigger.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(88241506), CleanPlayerId.Local);
            trigger.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (trigger.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Trigger Effect rule failed: Maiden with Eyes of Blue should not have a generic Activate button.");
            }

            trigger.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            InvokeResolveBattle(trigger, trigger.State.Rival, trigger.State.Local, 0, 0);
            if (trigger.State.Local.Monsters[1] == null || trigger.State.Local.Monsters[1].Definition.Id != 89631139)
            {
                throw new InvalidOperationException("Trigger Effect rule failed: Maiden with Eyes of Blue should still trigger from the attack event.");
            }

            CleanDuelGame continuous = ReadyCleanGame();
            continuous.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            continuous.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local);
            if (continuous.CanActivateMonsterZoneCard(1))
            {
                throw new InvalidOperationException("Continuous Effect rule failed: Jet Dragon's protection must not appear as a generic field Activate button.");
            }

            InvokeDestroyStrongestFaceUpCard(continuous, continuous.State.Local);
            if (continuous.State.Local.Monsters[0] == null || continuous.State.Local.Monsters[0].Definition.Id != 89631139 || !continuous.State.Local.Graveyard.Exists(card => card.Definition.Id == 30576089))
            {
                throw new InvalidOperationException("Continuous Effect rule failed: face-up Jet Dragon should continuously protect the other controlled card.");
            }

            CleanDuelGame negatedContinuous = ReadyCleanGame();
            negatedContinuous.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            negatedContinuous.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local) { EffectsNegated = true };
            InvokeDestroyStrongestFaceUpCard(negatedContinuous, negatedContinuous.State.Local);
            if (!negatedContinuous.State.Local.Graveyard.Exists(card => card.Definition.Id == 89631139) || negatedContinuous.State.Local.Monsters[1] == null)
            {
                throw new InvalidOperationException("Continuous Effect rule failed: negated Jet Dragon should stop protecting other controlled cards.");
            }

            CleanDuelGame spiritRestriction = ReadyCleanGame();
            spiritRestriction.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(42097666), CleanPlayerId.Local);
            spiritRestriction.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Local));
            spiritRestriction.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (spiritRestriction.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Continuous Effect rule failed: Spirit with Eyes of Blue should continuously block non-Dragon Special Summons while face-up.");
            }

            spiritRestriction.State.Local.Monsters[0].EffectsNegated = true;
            if (!spiritRestriction.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Continuous Effect rule failed: negated Spirit with Eyes of Blue should stop applying its restriction.");
            }
        }

        private static void AssertFlipAndLinkRules()
        {
            CleanCardDefinition flipDefinition = new CleanCardDefinition(
                99001111,
                "Codex Flip Apprentice",
                CleanCardKind.Monster,
                4,
                100,
                100,
                "Spellcaster / Flip / Effect",
                "FLIP: Draw 1 card.",
                "flip_draw_clean");

            CleanDuelGame flip = ReadyCleanGame();
            flip.State.Local.Hand.Add(new CleanCard(flipDefinition, CleanPlayerId.Local));
            flip.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            flip.SetMonster(0);
            flip.State.Local.Monsters[0].SummonedThisTurn = false;
            if (flip.GetEffectTiming(flip.State.Local.Monsters[0]) != CleanEffectTiming.Trigger || flip.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Flip rule failed: FLIP effects should be Trigger Effects and should not expose a generic Activate button.");
            }

            if (!flip.CanFlipSummonMonster(0))
            {
                throw new InvalidOperationException("Flip rule failed: a set monster from a previous turn should be legal to Flip Summon.");
            }

            flip.FlipSummonMonster(0);
            if (flip.State.Local.Monsters[0].FaceDown || flip.State.Local.Monsters[0].Stance != CleanMonsterStance.Attack || !flip.State.Local.Hand.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Flip rule failed: Flip Summon should flip face-up attack and resolve the FLIP trigger.");
            }

            CleanDuelGame battleFlip = ReadyCleanGame();
            battleFlip.State.Local.Monsters[0] = new CleanCard(flipDefinition, CleanPlayerId.Local) { FaceDown = true, Stance = CleanMonsterStance.Defense };
            battleFlip.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            battleFlip.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            InvokeResolveBattle(battleFlip, battleFlip.State.Rival, battleFlip.State.Local, 0, 0);
            if (!battleFlip.State.Local.Hand.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Flip rule failed: a face-down monster attacked in battle should flip face-up and activate its FLIP trigger.");
            }

            CleanDuelGame link = ReadyCleanGame();
            link.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(42097666), CleanPlayerId.Local));
            link.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(71039903), CleanPlayerId.Local);
            link.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(24382602), CleanPlayerId.Local));
            if (!link.CanLinkSummonFromExtra(0))
            {
                throw new InvalidOperationException("Link rule failed: Spirit with Eyes of Blue should be Link Summonable with 1 Level 4 or lower Dragon material.");
            }

            link.LinkSummonFromExtra(0);
            if (link.State.ExtraMonsters[0] == null || link.State.ExtraMonsters[0].Definition.Id != 42097666 || !link.State.ExtraMonsters[0].LinkSummoned || !link.State.Local.Graveyard.Exists(card => card.Definition.Id == 71039903))
            {
                throw new InvalidOperationException("Link rule failed: Link Summon should place Spirit with Eyes of Blue in the Extra Monster Zone and send material to the GY.");
            }

            if (link.State.PendingChoice == null || !link.IsLocalMainMonsterZoneLinkedForExtraDeckSummon(2))
            {
                throw new InvalidOperationException("Link rule failed: Link Summoned Spirit should open its summon trigger and point to a linked Main Monster Zone.");
            }

            link.ChoosePendingOption(IndexOfChoiceOption(link, 1));
            link.State.Local.Hand.Clear();
            link.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Local));
            link.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (link.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Link rule failed: Spirit's continuous Dragon-only Special Summon restriction should apply from the Extra Monster Zone.");
            }

            link.State.ExtraMonsters[0].EffectsNegated = true;
            if (!link.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Link rule failed: a negated Link Monster should stop applying its continuous linked-field restriction.");
            }

            CleanDuelGame linkedSummon = ReadyCleanGame();
            linkedSummon.State.ExtraMonsters[0] = new CleanCard(CleanCardLibrary.Get(42097666), CleanPlayerId.Local) { LinkSummoned = true };
            linkedSummon.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(42097666), CleanPlayerId.Local));
            linkedSummon.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(71039903), CleanPlayerId.Local);
            if (!linkedSummon.CanLinkSummonFromExtra(0))
            {
                throw new InvalidOperationException("Link rule failed: a Link Arrow should open a Main Monster Zone for another Extra Deck Link Summon.");
            }

            linkedSummon.LinkSummonFromExtra(0);
            if (linkedSummon.State.Local.Monsters[2] == null || linkedSummon.State.Local.Monsters[2].Definition.Id != 42097666 || !linkedSummon.State.Local.Monsters[2].LinkSummoned)
            {
                throw new InvalidOperationException("Link rule failed: second Link Summon should use the linked Main Monster Zone.");
            }
        }

        private static void AssertPendulumRules()
        {
            CleanCardDefinition scaleOne = new CleanCardDefinition(
                99002101,
                "Codex Low Scale",
                CleanCardKind.Monster,
                4,
                1200,
                800,
                "Spellcaster / Pendulum / Effect / Scale 1",
                "Pendulum Scale 1. Pendulum Effect: Test low scale.",
                string.Empty);
            CleanCardDefinition scaleEight = new CleanCardDefinition(
                99002108,
                "Codex High Scale",
                CleanCardKind.Monster,
                4,
                1200,
                800,
                "Spellcaster / Pendulum / Effect / Scale 8",
                "Pendulum Scale 8. Pendulum Effect: Test high scale.",
                string.Empty);
            CleanCardDefinition levelFour = new CleanCardDefinition(
                99002104,
                "Codex Level Four",
                CleanCardKind.Monster,
                4,
                1600,
                1200,
                "Dragon / Effect",
                "Regular monster used for Pendulum Summon testing.",
                string.Empty);
            CleanCardDefinition faceUpExtra = new CleanCardDefinition(
                99002105,
                "Codex Face-Up Extra Pendulum",
                CleanCardKind.Monster,
                5,
                1800,
                1000,
                "Warrior / Pendulum / Effect / Scale 3",
                "Pendulum Scale 3. Face-up Extra Deck test card.",
                string.Empty);

            CleanDuelGame game = ReadyCleanGame();
            game.State.Local.Hand.Add(new CleanCard(scaleOne, CleanPlayerId.Local));
            game.State.Local.Hand.Add(new CleanCard(scaleEight, CleanPlayerId.Local));
            if (!game.CanActivatePendulumFromHand(0) || !game.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Pendulum rule failed: a Pendulum Monster in hand should be activatable as a Pendulum Spell.");
            }

            game.ActivateHandCard(0);
            if (game.State.Local.SpellsTraps[0] == null || game.State.Local.SpellsTraps[0].Definition.Id != 99002101 || game.State.Local.SpellsTraps[0].FaceDown)
            {
                throw new InvalidOperationException("Pendulum rule failed: first Pendulum activation should use the left Pendulum Zone face-up.");
            }

            game.ActivateHandCard(0);
            if (game.State.Local.SpellsTraps[CleanPlayerState.ZoneCount - 1] == null || game.State.Local.SpellsTraps[CleanPlayerState.ZoneCount - 1].Definition.Id != 99002108)
            {
                throw new InvalidOperationException("Pendulum rule failed: second Pendulum activation should use the right Pendulum Zone.");
            }

            game.State.Local.Hand.Add(new CleanCard(levelFour, CleanPlayerId.Local));
            game.State.Local.ExtraDeck.Add(new CleanCard(faceUpExtra, CleanPlayerId.Local) { FaceUpExtraDeck = true });
            if (!game.CanPendulumSummon())
            {
                throw new InvalidOperationException("Pendulum rule failed: scales 1 and 8 should allow Levels 2 through 7 to be Pendulum Summoned.");
            }

            game.PendulumSummon();
            if (game.State.PendingChoice == null)
            {
                throw new InvalidOperationException("Pendulum rule failed: Pendulum Summon should open a target choice.");
            }

            game.ChoosePendingOption(IndexOfChoiceOption(game, 1000));
            if (game.State.Local.Monsters[0] == null || game.State.Local.Monsters[0].Definition.Id != 99002104)
            {
                throw new InvalidOperationException("Pendulum rule failed: a legal monster should be Pendulum Summoned from the hand to a Main Monster Zone.");
            }

            if (game.State.PendingChoice == null)
            {
                throw new InvalidOperationException("Pendulum rule failed: choice should remain open while more legal Pendulum targets exist.");
            }

            game.ChoosePendingOption(IndexOfChoiceOption(game, 2000));
            if (game.State.ExtraMonsters[0] == null || game.State.ExtraMonsters[0].Definition.Id != 99002105 || game.State.ExtraMonsters[0].FaceUpExtraDeck)
            {
                throw new InvalidOperationException("Pendulum rule failed: face-up Pendulum Monsters from the Extra Deck should summon to the Extra Monster Zone when it is open.");
            }

            if (!game.State.Local.PendulumSummonUsed || game.CanPendulumSummon())
            {
                throw new InvalidOperationException("Pendulum rule failed: Pendulum Summon should be once per turn.");
            }

            CleanDuelGame sendToExtra = ReadyCleanGame();
            sendToExtra.State.Local.SpellsTraps[0] = new CleanCard(scaleOne, CleanPlayerId.Local);
            InvokeMoveSpellTrapToGrave(sendToExtra, sendToExtra.State.Local, 0);
            if (sendToExtra.State.Local.Graveyard.Count != 0 || sendToExtra.State.Local.ExtraDeck.Count != 1 || !sendToExtra.State.Local.ExtraDeck[0].FaceUpExtraDeck)
            {
                throw new InvalidOperationException("Pendulum rule failed: a Pendulum card in a Pendulum Zone should go face-up to the Extra Deck instead of the GY.");
            }

            sendToExtra.State.Local.Monsters[0] = new CleanCard(scaleEight, CleanPlayerId.Local);
            InvokeMoveMonsterToGrave(sendToExtra, sendToExtra.State.Local, 0);
            if (sendToExtra.State.Local.Graveyard.Count != 0 || sendToExtra.State.Local.ExtraDeck.Count != 2 || !sendToExtra.State.Local.ExtraDeck[1].FaceUpExtraDeck)
            {
                throw new InvalidOperationException("Pendulum rule failed: a Pendulum Monster on the field should go face-up to the Extra Deck instead of the GY.");
            }
        }
        private static void AssertKnowledgeAuditBuilds()
        {
            Type auditType = typeof(CleanDuelLuaSmokeTest).Assembly.GetType("IcarusGame.EditorTools.CleanDuelKnowledgeAudit");
            if (auditType == null)
            {
                return;
            }

            MethodInfo method = auditType.GetMethod("BuildReport", BindingFlags.Static | BindingFlags.Public);
            if (method == null)
            {
                throw new InvalidOperationException("Knowledge audit failed: BuildReport method is missing.");
            }

            string report = method.Invoke(null, null) as string;
            if (string.IsNullOrEmpty(report) ||
                report.IndexOf("Gateway to Chaos", StringComparison.OrdinalIgnoreCase) < 0 ||
                report.IndexOf("Black Luster Soldier - Legendary Swordsman", StringComparison.OrdinalIgnoreCase) < 0 ||
                report.IndexOf("Lua source", StringComparison.OrdinalIgnoreCase) < 0)
            {
                throw new InvalidOperationException("Knowledge audit failed: expected Gateway, Black Luster Soldier, and Lua source context.");
            }
        }

        private static void InvokeResolveEndPhaseTriggers(CleanDuelGame game, CleanPlayerState player)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("ResolveEndPhaseTriggers", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find ResolveEndPhaseTriggers for document trigger tests.");
            }

            method.Invoke(game, new object[] { player });
        }

        private static void InvokeResolveBattle(CleanDuelGame game, CleanPlayerState attackingPlayer, CleanPlayerState defendingPlayer, int attackerZone, int defenderZone)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("ResolveBattle", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find ResolveBattle for document trigger tests.");
            }

            method.Invoke(game, new object[] { attackingPlayer, defendingPlayer, attackerZone, defenderZone });
        }

        private static void InvokeMoveMonsterToGrave(CleanDuelGame game, CleanPlayerState player, int zone)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("MoveMonsterToGrave", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find MoveMonsterToGrave for Gateway counter tests.");
            }

            method.Invoke(game, new object[] { player, zone });
        }

        private static void InvokeMoveSpellTrapToGrave(CleanDuelGame game, CleanPlayerState player, int zone)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("MoveSpellTrapToGrave", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find MoveSpellTrapToGrave for Pendulum tests.");
            }

            method.Invoke(game, new object[] { player, zone });
        }

        private static int IndexOfChoiceOption(CleanDuelGame game, int optionId)
        {
            if (game.State.PendingChoice == null)
            {
                return -1;
            }

            for (int i = 0; i < game.State.PendingChoice.Options.Count; i++)
            {
                if (game.State.PendingChoice.Options[i].Id == optionId)
                {
                    return i;
                }
            }

            return -1;
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

        private static void AssertTrueLightLeavingFieldDestroysOwnMonsters()
        {
            CleanDuelGame game = ReadyCleanGame();
            game.State.Local.SpellsTraps[0] = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local);
            game.State.Local.SpellsTraps[0].FaceDown = false;
            game.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            game.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local);

            if (!InvokeDestroyStrongestFaceUpCard(game, game.State.Local))
            {
                throw new InvalidOperationException("True Light leave-field rule failed: face-up True Light was not destroyed.");
            }

            if (game.State.Local.SpellsTraps[0] != null || !game.State.Local.Graveyard.Exists(card => card.Definition.Id == 62089826))
            {
                throw new InvalidOperationException("True Light leave-field rule failed: True Light did not go to the GY.");
            }

            if (game.State.Local.Monsters[0] != null || game.State.Local.Monsters[1] != null)
            {
                throw new InvalidOperationException("True Light leave-field rule failed: own monsters were not destroyed.");
            }

            if (!game.State.Local.Graveyard.Exists(card => card.Definition.Id == 89631139) || !game.State.Local.Graveyard.Exists(card => card.Definition.Id == 17947697))
            {
                throw new InvalidOperationException("True Light leave-field rule failed: destroyed monsters were not sent to the GY.");
            }
        }

        private static bool InvokeDestroyStrongestFaceUpCard(CleanDuelGame game, CleanPlayerState player)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("DestroyStrongestFaceUpCard", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find DestroyStrongestFaceUpCard for True Light leave-field test.");
            }

            return (bool)method.Invoke(game, new object[] { player });
        }
        private static void AssertPhaseFlow()
        {
            CleanDuelGame game = new CleanDuelGame();
            game.StartNewDuel();
            if (game.State.Phase != CleanDuelPhase.Main)
            {
                throw new InvalidOperationException("Phase flow failed: Draw should automatically advance to Main Phase 1 after the opening draw.");
            }

            game.AdvancePhase();
            if (game.State.Phase != CleanDuelPhase.Battle)
            {
                throw new InvalidOperationException("Phase flow failed: Main Phase 1 should advance to Battle Phase.");
            }

            game.AdvancePhase();
            if (game.State.Phase != CleanDuelPhase.Battle || game.State.PendingChoice == null || game.State.PendingChoice.Options.Count != 2)
            {
                throw new InvalidOperationException("Phase flow failed: Battle Phase should open a Main Phase 2 / End Turn choice.");
            }

            game.ChoosePendingOption(0);
            if (game.State.PendingChoice != null || game.State.Phase != CleanDuelPhase.Main2)
            {
                throw new InvalidOperationException("Phase flow failed: choosing Main Phase 2 did not enter Main Phase 2.");
            }

            CleanDuelGame endFromMain2 = new CleanDuelGame();
            endFromMain2.StartNewDuel();
            endFromMain2.State.TurnPlayer = CleanPlayerId.Rival;
            endFromMain2.State.Phase = CleanDuelPhase.Main2;
            endFromMain2.State.Local.Hand.Clear();
            endFromMain2.State.Local.Deck.Clear();
            endFromMain2.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            endFromMain2.AdvancePhase();
            if (endFromMain2.State.TurnPlayer != CleanPlayerId.Local || endFromMain2.State.Phase != CleanDuelPhase.Main || endFromMain2.State.Local.Hand.Count != 1)
            {
                throw new InvalidOperationException("Phase flow failed: Main Phase 2 should go to End Phase, end the turn, draw, and auto-enter Main Phase 1.");
            }

            CleanDuelGame endFromBattle = new CleanDuelGame();
            endFromBattle.StartNewDuel();
            endFromBattle.State.TurnPlayer = CleanPlayerId.Rival;
            endFromBattle.State.Phase = CleanDuelPhase.Battle;
            endFromBattle.State.Local.Hand.Clear();
            endFromBattle.State.Local.Deck.Clear();
            endFromBattle.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            endFromBattle.AdvancePhase();
            if (endFromBattle.State.PendingChoice == null || endFromBattle.State.PendingChoice.Options.Count != 2)
            {
                throw new InvalidOperationException("Phase flow failed: Battle Phase end-turn choice did not open.");
            }

            endFromBattle.ChoosePendingOption(1);
            if (endFromBattle.State.PendingChoice != null || endFromBattle.State.TurnPlayer != CleanPlayerId.Local || endFromBattle.State.Phase != CleanDuelPhase.Main || endFromBattle.State.Local.Hand.Count != 1)
            {
                throw new InvalidOperationException("Phase flow failed: Battle Phase End Turn choice should pass the turn and auto-enter Main Phase 1 after draw.");
            }
        }
        private static void AssertDeckOutRules()
        {
            CleanDuelGame turnDraw = new CleanDuelGame();
            turnDraw.StartNewDuel();
            turnDraw.State.TurnPlayer = CleanPlayerId.Rival;
            turnDraw.State.Phase = CleanDuelPhase.Main2;
            turnDraw.State.Local.Hand.Clear();
            turnDraw.State.Local.Deck.Clear();

            turnDraw.AdvancePhase();
            if (!turnDraw.State.GameOver || turnDraw.State.WinnerText != "Messenger of Justice wins by deck-out." || turnDraw.State.Local.LifePoints != 0)
            {
                throw new InvalidOperationException("Deck-out rule failed: player should lose immediately when the automatic turn draw finds an empty Deck.");
            }

            CleanDuelGame effectDraw = ReadyCleanGame();
            effectDraw.State.Local.Deck.Clear();
            InvokeLuaDraw(effectDraw, effectDraw.State.Local, 1);
            if (!effectDraw.State.GameOver || effectDraw.State.WinnerText != "Messenger of Justice wins by deck-out." || effectDraw.State.Local.LifePoints != 0)
            {
                throw new InvalidOperationException("Deck-out rule failed: effect draw should also lose when the Deck is empty.");
            }
        }

        private static void InvokeLuaDraw(CleanDuelGame game, CleanPlayerState player, int count)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("LuaDraw", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find LuaDraw for deck-out test.");
            }

            method.Invoke(game, new object[] { player, count });
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
            game.State.Local.Hand.Clear();
            game.State.Local.Deck.Clear();
            game.State.Local.ExtraDeck.Clear();
            game.State.Local.Graveyard.Clear();
            game.State.Local.Banished.Clear();
            game.State.Local.FieldSpell = null;
            game.State.Local.GatewayToChaosCounters = 0;
            game.State.Local.ExtraDeckDragonOnlyThisTurn = false;
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

        private static void AssertCardCount(List<CleanCard> cards, int id, int expected, string name)
        {
            int count = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].Definition.Id == id)
                {
                    count++;
                }
            }

            if (count != expected)
            {
                throw new InvalidOperationException("Deck list mismatch for " + name + ": expected " + expected + ", got " + count + ".");
            }
        }

        private static void AssertCardType(List<CleanCard> cards, int id, CleanCardKind expectedKind, string expectedTypeText, string name)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].Definition.Id != id)
                {
                    continue;
                }

                if (cards[i].Definition.Kind != expectedKind || cards[i].Definition.TypeLine.IndexOf(expectedTypeText, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    throw new InvalidOperationException("Card type mismatch for " + name + ": expected " + expectedKind + " / " + expectedTypeText + ", got " + cards[i].Definition.Kind + " / " + cards[i].Definition.TypeLine + ".");
                }

                return;
            }

            throw new InvalidOperationException("Card type mismatch for " + name + ": card not found.");
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
