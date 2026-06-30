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

        private void Start()
        {
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
            Refresh();
        }

        public void HandleZoneClicked(CleanPlayerId owner, CleanZoneKind zone, int index)
        {
            if (zone == CleanZoneKind.Monster)
            {
                if (owner == CleanPlayerId.Local)
                {
                    CleanCard card = game.State.Local.Monsters[index];
                    selectedMonster = card == null || selectedMonster == index ? -1 : index;
                    selectedHand = -1;
                    selectedSpellTrap = -1;
                    Refresh();
                    return;
                }

                if (selectedMonster >= 0)
                {
                    game.Attack(selectedMonster, index);
                    ClearSelection();
                    Refresh();
                }

                return;
            }

            if (zone == CleanZoneKind.SpellTrap && owner == CleanPlayerId.Local)
            {
                CleanCard card = game.State.Local.SpellsTraps[index];
                selectedSpellTrap = card == null || selectedSpellTrap == index ? -1 : index;
                selectedHand = -1;
                selectedMonster = -1;
                Refresh();
            }
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
            ClearSelection();
            if (handIndex >= 0)
            {
                if (game.CanActivateHandCard(handIndex))
                {
                    game.ActivateHandCard(handIndex);
                }
                else
                {
                    game.SetSpellTrap(handIndex);
                }
            }
            else if (monsterIndex >= 0)
            {
                game.ActivateMonsterZoneCard(monsterIndex);
            }

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
                hudView.Render(game, selectedCard, selectedHand, selectedMonster, selectedSpellTrap);
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

            if (selectedSpellTrap >= 0 && selectedSpellTrap < CleanPlayerState.ZoneCount)
            {
                return game.State.Local.SpellsTraps[selectedSpellTrap];
            }

            return null;
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

            if (selectedSpellTrap >= 0 && game.State.Local.SpellsTraps[selectedSpellTrap] == null)
            {
                selectedSpellTrap = -1;
            }
        }

        private void ClearSelection()
        {
            selectedHand = -1;
            selectedMonster = -1;
            selectedSpellTrap = -1;
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

            camera.transform.position = new Vector3(0f, 9.7f, -5.9f);
            camera.transform.rotation = Quaternion.Euler(64f, 0f, 0f);
            camera.fieldOfView = 52f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.026f, 0.036f, 0.052f, 1f);
            return camera;
        }

        private void EnsureLighting()
        {
            GameObject key = new GameObject("Clean Duel Key Light", typeof(Light));
            key.transform.SetParent(runtimeRoot, false);
            key.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
            Light light = key.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.color = new Color(1f, 0.94f, 0.86f, 1f);

            GameObject fill = new GameObject("Clean Duel Fill Light", typeof(Light));
            fill.transform.SetParent(runtimeRoot, false);
            fill.transform.position = new Vector3(0f, 5.5f, -4f);
            Light fillLight = fill.GetComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.intensity = 2.2f;
            fillLight.range = 12f;
            fillLight.color = new Color(0.46f, 0.74f, 1f, 1f);
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
