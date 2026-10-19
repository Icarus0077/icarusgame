"use client";

import { useEffect, useRef, useState } from "react";
import type { FormEvent } from "react";
import "./ranked.css";

type Season = { id: string; name: string; kind: "simulation" | "ranked" };
type Rank = { id: string; name: string; patronName?: string; material?: string; tierLevels: number; startStep: number; badgePath: string };
type RankedAccount = {
  id: string;
  displayName: string;
  email: string;
  eligible: boolean;
  isDemo: boolean;
  disabledReason?: string;
};
type RankProgress = {
  progressionVersion: number;
  ladderStep: number;
  level: number;
  tierLevels: number;
  rankId: string;
  rankName: string;
  rankPatronName?: string;
  levelWins: number;
  levelLosses: number;
  winsRequired: number;
  lossesRequired: number;
  diamonds: number;
  highestStep: number;
  lastRankedAt: string | null;
  nextDecayAt: string | null;
};
type RankedPlayer = RankProgress & {
  userId: string;
  displayName: string;
  rating: number;
  position: number;
  wins: number;
  losses: number;
  draws: number;
  games: number;
};
type RankedMatch = {
  id: string;
  createdAt: string;
  playerAId: string;
  playerBId: string;
  playerAName: string;
  playerBName: string;
  outcome: "a" | "b" | "draw";
  ratingABefore: number;
  ratingAAfter: number;
  ratingBBefore: number;
  ratingBAfter: number;
  deltaA: number;
  deltaB: number;
  rankABefore: RankProgress | null;
  rankAAfter: RankProgress | null;
  rankBBefore: RankProgress | null;
  rankBAfter: RankProgress | null;
  rewardA: number;
  rewardB: number;
};
type RankedData = {
  selectedSeasonId: string;
  seasons: Season[];
  ranks: Rank[];
  accounts: RankedAccount[];
  leaderboard: RankedPlayer[];
  history: RankedMatch[];
};
type SimulationRequest = {
  seasonId: string;
  accountIds: string[];
  count: number;
  seed: number;
  batchId: string;
};

function sessionExpired(response: Response) {
  if (response.status !== 401) return false;
  window.location.assign("/login?expired=1");
  return true;
}

function signed(value: number) { return value > 0 ? `+${value}` : String(value); }

function dateLabel(value: string) {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value : date.toLocaleString("nl-NL", { dateStyle: "short", timeStyle: "short" });
}

function rankLabel(progress: RankProgress | null | undefined, ranks: Rank[]) {
  if (!progress || progress.progressionVersion !== 1) return "Rang destijds niet vastgelegd";
  const rank = ranks.find((entry) => entry.id === progress.rankId);
  return `${rank?.name ?? progress.rankName} ${progress.level}`;
}

function RankBadge({ rank, compact = false }: { rank?: Rank; compact?: boolean }) {
  const [failed, setFailed] = useState(false);
  return <span className={compact ? "rkBadge rkBadgeCompact" : "rkBadge"} aria-hidden="true">
    {rank && !failed ? (
      // Static original badges are shared with the Unity client, without image optimisation.
      // eslint-disable-next-line @next/next/no-img-element
      <img src={rank.badgePath} alt="" width={compact ? 43 : 90} height={compact ? 43 : 90} onError={() => setFailed(true)} />
    ) : <span className="rkBadgeFallback">{rank?.name.slice(0, 1) ?? "I"}</span>}
  </span>;
}

function PlayerRankLabel({ player, rank }: { player: RankedPlayer; rank?: Rank }) {
  const tierName = rank?.name ?? player.rankName;
  const patronName = rank?.patronName ?? player.rankPatronName;
  return <>
    <small><strong>{tierName} {player.level}</strong>{patronName ? ` · ${patronName}` : ""}</small>
    <small>Trede {player.ladderStep + 1} van 39</small>
  </>;
}

function LevelProgress({ player }: { player: RankProgress }) {
  return <div className="rkLevelProgress">
    <span className="rkWins">{player.levelWins}/{player.winsRequired} winst</span>
    <span>{player.levelLosses}/{player.lossesRequired} verlies</span>
    <div className="rkProgressDots" role="img" aria-label={`${player.levelWins} van ${player.winsRequired} winsten voor promotie`}>
      {Array.from({ length: player.winsRequired }, (_, index) => <i className={index < player.levelWins ? "rkDotFilled" : ""} key={index} />)}
    </div>
    <small>{player.nextDecayAt ? `Inactiviteitscontrole: ${dateLabel(player.nextDecayAt)}` : "Nog geen ranked-activiteit"}</small>
  </div>;
}

