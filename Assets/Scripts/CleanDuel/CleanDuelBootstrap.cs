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
        private Transform runtimeRoot;
        private Camera arenaCamera;
        private int selectedHand = -1;
        private int selectedMonster = -1;
        private int selectedSpellTrap = -1;
        private int selectedExtraDeck = -1;
        private bool hasInspection;
        private CleanPlayerId inspectedOwner;
        private CleanZoneKind inspectedZone;
        private int inspectedIndex = -1;

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

            game.Changed += Refresh;
            game.StartNewDuel();
        }

        public void SelectHand(int index)
        {
            selectedHand = selectedHand == index ? -1 : index;
            selectedMonster = -1;
            selectedSpellTrap = -1;
            selectedExtraDeck = -1;
            ClearInspection();
            Refresh();
        }

        public void HandleZoneClicked(CleanPlayerId owner, CleanZoneKind zone, int index)
        {
            if (owner == CleanPlayerId.Local && zone == CleanZoneKind.Monster)
            {
                CleanCard card = game.State.Local.Monsters[index];
                selectedMonster = card == null || selectedMonster == index ? -1 : index;
                selectedHand = -1;
                selectedSpellTrap = -1;
                selectedExtraDeck = -1;
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
                ClearInspection();
                Refresh();
                return;
            }
            if (owner == CleanPlayerId.Local && zone == CleanZoneKind.ExtraDeck)
            {
                int linkIndex = FirstLinkSummonCandidateFromExtraDeck();
                if (linkIndex >= 0)
                {
                    selectedExtraDeck = selectedExtraDeck == linkIndex ? -1 : linkIndex;
                    selectedHand = -1;
                    selectedMonster = -1;
                    selectedSpellTrap = -1;
                    ClearInspection();
                    Refresh();
                    return;
                }
            }
            if (owner == CleanPlayerId.Rival && zone == CleanZoneKind.Monster && selectedMonster >= 0 && game.CanAttackWith(selectedMonster))
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
            ClearSelection();
            if (handIndex >= 0)
            {
                if (game.CanActivateHandCard(handIndex))
                {
                    game.ActivateHandCard(handIndex);
                }
                else
                {
                    game.ActivateLocalQuickEffectHandCard(handIndex);
                }
            }
            else if (extraDeckIndex >= 0)
            {
                game.LinkSummonFromExtra(extraDeckIndex);
            }
            else if (monsterIndex >= 0)
            {
                if (game.CanActivateMonsterZoneCard(monsterIndex))
                {
                    game.ActivateMonsterZoneCard(monsterIndex);
                }
                else
                {
                    game.ActivateLocalQuickEffectMonsterZoneCard(monsterIndex);
                }
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
            int index = selectedSpellTrap;
            ClearSelection();
            game.ActivateSetCard(index);
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

        public void AttackDirectly()
        {
            int index = selectedMonster;
            ClearSelection();
            game.Attack(index, -1);
            Refresh();
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

        public void RestartDuel()
        {
            ClearSelection();
            game.StartNewDuel();
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
            if (boardView != null)
            {
                boardView.Render(game, selectedMonster, selectedSpellTrap);
            }

            if (hudView != null)
            {
                hudView.Render(game, selectedCard, selectedHand, selectedMonster, selectedSpellTrap, selectedExtraDeck);
            }
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

            return CardForInspection();
        }

        private void InspectZone(CleanPlayerId owner, CleanZoneKind zone, int index)
        {
            selectedHand = -1;
            selectedMonster = -1;
            selectedSpellTrap = -1;
            selectedExtraDeck = -1;
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

        private int FirstLinkSummonCandidateFromExtraDeck()
        {
            for (int i = game.State.Local.ExtraDeck.Count - 1; i >= 0; i--)
            {
                if (game.CanLinkSummonFromExtra(i))
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
        }

        private void ClearSelection()
        {
            selectedHand = -1;
            selectedMonster = -1;
            selectedSpellTrap = -1;
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

            camera.transform.position = new Vector3(0f, 14.8f, 0f);
            camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            camera.orthographic = true;
            camera.orthographicSize = 5.25f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 80f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.006f, 0.008f, 0.016f, 1f);
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
            key.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            Light light = key.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            light.color = new Color(0.78f, 0.95f, 1f, 1f);

            GameObject fill = new GameObject("Clean Duel Fill Light", typeof(Light));
            fill.transform.SetParent(runtimeRoot, false);
            fill.transform.position = new Vector3(0f, 3.6f, 0f);
            Light fillLight = fill.GetComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.intensity = 1.85f;
            fillLight.range = 14f;
            fillLight.color = new Color(0.5f, 0.74f, 1f, 1f);
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
