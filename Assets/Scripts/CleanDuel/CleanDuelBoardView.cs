using System.Collections.Generic;
using UnityEngine;

namespace IcarusGame.CleanDuel
{
    public sealed class CleanDuelBoardView
    {
        private readonly CleanDuelArtCache artCache;
        private readonly CleanDuelBootstrap runtime;
        private readonly List<BoardSlot> slots = new List<BoardSlot>();
        private readonly List<PileView> piles = new List<PileView>();
        private Transform root;
        private Camera sceneCamera;
        private Renderer arenaSurface;

        public CleanDuelBoardView(CleanDuelBootstrap runtime, CleanDuelArtCache artCache)
        {
            this.runtime = runtime;
            this.artCache = artCache;
        }

        public void Build(Transform parent, Camera camera)
        {
            sceneCamera = camera;
            root = new GameObject("Modern Master Duel Arena").transform;
            root.SetParent(parent, false);
            root.localPosition = Vector3.zero;

            CreateArena();
            CreateZones(CleanPlayerId.Rival, 1f);
            CreateExtraMonsterZones();
            CreateZones(CleanPlayerId.Local, -1f);
            CreateFieldSpellZones();
            CreatePiles(CleanPlayerId.Rival, 1f);
            CreatePiles(CleanPlayerId.Local, -1f);
        }

        public void Render(CleanDuelGame game, int selectedMonsterZone, int selectedSpellTrapZone, int selectedExtraMonsterZone, bool selectingAttackTarget)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                BoardSlot slot = slots[i];
                CleanCard card = CardForSlot(game, slot);
                bool selected = slot.Owner == CleanPlayerId.Local &&
                                ((slot.Zone == CleanZoneKind.Monster && slot.Index == selectedMonsterZone) ||
                                 (slot.Zone == CleanZoneKind.SpellTrap && slot.Index == selectedSpellTrapZone) ||
                                 (slot.Zone == CleanZoneKind.FieldSpell && selectedSpellTrapZone == CleanDuelGame.FieldSpellZoneIndex) ||
                                 (slot.Zone == CleanZoneKind.ExtraMonster && slot.Index == selectedExtraMonsterZone));
                bool attackTarget = selectingAttackTarget &&
                                    slot.Owner == CleanPlayerId.Rival &&
                                    slot.Zone == CleanZoneKind.Monster &&
                                    card != null;
                bool attackReady = slot.Owner == CleanPlayerId.Local &&
                                   slot.Zone == CleanZoneKind.Monster &&
                                   game.CanAttackWith(slot.Index);
                bool responseReady = slot.Owner == CleanPlayerId.Local &&
                                     slot.Zone == CleanZoneKind.SpellTrap &&
                                     game.CanActivateLocalSpellTrapResponse(slot.Index);
                RenderSlot(slot, card, selected, attackTarget, attackReady, responseReady);
            }

