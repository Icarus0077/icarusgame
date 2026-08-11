using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace IcarusGame.CleanDuel
{
    public sealed class CleanDuelBootstrap : MonoBehaviour
    {
        private readonly CleanDuelGame game = new CleanDuelGame();
        private CleanDuelArtCache artCache;
        private CleanDuelBoardView boardView;
        private CleanDuelHudView hudView;
        private CleanDuelCinematicView cinematicView;
        private readonly Queue<CleanDuelCinematicEvent> pendingCinematics = new Queue<CleanDuelCinematicEvent>();
        private Coroutine cinematicRoutine;
        private Transform runtimeRoot;
        private Camera arenaCamera;
        private int selectedHand = -1;
        private int selectedMonster = -1;
        private int selectedSpellTrap = -1;
        private int selectedExtraDeck = -1;
        private int selectedExtraMonster = -1;
        private int selectedGraveyard = -1;
        private bool selectingAttackTarget;
        private bool hasInspection;
        private CleanPlayerId inspectedOwner;
        private CleanZoneKind inspectedZone;
        private int inspectedIndex = -1;

        public CleanDuelAiDifficulty RivalAiDifficulty
        {
            get { return game.RivalAiDifficulty; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureDuelRuntimeExists()
        {
            if (Object.FindFirstObjectByType<CleanDuelBootstrap>(FindObjectsInactive.Include) != null ||
                Object.FindFirstObjectByType<IcarusGame.MasterDuel.MasterDuelRuntime>(FindObjectsInactive.Include) != null)
            {
                return;
            }

            new GameObject("Clean Duel Bootstrap (Automatic)").AddComponent<CleanDuelBootstrap>();
        }

        private void Start()
        {
            RemoveExistingRuntimeObjects();
            runtimeRoot = new GameObject("Clean Duel Runtime Objects").transform;
            artCache = new CleanDuelArtCache();
            arenaCamera = EnsureCamera();
            EnsureLighting();
            EnsureEventSystem();

            boardView = new CleanDuelBoardView(this, artCache);
            boardView.Build(runtimeRoot, arenaCamera);

            hudView = new CleanDuelHudView(this, artCache);
            hudView.Build();

            cinematicView = new CleanDuelCinematicView(artCache);
            cinematicView.Build();

            game.Changed += Refresh;
            game.CinematicRequested += QueueCinematic;
            game.StartNewDuel();
        }

        private void OnDestroy()
        {
            game.Changed -= Refresh;
            game.CinematicRequested -= QueueCinematic;
            if (cinematicView != null)
            {
                cinematicView.Dispose();
                cinematicView = null;
            }
        }

        public void SelectHand(int index)
        {
            selectedHand = selectedHand == index ? -1 : index;
            selectedMonster = -1;
            selectedSpellTrap = -1;
            selectedExtraDeck = -1;
            selectedExtraMonster = -1;
            selectedGraveyard = -1;
            selectingAttackTarget = false;
            ClearInspection();
            Refresh();
        }

        public void HandleZoneClicked(CleanPlayerId owner, CleanZoneKind zone, int index)
        {
            if (selectingAttackTarget && owner == CleanPlayerId.Rival && zone == CleanZoneKind.Monster && selectedMonster >= 0 && game.CanAttackWith(selectedMonster))
            {
                CleanCard target = game.State.Rival.Monsters[index];
                if (target != null)
                {
                    game.Attack(selectedMonster, index);
                    ClearSelection();
                    Refresh();
                    return;
                }
            }

            // Any click outside a valid highlighted target cancels target selection.
            selectingAttackTarget = false;

            if (owner == CleanPlayerId.Local && zone == CleanZoneKind.Graveyard)
            {
                int count = game.State.Local.Graveyard.Count;
                selectedGraveyard = count == 0 ? -1 : selectedGraveyard < 0 ? count - 1 : (selectedGraveyard - 1 + count) % count;
                selectedHand = -1;
                selectedMonster = -1;
                selectedSpellTrap = -1;
                selectedExtraDeck = -1;
                selectedExtraMonster = -1;
                ClearInspection();
                Refresh();
                return;
            }

            if (owner == CleanPlayerId.Local && zone == CleanZoneKind.Monster)
            {
                CleanCard card = game.State.Local.Monsters[index];
                selectedMonster = card == null || selectedMonster == index ? -1 : index;
                selectedHand = -1;
                selectedSpellTrap = -1;
                selectedExtraDeck = -1;
                selectedExtraMonster = -1;
                selectedGraveyard = -1;
                ClearInspection();
                Refresh();
                return;
            }

            if (owner == CleanPlayerId.Local && zone == CleanZoneKind.SpellTrap)
            {
                CleanCard card = game.State.Local.SpellsTraps[index];
                selectedSpellTrap = card == null || selectedSpellTrap == index ? -1 : index;
                selectedHand = -1;
                selectedMonster = -1;
                selectedExtraDeck = -1;
                selectedExtraMonster = -1;
                selectedGraveyard = -1;
                ClearInspection();
                Refresh();
                return;
            }

            if (owner == CleanPlayerId.Local && zone == CleanZoneKind.FieldSpell)
            {
                CleanCard card = game.State.Local.FieldSpell;
                selectedSpellTrap = card == null || selectedSpellTrap == CleanDuelGame.FieldSpellZoneIndex ? -1 : CleanDuelGame.FieldSpellZoneIndex;
                selectedHand = -1;
                selectedMonster = -1;
                selectedExtraDeck = -1;
                selectedExtraMonster = -1;
                selectedGraveyard = -1;
                ClearInspection();
                Refresh();
                return;
            }
            if (owner == CleanPlayerId.Local && zone == CleanZoneKind.ExtraDeck)
            {
                ClearSelection();
                if (game.OpenExtraDeckSummonSelection())
                {
                    Refresh();
                    return;
                }

                if (game.CanPendulumSummon())
                {
                    game.PendulumSummon();
                    Refresh();
                    return;
                }
            }
            if (owner == CleanPlayerId.Local && zone == CleanZoneKind.ExtraMonster)
            {
                CleanCard card = index >= 0 && index < game.State.ExtraMonsters.Length
                    ? game.State.ExtraMonsters[index]
                    : null;
                if (card != null && card.Controller == CleanPlayerId.Local)
                {
                    selectedExtraMonster = selectedExtraMonster == index ? -1 : index;
                    selectedHand = -1;
                    selectedMonster = -1;
                    selectedSpellTrap = -1;
                    selectedExtraDeck = -1;
                    selectedGraveyard = -1;
                    ClearInspection();
                    Refresh();
                    return;
                }
            }

            InspectZone(owner, zone, index);
            Refresh();
        }

        public void NormalSummonSelected()
        {
            int index = selectedHand;
            ClearSelection();
            game.NormalSummon(index);
            Refresh();
        }

        public void SetMonsterSelected()
        {
            int index = selectedHand;
            ClearSelection();
            game.SetMonster(index);
            Refresh();
        }

        public void ActivateOrSetSelected()
        {
            int handIndex = selectedHand;
            int monsterIndex = selectedMonster;
            int extraDeckIndex = selectedExtraDeck;
            int extraMonsterIndex = selectedExtraMonster;
            int graveyardIndex = selectedGraveyard;
            ClearSelection();
            if (handIndex >= 0)
            {
                if (game.CanActivateHandCard(handIndex))
                {
                    game.RequestActivateHandCard(handIndex);
                }
                else
                {
                    game.RequestActivateLocalQuickEffectHandCard(handIndex);
                }
            }
            else if (extraDeckIndex >= 0)
            {
                if (game.CanSynchroSummonFromExtra(extraDeckIndex))
                {
                    game.SynchroSummonFromExtra(extraDeckIndex);
                }
                else if (game.CanLinkSummonFromExtra(extraDeckIndex))
                {
                    game.LinkSummonFromExtra(extraDeckIndex);
                }
                else
                {
                    game.XyzSummonFromExtra(extraDeckIndex);
                }
            }
            else if (monsterIndex >= 0)
            {
                if (game.CanActivateMonsterZoneCard(monsterIndex))
                {
                    game.RequestActivateMonsterZoneCard(monsterIndex);
                }
                else
                {
                    game.RequestActivateLocalQuickEffectMonsterZoneCard(monsterIndex);
                }
            }
            else if (extraMonsterIndex >= 0)
            {
                game.RequestActivateLocalExtraMonsterZoneCard(extraMonsterIndex);
            }
            else if (graveyardIndex >= 0)
            {
                game.RequestActivateGraveyardCard(graveyardIndex);
            }

            Refresh();
        }

        public void SetSpellTrapSelected()
        {
            int handIndex = selectedHand;
            ClearSelection();
            game.SetSpellTrap(handIndex);
            Refresh();
        }

        public void ActivateSelectedZone()
        {
            int spellTrapIndex = selectedSpellTrap;
            int monsterIndex = selectedMonster;
            ClearSelection();
            if (monsterIndex >= 0)
            {
                game.ToggleLocalMonsterStance(monsterIndex);
            }
            else
            {
                if (game.CanActivateLocalSpellTrapResponse(spellTrapIndex))
                {
                    game.ActivateLocalSpellTrapResponse(spellTrapIndex);
                }
                else
                {
                    game.RequestActivateSetCard(spellTrapIndex);
                }
            }
            Refresh();
        }

        public void ChoosePendingOption(int optionIndex)
        {
            game.ChoosePendingOption(optionIndex);
            ClearSelection();
            Refresh();
        }

        public void CancelPendingChoice()
        {
            game.CancelPendingChoice();
            ClearSelection();
            Refresh();
        }
        public void ToggleSelectedMonsterStance()
        {
            int index = selectedMonster;
            game.ToggleLocalMonsterStance(index);
            Refresh();
        }

        public void AttackSelected()
        {
            int index = selectedMonster;
            if (!game.CanAttackWith(index))
            {
                selectingAttackTarget = false;
                Refresh();
                return;
            }

            if (RivalControlsMonster())
            {
                selectingAttackTarget = true;
                Refresh();
                return;
            }

            ClearSelection();
            game.Attack(index, -1);
            Refresh();
        }

        public void AttackDirectly()
        {
            AttackSelected();
        }

        private bool RivalControlsMonster()
        {
            for (int i = 0; i < CleanPlayerState.ZoneCount; i++)
            {
                if (game.State.Rival.Monsters[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        public void PendulumSummon()
        {
            ClearSelection();
            game.PendulumSummon();
            Refresh();
        }

        public void AdvancePhase()
        {
            if (game.State.GameOver)
            {
                RestartDuel();
                return;
            }

            ClearSelection();
            game.AdvancePhase();
            Refresh();
        }

        public void SelectPhase(CleanDuelPhase targetPhase)
        {
            if (!game.CanSelectPhase(targetPhase))
            {
                return;
            }

            ClearSelection();
            game.SelectPhase(targetPhase);
            Refresh();
        }

        public void RestartDuel()
        {
            CancelCinematics();
            ClearSelection();
            game.StartNewDuel();
            Refresh();
        }

        public void RequestSurrender()
        {
            ClearSelection();
            game.RequestSurrender();
            Refresh();
        }

        public void CycleRivalAiDifficulty()
        {
            game.CycleRivalAiDifficulty();
            Refresh();
        }

        private void RemoveExistingRuntimeObjects()
        {
            DestroyNamedObject("Clean Duel Runtime Objects");
            DestroyNamedObject("Modern Master Duel Arena");
            DestroyNamedObject("Top Down Glass Field");
            DestroyNamedObject("Top Down Field Surface");
            DestroyNamedObject("Top Down Clear Field Surface");
            DestroyNamedObject("Glass Duel Surface");
            DestroyNamedObject("Master Duel HUD Rebuild");
            DestroyNamedObject("Duel Cinematic Layer");
        }

        private void DestroyNamedObject(string objectName)
        {
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < objects.Length; i++)
            {
                GameObject stale = objects[i];
                if (stale == null || stale == gameObject || stale.name != objectName || !stale.scene.IsValid())
                {
                    continue;
                }

                DestroyImmediate(stale);
            }
        }

        private void Refresh()
        {
            ClampSelection();
            CleanCard selectedCard = SelectedCard();
            Vector2 boardActionAnchor = Vector2.zero;
            bool hasBoardActionAnchor = false;
            if (boardView != null)
            {
                boardView.Render(game, selectedMonster, selectedSpellTrap, selectedExtraMonster, selectingAttackTarget);
                hasBoardActionAnchor = TryGetSelectedBoardActionAnchor(out boardActionAnchor);
            }

            if (hudView != null)
            {
                hudView.Render(
                    game,
                    selectedCard,
                    CanRevealSelectedFaceDownCard(),
                    selectedHand,
                    selectedMonster,
                    selectedSpellTrap,
                    selectedExtraDeck,
                    selectedExtraMonster,
                    selectedGraveyard,
                    selectingAttackTarget,
                    hasBoardActionAnchor,
                    boardActionAnchor);
            }
        }

        private void QueueCinematic(CleanDuelCinematicEvent cinematicEvent)
        {
            if (cinematicEvent == null || cinematicView == null)
            {
                return;
            }

            pendingCinematics.Enqueue(cinematicEvent);
            if (cinematicRoutine == null && isActiveAndEnabled)
            {
                cinematicRoutine = StartCoroutine(PlayQueuedCinematics());
            }
        }

        private IEnumerator PlayQueuedCinematics()
        {
            try
            {
                while (pendingCinematics.Count > 0)
                {
                    CleanDuelCinematicEvent cinematicEvent = pendingCinematics.Dequeue();
                    IEnumerator playback = cinematicView.Play(cinematicEvent);
                    bool playbackRunning = true;
                    while (playbackRunning)
                    {
                        object current = null;
                        try
                        {
                            playbackRunning = playback.MoveNext();
                            if (playbackRunning)
                            {
                                current = playback.Current;
                            }
                        }
                        catch (System.Exception exception)
                        {
                            playbackRunning = false;
                            cinematicView.HideImmediate();
                            Debug.LogException(exception);
                        }

                        if (playbackRunning)
                        {
                            yield return current;
                        }
                    }
                }
            }
            finally
            {
                cinematicRoutine = null;
            }
        }

        private void CancelCinematics()
        {
            pendingCinematics.Clear();
            if (cinematicRoutine != null)
            {
                StopCoroutine(cinematicRoutine);
                cinematicRoutine = null;
            }

            if (cinematicView != null)
            {
                cinematicView.HideImmediate();
            }
        }

        private bool TryGetSelectedBoardActionAnchor(out Vector2 screenPoint)
        {
            screenPoint = Vector2.zero;
            if (boardView == null)
            {
                return false;
            }

            if (selectedMonster >= 0)
            {
                return boardView.TryGetActionAnchorScreenPoint(CleanPlayerId.Local, CleanZoneKind.Monster, selectedMonster, out screenPoint);
            }

            if (selectedSpellTrap == CleanDuelGame.FieldSpellZoneIndex)
            {
                return boardView.TryGetActionAnchorScreenPoint(CleanPlayerId.Local, CleanZoneKind.FieldSpell, 0, out screenPoint);
            }

            if (selectedSpellTrap >= 0)
            {
                return boardView.TryGetActionAnchorScreenPoint(CleanPlayerId.Local, CleanZoneKind.SpellTrap, selectedSpellTrap, out screenPoint);
            }

            if (selectedExtraMonster >= 0)
            {
                return boardView.TryGetActionAnchorScreenPoint(CleanPlayerId.Local, CleanZoneKind.ExtraMonster, selectedExtraMonster, out screenPoint);
            }

            return false;
        }

        private bool CanRevealSelectedFaceDownCard()
        {
            if (selectedHand >= 0 ||
                selectedMonster >= 0 ||
                selectedSpellTrap >= 0 ||
                selectedExtraDeck >= 0 ||
                selectedExtraMonster >= 0 ||
                selectedGraveyard >= 0)
            {
                return true;
            }

            return hasInspection && inspectedOwner == CleanPlayerId.Local;
        }

        private CleanCard SelectedCard()
        {
            if (selectedHand >= 0 && selectedHand < game.State.Local.Hand.Count)
            {
                return game.State.Local.Hand[selectedHand];
            }

            if (selectedMonster >= 0 && selectedMonster < CleanPlayerState.ZoneCount)
            {
                return game.State.Local.Monsters[selectedMonster];
            }

            if (selectedExtraMonster >= 0 && selectedExtraMonster < game.State.ExtraMonsters.Length)
            {
                return game.State.ExtraMonsters[selectedExtraMonster];
            }

            if (selectedExtraDeck >= 0 && selectedExtraDeck < game.State.Local.ExtraDeck.Count)
            {
                return game.State.Local.ExtraDeck[selectedExtraDeck];
            }

            if (selectedSpellTrap >= 0 && selectedSpellTrap < CleanPlayerState.ZoneCount)
            {
                return game.State.Local.SpellsTraps[selectedSpellTrap];
            }

            if (selectedSpellTrap == CleanDuelGame.FieldSpellZoneIndex)
            {
                return game.State.Local.FieldSpell;
            }

            if (selectedGraveyard >= 0 && selectedGraveyard < game.State.Local.Graveyard.Count)
            {
                return game.State.Local.Graveyard[selectedGraveyard];
            }

            return CardForInspection();
        }

        private void InspectZone(CleanPlayerId owner, CleanZoneKind zone, int index)
        {
            selectedHand = -1;
            selectedMonster = -1;
            selectedSpellTrap = -1;
            selectedExtraDeck = -1;
            selectedExtraMonster = -1;
            selectedGraveyard = -1;
            inspectedOwner = owner;
            inspectedZone = zone;
            inspectedIndex = index;
            hasInspection = true;
            hasInspection = CardForInspection() != null;
        }

        private CleanCard CardForInspection()
        {
            if (!hasInspection)
            {
                return null;
            }

            CleanPlayerState player = game.Player(inspectedOwner);
            switch (inspectedZone)
            {
                case CleanZoneKind.Monster:
                    return inspectedIndex >= 0 && inspectedIndex < CleanPlayerState.ZoneCount ? player.Monsters[inspectedIndex] : null;
                case CleanZoneKind.SpellTrap:
                    return inspectedIndex >= 0 && inspectedIndex < CleanPlayerState.ZoneCount ? player.SpellsTraps[inspectedIndex] : null;
                case CleanZoneKind.FieldSpell:
                    return player.FieldSpell;
                case CleanZoneKind.ExtraMonster:
                    return inspectedIndex >= 0 && inspectedIndex < game.State.ExtraMonsters.Length ? game.State.ExtraMonsters[inspectedIndex] : null;
                case CleanZoneKind.Graveyard:
                    return TopCard(player.Graveyard);
                case CleanZoneKind.Banished:
                    return TopCard(player.Banished);
                case CleanZoneKind.ExtraDeck:
                    return inspectedOwner == CleanPlayerId.Local ? TopCard(player.ExtraDeck) : null;
                default:
                    return null;
            }
        }

        private CleanCard TopCard(System.Collections.Generic.List<CleanCard> cards)
        {
            return cards.Count == 0 ? null : cards[cards.Count - 1];
        }

        private int FirstExtraDeckSummonCandidateFromExtraDeck()
        {
            for (int i = game.State.Local.ExtraDeck.Count - 1; i >= 0; i--)
            {
                if (game.CanSynchroSummonFromExtra(i) || game.CanLinkSummonFromExtra(i) || game.CanXyzSummonFromExtra(i))
                {
                    return i;
                }
            }

            return -1;
        }

        private void ClampSelection()
        {
            if (selectedHand >= game.State.Local.Hand.Count)
            {
                selectedHand = -1;
            }

            if (selectedMonster >= 0 && game.State.Local.Monsters[selectedMonster] == null)
            {
                selectedMonster = -1;
            }

            if (selectedExtraDeck >= game.State.Local.ExtraDeck.Count)
            {
                selectedExtraDeck = -1;
            }

            if (selectedExtraMonster >= 0 &&
                (selectedExtraMonster >= game.State.ExtraMonsters.Length ||
                 game.State.ExtraMonsters[selectedExtraMonster] == null ||
                 game.State.ExtraMonsters[selectedExtraMonster].Controller != CleanPlayerId.Local))
            {
                selectedExtraMonster = -1;
            }

            if (selectedGraveyard >= game.State.Local.Graveyard.Count)
            {
                selectedGraveyard = game.State.Local.Graveyard.Count - 1;
            }

            if (selectedSpellTrap >= 0)
            {
                if (selectedSpellTrap == CleanDuelGame.FieldSpellZoneIndex)
                {
                    if (game.State.Local.FieldSpell == null)
                    {
                        selectedSpellTrap = -1;
                    }
                }
                else if (selectedSpellTrap >= CleanPlayerState.ZoneCount || game.State.Local.SpellsTraps[selectedSpellTrap] == null)
                {
                    selectedSpellTrap = -1;
                }
            }
            if (hasInspection && CardForInspection() == null)
            {
                ClearInspection();
            }

            if (selectingAttackTarget && !game.CanAttackWith(selectedMonster))
            {
                selectingAttackTarget = false;
            }
        }

        private void ClearSelection()
        {
            selectedHand = -1;
            selectedMonster = -1;
            selectedSpellTrap = -1;
            selectedExtraDeck = -1;
            selectedExtraMonster = -1;
            selectedGraveyard = -1;
            selectingAttackTarget = false;
            ClearInspection();
        }

        private void ClearInspection()
        {
            hasInspection = false;
            inspectedIndex = -1;
        }

        private Camera EnsureCamera()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraObject.tag = "MainCamera";
                camera = cameraObject.GetComponent<Camera>();
            }

            camera.transform.position = new Vector3(0f, 13.5f, -9f);
            camera.transform.LookAt(new Vector3(0f, 0f, 0.35f));
            camera.orthographic = false;
            camera.fieldOfView = 35f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = CleanDuelVisualTheme.ArenaVoid;
            DisableCameraPostProcessing(camera);
            return camera;
        }

        private void DisableCameraPostProcessing(Camera camera)
        {
            Component[] components = camera.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                Component component = components[i];
                if (component == null)
                {
                    continue;
                }

                System.Type type = component.GetType();
                if (type.FullName != "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData")
                {
                    continue;
                }

                System.Reflection.PropertyInfo renderPostProcessing = type.GetProperty("renderPostProcessing");
                if (renderPostProcessing != null && renderPostProcessing.CanWrite)
                {
                    renderPostProcessing.SetValue(component, false, null);
                }
            }
        }

        private void EnsureLighting()
        {
            GameObject key = new GameObject("Clean Duel Key Light", typeof(Light));
            key.transform.SetParent(runtimeRoot, false);
            key.transform.rotation = Quaternion.Euler(58f, -24f, 0f);
            Light light = key.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.35f;
            light.color = new Color(0.72f, 0.9f, 1f, 1f);

            GameObject fill = new GameObject("Clean Duel Fill Light", typeof(Light));
            fill.transform.SetParent(runtimeRoot, false);
            fill.transform.position = new Vector3(0f, 5.2f, -1.5f);
            Light fillLight = fill.GetComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.intensity = 2.2f;
            fillLight.range = 16f;
            fillLight.color = new Color(0.42f, 0.72f, 1f, 1f);

            GameObject rim = new GameObject("Clean Duel Rival Rim Light", typeof(Light));
            rim.transform.SetParent(runtimeRoot, false);
            rim.transform.position = new Vector3(0f, 3.4f, 4.6f);
            Light rimLight = rim.GetComponent<Light>();
            rimLight.type = LightType.Point;
            rimLight.intensity = 1.25f;
            rimLight.range = 9f;
            rimLight.color = new Color(1f, 0.24f, 0.16f, 1f);
        }

        private void EnsureEventSystem()
        {
            GameObject go = EventSystem.current != null
                ? EventSystem.current.gameObject
                : new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            StandaloneInputModule legacy = go.GetComponent<StandaloneInputModule>();
            if (legacy != null)
            {
                legacy.enabled = false;
            }

            InputSystemUIInputModule inputModule = go.GetComponent<InputSystemUIInputModule>();
            if (inputModule == null)
            {
                inputModule = go.AddComponent<InputSystemUIInputModule>();
            }

            inputModule.enabled = true;
#else
            StandaloneInputModule inputModule = go.GetComponent<StandaloneInputModule>();
            if (inputModule == null)
            {
                inputModule = go.AddComponent<StandaloneInputModule>();
            }

            inputModule.enabled = true;
#endif
        }
    }
}
