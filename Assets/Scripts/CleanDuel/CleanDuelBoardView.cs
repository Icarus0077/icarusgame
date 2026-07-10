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

        public void Render(CleanDuelGame game, int selectedMonsterZone, int selectedSpellTrapZone)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                BoardSlot slot = slots[i];
                CleanCard card = CardForSlot(game, slot);
                bool selected = slot.Owner == CleanPlayerId.Local &&
                                ((slot.Zone == CleanZoneKind.Monster && slot.Index == selectedMonsterZone) ||
                                 (slot.Zone == CleanZoneKind.SpellTrap && slot.Index == selectedSpellTrapZone) ||
                                 (slot.Zone == CleanZoneKind.FieldSpell && selectedSpellTrapZone == CleanDuelGame.FieldSpellZoneIndex));
                RenderSlot(slot, card, selected);
            }

            for (int i = 0; i < piles.Count; i++)
            {
                RenderPile(piles[i], game.Player(piles[i].Owner));
            }
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
            CreateBox("Top Down Void Base", root, new Vector3(0f, -0.16f, 0f), new Vector3(16.2f, 0.16f, 9.35f), new Color(0.004f, 0.006f, 0.014f, 1f));
            CreateBox("Top Down Carbon Frame", root, new Vector3(0f, -0.065f, 0f), new Vector3(14.65f, 0.1f, 8.35f), new Color(0.018f, 0.024f, 0.036f, 1f));
            CreateBox("Recessed Duel Tray", root, new Vector3(0f, 0.005f, 0f), new Vector3(13.18f, 0.045f, 7.48f), new Color(0.01f, 0.028f, 0.034f, 1f));

            arenaSurface = CreateBox("Top Down Clear Field Surface", root, new Vector3(0f, 0.045f, 0f), new Vector3(12.28f, 0.04f, 6.98f), new Color(0.012f, 0.052f, 0.056f, 1f)).GetComponent<Renderer>();
            DisableEmission(arenaSurface.material);

            CreateModernRails();
            CreateSidePlatforms();
            CreateCenterTechnology();
            CreateAmbientReactors();
        }
        private void CreateModernRails()
        {
            CreateBox("Local Cyan Baseline", root, new Vector3(0f, 0.105f, -3.68f), new Vector3(11.95f, 0.045f, 0.09f), new Color(0.02f, 0.86f, 1f, 1f));
            CreateBox("Rival Amber Baseline", root, new Vector3(0f, 0.105f, 3.68f), new Vector3(11.95f, 0.045f, 0.09f), new Color(1f, 0.34f, 0.08f, 1f));
            CreateBox("Left Magenta Side Rail", root, new Vector3(-6.28f, 0.105f, 0f), new Vector3(0.08f, 0.045f, 6.94f), new Color(0.88f, 0.14f, 0.92f, 1f));
            CreateBox("Right Cyan Side Rail", root, new Vector3(6.28f, 0.105f, 0f), new Vector3(0.08f, 0.045f, 6.94f), new Color(0.04f, 0.82f, 1f, 1f));

            CreateBox("Local Monster Row Glow", root, new Vector3(0f, 0.085f, -0.92f), new Vector3(9.1f, 0.025f, 0.06f), new Color(0.02f, 0.72f, 1f, 1f));
            CreateBox("Local Spell Row Glow", root, new Vector3(0f, 0.085f, -2.24f), new Vector3(9.1f, 0.025f, 0.06f), new Color(0.08f, 0.45f, 1f, 1f));
            CreateBox("Rival Monster Row Glow", root, new Vector3(0f, 0.085f, 0.92f), new Vector3(9.1f, 0.025f, 0.06f), new Color(1f, 0.36f, 0.08f, 1f));
            CreateBox("Rival Spell Row Glow", root, new Vector3(0f, 0.085f, 2.24f), new Vector3(9.1f, 0.025f, 0.06f), new Color(0.9f, 0.12f, 0.9f, 1f));
            CreateBox("Center Phase Divider", root, new Vector3(0f, 0.12f, 0f), new Vector3(10.55f, 0.035f, 0.11f), new Color(1f, 0.78f, 0.18f, 1f));
        }
        private void CreateSidePlatforms()
        {
            CreateBox("Local Extra Dock", root, new Vector3(-5.2f, 0.055f, -2.92f), new Vector3(1.3f, 0.06f, 1.7f), new Color(0.045f, 0.025f, 0.07f, 1f));
            CreateBox("Local Deck Dock", root, new Vector3(5.2f, 0.055f, -2.92f), new Vector3(1.3f, 0.06f, 1.7f), new Color(0.018f, 0.04f, 0.075f, 1f));
            CreateBox("Rival Deck Dock", root, new Vector3(-5.2f, 0.055f, 2.92f), new Vector3(1.3f, 0.06f, 1.7f), new Color(0.075f, 0.035f, 0.02f, 1f));
            CreateBox("Rival Extra Dock", root, new Vector3(5.2f, 0.055f, 2.92f), new Vector3(1.3f, 0.06f, 1.7f), new Color(0.055f, 0.025f, 0.075f, 1f));
        }
        private void CreateCenterTechnology()
        {
            CreateBox("Center Phase Slot", root, new Vector3(0f, 0.13f, 0f), new Vector3(1.74f, 0.035f, 0.28f), new Color(0.012f, 0.016f, 0.026f, 1f));
            CreateBox("Center Phase Core", root, new Vector3(0f, 0.162f, 0f), new Vector3(1.08f, 0.026f, 0.07f), new Color(1f, 0.82f, 0.24f, 1f));
            CreateCylinder("Center Flat Lens", root, new Vector3(0f, 0.18f, 0f), new Vector3(0.28f, 0.014f, 0.28f), new Color(0.72f, 0.95f, 1f, 1f));
            CreateBox("Extra Monster Link A", root, new Vector3(-0.82f, 0.095f, 0f), new Vector3(0.8f, 0.025f, 0.05f), new Color(0.88f, 0.12f, 0.9f, 1f));
            CreateBox("Extra Monster Link B", root, new Vector3(0.82f, 0.095f, 0f), new Vector3(0.8f, 0.025f, 0.05f), new Color(0.02f, 0.84f, 1f, 1f));
        }
        private void CreateAmbientReactors()
        {
            CreateCylinder("Local Left Corner Node", root, new Vector3(-6.0f, 0.14f, -3.42f), new Vector3(0.2f, 0.022f, 0.2f), new Color(0.04f, 0.9f, 1f, 1f));
            CreateCylinder("Local Right Corner Node", root, new Vector3(6.0f, 0.14f, -3.42f), new Vector3(0.2f, 0.022f, 0.2f), new Color(0.04f, 0.9f, 1f, 1f));
            CreateCylinder("Rival Left Corner Node", root, new Vector3(-6.0f, 0.14f, 3.42f), new Vector3(0.2f, 0.022f, 0.2f), new Color(1f, 0.36f, 0.08f, 1f));
            CreateCylinder("Rival Right Corner Node", root, new Vector3(6.0f, 0.14f, 3.42f), new Vector3(0.2f, 0.022f, 0.2f), new Color(1f, 0.36f, 0.08f, 1f));
        }
        private void CreateZones(CleanPlayerId owner, float side)
        {
            float[] columns = { -3.28f, -1.64f, 0f, 1.64f, 3.28f };
            float monsterZ = side * 0.92f;
            float spellZ = side * 2.24f;

            for (int i = 0; i < columns.Length; i++)
            {
                slots.Add(CreateSlot(owner, CleanZoneKind.Monster, i, columns[i], monsterZ, 1.08f));
                slots.Add(CreateSlot(owner, CleanZoneKind.SpellTrap, i, columns[i], spellZ, 1.02f));
            }
        }
        private void CreateExtraMonsterZones()
        {
            slots.Add(CreateSlot(CleanPlayerId.Local, CleanZoneKind.ExtraMonster, 0, -0.82f, 0f, 0.92f));
            slots.Add(CreateSlot(CleanPlayerId.Rival, CleanZoneKind.ExtraMonster, 1, 0.82f, 0f, 0.92f));
        }

        private void CreateFieldSpellZones()
        {
            slots.Add(CreateSlot(CleanPlayerId.Local, CleanZoneKind.FieldSpell, 0, -5.18f, -1.64f, 0.98f));
            slots.Add(CreateSlot(CleanPlayerId.Rival, CleanZoneKind.FieldSpell, 0, 5.18f, 1.64f, 0.98f));
        }

        private void CreatePiles(CleanPlayerId owner, float side)
        {
            float deckX = owner == CleanPlayerId.Local ? 5.18f : -5.18f;
            float extraX = owner == CleanPlayerId.Local ? -5.18f : 5.18f;
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
            Renderer coreRenderer = CreateCylinder("Zone Lens", slotRoot, new Vector3(0f, 0.055f, 0f), new Vector3(size * 0.18f, 0.012f, size * 0.18f), new Color(0.11f, 0.24f, 0.28f, 1f)).GetComponent<Renderer>();
            Renderer card = CreateCardSurface("Card", slotRoot, new Vector3(0f, 0.095f, 0f), new Vector2(0.82f, 1.18f), new Color(0.1f, 0.055f, 0.032f, 1f));
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

            Renderer underlay = CreateBox("Flat Zone Plate", zoneRoot, Vector3.zero, new Vector3(size * 1.12f, 0.028f, size * 1.12f), new Color(0.018f, 0.026f, 0.046f, 1f)).GetComponent<Renderer>();
            CreateBox("Top Neon Edge", zoneRoot, new Vector3(0f, 0.029f, size * 0.56f), new Vector3(size * 1.06f, 0.026f, 0.045f), frameColor);
            CreateBox("Bottom Neon Edge", zoneRoot, new Vector3(0f, 0.029f, -size * 0.56f), new Vector3(size * 1.06f, 0.026f, 0.045f), frameColor);
            CreateBox("Left Neon Edge", zoneRoot, new Vector3(-size * 0.56f, 0.029f, 0f), new Vector3(0.045f, 0.026f, size * 1.06f), frameColor);
            CreateBox("Right Neon Edge", zoneRoot, new Vector3(size * 0.56f, 0.029f, 0f), new Vector3(0.045f, 0.026f, size * 1.06f), frameColor);
            CreateBox("Scanline A", zoneRoot, new Vector3(0f, 0.037f, size * 0.16f), new Vector3(size * 0.62f, 0.012f, 0.018f), new Color(frameColor.r, frameColor.g, frameColor.b, 0.9f));
            CreateBox("Scanline B", zoneRoot, new Vector3(0f, 0.037f, -size * 0.16f), new Vector3(size * 0.62f, 0.012f, 0.018f), new Color(frameColor.r, frameColor.g, frameColor.b, 0.9f));
            CreateCylinder("Flat Zone Core", zoneRoot, new Vector3(0f, 0.043f, 0f), new Vector3(size * 0.18f, 0.009f, size * 0.18f), new Color(0.42f, 0.94f, 1f, 1f));
            return underlay;
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

        private void RenderSlot(BoardSlot slot, CleanCard card, bool selected)
        {
            bool hasCard = card != null;
            string emptyLabel = EmptyZoneLabel(slot);
            bool showEmptyLabel = !hasCard && !string.IsNullOrEmpty(emptyLabel);
            slot.Card.gameObject.SetActive(hasCard);
            slot.Label.gameObject.SetActive(hasCard || showEmptyLabel);
            slot.Stats.gameObject.SetActive(hasCard);
            SetColor(slot.Frame, selected ? new Color(1f, 0.88f, 0.18f, 1f) : new Color(0.018f, 0.026f, 0.046f, 1f));
            SetColor(slot.Core, selected ? new Color(1f, 0.82f, 0.18f, 1f) : hasCard ? new Color(0.12f, 0.23f, 0.26f, 1f) : new Color(0.36f, 0.82f, 0.92f, 1f));
            if (!hasCard)
            {
                if (showEmptyLabel)
                {
                    slot.Label.text = emptyLabel;
                    FaceCamera(slot.Label.transform);
                }

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

            slot.Label.text = ShortName(card.PublicName, 14);
            slot.Stats.text = CardStats(card);
            FaceCamera(slot.Label.transform);
            FaceCamera(slot.Stats.transform);
        }

        private string EmptyZoneLabel(BoardSlot slot)
        {
            if (slot.Zone == CleanZoneKind.FieldSpell)
            {
                return "FIELD";
            }

            if (slot.Zone == CleanZoneKind.ExtraMonster)
            {
                return "LINK";
            }

            return string.Empty;
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

            return card.Stance == CleanMonsterStance.Attack ? "ATK " + card.CurrentAttack : "DEF " + card.CurrentDefense;
        }

        private string ShortName(string value, int max)
        {
            return value.Length <= max ? value : value.Substring(0, max - 1) + ".";
        }

        private Color ZoneAccent(CleanPlayerId owner, CleanZoneKind zone)
        {
            if (zone == CleanZoneKind.FieldSpell)
            {
                return owner == CleanPlayerId.Local ? new Color(0.05f, 0.92f, 0.54f, 1f) : new Color(1f, 0.58f, 0.12f, 1f);
            }

            if (zone == CleanZoneKind.ExtraMonster)
            {
                return owner == CleanPlayerId.Local ? new Color(0.02f, 0.9f, 1f, 1f) : new Color(0.95f, 0.14f, 1f, 1f);
            }

            if (owner == CleanPlayerId.Local)
            {
                return zone == CleanZoneKind.Monster ? new Color(0.02f, 0.86f, 1f, 1f) : new Color(0.06f, 0.48f, 1f, 1f);
            }

            return zone == CleanZoneKind.Monster ? new Color(1f, 0.32f, 0.08f, 1f) : new Color(0.88f, 0.12f, 0.92f, 1f);
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
            MakeMaterialMatte(material);
            return material;
        }

        private void DisableEmission(Material material)
        {
            MakeMaterialMatte(material);
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