function MatchRankProgress({ before, after, reward, ranks, mmrBefore, mmrAfter, mmrDelta }: {
  before: RankProgress | null;
  after: RankProgress | null;
  reward: number;
  ranks: Rank[];
  mmrBefore: number;
  mmrAfter: number;
  mmrDelta: number;
}) {
  const known = before?.progressionVersion === 1 && after?.progressionVersion === 1;
  const moved = known ? Math.sign(after.ladderStep - before.ladderStep) : 0;
  return <div className="rkMatchProgress">
    <strong>{known ? `${rankLabel(before, ranks)} → ${rankLabel(after, ranks)}` : "Rang destijds niet vastgelegd"}</strong>
    {known && <small className={moved > 0 ? "rkWins" : moved < 0 ? "rkLoss" : ""}>{moved > 0 ? "Promotie" : moved < 0 ? "Demotie" : `${after.levelWins}/${after.winsRequired} winst · ${after.levelLosses}/${after.lossesRequired} verlies`}</small>}
    {reward > 0 && <small className="rkReward">+{reward} ◆ · promotiebeloning</small>}
    <details className="rkMmrDetails"><summary>Technische MMR</summary><span>{mmrBefore} → {mmrAfter} ({signed(mmrDelta)})</span><small>Geen criterium voor de zichtbare rang.</small></details>
  </div>;
}

