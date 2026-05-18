using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class MasterDuelPrototypeAutoStart
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void StartPrototype()
    {
        if (Object.FindFirstObjectByType<MasterDuelPrototype>() != null) return;
        var go = new GameObject("Master Duel Prototype");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<MasterDuelPrototype>();
    }
}

public sealed class MasterDuelPrototype : MonoBehaviour
{
    private enum Kind { Monster, Spell, Trap }
    private enum Phase { Draw, Standby, Main1, Battle, Main2, End, Response }

    private sealed class Card
    {
        public string id, name, effect, text;
        public Kind kind;
        public int level, atk, def, setTurn, bonusAtk;
        public bool quick, defense, ready, attacked;
        public Color color;

        public int Power { get { return atk + bonusAtk; } }

        public Card Fresh(int number)
        {
            return new Card
            {
                id = id + "_" + number,
                name = name,
                effect = effect,
                text = text,
                kind = kind,
                level = level,
                atk = atk,
                def = def,
                quick = quick,
                color = color
            };
        }
    }

    private sealed class Player
    {
        public string name;
        public int lp = 8000;
        public List<Card> deck = new List<Card>();
        public List<Card> hand = new List<Card>();
        public List<Card> grave = new List<Card>();
        public Card[] monsters = new Card[5];
        public Card[] spells = new Card[5];
    }

    private sealed class Choice
    {
        public Card card;
        public int owner, attackerOwner, zone, attackerZone, defenderZone;
        public bool direct;
        public int reduction;
    }

    private readonly Dictionary<string, Card> defs = new Dictionary<string, Card>();
    private readonly List<string> log = new List<string>();
    private Player[] p;
    private Phase phase;
    private int turn, current, first, nextId, selectedOwner = -1, selectedIndex = -1;
    private string selectedArea = "";
    private bool normalSummonUsed, gameOver;
    private Choice pending;
    private Canvas canvas;
    private Font font;
    private RectTransform top, board, side, hand;
    private Text title, phaseLabel;
    private readonly Color paper = new Color(.96f, .90f, .74f, 1f);
    private readonly Color ink = new Color(.08f, .07f, .06f, 1f);
    private readonly Color green = new Color(.07f, .30f, .24f, 1f);
    private readonly Color gold = new Color(.92f, .65f, .18f, 1f);
    private readonly Color red = new Color(.74f, .22f, .18f, 1f);
    private readonly Color ok = new Color(.12f, .55f, .34f, 1f);

    private void Awake()
    {
        EnsureScene();
        BuildCards();
        BuildUi();
        NewDuel();
    }