            for (int i = 0; i < piles.Count; i++)
            {
                RenderPile(piles[i], game.Player(piles[i].Owner));
            }
        }

        public bool TryGetActionAnchorScreenPoint(CleanPlayerId owner, CleanZoneKind zone, int index, out Vector2 screenPoint)
        {
            screenPoint = Vector2.zero;
            if (sceneCamera == null)
            {
                return false;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                BoardSlot slot = slots[i];
                if (slot.Owner != owner || slot.Zone != zone || slot.Index != index)
                {
                    continue;
                }

                Transform slotRoot = slot.Card.transform.parent;
                Vector3 worldAnchor = slotRoot.TransformPoint(new Vector3(0f, 0f, 0.82f));
                Vector3 projected = sceneCamera.WorldToScreenPoint(worldAnchor);
                screenPoint = new Vector2(projected.x, projected.y);
                return projected.z > 0f;
            }

            return false;
        }

        private CleanCard CardForSlot(CleanDuelGame game, BoardSlot slot)
        {
            if (slot.Zone == CleanZoneKind.ExtraMonster)
            {
                return slot.Index >= 0 && slot.Index < game.State.ExtraMonsters.Length ? game.State.ExtraMonsters[slot.Index] : null;
            }

            CleanPlayerState player = game.Player(slot.Owner);
            switch (slot.Zone)
            {
                case CleanZoneKind.Monster:
                    return player.Monsters[slot.Index];
                case CleanZoneKind.SpellTrap:
                    return player.SpellsTraps[slot.Index];
                case CleanZoneKind.FieldSpell:
                    return player.FieldSpell;
                default:
                    return null;
            }
        }

        private void CreateArena()
        {
            CreateBox("Arena Shadow", root, new Vector3(0f, -0.25f, 0.18f), new Vector3(16.8f, 0.24f, 9.9f), CleanDuelVisualTheme.ArenaVoid);
            CreateBox("Lower Bevel", root, new Vector3(0f, -0.13f, 0f), new Vector3(15.85f, 0.18f, 9.05f), new Color(0.025f, 0.029f, 0.052f, 1f));
            CreateBox("Machined Outer Frame", root, new Vector3(0f, -0.035f, 0f), new Vector3(15.15f, 0.12f, 8.52f), CleanDuelVisualTheme.FrameMetal);
            CreateBox("Recessed Duel Tray", root, new Vector3(0f, 0.025f, 0f), new Vector3(13.78f, 0.075f, 7.78f), CleanDuelVisualTheme.FrameInset);

            arenaSurface = CreateBox("Teal Duel Field", root, new Vector3(0f, 0.075f, 0f), new Vector3(12.86f, 0.055f, 7.12f), CleanDuelVisualTheme.FieldSurface).GetComponent<Renderer>();
            DisableEmission(arenaSurface.material);
            CreateBox("Rival Field Inlay", root, new Vector3(0f, 0.108f, 1.78f), new Vector3(11.65f, 0.014f, 3.28f), new Color(0.025f, 0.22f, 0.16f, 1f));
            CreateBox("Local Field Inlay", root, new Vector3(0f, 0.108f, -1.78f), new Vector3(11.65f, 0.014f, 3.28f), new Color(0.018f, 0.14f, 0.17f, 1f));

            CreateModernRails();
            CreateSidePlatforms();
            CreateCenterTechnology();
            CreateAmbientReactors();
        }
        private void CreateModernRails()
        {
            CreateBox("Local Lower Rail", root, new Vector3(0f, 0.16f, -3.66f), new Vector3(12.3f, 0.11f, 0.22f), CleanDuelVisualTheme.FrameMetal);
            CreateBox("Local Rail Light", root, new Vector3(0f, 0.222f, -3.55f), new Vector3(11.72f, 0.022f, 0.045f), CleanDuelVisualTheme.LocalAccent);
            CreateBox("Rival Upper Rail", root, new Vector3(0f, 0.16f, 3.66f), new Vector3(12.3f, 0.11f, 0.22f), CleanDuelVisualTheme.FrameMetal);
            CreateBox("Rival Rail Light", root, new Vector3(0f, 0.222f, 3.55f), new Vector3(11.72f, 0.022f, 0.045f), CleanDuelVisualTheme.RivalAccent);
            CreateBox("Left Structural Rail", root, new Vector3(-6.42f, 0.15f, 0f), new Vector3(0.24f, 0.1f, 7.28f), CleanDuelVisualTheme.FrameMetal);
            CreateBox("Right Structural Rail", root, new Vector3(6.42f, 0.15f, 0f), new Vector3(0.24f, 0.1f, 7.28f), CleanDuelVisualTheme.FrameMetal);
            CreateBox("Center Bridge", root, new Vector3(0f, 0.142f, 0f), new Vector3(10.9f, 0.055f, 0.34f), new Color(0.115f, 0.11f, 0.17f, 1f));
            CreateBox("Center Energy Seam", root, new Vector3(0f, 0.177f, 0f), new Vector3(8.7f, 0.018f, 0.035f), new Color(0.55f, 0.38f, 0.88f, 1f));

            float[] ribs = { -4.18f, -2.5f, -0.84f, 0.84f, 2.5f, 4.18f };
            for (int i = 0; i < ribs.Length; i++)
            {
                CreateBox("Field Rib " + i, root, new Vector3(ribs[i], 0.123f, 0f), new Vector3(0.025f, 0.018f, 6.35f), new Color(0.34f, 0.48f, 0.48f, 0.28f));
            }
        }
        private void CreateSidePlatforms()
        {
            CreateBox("Local Extra Dock", root, new Vector3(-5.42f, 0.145f, -2.92f), new Vector3(1.38f, 0.1f, 1.74f), new Color(0.075f, 0.035f, 0.11f, 1f));
            CreateBox("Local Deck Dock", root, new Vector3(5.42f, 0.145f, -2.92f), new Vector3(1.38f, 0.1f, 1.74f), new Color(0.025f, 0.07f, 0.12f, 1f));
            CreateBox("Rival Deck Dock", root, new Vector3(-5.42f, 0.145f, 2.92f), new Vector3(1.38f, 0.1f, 1.74f), new Color(0.12f, 0.055f, 0.025f, 1f));
            CreateBox("Rival Extra Dock", root, new Vector3(5.42f, 0.145f, 2.92f), new Vector3(1.38f, 0.1f, 1.74f), new Color(0.085f, 0.035f, 0.115f, 1f));
        }
        private void CreateCenterTechnology()
        {
            CreateBox("Center Console", root, new Vector3(0f, 0.195f, 0f), new Vector3(1.82f, 0.09f, 0.42f), CleanDuelVisualTheme.FrameInset);
            CreateBox("Center Console Light", root, new Vector3(0f, 0.245f, 0f), new Vector3(1.16f, 0.018f, 0.06f), CleanDuelVisualTheme.GoldAccent);
            CreateCylinder("Center Lens Rim", root, new Vector3(0f, 0.255f, 0f), new Vector3(0.46f, 0.018f, 0.46f), CleanDuelVisualTheme.VioletAccent);
            CreateCylinder("Center Lens", root, new Vector3(0f, 0.276f, 0f), new Vector3(0.3f, 0.013f, 0.3f), new Color(0.65f, 0.94f, 1f, 1f));
        }
        private void CreateAmbientReactors()
        {
            CreateReactor("Local Left Reactor", -6.14f, -3.42f, CleanDuelVisualTheme.LocalAccent);
            CreateReactor("Local Right Reactor", 6.14f, -3.42f, CleanDuelVisualTheme.LocalAccent);
            CreateReactor("Rival Left Reactor", -6.14f, 3.42f, CleanDuelVisualTheme.RivalAccent);
            CreateReactor("Rival Right Reactor", 6.14f, 3.42f, CleanDuelVisualTheme.RivalAccent);
        }

        private void CreateReactor(string name, float x, float z, Color accent)
        {
            Transform reactor = new GameObject(name).transform;
            reactor.SetParent(root, false);
            reactor.localPosition = new Vector3(x, 0.15f, z);
            CreateCylinder("Housing", reactor, Vector3.zero, new Vector3(0.48f, 0.07f, 0.48f), CleanDuelVisualTheme.FrameInset);
            CreateCylinder("Energy Rim", reactor, new Vector3(0f, 0.085f, 0f), new Vector3(0.34f, 0.025f, 0.34f), accent);
            CreateCylinder("Energy Core", reactor, new Vector3(0f, 0.115f, 0f), new Vector3(0.2f, 0.018f, 0.2f), Color.Lerp(accent, Color.white, 0.58f));
        }
        private void CreateZones(CleanPlayerId owner, float side)
        {
            float[] columns = { -3.34f, -1.67f, 0f, 1.67f, 3.34f };
            float monsterZ = side * 0.98f;
            float spellZ = side * 2.28f;

            for (int i = 0; i < columns.Length; i++)
            {
                slots.Add(CreateSlot(owner, CleanZoneKind.Monster, i, columns[i], monsterZ, 1.12f));
                slots.Add(CreateSlot(owner, CleanZoneKind.SpellTrap, i, columns[i], spellZ, 1.08f));
            }
        }
        private void CreateExtraMonsterZones()
        {
            slots.Add(CreateSlot(CleanPlayerId.Local, CleanZoneKind.ExtraMonster, 0, -0.82f, 0f, 0.92f));
            slots.Add(CreateSlot(CleanPlayerId.Rival, CleanZoneKind.ExtraMonster, 1, 0.82f, 0f, 0.92f));
        }

        private void CreateFieldSpellZones()
        {
            slots.Add(CreateSlot(CleanPlayerId.Local, CleanZoneKind.FieldSpell, 0, -5.42f, -1.66f, 0.98f));
            slots.Add(CreateSlot(CleanPlayerId.Rival, CleanZoneKind.FieldSpell, 0, 5.42f, 1.66f, 0.98f));
        }

        private void CreatePiles(CleanPlayerId owner, float side)
        {
            float deckX = owner == CleanPlayerId.Local ? 5.42f : -5.42f;
            float extraX = owner == CleanPlayerId.Local ? -5.42f : 5.42f;
            float deckZ = side * 2.92f;
            float graveZ = side * 1.64f;
            float banishedZ = graveZ - side * 1.22f;
            float pileYaw = owner == CleanPlayerId.Local ? 0f : 180f;

            piles.Add(CreatePile(owner, CleanZoneKind.Deck, deckX, deckZ, pileYaw));
            piles.Add(CreatePile(owner, CleanZoneKind.Graveyard, deckX, graveZ, pileYaw));
            piles.Add(CreatePile(owner, CleanZoneKind.Banished, deckX, banishedZ, pileYaw));
            piles.Add(CreatePile(owner, CleanZoneKind.ExtraDeck, extraX, deckZ, pileYaw));
        }
        private BoardSlot CreateSlot(CleanPlayerId owner, CleanZoneKind zone, int index, float x, float z, float size)
        {
            Transform slotRoot = new GameObject(owner + " " + zone + " " + index).transform;
            slotRoot.SetParent(root, false);
            slotRoot.localPosition = new Vector3(x, 0.105f, z);

            Color accent = ZoneAccent(owner, zone);
            Renderer frame = CreateZoneDecoration("Zone Frame", 0f, 0f, size, accent, slotRoot, false);
            CreateCylinder("Zone Halo", slotRoot, new Vector3(0f, 0.068f, 0f), new Vector3(size * 0.7f, 0.012f, size * 0.7f), Color.Lerp(accent, Color.white, 0.22f));
            Renderer coreRenderer = CreateCylinder("Zone Lens", slotRoot, new Vector3(0f, 0.085f, 0f), new Vector3(size * 0.54f, 0.012f, size * 0.54f), CleanDuelVisualTheme.FieldShadow).GetComponent<Renderer>();
            Renderer card = CreateCardSurface("Card", slotRoot, new Vector3(0f, 0.095f, 0f), new Vector2(0.82f, 1.18f), new Color(0.1f, 0.055f, 0.032f, 1f));
            List<Renderer> xyzMaterialCards = new List<Renderer>();
            for (int materialIndex = 0; materialIndex < 4; materialIndex++)
            {
                Renderer materialCard = CreateCardSurface(
                    "Xyz Material " + materialIndex,
                    slotRoot,
                    new Vector3(0f, 0.082f + materialIndex * 0.002f, 0f),
                    new Vector2(0.82f, 1.18f),
                    new Color(0.05f, 0.05f, 0.07f, 1f));
                Collider materialCollider = materialCard.GetComponent<Collider>();
                if (materialCollider != null)
                {
                    materialCollider.enabled = false;
                }

                materialCard.gameObject.SetActive(false);
                xyzMaterialCards.Add(materialCard);
            }
            TextMesh label = CreateLabel("Name", slotRoot, new Vector3(-0.45f, 0.16f, -0.47f), string.Empty, 24);
            TextMesh stats = CreateLabel("Stats", slotRoot, new Vector3(-0.34f, 0.16f, 0.45f), string.Empty, 22);

            AddTarget(frame.gameObject, owner, zone, index);
            AddTarget(card.gameObject, owner, zone, index);
            card.gameObject.SetActive(false);
            label.gameObject.SetActive(false);
            stats.gameObject.SetActive(false);

            return new BoardSlot
            {
                Owner = owner,
                Zone = zone,
                Index = index,
                Frame = frame,
                Core = coreRenderer,
                Card = card,
                XyzMaterialCards = xyzMaterialCards,
                Label = label,
                Stats = stats
            };
        }

        private Renderer CreateZoneDecoration(string name, float x, float z, float size, Color frameColor)
        {
            return CreateZoneDecoration(name, x, z, size, frameColor, root, false);
        }

        private Renderer CreateZoneDecoration(string name, float x, float z, float size, Color frameColor, Transform parent, bool diamond)
        {
            Transform zoneRoot = new GameObject(name).transform;
            zoneRoot.SetParent(parent, false);
            zoneRoot.localPosition = new Vector3(x, 0.032f, z);
            if (diamond)
            {
                zoneRoot.localRotation = Quaternion.Euler(0f, 45f, 0f);
            }

            CreateBox("Zone Shadow", zoneRoot, new Vector3(0f, -0.012f, -0.025f), new Vector3(size * 1.25f, 0.045f, size * 0.98f), new Color(0.004f, 0.01f, 0.018f, 1f));
            CreateBox("Machined Zone Plate", zoneRoot, Vector3.zero, new Vector3(size * 1.2f, 0.055f, size * 0.93f), CleanDuelVisualTheme.FrameMetal);
            Renderer rim = CreateBox("Illuminated Zone Rim", zoneRoot, new Vector3(0f, 0.038f, 0f), new Vector3(size * 1.12f, 0.025f, size * 0.85f), frameColor).GetComponent<Renderer>();
            CreateBox("Recessed Zone Bed", zoneRoot, new Vector3(0f, 0.057f, 0f), new Vector3(size * 1.02f, 0.022f, size * 0.75f), CleanDuelVisualTheme.FieldShadow);
            float cornerX = size * 0.54f;
            float cornerZ = size * 0.39f;
            Vector3 cornerScale = new Vector3(size * 0.14f, 0.035f, size * 0.11f);
            CreateBox("Corner TL", zoneRoot, new Vector3(-cornerX, 0.066f, cornerZ), cornerScale, Color.Lerp(frameColor, CleanDuelVisualTheme.FrameMetal, 0.35f));
            CreateBox("Corner TR", zoneRoot, new Vector3(cornerX, 0.066f, cornerZ), cornerScale, Color.Lerp(frameColor, CleanDuelVisualTheme.FrameMetal, 0.35f));
            CreateBox("Corner BL", zoneRoot, new Vector3(-cornerX, 0.066f, -cornerZ), cornerScale, Color.Lerp(frameColor, CleanDuelVisualTheme.FrameMetal, 0.35f));
            CreateBox("Corner BR", zoneRoot, new Vector3(cornerX, 0.066f, -cornerZ), cornerScale, Color.Lerp(frameColor, CleanDuelVisualTheme.FrameMetal, 0.35f));
            return rim;
        }
        private PileView CreatePile(CleanPlayerId owner, CleanZoneKind zone, float x, float z, float yaw)
        {
            Transform pileRoot = new GameObject(owner + " " + zone).transform;
            pileRoot.SetParent(root, false);
            pileRoot.localPosition = new Vector3(x, 0.105f, z);
            pileRoot.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Color accent = zone == CleanZoneKind.Deck
                ? new Color(0.04f, 0.78f, 1f, 1f)
                : zone == CleanZoneKind.ExtraDeck
                    ? new Color(0.72f, 0.15f, 1f, 1f)
                    : zone == CleanZoneKind.Banished
                        ? new Color(0.95f, 0.92f, 0.72f, 1f)
                        : new Color(1f, 0.32f, 0.08f, 1f);
            Color stackColor = zone == CleanZoneKind.Deck
                ? new Color(0.045f, 0.08f, 0.15f, 1f)
                : zone == CleanZoneKind.ExtraDeck
                    ? new Color(0.09f, 0.04f, 0.16f, 1f)
                    : zone == CleanZoneKind.Banished
                        ? new Color(0.16f, 0.14f, 0.1f, 1f)
                        : new Color(0.12f, 0.06f, 0.07f, 1f);

            Renderer frame = CreateBox("Pile Frame", pileRoot, Vector3.zero, new Vector3(1.04f, 0.045f, 1.4f), accent).GetComponent<Renderer>();
            Renderer stack = CreateBox("Pile Stack", pileRoot, new Vector3(0f, 0.064f, 0f), new Vector3(0.78f, 0.06f, 1.08f), stackColor).GetComponent<Renderer>();
            CreateBox("Pile Front Light", pileRoot, new Vector3(0f, 0.103f, -0.62f), new Vector3(0.64f, 0.02f, 0.07f), accent);
            TextMesh label = CreateLabel("Pile Label", pileRoot, new Vector3(-0.42f, 0.16f, -0.48f), string.Empty, 22);
            AddTarget(frame.gameObject, owner, zone, 0);
            AddTarget(stack.gameObject, owner, zone, 0);

            return new PileView { Owner = owner, Zone = zone, Stack = stack, Label = label };
        }

        private void RenderSlot(
            BoardSlot slot,
            CleanCard card,
            bool selected,
            bool attackTarget,
            bool attackReady,
            bool responseReady)
        {
            bool hasCard = card != null;
            slot.Card.gameObject.SetActive(hasCard);
            slot.Label.gameObject.SetActive(false);
            slot.Stats.gameObject.SetActive(false);
            RenderXyzMaterialStack(slot, card);
            Color readyFrame = attackReady
                ? new Color(0.05f, 0.92f, 0.42f, 1f)
                : responseReady
                    ? new Color(0.92f, 0.18f, 0.96f, 1f)
                    : ZoneAccent(slot.Owner, slot.Zone);
            Color readyCore = attackReady
                ? new Color(0.16f, 1f, 0.52f, 1f)
                : responseReady
                    ? new Color(1f, 0.34f, 1f, 1f)
                    : hasCard
                        ? CleanDuelVisualTheme.FieldShadow
                        : slot.Owner == CleanPlayerId.Local ? CleanDuelVisualTheme.LocalGlow : CleanDuelVisualTheme.RivalGlow;
            Color frameColor = selected ? new Color(1f, 0.88f, 0.18f, 1f) : attackTarget ? new Color(1f, 0.16f, 0.12f, 1f) : readyFrame;
            Color coreColor = selected ? new Color(1f, 0.82f, 0.18f, 1f) : attackTarget ? new Color(1f, 0.32f, 0.08f, 1f) : readyCore;
            SetColor(slot.Frame, frameColor);
            SetColor(slot.Core, coreColor);
            if (!hasCard)
            {
                return;
            }

            ApplyCardLayout(slot, card);
            Texture2D texture = card.FaceDown ? artCache.GetCardBackTexture() : artCache.GetTexture(card.Definition.Id);
            Material material = slot.Card.material;
            SetTexture(material, texture);
            SetColor(slot.Card, texture == null ? artCache.CardColor(card) : Color.white);
            if (selected)
            {
                SetColor(slot.Card, new Color(1f, 0.82f, 0.45f, 1f));
            }

            if (texture == null)
            {
                slot.Label.gameObject.SetActive(true);
                slot.Stats.gameObject.SetActive(true);
                slot.Label.text = ShortName(card.PublicName, 14);
                slot.Stats.text = CardStats(card);
                FaceCamera(slot.Label.transform);
                FaceCamera(slot.Stats.transform);
            }
        }

        private void ApplyCardLayout(BoardSlot slot, CleanCard card)
        {
            bool defenseMonster = slot.Zone == CleanZoneKind.Monster &&
                                  card.Definition.Kind == CleanCardKind.Monster &&
                                  card.Stance == CleanMonsterStance.Defense;
            slot.Card.transform.localScale = new Vector3(1f, 1f, 1f);
            float ownerYaw = slot.Owner == CleanPlayerId.Local ? 0f : 180f;
            float stanceYaw = defenseMonster ? 90f : 0f;
            slot.Card.transform.localRotation = Quaternion.Euler(0f, ownerYaw + stanceYaw, 0f);
            slot.Card.transform.localPosition = new Vector3(0f, 0.12f, 0f);
        }

        private void RenderXyzMaterialStack(BoardSlot slot, CleanCard card)
        {
            int materialCount = card == null ? 0 : card.XyzMaterials.Count;
            int displayCount = Mathf.Min(materialCount, slot.XyzMaterialCards.Count);
            for (int i = 0; i < slot.XyzMaterialCards.Count; i++)
            {
                Renderer materialCard = slot.XyzMaterialCards[i];
                bool visible = i < displayCount;
                materialCard.gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                CleanCard xyzMaterial = card.XyzMaterials[i];
                Texture2D texture = artCache.GetTexture(xyzMaterial.Definition.Id);
                SetTexture(materialCard.material, texture);
                SetColor(materialCard, texture == null ? artCache.CardColor(xyzMaterial) : Color.white);
                float ownerYaw = slot.Owner == CleanPlayerId.Local ? 0f : 180f;
                float fanOffset = (i - (displayCount - 1) * 0.5f) * 5f;
                materialCard.transform.localRotation = Quaternion.Euler(0f, ownerYaw + fanOffset, 0f);
                materialCard.transform.localPosition = new Vector3(
                    (i - (displayCount - 1) * 0.5f) * 0.045f,
                    0.094f + i * 0.002f,
                    0.055f - i * 0.012f);
                materialCard.transform.localScale = new Vector3(0.98f, 1f, 0.98f);
            }
        }

        private void RenderPile(PileView pile, CleanPlayerState player)
        {
            int count = PileCount(pile.Zone, player);
            pile.Stack.gameObject.SetActive(count > 0);
            if (count > 0 && (pile.Zone == CleanZoneKind.Deck || pile.Zone == CleanZoneKind.ExtraDeck))
            {
                Texture2D backTexture = artCache.GetCardBackTexture();
                if (backTexture != null)
                {
                    SetTexture(pile.Stack.material, backTexture);
                    SetColor(pile.Stack, Color.white);
                }
            }
            else if (count > 0 && pile.Zone == CleanZoneKind.Banished)
            {
                CleanCard topBanished = TopCard(player.Banished);
                Texture2D banishedTexture = topBanished == null ? null : artCache.GetTexture(topBanished.Definition.Id);
                SetTexture(pile.Stack.material, banishedTexture);
                SetColor(pile.Stack, banishedTexture == null ? new Color(0.16f, 0.14f, 0.1f, 1f) : Color.white);
            }

            float height = 0.045f + Mathf.Clamp(count, 0, 35) * 0.0035f;
            pile.Stack.transform.localScale = new Vector3(0.78f, height, 1.08f);
            pile.Stack.transform.localPosition = new Vector3(0f, 0.032f + height * 0.5f, 0f);
            pile.Label.text = PileLabel(pile.Zone) + "\n" + count;
            FaceCamera(pile.Label.transform);
        }

        private int PileCount(CleanZoneKind zone, CleanPlayerState player)
        {
            switch (zone)
            {
                case CleanZoneKind.Deck:
                    return player.Deck.Count;
                case CleanZoneKind.ExtraDeck:
                    return player.ExtraDeck.Count;
                case CleanZoneKind.Banished:
                    return player.Banished.Count;
                default:
                    return player.Graveyard.Count;
            }
        }

        private string PileLabel(CleanZoneKind zone)
        {
            switch (zone)
            {
                case CleanZoneKind.Deck:
                    return "DECK";
                case CleanZoneKind.ExtraDeck:
                    return "EX";
                case CleanZoneKind.Banished:
                    return "BAN";
                default:
                    return "GY";
            }
        }

        private CleanCard TopCard(List<CleanCard> cards)
        {
            return cards.Count == 0 ? null : cards[cards.Count - 1];
        }

        private string CardStats(CleanCard card)
        {
            if (card.FaceDown)
            {
                return "SET";
            }

            if (card.Definition.Kind != CleanCardKind.Monster)
            {
                return card.Definition.Kind.ToString().ToUpperInvariant();
            }

            string combatStats = card.Stance == CleanMonsterStance.Attack ? "ATK " + card.CurrentAttack : "DEF " + card.CurrentDefense;
            string typeLine = card.Definition.TypeLine ?? string.Empty;
            if (typeLine.IndexOf("Xyz", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return combatStats + "  MAT " + card.XyzMaterials.Count;
            }

            return combatStats;
        }

        private string ShortName(string value, int max)
        {
            return value.Length <= max ? value : value.Substring(0, max - 1) + ".";
        }

        private Color ZoneAccent(CleanPlayerId owner, CleanZoneKind zone)
        {
            if (zone == CleanZoneKind.FieldSpell)
            {
                return owner == CleanPlayerId.Local ? new Color(0.12f, 0.78f, 0.5f, 1f) : CleanDuelVisualTheme.RivalAccent;
            }

            if (zone == CleanZoneKind.ExtraMonster)
            {
                return owner == CleanPlayerId.Local ? CleanDuelVisualTheme.LocalAccent : CleanDuelVisualTheme.RivalAccent;
            }

            if (owner == CleanPlayerId.Local)
            {
                return CleanDuelVisualTheme.LocalAccent;
            }

            return CleanDuelVisualTheme.RivalAccent;
        }

        private void AddTarget(GameObject go, CleanPlayerId owner, CleanZoneKind zone, int index)
        {
            CleanDuelZoneTarget target = go.AddComponent<CleanDuelZoneTarget>();
            target.Runtime = runtime;
            target.Owner = owner;
            target.Zone = zone;
            target.Index = index;
        }

        private GameObject CreateBox(string name, Transform parent, Vector3 localPosition, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = CreateMaterial(color);
            return go;
        }

        private GameObject CreateCylinder(string name, Transform parent, Vector3 localPosition, Vector3 scale, Color color)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().material = CreateMaterial(color);
            return go;
        }

        private Renderer CreateCardSurface(string name, Transform parent, Vector3 localPosition, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;

            float halfWidth = size.x * 0.5f;
            float halfHeight = size.y * 0.5f;
            Mesh mesh = new Mesh();
            mesh.name = name + " Mesh";
            mesh.vertices = new[]
            {
                new Vector3(-halfWidth, 0f, -halfHeight),
                new Vector3(halfWidth, 0f, -halfHeight),
                new Vector3(-halfWidth, 0f, halfHeight),
                new Vector3(halfWidth, 0f, halfHeight)
            };
            mesh.uv = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 1f),
                new Vector2(1f, 1f)
            };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateNormals();
            go.GetComponent<MeshFilter>().sharedMesh = mesh;

            MeshRenderer renderer = go.GetComponent<MeshRenderer>();
            renderer.material = CreateMaterial(color);

            BoxCollider collider = go.GetComponent<BoxCollider>();
            collider.size = new Vector3(size.x, 0.08f, size.y);
            collider.center = Vector3.zero;
            return renderer;
        }

        private TextMesh CreateLabel(string name, Transform parent, Vector3 localPosition, string text, int size)
        {
            GameObject go = new GameObject(name, typeof(TextMesh));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            TextMesh mesh = go.GetComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = size;
            mesh.characterSize = 0.03f;
            mesh.anchor = TextAnchor.MiddleLeft;
            mesh.alignment = TextAlignment.Left;
            mesh.color = Color.white;
            return mesh;
        }

        private Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            SetColor(material, color);
            SetMaterialFloat(material, "_Smoothness", 0.28f);
            SetMaterialFloat(material, "_Glossiness", 0.28f);
            SetMaterialFloat(material, "_Metallic", 0.16f);
            return material;
        }

        private void DisableEmission(Material material)
        {
            if (material != null && material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", Color.black);
                material.DisableKeyword("_EMISSION");
            }
        }

        private void MakeMaterialMatte(Material material)
        {
            if (material == null)
            {
                return;
            }

            SetMaterialFloat(material, "_Smoothness", 0f);
            SetMaterialFloat(material, "_Glossiness", 0f);
            SetMaterialFloat(material, "_Metallic", 0f);
            SetMaterialFloat(material, "_SpecularHighlights", 0f);
            SetMaterialFloat(material, "_EnvironmentReflections", 0f);
            if (material.HasProperty("_SpecColor"))
            {
                material.SetColor("_SpecColor", Color.black);
            }

            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", Color.black);
                material.DisableKeyword("_EMISSION");
            }
        }

        private void SetMaterialFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private void SetColor(Renderer renderer, Color color)
        {
            if (renderer != null)
            {
                SetColor(renderer.material, color);
            }
        }

        private void SetColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private void SetTexture(Material material, Texture texture)
        {
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }

            MakeMaterialMatte(material);
        }

        private void FaceCamera(Transform target)
        {
            if (sceneCamera == null || target == null)
            {
                return;
            }

            Vector3 direction = sceneCamera.transform.position - target.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                target.rotation = Quaternion.LookRotation(direction);
            }
        }

        private sealed class BoardSlot
        {
            public CleanPlayerId Owner;
            public CleanZoneKind Zone;
            public int Index;
            public Renderer Frame;
            public Renderer Core;
            public Renderer Card;
            public List<Renderer> XyzMaterialCards;
            public TextMesh Label;
            public TextMesh Stats;
        }

        private sealed class PileView
        {
            public CleanPlayerId Owner;
            public CleanZoneKind Zone;
            public Renderer Stack;
            public TextMesh Label;
        }
    }
}
