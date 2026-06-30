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
            CreatePiles(CleanPlayerId.Rival, 1f);
            CreatePiles(CleanPlayerId.Local, -1f);
        }

        public void Render(CleanDuelGame game, int selectedMonsterZone, int selectedSpellTrapZone)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                BoardSlot slot = slots[i];
                CleanPlayerState player = game.Player(slot.Owner);
                CleanCard card = slot.Zone == CleanZoneKind.Monster
                    ? player.Monsters[slot.Index]
                    : player.SpellsTraps[slot.Index];
                bool selected = slot.Owner == CleanPlayerId.Local &&
                                ((slot.Zone == CleanZoneKind.Monster && slot.Index == selectedMonsterZone) ||
                                 (slot.Zone == CleanZoneKind.SpellTrap && slot.Index == selectedSpellTrapZone));
                RenderSlot(slot, card, selected);
            }

            for (int i = 0; i < piles.Count; i++)
            {
                RenderPile(piles[i], game.Player(piles[i].Owner));
            }
        }

        private void CreateArena()
        {
            CreateBox("Void Base", root, new Vector3(0f, -0.18f, 0f), new Vector3(17.2f, 0.18f, 10.8f), new Color(0.006f, 0.008f, 0.018f, 1f));
            CreateBox("Carbon Plinth", root, new Vector3(0f, -0.08f, 0f), new Vector3(15.1f, 0.16f, 8.95f), new Color(0.018f, 0.028f, 0.048f, 1f));
            CreateBox("Inset Shadow", root, new Vector3(0f, 0.0f, 0f), new Vector3(13.4f, 0.08f, 8.05f), new Color(0.025f, 0.035f, 0.052f, 1f));

            arenaSurface = CreateBox("Glass Duel Surface", root, new Vector3(0f, 0.055f, 0f), new Vector3(12.35f, 0.055f, 7.45f), new Color(0.018f, 0.34f, 0.29f, 1f)).GetComponent<Renderer>();
            Texture2D fieldTexture = artCache.GetFirstFieldTexture();
            if (fieldTexture != null)
            {
                SetTexture(arenaSurface.material, fieldTexture);
                SetColor(arenaSurface, new Color(0.62f, 0.95f, 0.86f, 1f));
            }

            CreateModernRails();
            CreateSidePlatforms();
            CreateCenterTechnology();
            CreateAmbientReactors();
        }

        private void CreateModernRails()
        {
            CreateBox("Front Magenta Rail", root, new Vector3(0f, 0.17f, -4.18f), new Vector3(12.75f, 0.11f, 0.16f), new Color(0.78f, 0.08f, 0.86f, 1f));
            CreateBox("Back Amber Rail", root, new Vector3(0f, 0.17f, 4.18f), new Vector3(12.75f, 0.11f, 0.16f), new Color(1f, 0.32f, 0.08f, 1f));
            CreateBox("Left Amber Spine", root, new Vector3(-6.42f, 0.17f, 0f), new Vector3(0.18f, 0.11f, 7.75f), new Color(1f, 0.24f, 0.06f, 1f));
            CreateBox("Right Cyan Spine", root, new Vector3(6.42f, 0.17f, 0f), new Vector3(0.18f, 0.11f, 7.75f), new Color(0.02f, 0.75f, 1f, 1f));

            CreateBox("Player Holo Line", root, new Vector3(0f, 0.19f, -3.42f), new Vector3(10.55f, 0.045f, 0.08f), new Color(0.02f, 0.88f, 1f, 1f));
            CreateBox("Rival Holo Line", root, new Vector3(0f, 0.19f, 3.42f), new Vector3(10.55f, 0.045f, 0.08f), new Color(1f, 0.22f, 0.1f, 1f));
            CreateBox("Center Phase Beam", root, new Vector3(0f, 0.2f, 0f), new Vector3(10.85f, 0.045f, 0.12f), new Color(1f, 0.78f, 0.18f, 1f));
        }

        private void CreateSidePlatforms()
        {
            CreateBox("Left Dock", root, new Vector3(-7.05f, 0.02f, -1.18f), new Vector3(1.55f, 0.24f, 3.15f), new Color(0.04f, 0.035f, 0.07f, 1f));
            CreateBox("Right Dock", root, new Vector3(7.05f, 0.02f, 1.18f), new Vector3(1.55f, 0.24f, 3.15f), new Color(0.04f, 0.035f, 0.07f, 1f));
            CreateBox("Left Dock Light", root, new Vector3(-7.05f, 0.18f, -2.15f), new Vector3(1.12f, 0.05f, 0.14f), new Color(0.78f, 0.08f, 0.86f, 1f));
            CreateBox("Right Dock Light", root, new Vector3(7.05f, 0.18f, 2.15f), new Vector3(1.12f, 0.05f, 0.14f), new Color(0.02f, 0.75f, 1f, 1f));
        }

        private void CreateCenterTechnology()
        {
            CreateBox("Center Black Channel", root, new Vector3(0f, 0.14f, 0f), new Vector3(2.15f, 0.06f, 0.36f), new Color(0.01f, 0.014f, 0.025f, 1f));
            CreateBox("Center Gold Core", root, new Vector3(0f, 0.21f, 0f), new Vector3(1.35f, 0.04f, 0.08f), new Color(1f, 0.78f, 0.18f, 1f));
            CreateCylinder("Center Lens", root, new Vector3(0f, 0.245f, 0f), new Vector3(0.42f, 0.028f, 0.42f), new Color(0.72f, 0.94f, 1f, 1f));
        }

        private void CreateAmbientReactors()
        {
            CreateCylinder("Front Left Reactor", root, new Vector3(-5.95f, 0.24f, -3.12f), new Vector3(0.42f, 0.075f, 0.42f), new Color(0.04f, 0.9f, 1f, 1f));
            CreateCylinder("Front Right Reactor", root, new Vector3(5.95f, 0.24f, -3.12f), new Vector3(0.42f, 0.075f, 0.42f), new Color(0.04f, 0.9f, 1f, 1f));
            CreateCylinder("Back Left Reactor", root, new Vector3(-5.95f, 0.24f, 3.12f), new Vector3(0.42f, 0.075f, 0.42f), new Color(1f, 0.34f, 0.08f, 1f));
            CreateCylinder("Back Right Reactor", root, new Vector3(5.95f, 0.24f, 3.12f), new Vector3(0.42f, 0.075f, 0.42f), new Color(1f, 0.34f, 0.08f, 1f));
        }

        private void CreateZones(CleanPlayerId owner, float side)
        {
            float[] columns = { -3.42f, -1.72f, 0f, 1.72f, 3.42f };
            float monsterZ = side * 1.25f;
            float spellZ = side * 2.38f;

            for (int i = 0; i < columns.Length; i++)
            {
                slots.Add(CreateSlot(owner, CleanZoneKind.Monster, i, columns[i], monsterZ, 1.22f));
                slots.Add(CreateSlot(owner, CleanZoneKind.SpellTrap, i, columns[i], spellZ, 1.04f));
            }
        }

        private void CreateExtraMonsterZones()
        {
            CreateZoneDecoration("Extra Monster Zone A", -1.72f, 0f, 1.08f, new Color(0.95f, 0.14f, 1f, 1f), root, true);
            CreateZoneDecoration("Extra Monster Zone B", 1.72f, 0f, 1.08f, new Color(0.02f, 0.82f, 1f, 1f), root, true);
        }

        private void CreatePiles(CleanPlayerId owner, float side)
        {
            float deckX = owner == CleanPlayerId.Local ? 5.38f : -5.38f;
            float graveX = owner == CleanPlayerId.Local ? -5.38f : 5.38f;
            piles.Add(CreatePile(owner, CleanZoneKind.Deck, deckX, side * 2.48f, side > 0f ? 14f : -14f));
            piles.Add(CreatePile(owner, CleanZoneKind.ExtraDeck, deckX, side * 1.34f, side > 0f ? 14f : -14f));
            piles.Add(CreatePile(owner, CleanZoneKind.Graveyard, graveX, side * 2.48f, side > 0f ? -14f : 14f));
        }

        private BoardSlot CreateSlot(CleanPlayerId owner, CleanZoneKind zone, int index, float x, float z, float size)
        {
            Transform slotRoot = new GameObject(owner + " " + zone + " " + index).transform;
            slotRoot.SetParent(root, false);
            slotRoot.localPosition = new Vector3(x, 0.17f, z);

            Color accent = ZoneAccent(owner, zone);
            Renderer frame = CreateZoneDecoration("Zone Frame", 0f, 0f, size, accent, slotRoot, false);
            Renderer coreRenderer = CreateCylinder("Zone Lens", slotRoot, new Vector3(0f, 0.09f, 0f), new Vector3(size * 0.25f, 0.024f, size * 0.25f), new Color(0.11f, 0.24f, 0.28f, 1f)).GetComponent<Renderer>();
            Renderer card = CreateCardSurface("Card", slotRoot, new Vector3(0f, 0.18f, 0f), new Vector2(0.82f, 1.18f), new Color(0.1f, 0.055f, 0.032f, 1f));
            TextMesh label = CreateLabel("Name", slotRoot, new Vector3(-0.46f, 0.42f, -0.45f), string.Empty, 32);
            TextMesh stats = CreateLabel("Stats", slotRoot, new Vector3(-0.34f, 0.42f, 0.44f), string.Empty, 28);

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
            zoneRoot.localPosition = new Vector3(x, 0.035f, z);
            if (diamond)
            {
                zoneRoot.localRotation = Quaternion.Euler(0f, 45f, 0f);
            }

            Renderer underlay = CreateBox("Glass Underlay", zoneRoot, Vector3.zero, new Vector3(size * 1.14f, 0.045f, size * 1.14f), new Color(0.018f, 0.026f, 0.046f, 1f)).GetComponent<Renderer>();
            CreateBox("Top Rail", zoneRoot, new Vector3(0f, 0.052f, size * 0.58f), new Vector3(size * 1.08f, 0.055f, 0.07f), frameColor);
            CreateBox("Bottom Rail", zoneRoot, new Vector3(0f, 0.052f, -size * 0.58f), new Vector3(size * 1.08f, 0.055f, 0.07f), frameColor);
            CreateBox("Left Rail", zoneRoot, new Vector3(-size * 0.58f, 0.052f, 0f), new Vector3(0.07f, 0.055f, size * 1.08f), frameColor);
            CreateBox("Right Rail", zoneRoot, new Vector3(size * 0.58f, 0.052f, 0f), new Vector3(0.07f, 0.055f, size * 1.08f), frameColor);
            CreateBox("Inner Scanline A", zoneRoot, new Vector3(0f, 0.058f, size * 0.18f), new Vector3(size * 0.72f, 0.022f, 0.025f), new Color(frameColor.r, frameColor.g, frameColor.b, 1f));
            CreateBox("Inner Scanline B", zoneRoot, new Vector3(0f, 0.058f, -size * 0.18f), new Vector3(size * 0.72f, 0.022f, 0.025f), new Color(frameColor.r, frameColor.g, frameColor.b, 1f));
            CreateCylinder("Zone Glow Core", zoneRoot, new Vector3(0f, 0.083f, 0f), new Vector3(size * 0.28f, 0.022f, size * 0.28f), new Color(0.42f, 0.94f, 1f, 1f));
            return underlay;
        }

        private PileView CreatePile(CleanPlayerId owner, CleanZoneKind zone, float x, float z, float yaw)
        {
            Transform pileRoot = new GameObject(owner + " " + zone).transform;
            pileRoot.SetParent(root, false);
            pileRoot.localPosition = new Vector3(x, 0.21f, z);
            pileRoot.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Color accent = zone == CleanZoneKind.Deck
                ? new Color(0.04f, 0.78f, 1f, 1f)
                : zone == CleanZoneKind.ExtraDeck
                    ? new Color(0.72f, 0.15f, 1f, 1f)
                    : new Color(1f, 0.32f, 0.08f, 1f);
            Color stackColor = zone == CleanZoneKind.Deck
                ? new Color(0.045f, 0.08f, 0.15f, 1f)
                : zone == CleanZoneKind.ExtraDeck
                    ? new Color(0.09f, 0.04f, 0.16f, 1f)
                    : new Color(0.12f, 0.06f, 0.07f, 1f);

            Renderer frame = CreateBox("Pile Frame", pileRoot, Vector3.zero, new Vector3(1.06f, 0.08f, 1.42f), accent).GetComponent<Renderer>();
            Renderer stack = CreateBox("Pile Stack", pileRoot, new Vector3(0f, 0.115f, 0f), new Vector3(0.78f, 0.13f, 1.08f), stackColor).GetComponent<Renderer>();
            CreateBox("Pile Front Light", pileRoot, new Vector3(0f, 0.19f, -0.62f), new Vector3(0.64f, 0.035f, 0.08f), accent);
            TextMesh label = CreateLabel("Pile Label", pileRoot, new Vector3(-0.43f, 0.38f, -0.49f), string.Empty, 28);
            AddTarget(frame.gameObject, owner, zone, 0);
            AddTarget(stack.gameObject, owner, zone, 0);

            return new PileView { Owner = owner, Zone = zone, Stack = stack, Label = label };
        }

        private void RenderSlot(BoardSlot slot, CleanCard card, bool selected)
        {
            bool hasCard = card != null;
            slot.Card.gameObject.SetActive(hasCard);
            slot.Label.gameObject.SetActive(hasCard);
            slot.Stats.gameObject.SetActive(hasCard);
            SetColor(slot.Frame, selected ? new Color(1f, 0.88f, 0.18f, 1f) : new Color(0.018f, 0.026f, 0.046f, 1f));
            SetColor(slot.Core, selected ? new Color(1f, 0.82f, 0.18f, 1f) : hasCard ? new Color(0.12f, 0.23f, 0.26f, 1f) : new Color(0.36f, 0.82f, 0.92f, 1f));
            if (!hasCard)
            {
                return;
            }

            ApplyCardLayout(slot, card);
            Texture2D texture = card.FaceDown ? null : artCache.GetTexture(card.Definition.Id);
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

        private void ApplyCardLayout(BoardSlot slot, CleanCard card)
        {
            bool defenseMonster = slot.Zone == CleanZoneKind.Monster &&
                                  card.Definition.Kind == CleanCardKind.Monster &&
                                  card.Stance == CleanMonsterStance.Defense;
            slot.Card.transform.localScale = new Vector3(1f, 1f, 1f);
            float ownerYaw = slot.Owner == CleanPlayerId.Local ? 0f : 180f;
            float stanceYaw = defenseMonster ? 90f : 0f;
            slot.Card.transform.localRotation = Quaternion.Euler(0f, ownerYaw + stanceYaw, 0f);
            slot.Card.transform.localPosition = new Vector3(0f, 0.19f, 0f);
        }

        private void RenderPile(PileView pile, CleanPlayerState player)
        {
            int count = pile.Zone == CleanZoneKind.Deck
                ? player.Deck.Count
                : pile.Zone == CleanZoneKind.ExtraDeck ? player.ExtraDeck.Count : player.Graveyard.Count;
            pile.Stack.gameObject.SetActive(count > 0);
            float height = 0.08f + Mathf.Clamp(count, 0, 35) * 0.0055f;
            pile.Stack.transform.localScale = new Vector3(0.78f, height, 1.08f);
            pile.Stack.transform.localPosition = new Vector3(0f, 0.075f + height * 0.5f, 0f);
            pile.Label.text = (pile.Zone == CleanZoneKind.Deck ? "DECK" : pile.Zone == CleanZoneKind.ExtraDeck ? "EX" : "GY") + "\n" + count;
            FaceCamera(pile.Label.transform);
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
            if (material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 0.45f);
            }

            return material;
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