export function RankedPanel({ search = "", canManage = false }: { search?: string; canManage?: boolean }) {
  const [data, setData] = useState<RankedData | null>(null);
  const [seasonId, setSeasonId] = useState("simulation-v1");
  const [historyUserId, setHistoryUserId] = useState("");
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [count, setCount] = useState("20");
  const [seed, setSeed] = useState("20261019");
  const [refresh, setRefresh] = useState(0);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const submitting = useRef(false);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  // Keep the exact request on a transport failure: rewards and results cannot be awarded twice.
  const [retryRequest, setRetryRequest] = useState<SimulationRequest | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    const params = new URLSearchParams({ seasonId });
    if (historyUserId) params.set("userId", historyUserId);
    fetch(`/api/admin/ranked?${params}`, { cache: "no-store", signal: AbortSignal.any([controller.signal, AbortSignal.timeout(20_000)]) })
      .then(async (response) => {
        if (sessionExpired(response)) return null;
        const payload = await response.json() as RankedData & { error?: string };
        if (!response.ok) throw new Error(payload.error || "De competitie kon niet worden geladen.");
        return payload;
      })
      .then((payload) => {
        if (controller.signal.aborted || !payload) return;
        setData(payload);
        setSelectedIds((current) => current.filter((id) => payload.accounts.some((account) => account.id === id && account.eligible)));
      })
      .catch((caught: unknown) => {
        if (!controller.signal.aborted) setError(caught instanceof Error ? caught.message : "Geen verbinding met de competitie.");
      })
      .finally(() => { if (!controller.signal.aborted) setLoading(false); });
    return () => controller.abort();
  }, [seasonId, historyUserId, refresh]);

  function reload() {
    setLoading(true);
    setError("");
    setRefresh((current) => current + 1);
  }

  function selectSeason(value: string) {
    setLoading(true);
    setError("");
    setNotice("");
    setSeasonId(value);
  }

  async function post(action: "simulate" | "create-demo-accounts", request?: SimulationRequest) {
    if (submitting.current) return;
    submitting.current = true;
    setBusy(true);
    setError("");
    setNotice("");
    if (request) setRetryRequest(request);
    try {
      const response = await fetch("/api/admin/ranked", {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify({ action, ...request }),
        signal: AbortSignal.timeout(20_000),
      });
      if (sessionExpired(response)) return;
      const payload = await response.json() as { error?: string; accountIds?: string[]; report?: { count: number; batchId: string; seed: number } };
      if (!response.ok) {
        // Rejected requests did not commit a batch; correct the input before trying again.
        if (response.status < 500) setRetryRequest(null);
        throw new Error(payload.error || "De opdracht kon niet worden opgeslagen.");
      }
      setRetryRequest(null);
      if (action === "create-demo-accounts") {
        setSelectedIds(payload.accountIds ?? []);
        setNotice("Demo-accounts zijn beschikbaar. Ze krijgen geen inlogwachtwoord en spelen alleen mee in de testcompetitie.");
      } else {
        setNotice(`${payload.report?.count ?? request?.count ?? 0} gesimuleerde uitslagen opgeslagen. Seed: ${payload.report?.seed ?? request?.seed}. Echte rangen en diamanten zijn niet gewijzigd.`);
      }
      reload();
    } catch (caught) {
      setError(caught instanceof Error && caught.name === "TimeoutError" ? "De server antwoordt niet op tijd. Probeer dezelfde opdracht opnieuw." : caught instanceof Error ? caught.message : "Geen verbinding. Probeer dezelfde opdracht opnieuw.");
    } finally {
      submitting.current = false;
      setBusy(false);
    }
  }

  function simulate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const numericCount = Number(count);
    const numericSeed = Number(seed);
    if (selectedIds.length < 2 || selectedIds.length > 32 || !Number.isInteger(numericCount) || numericCount < 1 || numericCount > 100 || !Number.isInteger(numericSeed) || numericSeed < 0 || numericSeed > 4294967295) {
      setError("Kies 2 tot 32 accounts, 1 tot 100 duels en een geheel seed-getal tussen 0 en 4294967295.");
      return;
    }
    void post("simulate", { seasonId: "simulation-v1", accountIds: [...selectedIds], count: numericCount, seed: numericSeed, batchId: `sim-${crypto.randomUUID()}` });
  }

  function toggleAccount(id: string) {
    setSelectedIds((current) => current.includes(id) ? current.filter((entry) => entry !== id) : [...current, id]);
  }

  function selectHistory(value: string) {
    if (value === historyUserId) { reload(); return; }
    setLoading(true);
    setError("");
    setHistoryUserId(value);
  }

  const season = data?.seasons.find((entry) => entry.id === seasonId);
  const isSimulation = seasonId === "simulation-v1";
  const query = search.trim().toLowerCase();
  const accounts = data?.accounts.filter((account) => `${account.displayName} ${account.email}`.toLowerCase().includes(query)) ?? [];
  const leaderboard = data?.leaderboard.filter((player) => player.displayName.toLowerCase().includes(query)) ?? [];
  const selectedPlayer = data?.leaderboard.find((player) => player.userId === historyUserId);
  const history = data?.history ?? [];
  const ranks = data?.ranks ?? [];
  const hasCurrentData = data?.selectedSeasonId === seasonId && !loading;

  return (
    <section className="rkRoot" aria-label="Ranked competitie" aria-busy={busy || loading}>
      <div className="rkIntro">
        <div><span className="rkEyebrow">ICARUS · OLYMPUS LADDER</span><h2>Van Icarus naar Olympus</h2><p>39 niveaus van Bronze 4 tot Master 1, met zes Griekse beschermfiguren. Je resultaten bepalen je zichtbare rang; MMR blijft op de achtergrond.</p></div>
        <div className="rkSeasonPicker"><label htmlFor="rk-season">Competitie</label><select id="rk-season" value={seasonId} disabled={busy || !!retryRequest} onChange={(event) => selectSeason(event.target.value)}>
          {(data?.seasons ?? [{ id: "simulation-v1", name: "Testcompetitie", kind: "simulation" }, { id: "ranked-v1", name: "Ranked seizoen 1", kind: "ranked" }]).map((entry) => <option key={entry.id} value={entry.id}>{entry.name}{entry.kind === "simulation" ? " · simulatie" : " · alleen lezen"}</option>)}
        </select></div>
      </div>

      <div className={isSimulation ? "rkSafety" : "rkSafety rkSafetyReal"}>
        <strong>{isSimulation ? "TESTCOMPETITIE · GESIMULEERD" : "ECHTE COMPETITIE · ALLEEN LEZEN"}</strong>
        <p>{isSimulation ? "Deze simulator berekent fictieve uitslagen met verborgen MMR en een seed. Er worden geen echte kaartduels uitgespeeld. Testrangen en testdiamanten blijven apart: echte rangen en echte diamanten veranderen niet." : "Hier verschijnen uitsluitend door de server bevestigde ranked-resultaten en de echte ranked-diamantenbalans. Je kunt vanuit dit scherm geen uitslagen, rangen of diamanten aanpassen."}</p>
      </div>

      <div className="rkGallery" aria-label="Griekse rangen">
        {ranks.map((rank, index) => <article className="rkRankCard" key={rank.id}><RankBadge rank={rank} /><span>RANG {index + 1}</span><strong>{rank.name}</strong>{rank.patronName && <small>{rank.patronName}</small>}<small className="rkTierLevels">{rank.tierLevels} niveaus · {rank.tierLevels} → 1</small></article>)}
      </div>

      <div className="rkRules" aria-label="Regels voor rangvoortgang">
        <article><strong>4 winsten → omhoog</strong><p>Winsten worden opgeteld, niet als reeks. Bij 3 verliezen daal je één niveau. Na de drempel starten beide tellers opnieuw; gelijkspel telt niet mee.</p></article>
        <article><strong>100 / 200 diamanten</strong><p>Elke promotie geeft 100 ◆ binnen een rang of 200 ◆ totaal bij een rangovergang. Ook opnieuw promoveren na een degradatie geeft deze beloning.</p></article>
        <article><strong>30 dagen inactiviteit</strong><p>Elke volle periode van 30 dagen zonder rankedduel kost één niveau, met Bronze 4 als ondergrens. Alleen inloggen stopt dit verval niet.</p></article>
      </div>

      {error && <div className="rkMessage rkError" role="alert"><span>{error}</span><button type="button" className="tableAction" disabled={busy} onClick={reload}>Vernieuwen</button></div>}
      {notice && <p className="rkMessage rkSuccess" role="status">{notice}</p>}
      {retryRequest && !busy && <div className="rkMessage"><p>De bevestiging van je laatste simulatie ontbreekt. Herhaal eerst dezelfde opdracht; opgeslagen uitslagen worden niet dubbel geteld.</p><button type="button" className="tableAction" onClick={() => void post("simulate", retryRequest)}>Dezelfde simulatie opnieuw proberen</button></div>}

      <div className={isSimulation ? "rkWorkspace" : "rkWorkspace rkReadonly"}>
        <article className="panel rkLeaderboard">
          <div className="panelHeader"><div><span>{isSimulation ? "TESTSEIZOEN" : "RANKED SEIZOEN"}</span><h2>{season?.name ?? "Competitie"}</h2></div><button type="button" onClick={reload} disabled={busy || loading}>{loading ? "Laden…" : "Vernieuwen"}</button></div>
          <div className="tableScroller"><table><thead><tr><th scope="col">#</th><th scope="col">Speler / rang</th><th scope="col">Niveauvoortgang</th><th scope="col">{isSimulation ? "Testdiamanten" : "Diamanten"}</th><th scope="col">W / V / G</th><th scope="col">Duels</th><th scope="col">Uitslagen</th></tr></thead><tbody>
            {hasCurrentData && leaderboard.map((player) => <tr key={player.userId} className={historyUserId === player.userId ? "rkSelectedRow" : ""}><td>{player.position}</td><td><div className="rkPlayer"><RankBadge key={player.rankId} rank={ranks.find((rank) => rank.id === player.rankId)} compact /><div><strong>{player.displayName}</strong><PlayerRankLabel player={player} rank={ranks.find((rank) => rank.id === player.rankId)} /></div></div></td><td><LevelProgress player={player} /></td><td><strong className="rkWallet">{player.diamonds.toLocaleString("nl-NL")} ◆</strong></td><td><span className="rkWins">{player.wins}</span> / {player.losses} / {player.draws}</td><td>{player.games}</td><td><button type="button" className="tableAction" disabled={busy} onClick={() => selectHistory(player.userId)}>Bekijken<span className="rkSrOnly"> van {player.displayName}</span></button></td></tr>)}
            {(!hasCurrentData || leaderboard.length === 0) && <tr><td colSpan={7} className="rkEmpty">{loading ? "Competitie laden…" : query ? "Geen spelers gevonden voor deze zoekopdracht." : "Nog geen uitslagen in dit seizoen."}</td></tr>}
          </tbody></table></div>
          <p className="rkFootnote">De zichtbare rang bepaalt je positie; bij dezelfde trede delen spelers de positie. De MMR bepaalt deze rang niet. W = winst, V = verlies, G = gelijkspel. {isSimulation ? "Testdiamanten hebben geen waarde buiten de testcompetitie." : "Ranked-diamanten en je hoogste behaalde positie blijven over echte seizoenen heen bewaard."}</p>
        </article>

        {isSimulation && <article className="panel rkSimulator"><div className="panelHeader"><div><span>VEILIGE TESTOMGEVING</span><h2>Uitslagen simuleren</h2></div></div>
          <form onSubmit={simulate}>
            {!canManage && <p className="rkMuted">Je account heeft alleen leesrechten. Een beheerder kan simulaties uitvoeren.</p>}
            <fieldset disabled={busy || loading || !canManage || !!retryRequest}><legend>Deelnemers <span>{selectedIds.length}/32 geselecteerd</span></legend>
              <div className="rkAccountTools"><button type="button" className="tableAction" onClick={() => setSelectedIds((data?.accounts ?? []).filter((account) => account.eligible).slice(0, 32).map((account) => account.id))}>Selecteer maximaal 32</button><button type="button" className="tableAction" onClick={() => setSelectedIds([])}>Wis selectie</button></div>
              <div className="rkAccounts">
                {accounts.map((account) => <label key={account.id} htmlFor={`rk-account-${account.id}`} className={!account.eligible ? "rkAccountDisabled" : ""}><input id={`rk-account-${account.id}`} type="checkbox" aria-label={account.displayName} checked={selectedIds.includes(account.id)} disabled={!account.eligible || (!selectedIds.includes(account.id) && selectedIds.length >= 32)} onChange={() => toggleAccount(account.id)} /><span><strong>{account.displayName}{account.isDemo && <em>DEMO</em>}</strong><small>{account.disabledReason || account.email}</small></span></label>)}
                {accounts.length === 0 && <p className="rkMuted">{query ? "Geen accounts gevonden." : "Nog geen accounts beschikbaar. Maak hieronder demo-accounts aan."}</p>}
              </div>
            </fieldset>
            <div className="rkFormRow"><label>Aantal duels<input type="number" min="1" max="100" step="1" value={count} disabled={busy || !canManage || !!retryRequest} required onChange={(event) => setCount(event.target.value)} /></label><label>Seed<input type="number" min="0" max="4294967295" step="1" value={seed} disabled={busy || !canManage || !!retryRequest} required onChange={(event) => setSeed(event.target.value)} /></label></div>
            <p className="rkMuted">Dezelfde seed, deelnemersvolgorde én start-MMR geven dezelfde uitkomsten. Een nieuwe batch telt als een nieuwe reeks; alleen testvoortgang en testdiamanten wijzigen.</p>
            <button className="primaryButton" type="submit" disabled={busy || loading || !canManage || selectedIds.length < 2 || !!retryRequest}>{busy ? "Bezig met opslaan…" : `${count || "0"} testduels simuleren en opslaan`}</button>
            <button type="button" className="tableAction rkDemoButton" disabled={busy || loading || !canManage || !!retryRequest} onClick={() => void post("create-demo-accounts")}>Demo-accounts klaarzetten</button>
          </form>
        </article>}
      </div>

      <article className="panel rkHistory"><div className="panelHeader"><div><span>OPGESLAGEN UITSLAGEN</span><h2>{selectedPlayer ? `Historie · ${selectedPlayer.displayName}` : "Recente duels"}</h2></div><label className="rkHistoryFilter"><span className="rkSrOnly">Filter historie op account</span><select value={historyUserId} disabled={busy} onChange={(event) => selectHistory(event.target.value)}><option value="">Alle accounts</option>{data?.accounts.map((account) => <option key={account.id} value={account.id}>{account.displayName}</option>)}</select></label></div>
        <div className="tableScroller"><table><thead><tr><th scope="col">Datum</th><th scope="col">Speler A</th><th scope="col">Rangvoortgang A</th><th scope="col">Uitslag</th><th scope="col">Speler B</th><th scope="col">Rangvoortgang B</th></tr></thead><tbody>
          {hasCurrentData && history.map((match) => <tr key={match.id}><td>{dateLabel(match.createdAt)}<small className="cellMeta">{isSimulation ? "Gesimuleerd" : "Ranked"}</small></td><td><strong>{match.playerAName}</strong></td><td><MatchRankProgress before={match.rankABefore} after={match.rankAAfter} reward={match.rewardA} ranks={ranks} mmrBefore={match.ratingABefore} mmrAfter={match.ratingAAfter} mmrDelta={match.deltaA} /></td><td><span className={`rkOutcome ${match.outcome === "draw" ? "rkDraw" : ""}`}>{match.outcome === "draw" ? "Gelijkspel" : match.outcome === "a" ? "A wint" : "B wint"}</span></td><td><strong>{match.playerBName}</strong></td><td><MatchRankProgress before={match.rankBBefore} after={match.rankBAfter} reward={match.rewardB} ranks={ranks} mmrBefore={match.ratingBBefore} mmrAfter={match.ratingBAfter} mmrDelta={match.deltaB} /></td></tr>)}
          {(!hasCurrentData || history.length === 0) && <tr><td colSpan={6} className="rkEmpty">{loading ? "Uitslagen laden…" : "Nog geen duels voor deze selectie."}</td></tr>}
        </tbody></table></div>
        <p className="rkFootnote">Historische rangen zijn opgeslagen momentopnamen, nooit afgeleid van MMR. Oudere uitslagen zonder rangopname blijven herkenbaar. Eventueel inactiviteitsverval wordt vóór een duel verwerkt; de actuele planning staat bij het account. {isSimulation ? "Alle getoonde beloningen zijn uitsluitend testdiamanten." : "Elke promotie geeft een beloning; dezelfde dueluitslag wordt nooit dubbel uitbetaald."}</p>
      </article>
    </section>
  );
}