    private void EnsureScene()
    {
        if (Camera.main == null)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(.02f, .025f, .035f, 1f);
            cam.orthographic = true;
            camGo.transform.position = new Vector3(0, 0, -10);
        }
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
    }

    private void BuildCards()
    {
        Add("star", "Sterrenwachter", Kind.Monster, 4, 1700, 1200, "", false, "Solide aanvaller.", new Color(.95f, .75f, .25f));
        Add("flame", "Vlamduelist", Kind.Monster, 4, 1800, 800, "", false, "Veel ATK, weinig DEF.", new Color(.85f, .22f, .16f));
        Add("crystal", "Kristalwachter", Kind.Monster, 4, 1000, 2100, "", false, "Sterke verdediger.", new Color(.20f, .60f, .85f));
        Add("cyber", "Cyberdrakenling", Kind.Monster, 4, 1600, 1000, "", false, "Stabiel machine-monster.", new Color(.55f, .70f, .78f));
        Add("shadow", "Schaduwridder", Kind.Monster, 5, 2200, 1600, "", false, "Level 5: 1 tribute.", new Color(.42f, .26f, .58f));
        Add("behemoth", "Donderbehemoth", Kind.Monster, 7, 2700, 2300, "", false, "Level 7: 2 tributes.", new Color(.55f, .36f, .18f));
        Add("draw", "Focus Pot", Kind.Spell, 0, 0, 0, "draw2", false, "Trek 2 kaarten.", new Color(.25f, .70f, .50f));
        Add("boost", "Krachtspreuk", Kind.Spell, 0, 0, 0, "boost", false, "+500 ATK tot End Phase.", new Color(.88f, .64f, .18f));
        Add("break", "Breekspreuk", Kind.Spell, 0, 0, 0, "break", false, "Vernietig zwakste monster.", new Color(.67f, .35f, .82f));
        Add("mirror", "Spiegelval", Kind.Trap, 0, 0, 0, "mirror", false, "Reactie: vernietig aanvaller.", new Color(.82f, .24f, .30f));
        Add("barrier", "Schildbarriere", Kind.Trap, 0, 0, 0, "barrier", false, "Reactie: directe schade -1000.", new Color(.80f, .36f, .26f));
        Add("quick", "Bliksemschild", Kind.Spell, 0, 0, 0, "quick", true, "Quick-Play: directe schade -1200.", new Color(.35f, .65f, 1f));
    }

    private void Add(string id, string name, Kind kind, int level, int atk, int def, string effect, bool quick, string text, Color color)
    {
        defs[id] = new Card { id = id, name = name, kind = kind, level = level, atk = atk, def = def, effect = effect, quick = quick, text = text, color = color };
    }

    private void BuildUi()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        var cgo = new GameObject("Master Duel Canvas");
        cgo.transform.SetParent(transform, false);
        canvas = cgo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        cgo.AddComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        cgo.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cgo.AddComponent<GraphicRaycaster>();
        Panel(cgo.transform, "BG", new Color(.025f, .03f, .04f, 1), Vector2.zero, Vector2.one);
        top = Panel(cgo.transform, "Top", new Color(.04f, .045f, .05f, .95f), new Vector2(0, .88f), Vector2.one);
        board = Panel(cgo.transform, "Board", green, new Vector2(0, .18f), new Vector2(.76f, .88f));
        side = Panel(cgo.transform, "Side", new Color(.90f, .84f, .68f, .97f), new Vector2(.76f, .18f), new Vector2(1, .88f));
        hand = Panel(cgo.transform, "Hand", new Color(.04f, .045f, .05f, .95f), Vector2.zero, new Vector2(1, .18f));
        title = Txt(top, "Mini Master Duel", 34, TextAnchor.MiddleLeft, paper, new Vector2(.02f, 0), new Vector2(.58f, 1));
        phaseLabel = Txt(top, "", 32, TextAnchor.MiddleCenter, gold, new Vector2(.58f, 0), new Vector2(.82f, 1));
        Btn(top, "Nieuw duel", new Color(.10f, .48f, .44f), paper, NewDuel, new Vector2(.84f, .22f), new Vector2(.98f, .78f));
    }

    private void NewDuel()
    {
        CancelInvoke();
        log.Clear();
        nextId = 1;
        p = new[] { MakePlayer("Jij"), MakePlayer("AI Duelist") };
        current = Random.value < .5f ? 0 : 1;
        first = current;
        turn = 1;
        gameOver = false;
        pending = null;
        selectedOwner = -1;
        Draw(0, 5, true);
        Draw(1, 5, true);
        Log(p[current].name + " begint. Eerste speler slaat Battle Phase over.");
        BeginTurn();
        Enter(Phase.Draw);
    }

    private Player MakePlayer(string name)
    {
        var pl = new Player { name = name };
        string[] recipe = { "star", "star", "flame", "flame", "crystal", "crystal", "cyber", "cyber", "shadow", "shadow", "behemoth", "draw", "draw", "boost", "boost", "break", "mirror", "mirror", "barrier", "quick", "quick" };
        foreach (string id in recipe) pl.deck.Add(defs[id].Fresh(nextId++));
        for (int i = pl.deck.Count - 1; i > 0; i--) { int j = Random.Range(0, i + 1); Card t = pl.deck[i]; pl.deck[i] = pl.deck[j]; pl.deck[j] = t; }
        return pl;
    }

    private void BeginTurn()
    {
        normalSummonUsed = false;
        foreach (Card c in p[current].monsters) if (c != null) c.attacked = false;
    }

    private void Enter(Phase ph)
    {
        CancelInvoke(nameof(AutoStep));
        CancelInvoke(nameof(AiStep));
        if (gameOver) { Render(); return; }
        phase = ph;
        selectedOwner = -1;
        if (phase == Phase.Draw) Draw(current, 1, false);
        if (phase == Phase.Standby) ReadySetCards();
        if (phase == Phase.Battle && turn == 1 && current == first)
        {
            Log(p[current].name + " slaat Battle Phase over.");
            Render();
            Invoke(nameof(AutoStep), .35f);
            return;
        }
        if (phase == Phase.End) ClearBoosts(current);
        Render();
        if (current == 0 && (phase == Phase.Draw || phase == Phase.Standby)) Invoke(nameof(AutoStep), .45f);
        if (current == 1) Invoke(nameof(AiStep), .65f);
    }

    private void AutoStep()
    {
        if (gameOver || pending != null) return;
        if (phase == Phase.Draw) Enter(Phase.Standby);
        else if (phase == Phase.Standby) Enter(Phase.Main1);
        else if (phase == Phase.Battle) Enter(Phase.Main2);
    }

    private void NextPhase()
    {
        if (current != 0 || pending != null || gameOver) return;
        if (phase == Phase.Main1) Enter(Phase.Battle);
        else if (phase == Phase.Battle) Enter(Phase.Main2);
        else if (phase == Phase.Main2) Enter(Phase.End);
        else if (phase == Phase.End) EndTurn();
    }

    private void EndTurn()
    {
        if (pending != null || gameOver) return;
        ClearBoosts(current);
        current = 1 - current;
        turn++;
        BeginTurn();
        Log("Beurt naar " + p[current].name + ".");
        Enter(Phase.Draw);
    }

    private void Draw(int who, int count, bool silent)
    {
        for (int i = 0; i < count; i++)
        {
            if (p[who].deck.Count == 0) { gameOver = true; Log(p[1 - who].name + " wint door deck-out."); return; }
            int last = p[who].deck.Count - 1;
            p[who].hand.Add(p[who].deck[last]);
            p[who].deck.RemoveAt(last);
        }
        if (!silent) Log(p[who].name + " trekt " + count + " kaart" + (count == 1 ? "." : "en."));
    }

    private void Summon(int handIndex, bool defense)
    {
        Card c = p[current].hand[handIndex];
        if (current != 0 && current != 1) return;
        if (c.kind != Kind.Monster || !Main() || normalSummonUsed || Empty(p[current].monsters) < 0) return;
        int cost = c.level >= 7 ? 2 : c.level >= 5 ? 1 : 0;
        if (p[current].monsters.Count(x => x != null) < cost) return;
        for (int i = 0; i < cost; i++)
        {
            int z = Enumerable.Range(0, 5).Where(x => p[current].monsters[x] != null).OrderBy(x => Value(p[current].monsters[x])).First();
            Grave(current, p[current].monsters[z]);
            p[current].monsters[z] = null;
        }
        p[current].hand.RemoveAt(handIndex);
        c.defense = defense;
        p[current].monsters[Empty(p[current].monsters)] = c;
        normalSummonUsed = true;
        Log(p[current].name + " roept " + c.name + " op.");
        Render();
    }

    private void UseSpell(int handIndex)
    {
        Card c = p[current].hand[handIndex];
        if (!Main() || c.kind != Kind.Spell || c.quick) return;
        p[current].hand.RemoveAt(handIndex);
        Log(p[current].name + " activeert " + c.name + ".");
        if (c.effect == "draw2") Draw(current, 2, false);
        if (c.effect == "boost" && p[current].monsters.Any(x => x != null)) p[current].monsters.Where(x => x != null).OrderByDescending(x => x.Power).First().bonusAtk += 500;
        if (c.effect == "break")
        {
            int enemy = 1 - current;
            var zones = Enumerable.Range(0, 5).Where(x => p[enemy].monsters[x] != null).OrderBy(x => Value(p[enemy].monsters[x])).ToList();
            if (zones.Count > 0) { Grave(enemy, p[enemy].monsters[zones[0]]); p[enemy].monsters[zones[0]] = null; }
        }
        Grave(current, c);
        Render();
    }

    private void SetReactive(int handIndex)
    {
        Card c = p[current].hand[handIndex];
        int z = Empty(p[current].spells);
        if (!Main() || z < 0 || !(c.kind == Kind.Trap || c.quick)) return;
        p[current].hand.RemoveAt(handIndex);
        c.setTurn = turn;
        p[current].spells[z] = c;
        Log(p[current].name + " zet een " + (c.kind == Kind.Trap ? "trap." : "quick-spell."));
        Render();
    }

    private bool Attack(int attackerOwner, int attackerZone, int defenderZone)
    {
        Card a = p[attackerOwner].monsters[attackerZone];
        if (a == null || a.defense || a.attacked || phase != Phase.Battle || current != attackerOwner) return false;
        int defOwner = 1 - attackerOwner;
        bool direct = defenderZone < 0;
        if (direct && p[defOwner].monsters.Any(x => x != null)) return false;
        a.attacked = true;
        pending = FindTrigger(defOwner, attackerZone, defenderZone, direct);
        if (pending != null && defOwner == 0 && attackerOwner == 1) { phase = Phase.Response; Render(); return true; }
        if (pending != null) { ResolveTrigger(true); return false; }
        FinishAttack(attackerOwner, attackerZone, defenderZone, direct, 0);
        return false;
    }

    private Choice FindTrigger(int owner, int attackerZone, int defenderZone, bool direct)
    {
        for (int z = 0; z < 5; z++)
        {
            Card c = p[owner].spells[z];
            if (c == null || !c.ready) continue;
            if (c.effect == "mirror" || (direct && (c.effect == "barrier" || c.effect == "quick"))) return new Choice { card = c, owner = owner, attackerOwner = 1 - owner, zone = z, attackerZone = attackerZone, defenderZone = defenderZone, direct = direct };
        }
        return null;
    }

    private void ResolveTrigger(bool activate)
    {
        Choice c = pending;
        pending = null;
        phase = Phase.Battle;
        if (!activate) { Log("Reactie geannuleerd."); FinishAttack(c.attackerOwner, c.attackerZone, c.defenderZone, c.direct, 0); ResumeAi(); return; }
        p[c.owner].spells[c.zone] = null;
        Grave(c.owner, c.card);
        if (c.card.effect == "mirror")
        {
            Grave(c.attackerOwner, p[c.attackerOwner].monsters[c.attackerZone]);
            p[c.attackerOwner].monsters[c.attackerZone] = null;
            Log("Spiegelval vernietigt de aanvaller.");
            Render();
            ResumeAi();
            return;
        }
        int reduction = c.card.effect == "quick" ? 1200 : 1000;
        Log(c.card.name + " vermindert schade met " + reduction + ".");
        FinishAttack(c.attackerOwner, c.attackerZone, c.defenderZone, c.direct, reduction);
        ResumeAi();
    }

    private void FinishAttack(int atkOwner, int atkZone, int defZone, bool direct, int reduction)
    {
        Card a = p[atkOwner].monsters[atkZone];
        if (a == null) { Render(); return; }
        int defOwner = 1 - atkOwner;
        if (direct)
        {
            int dmg = Mathf.Max(0, a.Power - reduction);
            p[defOwner].lp -= dmg;
            Log(a.name + " valt direct aan voor " + dmg + ".");
        }
        else
        {
            Card d = p[defOwner].monsters[defZone];
            if (d.defense)
            {
                if (a.Power > d.def) { Grave(defOwner, d); p[defOwner].monsters[defZone] = null; Log(a.name + " breekt door " + d.name + "."); }
                else if (a.Power < d.def) { p[atkOwner].lp -= d.def - a.Power; Log(d.name + " houdt stand."); }
            }
            else if (a.Power > d.Power) { Grave(defOwner, d); p[defOwner].monsters[defZone] = null; p[defOwner].lp -= a.Power - d.Power; }
            else if (a.Power < d.Power) { Grave(atkOwner, a); p[atkOwner].monsters[atkZone] = null; p[atkOwner].lp -= d.Power - a.Power; }
            else { Grave(atkOwner, a); Grave(defOwner, d); p[atkOwner].monsters[atkZone] = null; p[defOwner].monsters[defZone] = null; }
        }
        if (p[0].lp <= 0 || p[1].lp <= 0) { gameOver = true; Log((p[0].lp > p[1].lp ? p[0].name : p[1].name) + " wint."); }
        Render();
    }

    private void AiStep()
    {
        if (gameOver || pending != null || current != 1) return;
        if (phase == Phase.Main1 || phase == Phase.Main2) AiMain();
        else if (phase == Phase.Battle && AiBattle()) return;
        if (phase == Phase.Draw) Enter(Phase.Standby);
        else if (phase == Phase.Standby) Enter(Phase.Main1);
        else if (phase == Phase.Main1) Enter(Phase.Battle);
        else if (phase == Phase.Battle) Enter(Phase.Main2);
        else if (phase == Phase.Main2) Enter(Phase.End);
        else if (phase == Phase.End) EndTurn();
    }

    private void AiMain()
    {
        int s = p[1].hand.FindIndex(x => x.kind == Kind.Spell && !x.quick);
        if (s >= 0) UseSpell(s);
        if (!normalSummonUsed)
        {
            int m = p[1].hand.FindIndex(x => x.kind == Kind.Monster && (x.level < 5 || p[1].monsters.Any(y => y != null)));
            if (m >= 0) Summon(m, p[1].hand[m].def > p[1].hand[m].atk + 300);
        }
        int r = p[1].hand.FindIndex(x => x.kind == Kind.Trap || x.quick);
        while (r >= 0 && Empty(p[1].spells) >= 0) { SetReactive(r); r = p[1].hand.FindIndex(x => x.kind == Kind.Trap || x.quick); }
    }

    private bool AiBattle()
    {
        for (int z = 0; z < 5; z++)
        {
            Card a = p[1].monsters[z];
            if (a == null || a.defense || a.attacked) continue;
            var targets = Enumerable.Range(0, 5).Where(i => p[0].monsters[i] != null).OrderBy(i => Value(p[0].monsters[i])).ToList();
            if (targets.Count == 0) { if (Attack(1, z, -1)) return true; }
            else if (Attack(1, z, targets[0])) return true;
        }
        return false;
    }

    private void ResumeAi() { if (!gameOver && current == 1 && phase == Phase.Battle) Invoke(nameof(AiStep), .6f); }
    private bool Main() { return phase == Phase.Main1 || phase == Phase.Main2; }
    private int Empty(Card[] a) { for (int i = 0; i < a.Length; i++) if (a[i] == null) return i; return -1; }
    private int Value(Card c) { return c.defense ? c.def : c.Power; }
    private void Grave(int who, Card c) { if (c != null) { c.ready = false; c.bonusAtk = 0; p[who].grave.Add(c); } }
    private void ReadySetCards() { foreach (Player pl in p) foreach (Card c in pl.spells) if (c != null && c.setTurn < turn) c.ready = true; }
    private void ClearBoosts(int who) { foreach (Card c in p[who].monsters) if (c != null) c.bonusAtk = 0; }
    private void Log(string s) { log.Insert(0, s); if (log.Count > 10) log.RemoveAt(log.Count - 1); }

    private void Render()
    {
        title.text = gameOver ? "Duel klaar" : "Beurt " + turn + ": " + p[current].name;
        phaseLabel.text = phase.ToString();
        Clear(board); Clear(side); Clear(hand);
        Row(1, "spell", .66f, .80f); Row(1, "monster", .49f, .64f); Phases(); Row(0, "monster", .20f, .35f); Row(0, "spell", .04f, .18f);
        PlayerPanel(1, .82f, .98f); PlayerPanel(0, 0, .14f);
        Txt(hand, "Jouw hand", 24, TextAnchor.MiddleLeft, paper, new Vector2(.02f, .68f), new Vector2(.20f, 1));
        for (int i = 0; i < p[0].hand.Count; i++) CardButton(hand, p[0].hand[i], 0, "hand", i, new Vector2(.02f + i * .075f, .05f), new Vector2(.085f + i * .075f, .88f));
        Side();
    }

    private void PlayerPanel(int who, float y0, float y1)
    {
        RectTransform r = Panel(board, p[who].name, paper, new Vector2(.02f, y0), new Vector2(.98f, y1));
        Txt(r, p[who].name + "   " + Mathf.Max(0, p[who].lp) + " LP", 24, TextAnchor.MiddleLeft, ink, new Vector2(.02f, 0), new Vector2(.55f, 1));
        Txt(r, "Deck " + p[who].deck.Count + "  Hand " + p[who].hand.Count + "  Grave " + p[who].grave.Count, 18, TextAnchor.MiddleRight, ink, new Vector2(.55f, 0), new Vector2(.98f, 1));
    }

    private void Row(int owner, string area, float y0, float y1)
    {
        for (int i = 0; i < 5; i++)
        {
            RectTransform z = Panel(board, area + i, new Color(0, 0, 0, .22f), new Vector2(.02f + i * .192f, y0), new Vector2(.18f + i * .192f, y1));
            Card c = area == "monster" ? p[owner].monsters[i] : p[owner].spells[i];
            if (c == null) Txt(z, area == "monster" ? "MONSTER" : "SPELL/TRAP", 14, TextAnchor.MiddleCenter, new Color(1, 1, 1, .45f), Vector2.zero, Vector2.one);
            else CardButton(z, c, owner, area, i, Vector2.zero, Vector2.one);
        }
    }

    private void Phases()
    {
        Phase[] ph = { Phase.Draw, Phase.Standby, Phase.Main1, Phase.Battle, Phase.Main2, Phase.End };
        for (int i = 0; i < ph.Length; i++)
        {
            RectTransform r = Panel(board, ph[i].ToString(), ph[i] == phase ? gold : new Color(0, 0, 0, .28f), new Vector2(.02f + i * .16f, .38f), new Vector2(.17f + i * .16f, .46f));
            Txt(r, ph[i].ToString(), 16, TextAnchor.MiddleCenter, ph[i] == phase ? ink : paper, Vector2.zero, Vector2.one);
        }
    }

    private void Side()
    {
        if (pending != null)
        {
            Txt(side, pending.card.name + " kan reageren.\n" + pending.card.text, 22, TextAnchor.UpperLeft, ink, new Vector2(.05f, .55f), new Vector2(.95f, .95f));
            Btn(side, "Activeren", ok, paper, () => ResolveTrigger(true), new Vector2(.05f, .36f), new Vector2(.95f, .45f));
            Btn(side, "Annuleren", red, paper, () => ResolveTrigger(false), new Vector2(.05f, .25f), new Vector2(.95f, .34f));
            return;
        }
        Txt(side, "Acties\n" + string.Join("\n", log.Take(5)), 20, TextAnchor.UpperLeft, ink, new Vector2(.05f, .60f), new Vector2(.95f, .98f));
        Card s = Selected();
        if (s != null)
        {
            Txt(side, s.name + "\n" + s.text, 20, TextAnchor.UpperLeft, ink, new Vector2(.05f, .38f), new Vector2(.95f, .58f));
            if (selectedOwner == 0 && selectedArea == "hand" && s.kind == Kind.Monster) { Btn(side, "Oproepen ATK", ok, paper, () => Summon(selectedIndex, false), new Vector2(.05f, .28f), new Vector2(.95f, .36f)); Btn(side, "Zet DEF", ok, paper, () => Summon(selectedIndex, true), new Vector2(.05f, .19f), new Vector2(.95f, .27f)); }
            if (selectedOwner == 0 && selectedArea == "hand" && s.kind == Kind.Spell && !s.quick) Btn(side, "Activeer spell", ok, paper, () => UseSpell(selectedIndex), new Vector2(.05f, .28f), new Vector2(.95f, .36f));
            if (selectedOwner == 0 && selectedArea == "hand" && (s.kind == Kind.Trap || s.quick)) Btn(side, "Zet reactiekaart", ok, paper, () => SetReactive(selectedIndex), new Vector2(.05f, .28f), new Vector2(.95f, .36f));
            if (selectedOwner == 0 && selectedArea == "monster" && phase == Phase.Battle && !p[1].monsters.Any(x => x != null)) Btn(side, "Directe aanval", red, paper, () => Attack(0, selectedIndex, -1), new Vector2(.05f, .28f), new Vector2(.95f, .36f));
        }
        Btn(side, "Volgende fase", gold, ink, NextPhase, new Vector2(.05f, .10f), new Vector2(.95f, .18f));
        Btn(side, "Beurt klaar", paper, ink, EndTurn, new Vector2(.05f, .02f), new Vector2(.95f, .09f));
    }

    private Card Selected()
    {
        if (selectedOwner < 0) return null;
        if (selectedArea == "hand") return selectedIndex < p[selectedOwner].hand.Count ? p[selectedOwner].hand[selectedIndex] : null;
        return selectedArea == "monster" ? p[selectedOwner].monsters[selectedIndex] : p[selectedOwner].spells[selectedIndex];
    }

    private void CardButton(RectTransform parent, Card c, int owner, string area, int index, Vector2 min, Vector2 max)
    {
        bool hidden = owner == 1 && area == "spell";
        Button b = Btn(parent, hidden ? "SET" : c.name + (c.kind == Kind.Monster ? "\n" + c.Power + "/" + c.def : "\n" + (c.ready ? "READY" : c.kind.ToString())), hidden ? new Color(.25f, .08f, .07f) : c.color, hidden ? paper : ink, () => ClickCard(owner, area, index), min, max);
        b.GetComponentInChildren<Text>().fontSize = 14;
    }

    private void ClickCard(int owner, string area, int index)
    {
        if (pending != null) return;
        if (owner == 1 && area == "monster" && selectedOwner == 0 && selectedArea == "monster") { Attack(0, selectedIndex, index); return; }
        if (owner == 1 && area == "spell") return;
        selectedOwner = owner; selectedArea = area; selectedIndex = index; Render();
    }

    private RectTransform Panel(Transform parent, string name, Color color, Vector2 min, Vector2 max)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>(); r.anchorMin = min; r.anchorMax = max; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color; return r;
    }

    private Text Txt(Transform parent, string text, int size, TextAnchor anchor, Color color, Vector2 min, Vector2 max)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>(); r.anchorMin = min; r.anchorMax = max; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        var t = go.GetComponent<Text>(); t.text = text; t.font = font; t.fontSize = size; t.alignment = anchor; t.color = color; return t;
    }

    private Button Btn(Transform parent, string text, Color bg, Color fg, UnityEngine.Events.UnityAction action, Vector2 min, Vector2 max)
    {
        var go = new GameObject(text, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>(); r.anchorMin = min; r.anchorMax = max; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = bg; var b = go.GetComponent<Button>(); b.onClick.AddListener(action);
        Txt(go.transform, text, 20, TextAnchor.MiddleCenter, fg, Vector2.zero, Vector2.one); return b;
    }

    private void Clear(Transform t) { for (int i = t.childCount - 1; i >= 0; i--) Destroy(t.GetChild(i).gameObject); }
}
