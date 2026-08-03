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

            if (game.State.Local.ExtraDeck.Count != 12)
            {
                throw new InvalidOperationException("Blue-Eyes deck test failed: expected 12 cards in the Extra Deck after adding the Rank 8 Xyz choices.");
            }

            AssertScreenshotDeckListFromSummary();
            AssertEveryCardHasCleanEffect(CleanCardLibrary.BuildDeck(CleanPlayerId.Local), "Main Deck");
            AssertEveryCardHasCleanEffect(CleanCardLibrary.BuildExtraDeck(CleanPlayerId.Local), "Extra Deck");
            AssertOfficialCardTextLoaded();
            AssertLuaScriptsAreDiagnosticsOnly();
            AssertLuaDestroyTargetChoiceAllowsManualTarget();
            AssertWishesSearchesBlueEyes();
            AssertDocumentSpellEffects();
            AssertDocumentMonsterEffects();
            AssertCorrectedDeckEffectSemantics();
            AssertGatewayAndSpiritScreenshotRules();
            AssertKnowledgeAuditBuilds();
            AssertPromptedTriggerAndSpecialSummonFlow();
            AssertPromptedManualCardEffectFlow();
            AssertEffectTimingRules();
            AssertSpellTrapRulebookTiming();
            AssertOpponentTurnTrapResponsePrompts();
            AssertFlipAndLinkRules();
            AssertXyzRules();
            AssertXyzExtraDeckChoiceFlow();
            AssertPendulumRules();
            AssertUnifiedSummonChoiceFlows();
            AssertDocumentTriggeredEffects();
            AssertMaidenFromHandPlacesTrueLight();
            AssertMaidenFromFieldPlacesTrueLight();
            AssertMaidenReviveAfterTrueLightIsOptional();
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
            AssertDuelRulebookBasics();
            AssertDeckOutRules();
            AssertNormalMonsterHasNoActivationAndUsesTributes();
            Debug.Log("CleanDuel deck smoke test passed. Main=41, Extra=12, filtered Fusion/Ritual/Synchro/Xyz/Pendulum/Link choices, stacked Xyz materials, clean activations, real fusion summons, rulebook Spell/Trap timing, ritual summons, Flip/Trigger rules, Link Summon rules, Synchro Summon rules, Xyz Summon rules, Pendulum rules, True Light rules, phase flow, first-turn battle lock, battle position rules, draw result, card-effect win, deck-out, targeting protection, and tribute rules verified.");
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
            AssertCardCount(extra, 63767246, 1, "Number 38: Hope Harbinger Dragon Titanic Galaxy");
            AssertCardCount(extra, 18963306, 1, "Galaxy-Eyes Cipher Dragon");
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
            ConfirmSpecialSummonPrompt(roar, "Roar");
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
            if (majesty.State.PendingChoice == null || majesty.State.PendingChoice.SourceCardId != -27836611)
            {
                throw new InvalidOperationException("Majesty failed: it should ask which Blue-Eyes monster to send.");
            }

            majesty.ChoosePendingOption(0);
            if (majesty.State.PendingChoice == null || IndexOfChoiceOption(majesty, 100) < 0)
            {
                throw new InvalidOperationException("Majesty failed: it should ask which face-up monster will be unable to attack.");
            }

            majesty.ChoosePendingOption(IndexOfChoiceOption(majesty, 100));
            if (!majesty.State.Local.Graveyard.Exists(card => card.Definition.Id == 30576089) || !majesty.State.Rival.Monsters[0].CannotAttack)
            {
                throw new InvalidOperationException("Majesty failed: it should send the chosen Blue-Eyes monster to the GY and prevent the chosen target from attacking.");
            }
        }

        private static void AssertDocumentMonsterEffects()
        {
            CleanDuelGame maxx = ReadyCleanGame();
            maxx.State.TurnPlayer = CleanPlayerId.Rival;
            maxx.State.Phase = CleanDuelPhase.Main;
            maxx.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(23434538), CleanPlayerId.Local));
            maxx.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (!maxx.CanActivateLocalQuickEffectHandCard(0))
            {
                throw new InvalidOperationException("Maxx C failed: it should be activatable from hand during the opponent's turn.");
            }

            maxx.ActivateLocalQuickEffectHandCard(0);
            if (maxx.State.Local.Hand.Exists(card => card.Definition.Id == 89631139) ||
                !maxx.State.Local.Graveyard.Exists(card => card.Definition.Id == 23434538) ||
                !maxx.State.Local.MaxxCDrawActive)
            {
                throw new InvalidOperationException("Maxx C failed: activation should send it to the GY and arm its draw listener without drawing immediately.");
            }

            InvokeNotifySpecialSummon(maxx, maxx.State.Rival);
            if (!maxx.State.Local.Hand.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Maxx C failed: it should draw exactly when the opponent Special Summons.");
            }

            CleanDuelGame ghost = ReadyCleanGame();
            CleanCard activatedMonster = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            ghost.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(59438930), CleanPlayerId.Local));
            ghost.State.Rival.Monsters[0] = activatedMonster;
            if (ghost.CanActivateHandCard(0) || ghost.CanActivateLocalQuickEffectHandCard(0))
            {
                throw new InvalidOperationException("Ghost Ogre failed: it must not be freely activatable without a face-up field effect activation to respond to.");
            }

            InvokeBeginFieldResponseContext(ghost, ghost.State.Rival, activatedMonster, true);
            if (!ghost.CanActivateLocalQuickEffectHandCard(0))
            {
                throw new InvalidOperationException("Ghost Ogre failed: a monster already face-up on the field activated its effect, so a response should be legal.");
            }

            ghost.ActivateLocalQuickEffectHandCard(0);
            if (!ghost.State.Local.Graveyard.Exists(card => card.Definition.Id == 59438930) ||
                ghost.State.Rival.Monsters[0] != null ||
                !ghost.State.Rival.Graveyard.Exists(card => card.Definition.Id == 66961194))
            {
                throw new InvalidOperationException("Ghost Ogre failed: it should send itself to the GY and destroy the already face-up activated monster.");
            }
            CleanDuelGame spiritTrigger = ReadyCleanGame();
            spiritTrigger.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(45467446), CleanPlayerId.Local);
            spiritTrigger.State.Rival.SpellsTraps[0] = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Rival);
            spiritTrigger.State.Rival.SpellsTraps[0].FaceDown = true;
            InvokeResolveSummonTrigger(spiritTrigger, spiritTrigger.State.Local, spiritTrigger.State.Rival, spiritTrigger.State.Local.Monsters[0], false);
            if (spiritTrigger.State.PendingChoice == null || IndexOfChoiceOption(spiritTrigger, 0) < 0)
            {
                throw new InvalidOperationException("Dragon Spirit of White failed: its Summon Trigger should let the player choose an opposing Spell/Trap, including a Set card.");
            }

            spiritTrigger.ChoosePendingOption(IndexOfChoiceOption(spiritTrigger, 0));
            if (spiritTrigger.State.Rival.SpellsTraps[0] != null || !spiritTrigger.State.Rival.Banished.Exists(card => card.Definition.Id == 62089826))
            {
                throw new InvalidOperationException("Dragon Spirit of White failed: the chosen Spell/Trap should be banished by its Summon Trigger.");
            }

            CleanDuelGame spirit = ReadyCleanGame();
            spirit.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(45467446), CleanPlayerId.Local);
            spirit.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            spirit.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            if (!spirit.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Dragon Spirit of White failed: its Tribute Quick Effect should be available while the opponent controls a monster.");
            }

            spirit.ActivateMonsterZoneCard(0);
            ConfirmSpecialSummonPrompt(spirit, "Dragon Spirit of White");
            if (spirit.State.Local.Monsters[0] == null || spirit.State.Local.Monsters[0].Definition.Id != 89631139 ||
                !spirit.State.Local.Graveyard.Exists(card => card.Definition.Id == 45467446))
            {
                throw new InvalidOperationException("Dragon Spirit of White failed: it should Tribute itself and Special Summon Blue-Eyes White Dragon from the hand.");
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
            sageHand.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Local);
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
            if (sageHand.State.Local.Monsters[0] == null || sageHand.State.Local.Monsters[0].Definition.Id != 30576089 || !sageHand.State.Local.Graveyard.Exists(card => card.Definition.Id == 8240199) || !sageHand.State.Local.Graveyard.Exists(card => card.Definition.Id == 66961194))
            {
                throw new InvalidOperationException("Sage hand effect failed: Sage and target should go to GY, then chosen Blue-Eyes should be Special Summoned from Deck.");
            }

            CleanDuelGame jet = ReadyCleanGame();
            jet.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            jet.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (jet.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Blue-Eyes Jet Dragon failed: its hand/GY Special Summon is a destruction Trigger Effect, not a free hand activation.");
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
            jetTrigger.State.TurnNumber = 2;
            jetTrigger.State.Phase = CleanDuelPhase.Battle;
            jetTrigger.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            jetTrigger.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            jetTrigger.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(55410871), CleanPlayerId.Rival);
            jetTrigger.Attack(0, 0);
            ConfirmTriggerPrompt(jetTrigger, "Blue-Eyes Jet Dragon destruction trigger");
            if (jetTrigger.State.Local.Monsters[0] == null || jetTrigger.State.Local.Monsters[0].Definition.Id != 30576089)
            {
                throw new InvalidOperationException("Blue-Eyes Jet Dragon failed: it should Special Summon itself after a field card is destroyed by battle.");
            }

            CleanDuelGame jetBounce = ReadyCleanGame();
            jetBounce.State.TurnNumber = 2;
            jetBounce.State.Phase = CleanDuelPhase.Battle;
            jetBounce.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local);
            jetBounce.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            jetBounce.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Rival);
            jetBounce.Attack(0, 0);
            if (jetBounce.State.PendingChoice == null || IndexOfChoiceOption(jetBounce, 0) < 0)
            {
                throw new InvalidOperationException("Blue-Eyes Jet Dragon failed: with Blue-Eyes White Dragon on field/GY, its battle trigger should ask for a bounce target.");
            }

            jetBounce.ChoosePendingOption(IndexOfChoiceOption(jetBounce, 0));
            if (jetBounce.State.Rival.Monsters[0] != null || !jetBounce.State.Rival.Hand.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Blue-Eyes Jet Dragon failed: the chosen opponent card should return to the hand.");
            }
        }
        private static void AssertCorrectedDeckEffectSemantics()
        {
            CleanDuelGame veiler = ReadyCleanGame();
            veiler.State.TurnPlayer = CleanPlayerId.Rival;
            veiler.State.Phase = CleanDuelPhase.Main;
            veiler.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(97268402), CleanPlayerId.Local));
            veiler.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            veiler.State.Rival.Monsters[1] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Rival);
            veiler.State.Rival.Monsters[1].AttackBonus = 700;
            veiler.ActivateLocalQuickEffectHandCard(0);
            if (veiler.State.PendingChoice == null || IndexOfChoiceOption(veiler, 0) < 0 || IndexOfChoiceOption(veiler, 1) < 0)
            {
                throw new InvalidOperationException("Effect Veiler failed: its controller should choose the opposing face-up Effect Monster to negate.");
            }

            veiler.ChoosePendingOption(IndexOfChoiceOption(veiler, 1));
            if (!veiler.State.Rival.Monsters[1].EffectsNegated ||
                veiler.State.Rival.Monsters[0].EffectsNegated ||
                veiler.State.Rival.Monsters[1].AttackBonus != 700)
            {
                throw new InvalidOperationException("Effect Veiler failed: negate only the chosen monster's effects without erasing a resolved ATK modifier.");
            }

            CleanDuelGame gateway = ReadyCleanGame();
            gateway.State.Local.FieldSpell = new CleanCard(CleanCardLibrary.Get(40089744), CleanPlayerId.Local);
            gateway.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            InvokeMoveMonsterToGrave(gateway, gateway.State.Rival, 0);
            if (gateway.State.Local.GatewayToChaosCounters != 1)
            {
                throw new InvalidOperationException("Gateway to Chaos failed: an opponent's Monster Card sent from field to GY should also give it a Spell Counter.");
            }

            CleanDuelGame stone = ReadyCleanGame();
            stone.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(71039903), CleanPlayerId.Local));
            stone.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            stone.ActivateGraveyardCard(0);
            if (stone.State.PendingChoice == null)
            {
                throw new InvalidOperationException("White Stone failed: banishing it from the GY should open a Blue-Eyes recovery choice.");
            }

            stone.ChoosePendingOption(0);
            if (!stone.State.Local.Banished.Exists(card => card.Definition.Id == 71039903) ||
                !stone.State.Local.Hand.Exists(card => card.Definition.Id == 30576089))
            {
                throw new InvalidOperationException("White Stone failed: it should banish itself and add the chosen Blue-Eyes monster from GY to hand.");
            }

            CleanDuelGame mausoleum = ReadyCleanGame();
            mausoleum.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(24382602), CleanPlayerId.Local));
            mausoleum.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(17655904), CleanPlayerId.Local));
            mausoleum.ActivateGraveyardCard(0);
            if (!mausoleum.State.Local.Banished.Exists(card => card.Definition.Id == 24382602) ||
                !mausoleum.State.Local.Hand.Exists(card => card.Definition.Id == 17655904))
            {
                throw new InvalidOperationException("Mausoleum failed: its GY effect should banish itself and add Burst Stream of Destruction.");
            }

            CleanDuelGame wishes = ReadyCleanGame();
            CleanCard blueEyes = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            wishes.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(80326401), CleanPlayerId.Local));
            wishes.State.Local.Monsters[0] = blueEyes;
            wishes.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(23995346), CleanPlayerId.Local));
            wishes.ActivateGraveyardCard(0);
            wishes.ChoosePendingOption(0);
            wishes.ChoosePendingOption(0);
            if (blueEyes.CurrentAttack != 3400 || wishes.State.Local.SpellsTraps[0] == null ||
                wishes.State.Local.SpellsTraps[0].Definition.Id != 23995346 ||
                !wishes.State.Local.Banished.Exists(card => card.Definition.Id == 80326401))
            {
                throw new InvalidOperationException("Wishes failed: its GY effect should equip the chosen Blue-Eyes Extra Deck monster and grant exactly 400 ATK.");
            }

            CleanDuelGame roar = ReadyCleanGame();
            roar.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(17725109), CleanPlayerId.Local));
            roar.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            roar.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            roar.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            roar.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(23995346), CleanPlayerId.Local));
            if (!roar.CanActivateGraveyardCard(0))
            {
                throw new InvalidOperationException("Roar failed: its GY Fusion effect should be legal with Blue-Eyes materials in hand.");
            }

            roar.ActivateGraveyardCard(0);
            ConfirmSpecialSummonPrompt(roar, "Roar GY effect");
            if (roar.State.ExtraMonsters[0] == null || roar.State.ExtraMonsters[0].Definition.Id != 23995346 || roar.State.Local.Hand.Count != 0)
            {
                throw new InvalidOperationException("Roar failed: its GY effect should Fusion Summon using only hand/field materials, including Blue-Eyes.");
            }

            CleanDuelGame banishLock = ReadyCleanGame();
            banishLock.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89604813), CleanPlayerId.Rival);
            banishLock.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(24382602), CleanPlayerId.Local));
            banishLock.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(17655904), CleanPlayerId.Local));
            if (banishLock.CanActivateGraveyardCard(0))
            {
                throw new InvalidOperationException("Blue-Eyes Ultimate Spirit Dragon failed: its opponent must not be able to banish cards from their GY.");
            }

            banishLock.State.Rival.Monsters[0].EffectsNegated = true;
            if (!banishLock.CanActivateGraveyardCard(0))
            {
                throw new InvalidOperationException("Blue-Eyes Ultimate Spirit Dragon failed: its GY banish lock should stop while its effects are negated.");
            }

            CleanDuelGame trueLight = ReadyCleanGame();
            trueLight.State.Local.SpellsTraps[0] = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local);
            trueLight.State.Local.SpellsTraps[0].FaceDown = false;
            trueLight.State.ExtraMonsters[0] = new CleanCard(CleanCardLibrary.Get(42097666), CleanPlayerId.Local);
            InvokeMoveSpellTrapToGrave(trueLight, trueLight.State.Local, 0);
            if (trueLight.State.ExtraMonsters[0] != null || !trueLight.State.Local.Graveyard.Exists(card => card.Definition.Id == 42097666))
            {
                throw new InvalidOperationException("True Light failed: leaving face-up must destroy monsters in the Extra Monster Zone too.");
            }

            CleanDuelGame emptyTrueLight = ReadyCleanGame();
            emptyTrueLight.State.Local.SpellsTraps[0] = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local)
            {
                FaceDown = true,
                SetThisTurn = false
            };
            if (!emptyTrueLight.CanActivateSetCard(0))
            {
                throw new InvalidOperationException("True Light failed: the Continuous Trap itself may be activated even when neither optional effect can resolve yet.");
            }

            emptyTrueLight.ActivateSetCard(0);
            if (emptyTrueLight.State.Local.SpellsTraps[0].FaceDown || emptyTrueLight.State.PendingChoice != null)
            {
                throw new InvalidOperationException("True Light failed: it should remain face-up without forcing an unavailable optional effect.");
            }

            CleanDuelGame magia = ReadyCleanGame();
            CleanCard magiaCard = new CleanCard(CleanCardLibrary.Get(12381100), CleanPlayerId.Local);
            CleanCard firstMonsterEffect = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            magia.State.Local.Monsters[0] = magiaCard;
            magia.State.Rival.Monsters[0] = firstMonsterEffect;
            InvokeBeginFieldResponseContext(magia, magia.State.Rival, firstMonsterEffect, true);
            if (!magia.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Dragon Master Magia failed: it should be able to negate the first Monster effect this turn.");
            }

            magia.ActivateMonsterZoneCard(0);
            CleanCard secondMonsterEffect = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Rival);
            magia.State.Rival.Monsters[0] = secondMonsterEffect;
            InvokeBeginFieldResponseContext(magia, magia.State.Rival, secondMonsterEffect, true);
            if (magia.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Dragon Master Magia failed: it may negate a Monster activation only once per turn.");
            }

            CleanCard spellEffect = new CleanCard(CleanCardLibrary.Get(41620959), CleanPlayerId.Rival);
            magia.State.Rival.SpellsTraps[0] = spellEffect;
            InvokeBeginFieldResponseContext(magia, magia.State.Rival, spellEffect, true);
            if (!magia.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Dragon Master Magia failed: its separate Spell negate should still be available after using the Monster negate.");
            }

            magia.ActivateMonsterZoneCard(0);
            CleanCard trapEffect = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Rival);
            magia.State.Rival.SpellsTraps[0] = trapEffect;
            InvokeBeginFieldResponseContext(magia, magia.State.Rival, trapEffect, true);
            if (!magia.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Dragon Master Magia failed: its separate Trap negate should still be available after Monster and Spell negates.");
            }

            CleanDuelGame magiaDeckEffect = ReadyCleanGame();
            magiaDeckEffect.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(12381100), CleanPlayerId.Local);
            CleanCard deckSpellEffect = new CleanCard(CleanCardLibrary.Get(41620959), CleanPlayerId.Rival);
            InvokeBeginDeckResponseContext(magiaDeckEffect, magiaDeckEffect.State.Rival, deckSpellEffect, false);
            if (!magiaDeckEffect.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Dragon Master Magia failed: it must respond to opposing card effects regardless of the source zone.");
            }

            CleanDuelGame targetedMaiden = ReadyCleanGame();
            targetedMaiden.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local));
            targetedMaiden.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(88241506), CleanPlayerId.Local);
            targetedMaiden.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            targetedMaiden.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            targetedMaiden.ActivateHandCard(0);
            targetedMaiden.ChoosePendingOption(IndexOfChoiceOption(targetedMaiden, 0));
            ConfirmTriggerPrompt(targetedMaiden, "Maiden with Eyes targeted trigger");
            if (!HasMonsterWithId(targetedMaiden.State.Local, 89631139) ||
                !targetedMaiden.State.Local.Graveyard.Exists(card => card.Definition.Id == 88241506))
            {
                throw new InvalidOperationException("Maiden with Eyes of Blue failed: Sage targeting it should trigger a Blue-Eyes White Dragon Special Summon before Sage sends it to the GY.");
            }

            CleanDuelGame targetedMaidenWhite = ReadyCleanGame();
            targetedMaidenWhite.State.Local.FieldSpell = new CleanCard(CleanCardLibrary.Get(24382602), CleanPlayerId.Local);
            targetedMaidenWhite.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local);
            targetedMaidenWhite.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local));
            targetedMaidenWhite.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            targetedMaidenWhite.ActivateSetCard(CleanDuelGame.FieldSpellZoneIndex);
            targetedMaidenWhite.ChoosePendingOption(IndexOfChoiceOption(targetedMaidenWhite, 0));
            ConfirmTriggerPrompt(targetedMaidenWhite, "Maiden of White targeted trigger");
            if (!HasMonsterWithId(targetedMaidenWhite.State.Local, 8240199))
            {
                throw new InvalidOperationException("Maiden of White failed: Mausoleum targeting it should trigger a legal Blue-Eyes/Level 1 LIGHT Tuner revival from the GY.");
            }
            CleanDuelGame ash = ReadyCleanGame();
            CleanCard deckEffect = new CleanCard(CleanCardLibrary.Get(41620959), CleanPlayerId.Rival);
            ash.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(14558127), CleanPlayerId.Local));
            if (ash.CanActivateLocalQuickEffectHandCard(0))
            {
                throw new InvalidOperationException("Ash Blossom failed: it must wait for an effect that moves a card from the Deck.");
            }

            InvokeBeginDeckResponseContext(ash, ash.State.Rival, deckEffect, false);
            if (!ash.CanActivateLocalQuickEffectHandCard(0))
            {
                throw new InvalidOperationException("Ash Blossom failed: Dragon Shrine includes sending from Deck to GY, so its response should be legal.");
            }

            ash.ActivateLocalQuickEffectHandCard(0);
            if (!ReadPrivateBool(ash, "pendingResponseNegated") || !ash.State.Local.Graveyard.Exists(card => card.Definition.Id == 14558127))
            {
                throw new InvalidOperationException("Ash Blossom failed: it should discard itself and negate the Deck-moving effect.");
            }

            CleanDuelGame dictatorSummon = ReadyCleanGame();
            dictatorSummon.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Local));
            dictatorSummon.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            dictatorSummon.ActivateHandCard(0);
            if (dictatorSummon.State.PendingChoice == null)
            {
                throw new InvalidOperationException("Dictator of D. failed: its hand effect should ask which Blue-Eyes White Dragon to send.");
            }

            dictatorSummon.ChoosePendingOption(0);
            if (dictatorSummon.State.Local.Monsters[0] == null || dictatorSummon.State.Local.Monsters[0].Definition.Id != 66961194 ||
                !dictatorSummon.State.Local.Graveyard.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Dictator of D. failed: it should send Blue-Eyes White Dragon from hand/Deck to GY and Special Summon itself.");
            }

            CleanDuelGame dictatorRevive = ReadyCleanGame();
            dictatorRevive.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Local);
            dictatorRevive.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(80326401), CleanPlayerId.Local));
            dictatorRevive.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            dictatorRevive.ActivateMonsterZoneCard(0);
            dictatorRevive.ChoosePendingOption(0);
            dictatorRevive.ChoosePendingOption(0);
            if (dictatorRevive.State.Local.Monsters[1] == null || dictatorRevive.State.Local.Monsters[1].Definition.Id != 30576089 ||
                !dictatorRevive.State.Local.Graveyard.Exists(card => card.Definition.Id == 80326401))
            {
                throw new InvalidOperationException("Dictator of D. failed: it should discard a Blue-Eyes-mentioning card and revive the chosen Blue-Eyes monster.");
            }

            CleanDuelGame dictatorTarget = ReadyCleanGame();
            dictatorTarget.State.TurnPlayer = CleanPlayerId.Rival;
            dictatorTarget.State.Phase = CleanDuelPhase.Battle;
            dictatorTarget.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            dictatorTarget.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Local);
            dictatorTarget.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            dictatorTarget.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            InvokeRivalBattle(dictatorTarget);
            if (dictatorTarget.State.PendingChoice == null || IndexOfChoiceOption(dictatorTarget, 1) < 0)
            {
                throw new InvalidOperationException("Dictator of D. failed: controlling a Blue-Eyes should let its controller choose the opponent's attack target.");
            }

            dictatorTarget.ChoosePendingOption(IndexOfChoiceOption(dictatorTarget, 1));
            if (dictatorTarget.State.Rival.Monsters[0] != null ||
                dictatorTarget.State.Local.Monsters[1] == null ||
                dictatorTarget.State.Local.Monsters[1].Definition.Id != 89631139)
            {
                throw new InvalidOperationException("Dictator of D. failed: the opponent's attack must resolve against the manually chosen Blue-Eyes target.");
            }
            CleanDuelGame bingo = ReadyCleanGame();
            bingo.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(93437091), CleanPlayerId.Local));
            bingo.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            bingo.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            bingo.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local));
            bingo.ActivateHandCard(0);
            bingo.ChoosePendingOption(0);
            bingo.ChoosePendingOption(0);
            bingo.ChoosePendingOption(0);
            if (bingo.State.Local.Hand.Count != 1 || bingo.State.Local.Deck.Count != 2 ||
                !bingo.State.Local.Graveyard.Exists(card => card.Definition.Id == 93437091))
            {
                throw new InvalidOperationException("Bingo Machine failed: reveal exactly 3 legal cards, randomly add 1, shuffle the other 2, and send Bingo to GY.");
            }

            CleanDuelGame chaosProtection = ReadyCleanGame();
            chaosProtection.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(55410871), CleanPlayerId.Local);
            if (InvokeLuaDestroyStrongest(chaosProtection, chaosProtection.State.Local) != 0)
            {
                throw new InvalidOperationException("Chaos MAX failed: the opponent cannot target it with card effects.");
            }

            InvokeDestroyAllMonsters(chaosProtection, chaosProtection.State.Local, true);
            if (chaosProtection.State.Local.Monsters[0] == null || chaosProtection.State.Local.Monsters[0].Definition.Id != 55410871)
            {
                throw new InvalidOperationException("Chaos MAX failed: it cannot be destroyed by the opponent's non-targeting card effects.");
            }

            CleanDuelGame chaosMaxBattle = ReadyCleanGame();
            chaosMaxBattle.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(55410871), CleanPlayerId.Local);
            chaosMaxBattle.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Rival)
            {
                Stance = CleanMonsterStance.Defense
            };
            InvokeResolveBattle(chaosMaxBattle, chaosMaxBattle.State.Local, chaosMaxBattle.State.Rival, 0, 0);
            if (chaosMaxBattle.State.Rival.LifePoints != 3000)
            {
                throw new InvalidOperationException("Chaos MAX failed: attacking 1500 DEF should inflict double piercing damage of 5000.");
            }

            CleanDuelGame chaosDragonBattle = ReadyCleanGame();
            chaosDragonBattle.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(20654247), CleanPlayerId.Local)
            {
                RitualSummonedUsingBlueEyesWhiteDragon = true
            };
            chaosDragonBattle.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival)
            {
                Stance = CleanMonsterStance.Attack
            };
            chaosDragonBattle.State.Rival.Monsters[1] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Rival)
            {
                Stance = CleanMonsterStance.Defense
            };
            InvokeResolveBattle(chaosDragonBattle, chaosDragonBattle.State.Local, chaosDragonBattle.State.Rival, 0, 0);
            if (chaosDragonBattle.State.Rival.LifePoints != 5000 ||
                chaosDragonBattle.State.Rival.Monsters[1] == null ||
                chaosDragonBattle.State.Rival.Monsters[1].Stance != CleanMonsterStance.Attack ||
                chaosDragonBattle.State.Rival.Monsters[1].CurrentAttack != 0 ||
                chaosDragonBattle.State.Rival.Monsters[1].CurrentDefense != 0)
            {
                throw new InvalidOperationException("Blue-Eyes Chaos Dragon failed: using Blue-Eyes material should change legal battle positions, set ATK/DEF to 0, and grant piercing for the turn.");
            }
        }
        private static void AssertOfficialCardTextLoaded()
        {
            List<CleanCard> cards = CleanCardLibrary.BuildDeck(CleanPlayerId.Local);
            cards.AddRange(CleanCardLibrary.BuildExtraDeck(CleanPlayerId.Local));
            HashSet<int> seen = new HashSet<int>();
            for (int i = 0; i < cards.Count; i++)
            {
                CleanCardDefinition definition = cards[i].Definition;
                if (!seen.Add(definition.Id))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(definition.Text))
                {
                    throw new InvalidOperationException(
                        "Official card text failed: " + definition.Name + " has no text from cards.cdb.");
                }

                if (definition.Text.IndexOf("Lua:", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    throw new InvalidOperationException(
                        "Official card text failed: developer Lua diagnostics leaked into " + definition.Name + ".");
                }
            }

            Dictionary<int, string> officialSnippets = new Dictionary<int, string>
            {
                { 8240199, "When this card is Normal Summoned" },
                { 12381100, "in response to each card type" },
                { 17725109, "Deck, GY, or banishment" },
                { 62089826, "cannot target \"Blue-Eyes White Dragon\"" }
            };
            foreach (KeyValuePair<int, string> expected in officialSnippets)
            {
                string text = CleanCardLibrary.Get(expected.Key).Text ?? string.Empty;
                if (text.IndexOf(expected.Value, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    throw new InvalidOperationException(
                        "Official card text failed for card " + expected.Key + ": missing '" + expected.Value + "'.");
                }
            }
        }

        private static void AssertLuaScriptsAreDiagnosticsOnly()
        {
            CleanLuaScriptRepository repository = new CleanLuaScriptRepository();
            repository.Initialize();
            CleanLuaScriptInfo potScript;
            if (!repository.TryGet(55144522, out potScript))
            {
                throw new InvalidOperationException("Lua diagnostics failed: Pot of Greed script was not found.");
            }

            CleanCardDefinition potOfGreed = CleanCardLibrary.Get(55144522);
            if (!string.IsNullOrEmpty(potOfGreed.EffectKey))
            {
                throw new InvalidOperationException(
                    "Lua safety failed: an unmapped Lua script received gameplay key " + potOfGreed.EffectKey + ".");
            }

            CleanDuelGame game = ReadyCleanGame();
            game.State.Local.Hand.Add(new CleanCard(potOfGreed, CleanPlayerId.Local));
            game.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            game.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local));
            if (game.CanActivateHandCard(0))
            {
                throw new InvalidOperationException(
                    "Lua safety failed: a classified but unmapped script became activatable through a guessed effect.");
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

        private static void AssertMaidenReviveAfterTrueLightIsOptional()
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
                throw new InvalidOperationException("True Light failed: it did not Special Summon Blue-Eyes White Dragon.");
            }

            if (game.State.Local.Monsters[1] != null || !game.State.Local.Graveyard.Exists(card => card.Definition.Id == 17947697))
            {
                throw new InvalidOperationException("Maiden of White failed: True Light must not automatically Special Summon Maiden from the GY.");
            }

            if (game.State.PendingChoice == null || game.State.PendingChoice.Title != "Maiden of White" || IndexOfChoiceOption(game, -1) < 0)
            {
                throw new InvalidOperationException("Maiden of White failed: its separate optional GY Trigger Effect was not offered.");
            }

            game.ChoosePendingOption(IndexOfChoiceOption(game, -1));
            if (game.State.Local.Monsters[1] != null || !game.State.Local.Graveyard.Exists(card => card.Definition.Id == 17947697))
            {
                throw new InvalidOperationException("Maiden of White failed: declining its Trigger Effect should leave it in the GY.");
            }

            CleanDuelGame activated = ReadyCleanGame();
            activated.State.Local.SpellsTraps[0] = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local);
            activated.State.Local.SpellsTraps[0].FaceDown = false;
            CleanCard maiden = new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local);
            activated.State.Local.Graveyard.Add(maiden);
            activated.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            activated.ActivateSetCard(0);
            activated.ChoosePendingOption(0);

            int activateMaidenOption = IndexOfChoiceOption(activated, maiden.RuntimeId);
            if (activateMaidenOption < 0)
            {
                throw new InvalidOperationException("Maiden of White failed: its optional activation choice was unavailable.");
            }

            activated.ChoosePendingOption(activateMaidenOption);
            if (activated.State.Local.Monsters[1] == null || activated.State.Local.Monsters[1].Definition.Id != 17947697)
            {
                throw new InvalidOperationException("Maiden of White failed: accepting its separate Trigger Effect did not Special Summon it.");
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

            CleanDuelGame noExtraDeckZone = ReadyCleanGame();
            noExtraDeckZone.State.ExtraMonsters[0] = new CleanCard(CleanCardLibrary.Get(40908371), CleanPlayerId.Local);
            noExtraDeckZone.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(71143015), CleanPlayerId.Local));
            noExtraDeckZone.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            noExtraDeckZone.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            noExtraDeckZone.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            noExtraDeckZone.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(23995346), CleanPlayerId.Local));
            if (noExtraDeckZone.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Fusion rule failed: an occupied Extra Monster Zone without a Link Arrow should block Extra Deck Fusion Summons.");
            }

            CleanDuelGame linkedFusion = ReadyCleanGame();
            linkedFusion.State.ExtraMonsters[0] = new CleanCard(CleanCardLibrary.Get(42097666), CleanPlayerId.Local) { LinkSummoned = true };
            linkedFusion.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(71143015), CleanPlayerId.Local));
            linkedFusion.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            linkedFusion.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            linkedFusion.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            linkedFusion.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(23995346), CleanPlayerId.Local));
            if (!linkedFusion.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Fusion rule failed: a Link Arrow should open a Main Monster Zone for an Extra Deck Fusion Summon.");
            }

            linkedFusion.ActivateHandCard(0);
            ConfirmSpecialSummonPrompt(linkedFusion, "Linked Ultimate Fusion");
            if (linkedFusion.State.Local.Monsters[2] == null || linkedFusion.State.Local.Monsters[2].Definition.Id != 23995346 || !linkedFusion.State.Local.Monsters[2].FusionSummoned)
            {
                throw new InvalidOperationException("Fusion rule failed: Fusion Summon should use the linked Main Monster Zone when the Extra Monster Zone is occupied.");
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
            ConfirmSpecialSummonPrompt(game, "Ultimate Fusion");
            if (game.State.ExtraMonsters[0] == null || game.State.ExtraMonsters[0].Definition.Id != 23995346 || !game.State.ExtraMonsters[0].FusionSummoned)
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
            ConfirmSpecialSummonPrompt(magia, "Dragon Master Magia Fusion Summon");
            if (magia.State.ExtraMonsters[0] == null || magia.State.ExtraMonsters[0].Definition.Id != 12381100 || !magia.State.ExtraMonsters[0].FusionSummoned)
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

            CleanDuelGame twoTunersBlocked = ReadyCleanGame();
            twoTunersBlocked.State.Local.ExtraDeck.Add(TestMonster(99000004, "Generic Level 9 Synchro", 9, "Dragon / Synchro / Effect"));
            twoTunersBlocked.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local);
            twoTunersBlocked.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(14558127), CleanPlayerId.Local);
            twoTunersBlocked.State.Local.Monsters[2] = TestMonster(99000005, "Level 5 non-Tuner", 5, "Warrior / Effect");
            if (twoTunersBlocked.CanSynchroSummonFromExtra(0))
            {
                throw new InvalidOperationException("Synchro rule failed: generic Synchro Summons should use exactly 1 Tuner, not multiple Tuners.");
            }

            CleanDuelGame faceDownBlocked = ReadyCleanGame();
            faceDownBlocked.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(59822133), CleanPlayerId.Local));
            faceDownBlocked.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local) { FaceDown = true };
            faceDownBlocked.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            if (faceDownBlocked.CanSynchroSummonFromExtra(0))
            {
                throw new InvalidOperationException("Synchro rule failed: face-down monsters cannot be used as Synchro Materials.");
            }

            CleanDuelGame noExtraDeckZone = ReadyCleanGame();
            noExtraDeckZone.State.ExtraMonsters[0] = new CleanCard(CleanCardLibrary.Get(40908371), CleanPlayerId.Local);
            noExtraDeckZone.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(59822133), CleanPlayerId.Local));
            noExtraDeckZone.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local);
            noExtraDeckZone.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            if (noExtraDeckZone.CanSynchroSummonFromExtra(0))
            {
                throw new InvalidOperationException("Synchro rule failed: an occupied Extra Monster Zone without a Link Arrow should block Extra Deck Synchro Summons.");
            }

            CleanDuelGame valid = ReadyCleanGame();
            valid.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(59822133), CleanPlayerId.Local));
            valid.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local);
            valid.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            if (!valid.CanSynchroSummonFromExtra(0) || !InvokeLuaCanSynchroSummon(valid, valid.State.Local, 59822133) || !InvokeLuaSynchroSummon(valid, valid.State.Local, 59822133))
            {
                throw new InvalidOperationException("Synchro rule failed: Blue-Eyes Spirit Dragon should be summonable with 1 face-up Tuner plus 1 face-up non-Tuner Blue-Eyes monster at Level 9 total.");
            }

            if (valid.State.ExtraMonsters[0] == null || valid.State.ExtraMonsters[0].Definition.Id != 59822133 || !valid.State.ExtraMonsters[0].SynchroSummoned)
            {
                throw new InvalidOperationException("Synchro summon failed: Blue-Eyes Spirit Dragon was not marked as Synchro Summoned in the Extra Monster Zone.");
            }

            if (valid.State.Local.Monsters[0] != null || valid.State.Local.Monsters[1] != null || !valid.State.Local.Graveyard.Exists(card => card.Definition.Id == 8240199) || !valid.State.Local.Graveyard.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Synchro summon failed: Tuner and non-Tuner materials were not sent from field to GY.");
            }

            CleanDuelGame multipleNonTuners = ReadyCleanGame();
            multipleNonTuners.State.Local.ExtraDeck.Add(TestMonster(99000006, "Generic Level 9 Synchro", 9, "Dragon / Synchro / Effect"));
            multipleNonTuners.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(14558127), CleanPlayerId.Local);
            multipleNonTuners.State.Local.Monsters[1] = TestMonster(99000007, "Level 4 non-Tuner", 4, "Warrior / Effect");
            multipleNonTuners.State.Local.Monsters[2] = TestMonster(99000008, "Level 2 non-Tuner", 2, "Warrior / Effect");
            if (!multipleNonTuners.CanSynchroSummonFromExtra(0))
            {
                throw new InvalidOperationException("Synchro rule failed: 1 Tuner plus multiple face-up non-Tuners with exact total Level should be legal.");
            }

            multipleNonTuners.SynchroSummonFromExtra(0);
            ConfirmSpecialSummonPrompt(multipleNonTuners, "Synchro Summon");
            if (multipleNonTuners.State.ExtraMonsters[0] == null || multipleNonTuners.State.Local.Graveyard.Count < 3)
            {
                throw new InvalidOperationException("Synchro summon failed: multiple non-Tuner Synchro Materials should be sent to the GY and summon to the Extra Monster Zone.");
            }

            CleanDuelGame linkedSynchro = ReadyCleanGame();
            linkedSynchro.State.ExtraMonsters[0] = new CleanCard(CleanCardLibrary.Get(42097666), CleanPlayerId.Local) { LinkSummoned = true };
            linkedSynchro.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(59822133), CleanPlayerId.Local));
            linkedSynchro.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local);
            linkedSynchro.State.Local.Monsters[1] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            if (!linkedSynchro.CanSynchroSummonFromExtra(0))
            {
                throw new InvalidOperationException("Synchro rule failed: a Link Arrow should open a Main Monster Zone for an Extra Deck Synchro Summon.");
            }

            linkedSynchro.SynchroSummonFromExtra(0);
            ConfirmSpecialSummonPrompt(linkedSynchro, "Linked Synchro Summon");
            if (linkedSynchro.State.Local.Monsters[2] == null || linkedSynchro.State.Local.Monsters[2].Definition.Id != 59822133 || !linkedSynchro.State.Local.Monsters[2].SynchroSummoned)
            {
                throw new InvalidOperationException("Synchro rule failed: Synchro Summon should use the linked Main Monster Zone when the Extra Monster Zone is occupied.");
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

            if (ultimateValid.State.ExtraMonsters[0] == null || ultimateValid.State.ExtraMonsters[0].Definition.Id != 89604813 || !ultimateValid.State.ExtraMonsters[0].SynchroSummoned)
            {
                throw new InvalidOperationException("Synchro summon failed: Blue-Eyes Ultimate Spirit Dragon was not marked as Synchro Summoned in the Extra Monster Zone.");
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
            ConfirmSpecialSummonPrompt(tagOut, "Blue-Eyes Spirit Dragon tag-out");
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
            CleanCard ultimateSpirit = new CleanCard(CleanCardLibrary.Get(89604813), CleanPlayerId.Local);
            CleanCard activatedMonster = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            negate.State.Local.Monsters[0] = ultimateSpirit;
            negate.State.Rival.Monsters[0] = activatedMonster;
            if (negate.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Blue-Eyes Ultimate Spirit Dragon failed: its negate must wait for a field activation to respond to.");
            }

            InvokeBeginFieldResponseContext(negate, negate.State.Rival, activatedMonster, true);
            if (!negate.CanActivateMonsterZoneCard(0))
            {
                throw new InvalidOperationException("Blue-Eyes Ultimate Spirit Dragon failed: a card activated on the field, so its Quick Effect should be available.");
            }

            negate.ActivateMonsterZoneCard(0);
            if (ultimateSpirit.AttackBonus != 1000 || activatedMonster.EffectsNegated || !ReadPrivateBool(negate, "pendingResponseNegated"))
            {
                throw new InvalidOperationException("Blue-Eyes Ultimate Spirit Dragon failed: it should negate that activation and gain 1000 ATK, without continuously negating the source card.");
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

            ConfirmTriggerPrompt(destroyed, "Blue-Eyes Ultimate Spirit Dragon destroyed trigger");
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

        private static void AssertSpellTrapRulebookTiming()
        {
            CleanDuelGame quickPlaySet = ReadyCleanGame();
            quickPlaySet.State.Local.Hand.Clear();
            quickPlaySet.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(71143015), CleanPlayerId.Local));
            quickPlaySet.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            quickPlaySet.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            quickPlaySet.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            quickPlaySet.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(23995346), CleanPlayerId.Local));
            if (!quickPlaySet.CanSetSpellTrap(0))
            {
                throw new InvalidOperationException("Spell/Trap rule failed: Quick-Play Spell should be settable during Main Phase.");
            }

            quickPlaySet.SetSpellTrap(0);
            if (quickPlaySet.State.Local.SpellsTraps[0] == null || !quickPlaySet.State.Local.SpellsTraps[0].FaceDown || !quickPlaySet.State.Local.SpellsTraps[0].SetThisTurn)
            {
                throw new InvalidOperationException("Spell/Trap rule failed: set Quick-Play Spell did not remember that it was set this turn.");
            }

            if (quickPlaySet.CanActivateSetCard(0))
            {
                throw new InvalidOperationException("Spell/Trap rule failed: a Set Quick-Play Spell must not activate during the same turn it was Set.");
            }

            quickPlaySet.State.Local.SpellsTraps[0].SetThisTurn = false;
            if (!quickPlaySet.CanActivateSetCard(0))
            {
                throw new InvalidOperationException("Spell/Trap rule failed: a Set Quick-Play Spell should activate after the turn it was Set.");
            }

            CleanDuelGame quickPlayFromHandBattle = ReadyCleanGame();
            quickPlayFromHandBattle.State.Local.Hand.Clear();
            quickPlayFromHandBattle.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(71143015), CleanPlayerId.Local));
            quickPlayFromHandBattle.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            quickPlayFromHandBattle.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            quickPlayFromHandBattle.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            quickPlayFromHandBattle.State.Local.ExtraDeck.Add(new CleanCard(CleanCardLibrary.Get(23995346), CleanPlayerId.Local));
            quickPlayFromHandBattle.State.Phase = CleanDuelPhase.Battle;
            if (quickPlayFromHandBattle.CanActivateHandCard(0))
            {
                throw new InvalidOperationException("Ultimate Fusion failed: its printed 'During the Main Phase' condition overrides generic Quick-Play timing during the Battle Phase.");
            }

            CleanDuelGame trapSet = ReadyCleanGame();
            trapSet.State.Local.Hand.Clear();
            trapSet.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local));
            if (!trapSet.CanSetSpellTrap(0))
            {
                throw new InvalidOperationException("Trap rule failed: Trap should be settable during Main Phase.");
            }

            trapSet.SetSpellTrap(0);
            trapSet.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            if (trapSet.CanActivateSetCard(0))
            {
                throw new InvalidOperationException("Trap rule failed: a Trap Card cannot be activated during the same turn it was Set.");
            }

            trapSet.State.Local.SpellsTraps[0].SetThisTurn = false;
            if (!trapSet.CanActivateSetCard(0))
            {
                throw new InvalidOperationException("Trap rule failed: a Trap Card should be activatable after the turn it was Set.");
            }

            CleanDuelGame fieldSet = ReadyCleanGame();
            fieldSet.State.Local.Hand.Clear();
            fieldSet.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(40089744), CleanPlayerId.Local));
            fieldSet.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(70551291), CleanPlayerId.Local));
            if (!fieldSet.CanSetSpellTrap(0))
            {
                throw new InvalidOperationException("Field Spell rule failed: Gateway to Chaos should be settable.");
            }

            fieldSet.SetSpellTrap(0);
            if (fieldSet.State.Local.FieldSpell == null || fieldSet.State.Local.FieldSpell.Definition.Id != 40089744 || !fieldSet.State.Local.FieldSpell.FaceDown || !fieldSet.State.Local.FieldSpell.SetThisTurn || fieldSet.State.Local.SpellsTraps[0] != null)
            {
                throw new InvalidOperationException("Field Spell rule failed: a Set Field Spell should be placed face-down in the Field Spell Zone.");
            }

            if (!fieldSet.CanActivateSetCard(CleanDuelGame.FieldSpellZoneIndex))
            {
                throw new InvalidOperationException("Field Spell rule failed: a Set Field Spell should be activatable during the same Main Phase.");
            }

            fieldSet.ActivateSetCard(CleanDuelGame.FieldSpellZoneIndex);
            if (fieldSet.State.Local.FieldSpell == null || fieldSet.State.Local.FieldSpell.FaceDown || fieldSet.State.PendingChoice == null)
            {
                throw new InvalidOperationException("Field Spell rule failed: activating a Set Gateway to Chaos should flip it face-up and open its activation search choice.");
            }
        }
        private static void AssertOpponentTurnTrapResponsePrompts()
        {
            CleanDuelGame response = ReadyCleanGame();
            response.State.Local.Hand.Clear();
            response.State.Rival.Hand.Clear();
            response.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            response.State.Local.SpellsTraps[0] = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local)
            {
                FaceDown = true,
                SetThisTurn = false
            };
            response.State.TurnPlayer = CleanPlayerId.Rival;
            response.State.Phase = CleanDuelPhase.Main;

            if (!response.CanActivateSetCard(0))
            {
                throw new InvalidOperationException("Opponent-turn Trap response failed: a previously Set True Light should be activatable during the opponent's turn.");
            }

            if (!InvokeOpenLocalQuickEffectWindow(response) ||
                response.State.PendingChoice == null ||
                response.State.PendingChoice.Kind != CleanPromptKind.CardEffect ||
                IndexOfChoiceOption(response, 400) < 0 ||
                IndexOfChoiceOption(response, -1) < 0)
            {
                throw new InvalidOperationException("Opponent-turn Trap response failed: True Light should appear in an activate/pass prompt.");
            }

            response.ChoosePendingOption(IndexOfChoiceOption(response, 400));
            if (response.State.Local.SpellsTraps[0] == null ||
                response.State.Local.SpellsTraps[0].FaceDown ||
                response.State.PendingChoice == null ||
                response.State.PendingChoice.SourceCardId != 62089826)
            {
                throw new InvalidOperationException("Opponent-turn Trap response failed: selecting True Light should flip it face-up and wait for its effect choice.");
            }

            response.ChoosePendingOption(IndexOfChoiceOption(response, 1));
            if (!HasMonsterWithId(response.State.Local, 89631139))
            {
                throw new InvalidOperationException("Opponent-turn Trap response failed: True Light should Special Summon the selected Blue-Eyes before the rival turn continues.");
            }

            CleanDuelGame freshSet = ReadyCleanGame();
            freshSet.State.Local.Hand.Clear();
            freshSet.State.Rival.Hand.Clear();
            freshSet.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            freshSet.State.Local.SpellsTraps[0] = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local)
            {
                FaceDown = true,
                SetThisTurn = true
            };
            freshSet.State.TurnPlayer = CleanPlayerId.Rival;
            freshSet.State.Phase = CleanDuelPhase.Main;
            if (freshSet.CanActivateSetCard(0) || InvokeOpenLocalQuickEffectWindow(freshSet))
            {
                throw new InvalidOperationException("Opponent-turn Trap response failed: a Trap cannot activate during the same turn it was Set.");
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
            ConfirmSpecialSummonPrompt(ritual, "Chaos Form Ritual Summon");
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
            ConfirmSpecialSummonPrompt(graveyardMaterial, "Chaos Form GY material Ritual Summon");
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
            ConfirmSpecialSummonPrompt(blackLusterRitual, "Black Luster Soldier Ritual Summon");
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
            ConfirmTriggerPrompt(stone, "The White Stone of Ancients End Phase trigger");
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
            ConfirmSpecialSummonPrompt(roar, "Roar");
            if (roar.State.Local.Monsters[0] == null || roar.State.Local.Monsters[0].Definition.Id != 89631139 || roar.State.Local.Monsters[0].Stance != CleanMonsterStance.Defense || roar.State.Local.Banished.Count != 0)
            {
                throw new InvalidOperationException("Roar failed: it should Special Summon Blue-Eyes White Dragon from the banished pile in Defense Position.");
            }

            CleanDuelGame maiden = ReadyCleanGame();
            maiden.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(88241506), CleanPlayerId.Local);
            maiden.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            maiden.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            InvokeResolveBattle(maiden, maiden.State.Rival, maiden.State.Local, 0, 0);
            ConfirmTriggerPrompt(maiden, "Maiden with Eyes attack trigger");
            if (maiden.State.Local.Monsters[0] == null || maiden.State.Local.Monsters[0].Definition.Id != 88241506 || maiden.State.Local.Monsters[1] == null || maiden.State.Local.Monsters[1].Definition.Id != 89631139 || !maiden.State.Rival.Monsters[0].HasBattled)
            {
                throw new InvalidOperationException("Maiden with Eyes of Blue failed: attack target trigger should negate the attack and summon Blue-Eyes White Dragon.");
            }

            CleanDuelGame maidenWhite = ReadyCleanGame();
            maidenWhite.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local);
            maidenWhite.State.Local.Graveyard.Add(new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local));
            maidenWhite.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            InvokeResolveBattle(maidenWhite, maidenWhite.State.Rival, maidenWhite.State.Local, 0, 0);
            ConfirmTriggerPrompt(maidenWhite, "Maiden of White attack trigger");
            if (maidenWhite.State.Local.Monsters[0] != null ||
                maidenWhite.State.Local.Monsters[1] == null ||
                maidenWhite.State.Local.Monsters[1].Definition.Id != 8240199 ||
                maidenWhite.State.Local.Graveyard.Exists(card => card.Definition.Id == 8240199) ||
                !maidenWhite.State.Local.Graveyard.Exists(card => card.Definition.Id == 17947697) ||
                !maidenWhite.State.Rival.Monsters[0].HasBattled)
            {
                throw new InvalidOperationException("Maiden of White failed: it should revive a legal monster without negating the attack; the battle must continue normally.");
            }
        }

        private static void AssertPromptedTriggerAndSpecialSummonFlow()
        {
            CleanDuelGame specialSummon = ReadyCleanGame();
            CleanCard roar = new CleanCard(CleanCardLibrary.Get(17725109), CleanPlayerId.Local);
            CleanCard blueEyes = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            specialSummon.State.Local.Hand.Add(roar);
            specialSummon.State.Local.Deck.Add(blueEyes);
            specialSummon.ActivateHandCard(0);
            if (specialSummon.State.PendingChoice == null ||
                specialSummon.State.PendingChoice.Kind != CleanPromptKind.SpecialSummon ||
                IndexOfChoiceOption(specialSummon, 0) < 0 ||
                !specialSummon.State.Local.Hand.Contains(roar) ||
                !specialSummon.State.Local.Deck.Contains(blueEyes) ||
                HasMonsterWithId(specialSummon.State.Local, 89631139))
            {
                throw new InvalidOperationException("Special Summon prompt failed: the card and summon target must remain in place until confirmation.");
            }

            specialSummon.ChoosePendingOption(IndexOfChoiceOption(specialSummon, 0));
            if (!specialSummon.State.Local.Hand.Contains(roar) ||
                !specialSummon.State.Local.Deck.Contains(blueEyes) ||
                HasMonsterWithId(specialSummon.State.Local, 89631139))
            {
                throw new InvalidOperationException("Special Summon prompt failed: cancelling must not pay costs or move cards.");
            }

            CleanDuelGame trigger = ReadyCleanGame();
            CleanCard stone = new CleanCard(CleanCardLibrary.Get(71039903), CleanPlayerId.Local)
            {
                SentToGraveThisTurn = true
            };
            CleanCard jet = new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Local);
            trigger.State.Local.Graveyard.Add(stone);
            trigger.State.Local.Deck.Add(jet);
            InvokeResolveEndPhaseTriggers(trigger, trigger.State.Local);
            if (trigger.State.PendingChoice == null ||
                trigger.State.PendingChoice.Kind != CleanPromptKind.TriggerEffect ||
                IndexOfChoiceOption(trigger, 1) < 0 ||
                IndexOfChoiceOption(trigger, 0) < 0 ||
                !trigger.State.Local.Graveyard.Contains(stone) ||
                !trigger.State.Local.Deck.Contains(jet))
            {
                throw new InvalidOperationException("Trigger prompt failed: an optional Trigger Effect must show activate/skip before moving cards.");
            }

            trigger.ChoosePendingOption(IndexOfChoiceOption(trigger, 0));
            if (!trigger.State.Local.Graveyard.Contains(stone) ||
                !trigger.State.Local.Deck.Contains(jet) ||
                trigger.State.Local.Monsters[0] != null)
            {
                throw new InvalidOperationException("Trigger prompt failed: skipping the effect must preserve the GY, Deck, and field.");
            }
        }
        private static void AssertPromptedManualCardEffectFlow()
        {
            CleanDuelGame hand = ReadyCleanGame();
            CleanCard shrine = new CleanCard(CleanCardLibrary.Get(41620959), CleanPlayerId.Local);
            CleanCard blueEyes = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            hand.State.Local.Hand.Add(shrine);
            hand.State.Local.Deck.Add(blueEyes);
            hand.RequestActivateHandCard(0);
            AssertCardEffectPrompt(hand, "hand effect");
            if (!hand.State.Local.Hand.Contains(shrine) || !hand.State.Local.Deck.Contains(blueEyes))
            {
                throw new InvalidOperationException("Card Effect prompt failed: opening a hand-effect prompt moved cards before confirmation.");
            }
            DeclineCardEffectPrompt(hand, "hand effect");

            CleanDuelGame monster = ReadyCleanGame();
            CleanCard maiden = new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local);
            monster.State.Local.Monsters[0] = maiden;
            monster.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local));
            monster.RequestActivateMonsterZoneCard(0);
            AssertCardEffectPrompt(monster, "Monster Zone effect");
            DeclineCardEffectPrompt(monster, "Monster Zone effect");
            if (!object.ReferenceEquals(monster.State.Local.Monsters[0], maiden))
            {
                throw new InvalidOperationException("Card Effect prompt failed: declining a Monster Zone effect moved its source card.");
            }

            CleanDuelGame setCard = ReadyCleanGame();
            CleanCard setShrine = new CleanCard(CleanCardLibrary.Get(41620959), CleanPlayerId.Local)
            {
                FaceDown = true,
                SetThisTurn = false
            };
            setCard.State.Local.SpellsTraps[0] = setShrine;
            setCard.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            setCard.RequestActivateSetCard(0);
            AssertCardEffectPrompt(setCard, "Set Spell/Trap effect");
            DeclineCardEffectPrompt(setCard, "Set Spell/Trap effect");
            if (!setShrine.FaceDown || !object.ReferenceEquals(setCard.State.Local.SpellsTraps[0], setShrine))
            {
                throw new InvalidOperationException("Card Effect prompt failed: declining a Set card flipped or moved it.");
            }

            CleanDuelGame graveyard = ReadyCleanGame();
            CleanCard stone = new CleanCard(CleanCardLibrary.Get(71039903), CleanPlayerId.Local);
            CleanCard graveBlueEyes = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            graveyard.State.Local.Graveyard.Add(stone);
            graveyard.State.Local.Graveyard.Add(graveBlueEyes);
            graveyard.RequestActivateGraveyardCard(0);
            AssertCardEffectPrompt(graveyard, "GY effect");
            DeclineCardEffectPrompt(graveyard, "GY effect");
            if (!graveyard.State.Local.Graveyard.Contains(stone) || !graveyard.State.Local.Graveyard.Contains(graveBlueEyes))
            {
                throw new InvalidOperationException("Card Effect prompt failed: declining a GY effect banished or moved a card.");
            }

            CleanDuelGame extraMonster = ReadyCleanGame();
            CleanCard spirit = new CleanCard(CleanCardLibrary.Get(42097666), CleanPlayerId.Local)
            {
                LinkSummoned = true
            };
            extraMonster.State.ExtraMonsters[0] = spirit;
            extraMonster.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(24382602), CleanPlayerId.Local));
            extraMonster.RequestActivateLocalExtraMonsterZoneCard(0);
            AssertCardEffectPrompt(extraMonster, "Extra Monster Zone effect");
            DeclineCardEffectPrompt(extraMonster, "Extra Monster Zone effect");
            if (!object.ReferenceEquals(extraMonster.State.ExtraMonsters[0], spirit))
            {
                throw new InvalidOperationException("Card Effect prompt failed: declining an Extra Monster Zone effect moved its source card.");
            }

            CleanDuelGame quick = ReadyCleanGame();
            CleanCard maxx = new CleanCard(CleanCardLibrary.Get(23434538), CleanPlayerId.Local);
            quick.State.Local.Hand.Add(maxx);
            quick.State.TurnPlayer = CleanPlayerId.Rival;
            quick.RequestActivateLocalQuickEffectHandCard(0);
            AssertCardEffectPrompt(quick, "manual Quick Effect");
            DeclineCardEffectPrompt(quick, "manual Quick Effect");
            if (!quick.State.Local.Hand.Contains(maxx))
            {
                throw new InvalidOperationException("Card Effect prompt failed: declining a Quick Effect paid its discard cost.");
            }

            CleanDuelGame special = ReadyCleanGame();
            CleanCard sage = new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local);
            special.State.Local.Hand.Add(sage);
            special.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(17947697), CleanPlayerId.Local);
            special.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            special.RequestActivateHandCard(0);
            if (special.State.PendingChoice == null || special.State.PendingChoice.Kind != CleanPromptKind.SpecialSummon)
            {
                throw new InvalidOperationException("Card Effect prompt failed: an effect that Special Summons should retain the SPECIAL SUMMON prompt.");
            }
            special.ChoosePendingOption(IndexOfChoiceOption(special, 0));
        }

        private static void AssertCardEffectPrompt(CleanDuelGame game, string context)
        {
            if (game.State.PendingChoice == null ||
                game.State.PendingChoice.Kind != CleanPromptKind.CardEffect ||
                game.State.PendingChoice.CanCancel ||
                IndexOfChoiceOption(game, 1) < 0 ||
                IndexOfChoiceOption(game, 0) < 0)
            {
                throw new InvalidOperationException(context + " failed: a non-cancellable CARD EFFECT prompt with Activate/Do not activate was expected.");
            }
        }

        private static void DeclineCardEffectPrompt(CleanDuelGame game, string context)
        {
            int decline = IndexOfChoiceOption(game, 0);
            if (decline < 0)
            {
                throw new InvalidOperationException(context + " failed: the Do not activate option was missing.");
            }

            game.ChoosePendingOption(decline);
            if (game.State.PendingChoice != null)
            {
                throw new InvalidOperationException(context + " failed: declining left an unexpected prompt open.");
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
            if (quick.State.PendingChoice == null || IndexOfChoiceOption(quick, 0) < 0)
            {
                throw new InvalidOperationException("Quick Effect rule failed: Effect Veiler should ask for a legal target.");
            }

            quick.ChoosePendingOption(IndexOfChoiceOption(quick, 0));
            if (!quick.State.Local.Graveyard.Exists(card => card.Definition.Id == 97268402) || !quick.State.Rival.Monsters[0].EffectsNegated)
            {
                throw new InvalidOperationException("Quick Effect rule failed: Effect Veiler should resolve from hand and negate the chosen rival monster.");
            }

            CleanDuelGame quickPrompt = ReadyCleanGame();
            quickPrompt.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(23434538), CleanPlayerId.Local));
            quickPrompt.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            quickPrompt.State.Rival.Hand.Clear();
            quickPrompt.State.Rival.Deck.Clear();
            quickPrompt.State.Rival.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Rival));
            quickPrompt.EndTurn();
            if (quickPrompt.State.PendingChoice == null ||
                quickPrompt.State.PendingChoice.Kind != CleanPromptKind.CardEffect ||
                quickPrompt.State.PendingChoice.Title != "Activate a card or effect?" ||
                IndexOfChoiceOption(quickPrompt, 100) < 0 ||
                IndexOfChoiceOption(quickPrompt, -1) < 0)
            {
                throw new InvalidOperationException("Quick Effect rule failed: opponent turn should pause with an activate/pass prompt for an available Spell Speed 2 monster effect.");
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
            ConfirmTriggerPrompt(trigger, "Maiden Trigger Effect timing");
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
            ConfirmTriggerPrompt(flip, "FLIP Summon trigger");
            if (flip.State.Local.Monsters[0].FaceDown || flip.State.Local.Monsters[0].Stance != CleanMonsterStance.Attack || !flip.State.Local.Hand.Exists(card => card.Definition.Id == 89631139))
            {
                throw new InvalidOperationException("Flip rule failed: Flip Summon should flip face-up attack and resolve the FLIP trigger.");
            }

            CleanDuelGame battleFlip = ReadyCleanGame();
            battleFlip.State.Local.Monsters[0] = new CleanCard(flipDefinition, CleanPlayerId.Local) { FaceDown = true, Stance = CleanMonsterStance.Defense };
            battleFlip.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            battleFlip.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(66961194), CleanPlayerId.Rival);
            InvokeResolveBattle(battleFlip, battleFlip.State.Rival, battleFlip.State.Local, 0, 0);
            ConfirmTriggerPrompt(battleFlip, "Battle FLIP trigger");
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
            ConfirmSpecialSummonPrompt(link, "Link Summon");
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
            ConfirmSpecialSummonPrompt(linkedSummon, "Linked Link Summon");
            if (linkedSummon.State.Local.Monsters[2] == null || linkedSummon.State.Local.Monsters[2].Definition.Id != 42097666 || !linkedSummon.State.Local.Monsters[2].LinkSummoned)
            {
                throw new InvalidOperationException("Link rule failed: second Link Summon should use the linked Main Monster Zone.");
            }
        }

        private static void AssertXyzRules()
        {
            CleanCardDefinition xyzDefinition = new CleanCardDefinition(
                99003104,
                "Codex Rank Four Xyz",
                CleanCardKind.Monster,
                4,
                2400,
                1800,
                "Dragon / Xyz / Effect",
                "2 Level 4 monsters. Once per turn: detach 1 material; test effect.",
                string.Empty);
            CleanCardDefinition levelFourA = new CleanCardDefinition(
                99003141,
                "Codex Level Four A",
                CleanCardKind.Monster,
                4,
                1400,
                1000,
                "Warrior / Effect",
                "Xyz material test monster.",
                string.Empty);
            CleanCardDefinition levelFourB = new CleanCardDefinition(
                99003142,
                "Codex Level Four B",
                CleanCardKind.Monster,
                4,
                1500,
                1000,
                "Spellcaster / Effect",
                "Xyz material test monster.",
                string.Empty);
            CleanCardDefinition levelThree = new CleanCardDefinition(
                99003103,
                "Codex Level Three",
                CleanCardKind.Monster,
                3,
                1200,
                1000,
                "Warrior / Effect",
                "Wrong Level material test monster.",
                string.Empty);

            CleanDuelGame faceDownBlocked = ReadyCleanGame();
            faceDownBlocked.State.Local.ExtraDeck.Add(new CleanCard(xyzDefinition, CleanPlayerId.Local));
            faceDownBlocked.State.Local.Monsters[0] = new CleanCard(levelFourA, CleanPlayerId.Local);
            faceDownBlocked.State.Local.Monsters[1] = new CleanCard(levelFourB, CleanPlayerId.Local) { FaceDown = true };
            if (faceDownBlocked.CanXyzSummonFromExtra(0))
            {
                throw new InvalidOperationException("Xyz rule failed: face-down monsters cannot be used as Xyz Materials.");
            }

            CleanDuelGame wrongLevelBlocked = ReadyCleanGame();
            wrongLevelBlocked.State.Local.ExtraDeck.Add(new CleanCard(xyzDefinition, CleanPlayerId.Local));
            wrongLevelBlocked.State.Local.Monsters[0] = new CleanCard(levelFourA, CleanPlayerId.Local);
            wrongLevelBlocked.State.Local.Monsters[1] = new CleanCard(levelThree, CleanPlayerId.Local);
            if (wrongLevelBlocked.CanXyzSummonFromExtra(0))
            {
                throw new InvalidOperationException("Xyz rule failed: Xyz Materials must match the Xyz Monster's Rank/required Level.");
            }

            CleanDuelGame xyz = ReadyCleanGame();
            xyz.State.Local.ExtraDeck.Add(new CleanCard(xyzDefinition, CleanPlayerId.Local));
            xyz.State.Local.Monsters[0] = new CleanCard(levelFourA, CleanPlayerId.Local);
            xyz.State.Local.Monsters[1] = new CleanCard(levelFourB, CleanPlayerId.Local);
            if (!xyz.CanXyzSummonFromExtra(0))
            {
                throw new InvalidOperationException("Xyz rule failed: 2 face-up Level 4 monsters should Xyz Summon a Rank 4 Xyz Monster.");
            }

            xyz.XyzSummonFromExtra(0);
            ConfirmSpecialSummonPrompt(xyz, "Xyz Summon");
            CleanCard summoned = xyz.State.ExtraMonsters[0];
            if (summoned == null || summoned.Definition.Id != 99003104 || !summoned.XyzSummoned || summoned.XyzMaterials.Count != 2 || xyz.State.Local.Monsters[0] != null || xyz.State.Local.Monsters[1] != null)
            {
                throw new InvalidOperationException("Xyz rule failed: Xyz Summon should stack the materials under the Xyz Monster in the Extra Monster Zone.");
            }

            if (xyz.State.Local.Graveyard.Count != 0)
            {
                throw new InvalidOperationException("Xyz rule failed: Xyz Materials should not go to the GY while attached.");
            }

            if (!InvokeDetachXyzMaterial(xyz, xyz.State.Local, summoned) || summoned.XyzMaterials.Count != 1 || xyz.State.Local.Graveyard.Count != 1)
            {
                throw new InvalidOperationException("Xyz rule failed: detaching should send exactly 1 stacked Xyz Material to the GY.");
            }

            CleanDuelGame linkedXyz = ReadyCleanGame();
            linkedXyz.State.ExtraMonsters[0] = new CleanCard(CleanCardLibrary.Get(42097666), CleanPlayerId.Local) { LinkSummoned = true };
            linkedXyz.State.Local.ExtraDeck.Add(new CleanCard(xyzDefinition, CleanPlayerId.Local));
            linkedXyz.State.Local.Monsters[0] = new CleanCard(levelFourA, CleanPlayerId.Local);
            linkedXyz.State.Local.Monsters[1] = new CleanCard(levelFourB, CleanPlayerId.Local);
            if (!linkedXyz.CanXyzSummonFromExtra(0))
            {
                throw new InvalidOperationException("Xyz rule failed: a Link Arrow should open a Main Monster Zone for an Extra Deck Xyz Summon.");
            }

            linkedXyz.XyzSummonFromExtra(0);
            ConfirmSpecialSummonPrompt(linkedXyz, "Linked Xyz Summon");
            if (linkedXyz.State.Local.Monsters[2] == null || linkedXyz.State.Local.Monsters[2].Definition.Id != 99003104 || linkedXyz.State.Local.Monsters[2].XyzMaterials.Count != 2)
            {
                throw new InvalidOperationException("Xyz rule failed: Xyz Summon should use the linked Main Monster Zone when the Extra Monster Zone is occupied.");
            }

            InvokeMoveMonsterToGrave(linkedXyz, linkedXyz.State.Local, 2);
            if (linkedXyz.State.Local.Graveyard.Count < 3)
            {
                throw new InvalidOperationException("Xyz rule failed: when an Xyz Monster leaves the field, its remaining Xyz Materials should go to the GY too.");
            }
        }
        private static void AssertXyzExtraDeckChoiceFlow()
        {
            CleanCardDefinition rankEightA = new CleanCardDefinition(
                99003801,
                "Codex Rank Eight Alpha",
                CleanCardKind.Monster,
                8,
                3000,
                2500,
                "Dragon / Xyz / Effect",
                "2 Level 8 monsters. Test Xyz choice A.",
                "xyz_choice_alpha");
            CleanCardDefinition rankEightB = new CleanCardDefinition(
                99003802,
                "Codex Rank Eight Beta",
                CleanCardKind.Monster,
                8,
                3200,
                2200,
                "Dragon / Xyz / Effect",
                "2 Level 8 monsters. Test Xyz choice B.",
                "xyz_choice_beta");
            CleanCardDefinition rankFour = new CleanCardDefinition(
                99003400,
                "Codex Invalid Rank Four",
                CleanCardKind.Monster,
                4,
                2500,
                2000,
                "Warrior / Xyz / Effect",
                "2 Level 4 monsters. This card must stay hidden from the Rank 8 choice.",
                "xyz_choice_invalid");
            CleanCardDefinition levelEightA = new CleanCardDefinition(
                99003811,
                "Codex Level Eight A",
                CleanCardKind.Monster,
                8,
                2000,
                2000,
                "Dragon / Effect",
                "Rank 8 material A.",
                string.Empty);
            CleanCardDefinition levelEightB = new CleanCardDefinition(
                99003812,
                "Codex Level Eight B",
                CleanCardKind.Monster,
                8,
                2100,
                2100,
                "Dragon / Effect",
                "Rank 8 material B.",
                string.Empty);

            CleanDuelGame game = ReadyCleanGame();
            CleanCard invalid = new CleanCard(rankFour, CleanPlayerId.Local);
            CleanCard validA = new CleanCard(rankEightA, CleanPlayerId.Local);
            CleanCard validB = new CleanCard(rankEightB, CleanPlayerId.Local);
            CleanCard materialA = new CleanCard(levelEightA, CleanPlayerId.Local);
            CleanCard materialB = new CleanCard(levelEightB, CleanPlayerId.Local);
            game.State.Local.ExtraDeck.Add(invalid);
            game.State.Local.ExtraDeck.Add(validA);
            game.State.Local.ExtraDeck.Add(validB);
            game.State.Local.Monsters[0] = materialA;
            game.State.Local.Monsters[1] = materialB;

            if (!game.OpenXyzExtraDeckSelection() || game.State.PendingChoice == null)
            {
                throw new InvalidOperationException("Xyz Extra Deck choice failed: clicking an Extra Deck with valid Xyz monsters should open a choice prompt.");
            }

            if (game.State.PendingChoice.Kind != CleanPromptKind.SpecialSummon ||
                !game.State.PendingChoice.CanCancel ||
                game.State.PendingChoice.Options.Count != 2 ||
                IndexOfChoiceOption(game, invalid.RuntimeId) >= 0 ||
                IndexOfChoiceOption(game, validA.RuntimeId) < 0 ||
                IndexOfChoiceOption(game, validB.RuntimeId) < 0)
            {
                throw new InvalidOperationException("Xyz Extra Deck choice failed: only currently summonable Xyz monsters should be shown.");
            }

            int selectedOption = IndexOfChoiceOption(game, validB.RuntimeId);
            CleanChoiceOption selectedChoice = game.State.PendingChoice.Options[selectedOption];
            if (selectedChoice.CardDefinitionId != validB.Definition.Id)
            {
                throw new InvalidOperationException("Xyz Extra Deck choice failed: the option should expose the selected card artwork id.");
            }

            game.ChoosePendingOption(selectedOption);
            if (game.State.PendingChoice == null ||
                game.State.PendingChoice.Kind != CleanPromptKind.SpecialSummon ||
                game.State.PendingChoice.Title != validB.Definition.Name)
            {
                throw new InvalidOperationException("Xyz Extra Deck choice failed: choosing a card should ask for that Xyz Summon's confirmation.");
            }

            ConfirmSpecialSummonPrompt(game, "Chosen Xyz Summon");
            CleanCard summoned = game.State.ExtraMonsters[0];
            if (!object.ReferenceEquals(summoned, validB) ||
                summoned.XyzMaterials.Count != 2 ||
                !summoned.XyzMaterials.Contains(materialA) ||
                !summoned.XyzMaterials.Contains(materialB) ||
                game.State.Local.Monsters[0] != null ||
                game.State.Local.Monsters[1] != null)
            {
                throw new InvalidOperationException("Xyz Extra Deck choice failed: the chosen Xyz Monster should be placed on top of the 2 Level 8 materials.");
            }

            if (!game.State.Local.ExtraDeck.Contains(validA) ||
                !game.State.Local.ExtraDeck.Contains(invalid) ||
                game.State.Local.ExtraDeck.Contains(validB) ||
                game.State.Local.Graveyard.Count != 0)
            {
                throw new InvalidOperationException("Xyz Extra Deck choice failed: unchosen cards must remain in the Extra Deck and attached materials must stay out of the GY.");
            }
        }
        private static void AssertUnifiedSummonChoiceFlows()
        {
            CleanCardDefinition synchroDefinition = new CleanCardDefinition(
                99005001,
                "Codex Choice Synchro",
                CleanCardKind.Monster,
                8,
                2800,
                2200,
                "Dragon / Synchro / Effect",
                "1 Tuner + 1+ non-Tuner monsters.",
                "choice_synchro");
            CleanCardDefinition xyzDefinition = new CleanCardDefinition(
                99005002,
                "Codex Choice Xyz",
                CleanCardKind.Monster,
                4,
                2600,
                2100,
                "Dragon / Xyz / Effect",
                "2 Level 4 monsters.",
                "choice_xyz");
            CleanCardDefinition linkDefinition = new CleanCardDefinition(
                99005003,
                "Codex Choice Link",
                CleanCardKind.Monster,
                2,
                2000,
                0,
                "Dragon / Link / Effect",
                "2 monsters.",
                "choice_link");
            CleanCardDefinition tunerDefinition = new CleanCardDefinition(
                99005011,
                "Codex Level Four Tuner",
                CleanCardKind.Monster,
                4,
                1500,
                1000,
                "Dragon / Tuner / Effect",
                "Summon-choice test material.",
                string.Empty);
            CleanCardDefinition nonTunerDefinition = new CleanCardDefinition(
                99005012,
                "Codex Level Four Non-Tuner",
                CleanCardKind.Monster,
                4,
                1600,
                1200,
                "Dragon / Effect",
                "Summon-choice test material.",
                string.Empty);

            CleanDuelGame extraDeckChoice = ReadyCleanGame();
            CleanCard synchro = new CleanCard(synchroDefinition, CleanPlayerId.Local);
            CleanCard xyz = new CleanCard(xyzDefinition, CleanPlayerId.Local);
            CleanCard link = new CleanCard(linkDefinition, CleanPlayerId.Local);
            extraDeckChoice.State.Local.ExtraDeck.Add(synchro);
            extraDeckChoice.State.Local.ExtraDeck.Add(xyz);
            extraDeckChoice.State.Local.ExtraDeck.Add(link);
            extraDeckChoice.State.Local.Monsters[0] = new CleanCard(tunerDefinition, CleanPlayerId.Local);
            extraDeckChoice.State.Local.Monsters[1] = new CleanCard(nonTunerDefinition, CleanPlayerId.Local);

            if (!extraDeckChoice.OpenExtraDeckSummonSelection() ||
                extraDeckChoice.State.PendingChoice == null ||
                extraDeckChoice.State.PendingChoice.Options.Count != 3 ||
                IndexOfChoiceOption(extraDeckChoice, synchro.RuntimeId) < 0 ||
                IndexOfChoiceOption(extraDeckChoice, xyz.RuntimeId) < 0 ||
                IndexOfChoiceOption(extraDeckChoice, link.RuntimeId) < 0)
            {
                throw new InvalidOperationException("Unified Extra Deck choice failed: all and only legal Synchro, Xyz, and Link monsters should be offered together.");
            }

            int linkOption = IndexOfChoiceOption(extraDeckChoice, link.RuntimeId);
            if (extraDeckChoice.State.PendingChoice.Options[linkOption].CardDefinitionId != link.Definition.Id)
            {
                throw new InvalidOperationException("Unified Extra Deck choice failed: Link card artwork should be available in the chooser.");
            }

            extraDeckChoice.ChoosePendingOption(linkOption);
            ConfirmSpecialSummonPrompt(extraDeckChoice, "Chosen Link Summon");
            if (!object.ReferenceEquals(extraDeckChoice.State.ExtraMonsters[0], link) ||
                !link.LinkSummoned ||
                !extraDeckChoice.State.Local.ExtraDeck.Contains(synchro) ||
                !extraDeckChoice.State.Local.ExtraDeck.Contains(xyz) ||
                extraDeckChoice.State.Local.Graveyard.Count != 2)
            {
                throw new InvalidOperationException("Unified Extra Deck choice failed: the manually chosen Link Monster was not summoned with its materials.");
            }

            CleanDuelGame fusionChoice = ReadyCleanGame();
            CleanCard ultimateFusion = new CleanCard(CleanCardLibrary.Get(71143015), CleanPlayerId.Local);
            CleanCard blueEyesUltimate = new CleanCard(CleanCardLibrary.Get(23995346), CleanPlayerId.Local);
            CleanCard dragonMasterMagia = new CleanCard(CleanCardLibrary.Get(12381100), CleanPlayerId.Local);
            fusionChoice.State.Local.Hand.Add(ultimateFusion);
            fusionChoice.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            fusionChoice.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            fusionChoice.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            fusionChoice.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(55410871), CleanPlayerId.Local));
            fusionChoice.State.Local.ExtraDeck.Add(blueEyesUltimate);
            fusionChoice.State.Local.ExtraDeck.Add(dragonMasterMagia);
            fusionChoice.ActivateHandCard(0);
            ConfirmSpecialSummonPrompt(fusionChoice, "Fusion target choice activation");

            if (fusionChoice.State.PendingChoice == null ||
                fusionChoice.State.PendingChoice.Title != "Choose a Fusion Monster" ||
                fusionChoice.State.PendingChoice.CanCancel ||
                fusionChoice.State.PendingChoice.Options.Count != 2 ||
                IndexOfChoiceOption(fusionChoice, blueEyesUltimate.RuntimeId) < 0 ||
                IndexOfChoiceOption(fusionChoice, dragonMasterMagia.RuntimeId) < 0)
            {
                throw new InvalidOperationException("Fusion choice failed: Ultimate Fusion should show every currently legal Fusion Monster.");
            }

            fusionChoice.ChoosePendingOption(IndexOfChoiceOption(fusionChoice, blueEyesUltimate.RuntimeId));
            if (!object.ReferenceEquals(fusionChoice.State.ExtraMonsters[0], blueEyesUltimate) ||
                !blueEyesUltimate.FusionSummoned ||
                !fusionChoice.State.Local.ExtraDeck.Contains(dragonMasterMagia))
            {
                throw new InvalidOperationException("Fusion choice failed: the selected Fusion Monster should be summoned while the unchosen card remains in the Extra Deck.");
            }

            CleanDuelGame ritualChoice = ReadyCleanGame();
            CleanCard chaosForm = new CleanCard(CleanCardLibrary.Get(21082832), CleanPlayerId.Local);
            CleanCard chaosMax = new CleanCard(CleanCardLibrary.Get(55410871), CleanPlayerId.Local);
            CleanCard chaosDragon = new CleanCard(CleanCardLibrary.Get(20654247), CleanPlayerId.Local);
            ritualChoice.State.Local.Hand.Add(chaosForm);
            ritualChoice.State.Local.Hand.Add(chaosMax);
            ritualChoice.State.Local.Hand.Add(chaosDragon);
            ritualChoice.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            ritualChoice.ActivateHandCard(0);
            ConfirmSpecialSummonPrompt(ritualChoice, "Ritual target choice activation");

            if (ritualChoice.State.PendingChoice == null ||
                ritualChoice.State.PendingChoice.Title != "Choose a Ritual Monster" ||
                ritualChoice.State.PendingChoice.CanCancel ||
                ritualChoice.State.PendingChoice.Options.Count != 2 ||
                IndexOfChoiceOption(ritualChoice, chaosMax.RuntimeId) < 0 ||
                IndexOfChoiceOption(ritualChoice, chaosDragon.RuntimeId) < 0)
            {
                throw new InvalidOperationException("Ritual choice failed: Chaos Form should show every Ritual Monster with legal tribute materials.");
            }

            ritualChoice.ChoosePendingOption(IndexOfChoiceOption(ritualChoice, chaosDragon.RuntimeId));
            if (!object.ReferenceEquals(ritualChoice.State.Local.Monsters[0], chaosDragon) ||
                !chaosDragon.ProperlySummoned ||
                !ritualChoice.State.Local.Hand.Contains(chaosMax))
            {
                throw new InvalidOperationException("Ritual choice failed: the selected Ritual Monster should be summoned while the unchosen card remains in hand.");
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
            CleanCard invalidPendulumTarget = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local);
            game.State.Local.Hand.Add(invalidPendulumTarget);
            game.State.Local.ExtraDeck.Add(new CleanCard(faceUpExtra, CleanPlayerId.Local) { FaceUpExtraDeck = true });
            if (!game.CanPendulumSummon())
            {
                throw new InvalidOperationException("Pendulum rule failed: scales 1 and 8 should allow Levels 2 through 7 to be Pendulum Summoned.");
            }

            game.State.Rival.Hand.Clear();
            game.State.Rival.Deck.Clear();
            game.State.Rival.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Rival));
            game.State.Rival.Deck.Add(new CleanCard(CleanCardLibrary.Get(30576089), CleanPlayerId.Rival));
            game.State.Rival.MaxxCDrawActive = true;
            game.PendulumSummon();
            if (game.State.PendingChoice == null)
            {
                throw new InvalidOperationException("Pendulum rule failed: Pendulum Summon should open a target choice.");
            }

            if (game.State.PendingChoice.Options.Count != 3 ||
                IndexOfChoiceOption(game, 1001) >= 0 ||
                game.State.PendingChoice.Options[IndexOfChoiceOption(game, 1000)].CardDefinitionId != levelFour.Id)
            {
                throw new InvalidOperationException("Pendulum choice failed: only legal targets should be shown and their artwork id should be available.");
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

            if (game.State.Rival.Hand.Count != 1)
            {
                throw new InvalidOperationException("Pendulum/Maxx C rule failed: multiple monsters in one Pendulum Summon are one Special Summon event, so Maxx C draws once.");
            }

            CleanDuelGame spiritLimit = ReadyCleanGame();
            spiritLimit.State.Local.SpellsTraps[0] = new CleanCard(scaleOne, CleanPlayerId.Local);
            spiritLimit.State.Local.SpellsTraps[CleanPlayerState.ZoneCount - 1] = new CleanCard(scaleEight, CleanPlayerId.Local);
            spiritLimit.State.Local.Hand.Add(new CleanCard(levelFour, CleanPlayerId.Local));
            spiritLimit.State.Local.Hand.Add(new CleanCard(levelFour, CleanPlayerId.Local));
            spiritLimit.State.Rival.Monsters[0] = new CleanCard(CleanCardLibrary.Get(59822133), CleanPlayerId.Rival);
            spiritLimit.PendulumSummon();
            spiritLimit.ChoosePendingOption(IndexOfChoiceOption(spiritLimit, 1000));
            if (spiritLimit.State.Local.Hand.Count != 1 || spiritLimit.State.PendingChoice != null)
            {
                throw new InvalidOperationException("Blue-Eyes Spirit Dragon failed: neither player may Special Summon 2 or more monsters simultaneously.");
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

        private static bool ReadPrivateBool(CleanDuelGame game, string fieldName)
        {
            FieldInfo field = typeof(CleanDuelGame).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new InvalidOperationException("Private duel-state field was not found: " + fieldName + ".");
            }

            return (bool)field.GetValue(game);
        }
        private static void InvokeBeginDeckResponseContext(CleanDuelGame game, CleanPlayerState controller, CleanCard card, bool sourceWasFaceUp)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("BeginResponseContext", BindingFlags.Instance | BindingFlags.NonPublic);
            Type responseKindType = typeof(CleanDuelGame).GetNestedType("CleanResponseKind", BindingFlags.NonPublic);
            if (method == null || responseKindType == null)
            {
                throw new InvalidOperationException("Response context helper was not found.");
            }

            object deckEffect = Enum.Parse(responseKindType, "DeckEffect");
            method.Invoke(game, new object[] { controller, card, deckEffect, sourceWasFaceUp });
        }

        private static void InvokeDestroyAllMonsters(CleanDuelGame game, CleanPlayerState player, bool opponentCardEffect)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("DestroyAllMonsters", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("DestroyAllMonsters helper was not found.");
            }

            method.Invoke(game, new object[] { player, opponentCardEffect });
        }
        private static void InvokeBeginFieldResponseContext(CleanDuelGame game, CleanPlayerState controller, CleanCard card, bool sourceWasFaceUp)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("BeginResponseContext", BindingFlags.Instance | BindingFlags.NonPublic);
            Type responseKindType = typeof(CleanDuelGame).GetNestedType("CleanResponseKind", BindingFlags.NonPublic);
            if (method == null || responseKindType == null)
            {
                throw new InvalidOperationException("Response context helper was not found.");
            }

            object fieldEffect = Enum.Parse(responseKindType, "FieldEffect");
            method.Invoke(game, new object[] { controller, card, fieldEffect, sourceWasFaceUp });
        }
        private static void InvokeNotifySpecialSummon(CleanDuelGame game, CleanPlayerState player)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("NotifySpecialSummon", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("NotifySpecialSummon helper was not found.");
            }

            method.Invoke(game, new object[] { player });
        }

        private static void InvokeResolveSummonTrigger(CleanDuelGame game, CleanPlayerState player, CleanPlayerState rival, CleanCard card, bool normalSummon)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("ResolveSummonTrigger", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("ResolveSummonTrigger helper was not found.");
            }

            method.Invoke(game, new object[] { player, rival, card, normalSummon });
        }
        private static void InvokeResolveEndPhaseTriggers(CleanDuelGame game, CleanPlayerState player)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("ResolveEndPhaseTriggers", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find ResolveEndPhaseTriggers for document trigger tests.");
            }

            method.Invoke(game, null);
        }

        private static bool InvokeOpenLocalQuickEffectWindow(CleanDuelGame game)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("OpenLocalQuickEffectWindowForRivalTurn", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Opponent-turn card/effect response helper was not found.");
            }

            return (bool)method.Invoke(game, null);
        }
        private static void InvokeRivalBattle(CleanDuelGame game)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("RivalBattle", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("RivalBattle helper was not found.");
            }

            method.Invoke(game, null);
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

        private static bool InvokeDetachXyzMaterial(CleanDuelGame game, CleanPlayerState player, CleanCard xyzMonster)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("DetachXyzMaterial", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find DetachXyzMaterial for Xyz tests.");
            }

            return (bool)method.Invoke(game, new object[] { player, xyzMonster });
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

        private static void ConfirmSpecialSummonPrompt(CleanDuelGame game, string context)
        {
            if (game.State.PendingChoice == null ||
                game.State.PendingChoice.Kind != CleanPromptKind.SpecialSummon ||
                game.State.PendingChoice.CanCancel)
            {
                throw new InvalidOperationException(context + " failed: a non-cancellable SPECIAL SUMMON choice prompt was expected.");
            }

            int confirm = IndexOfChoiceOption(game, 1);
            if (confirm < 0)
            {
                throw new InvalidOperationException(context + " failed: the Special Summon confirmation option was missing.");
            }

            game.ChoosePendingOption(confirm);
        }

        private static void ConfirmTriggerPrompt(CleanDuelGame game, string context)
        {
            if (game.State.PendingChoice == null ||
                game.State.PendingChoice.Kind != CleanPromptKind.TriggerEffect ||
                game.State.PendingChoice.CanCancel)
            {
                throw new InvalidOperationException(context + " failed: a non-cancellable TRIGGER EFFECT choice prompt was expected.");
            }

            int confirm = IndexOfChoiceOption(game, 1);
            if (confirm < 0)
            {
                throw new InvalidOperationException(context + " failed: the Trigger Effect activation option was missing.");
            }

            game.ChoosePendingOption(confirm);
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
            CleanDuelGame firstTurn = new CleanDuelGame();
            firstTurn.StartNewDuel();
            firstTurn.State.Local.Monsters[0] = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local) { SummonedThisTurn = false };
            firstTurn.State.Phase = CleanDuelPhase.Battle;
            if (firstTurn.CanAttackWith(0))
            {
                throw new InvalidOperationException("Phase flow failed: the first player must not be allowed to attack during the first turn of the Duel.");
            }

            firstTurn.State.Phase = CleanDuelPhase.Main;
            firstTurn.State.Local.Deck.Clear();
            firstTurn.State.Local.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local));
            firstTurn.State.Rival.Hand.Clear();
            firstTurn.State.Rival.Deck.Clear();
            firstTurn.State.Rival.Deck.Add(new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Rival));
            firstTurn.AdvancePhase();
            if (firstTurn.State.TurnNumber == 1 && firstTurn.State.Phase == CleanDuelPhase.Battle)
            {
                throw new InvalidOperationException("Phase flow failed: Main Phase 1 should not advance to Battle Phase on the first turn.");
            }

            CleanDuelGame game = new CleanDuelGame();
            game.StartNewDuel();
            if (game.State.Phase != CleanDuelPhase.Standby)
            {
                throw new InvalidOperationException("Phase flow failed: the first Draw Phase should advance to Standby without drawing a sixth opening card.");
            }

            if (game.State.Local.Hand.Count != 5)
            {
                throw new InvalidOperationException("Phase flow failed: the first player must not draw during their first Draw Phase.");
            }

            game.AdvancePhase();
            if (game.State.Phase != CleanDuelPhase.Main)
            {
                throw new InvalidOperationException("Phase flow failed: Standby Phase should advance to Main Phase 1.");
            }

            game.State.TurnNumber = 2;
            game.AdvancePhase();
            if (game.State.Phase != CleanDuelPhase.Battle)
            {
                throw new InvalidOperationException("Phase flow failed: Main Phase 1 should advance to Battle Phase after the first turn restriction has passed.");
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
            if (endFromMain2.State.TurnPlayer != CleanPlayerId.Local || endFromMain2.State.Phase != CleanDuelPhase.Standby || endFromMain2.State.Local.Hand.Count != 1)
            {
                throw new InvalidOperationException("Phase flow failed: Main Phase 2 should go to End Phase, end the turn, draw, and enter Standby Phase.");
            }

            endFromMain2.AdvancePhase();
            if (endFromMain2.State.Phase != CleanDuelPhase.Main)
            {
                throw new InvalidOperationException("Phase flow failed: the new turn's Standby Phase should advance to Main Phase 1.");
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
            if (endFromBattle.State.PendingChoice != null || endFromBattle.State.TurnPlayer != CleanPlayerId.Local || endFromBattle.State.Phase != CleanDuelPhase.Standby || endFromBattle.State.Local.Hand.Count != 1)
            {
                throw new InvalidOperationException("Phase flow failed: Battle Phase End Turn choice should pass the turn, draw, and enter Standby Phase.");
            }

            CleanDuelGame standbyActions = new CleanDuelGame();
            standbyActions.StartNewDuel();
            standbyActions.State.Local.Hand.Clear();
            standbyActions.State.Local.Hand.Add(new CleanCard(CleanCardLibrary.Get(8240199), CleanPlayerId.Local));
            if (standbyActions.CanNormalSummon(0))
            {
                throw new InvalidOperationException("Standby Phase rule failed: normal summons must remain unavailable until Main Phase 1.");
            }

            standbyActions.State.Local.SpellsTraps[0] = new CleanCard(CleanCardLibrary.Get(62089826), CleanPlayerId.Local)
            {
                FaceDown = true,
                SetThisTurn = false
            };
            if (!standbyActions.CanActivateSetCard(0))
            {
                throw new InvalidOperationException("Standby Phase rule failed: a previously Set Trap should be activatable during Standby Phase.");
            }
        }

        private static void AssertDuelRulebookBasics()
        {
            CleanDuelGame setup = new CleanDuelGame();
            setup.StartNewDuel();
            if (setup.State.Local.LifePoints != 8000 || setup.State.Rival.LifePoints != 8000 || setup.State.Local.Hand.Count != 5 || setup.State.Rival.Hand.Count != 5)
            {
                throw new InvalidOperationException("Duel setup rule failed: both players should start with 8000 LP and a 5-card opening hand.");
            }

            CleanDuelGame position = ReadyCleanGame();
            CleanCard blueEyes = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local) { SummonedThisTurn = false };
            position.State.Local.Monsters[0] = blueEyes;
            position.ToggleLocalMonsterStance(0);
            if (blueEyes.Stance != CleanMonsterStance.Defense || !blueEyes.PositionChangedThisTurn)
            {
                throw new InvalidOperationException("Battle position rule failed: a legal manual position change should switch stance and mark the monster for the turn.");
            }

            position.ToggleLocalMonsterStance(0);
            if (blueEyes.Stance != CleanMonsterStance.Defense)
            {
                throw new InvalidOperationException("Battle position rule failed: a monster should not change battle position twice in one turn.");
            }

            CleanDuelGame attackedMain2 = ReadyCleanGame();
            CleanCard attacker = new CleanCard(CleanCardLibrary.Get(89631139), CleanPlayerId.Local) { SummonedThisTurn = false, HasBattled = true };
            attackedMain2.State.Local.Monsters[0] = attacker;
            attackedMain2.State.Phase = CleanDuelPhase.Main2;
            attackedMain2.ToggleLocalMonsterStance(0);
            if (attacker.Stance != CleanMonsterStance.Attack)
            {
                throw new InvalidOperationException("Battle position rule failed: a monster that attacked cannot change battle position in Main Phase 2.");
            }

            CleanDuelGame flip = ReadyCleanGame();
            CleanCard faceDown = new CleanCard(CleanCardLibrary.Get(88241506), CleanPlayerId.Local) { FaceDown = true, Stance = CleanMonsterStance.Defense, SummonedThisTurn = false };
            flip.State.Local.Monsters[0] = faceDown;
            if (!flip.CanFlipSummonMonster(0))
            {
                throw new InvalidOperationException("Flip Summon rule failed: an older face-down monster should be Flip Summonable.");
            }

            flip.FlipSummonMonster(0);
            if (faceDown.FaceDown || faceDown.Stance != CleanMonsterStance.Attack || !faceDown.PositionChangedThisTurn || flip.CanFlipSummonMonster(0))
            {
                throw new InvalidOperationException("Flip Summon rule failed: Flip Summon should turn the monster face-up Attack and consume its position change for the turn.");
            }

            CleanDuelGame draw = ReadyCleanGame();
            draw.State.Local.LifePoints = 0;
            draw.State.Rival.LifePoints = 0;
            InvokeCheckWin(draw);
            if (!draw.State.GameOver || draw.State.WinnerText != "Duel ends in a draw." || draw.State.Local.LifePoints != 0 || draw.State.Rival.LifePoints != 0)
            {
                throw new InvalidOperationException("Victory rule failed: if both players reach 0 LP at the same time, the Duel should be a draw.");
            }

            CleanDuelGame effectWin = ReadyCleanGame();
            InvokeWinByCardEffect(effectWin, effectWin.State.Local, "card effect");
            if (!effectWin.State.GameOver || effectWin.State.WinnerText != "Dr, Wheelie wins by card effect.")
            {
                throw new InvalidOperationException("Victory rule failed: a card effect that says you win should end the Duel for that player.");
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

        private static void InvokeCheckWin(CleanDuelGame game)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("CheckWin", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find CheckWin for victory condition tests.");
            }

            method.Invoke(game, null);
        }

        private static void InvokeWinByCardEffect(CleanDuelGame game, CleanPlayerState winner, string sourceName)
        {
            MethodInfo method = typeof(CleanDuelGame).GetMethod("WinByCardEffect", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException("Could not find WinByCardEffect for card-effect victory tests.");
            }

            method.Invoke(game, new object[] { winner, sourceName });
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
        private static bool HasMonsterWithId(CleanPlayerState player, int cardId)
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (player.Monsters[i] != null && player.Monsters[i].Definition.Id == cardId)
                {
                    return true;
                }
            }

            return false;
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
            game.State.Phase = CleanDuelPhase.Main;
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
