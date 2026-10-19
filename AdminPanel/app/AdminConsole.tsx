"use client";

import { useEffect, useMemo, useState } from "react";
import type { CSSProperties, FormEvent } from "react";
import { RankedPanel } from "./RankedPanel";

type Viewer = { name: string; email: string };
type ViewKey = "dashboard" | "users" | "ranked" | "network" | "cards" | "releases" | "audit";

type GameUser = {
  id: string;
  display_name: string;
  email: string;
  role: string;
  status: string;
  access_group: string;
  last_seen: string;
  created_at: string;
};

type GameSetting = {
  id: string;
  user_id: string;
  game_id: string;
  can_play: number;
  matchmaking_region: string;
  ranked_enabled: number;
  deck_slots: number;
  settings_json: string;
  updated_at: string;
};

type IpRule = {
  id: string;
  cidr: string;
  label: string;
  scope: string;
  environment: string;
  status: string;
  expires_at: string | null;
  created_at: string;
};

type CardPolicy = {
  id: string;
  card_id: string;
  card_name: string;
  availability: string;
  audience: string;
  regulation: string;
  effective_from: string;
  effective_to: string | null;
  updated_at: string;
};

type Release = {
  id: string;
  version: string;
  kind: string;
  environment: string;
  status: string;
  notes: string;
  created_at: string;
};

type AuditEvent = {
  id: string;
  actor: string;
  action: string;
  target: string;
  detail: string;
  severity: string;
  created_at: string;
};

type AdminData = {
  actor: { name: string; email: string; role: string };
  users: GameUser[];
  gameSettings: GameSetting[];
  ipRules: IpRule[];
  cardPolicies: CardPolicy[];
  releases: Release[];
  auditEvents: AuditEvent[];
  metrics: {
    onlinePlayers: number;
    liveDuels: number;
    catalogCards: number;
    pendingChanges: number;
  };
};

const navItems: Array<{ key: ViewKey; label: string; icon: string }> = [
  { key: "dashboard", label: "Overzicht", icon: "◫" },
  { key: "users", label: "Gebruikers", icon: "◎" },
  { key: "ranked", label: "Ranked · Olympus", icon: "♜" },
  { key: "network", label: "IP-toegang", icon: "⌁" },
  { key: "cards", label: "Kaarttoegang", icon: "◇" },
  { key: "releases", label: "Releases", icon: "⇧" },
  { key: "audit", label: "Auditlog", icon: "≡" },
];

const activityBars = [30, 42, 34, 58, 48, 72, 64, 88, 70, 96, 82, 91, 68, 78, 92, 84, 100, 88, 94, 77, 90, 84, 98, 91];
const regionBars = [78, 64, 51, 39];

function redirectIfSessionExpired(response: Response) {
  if (response.status !== 401) return false;
  window.location.assign("/login?expired=1");
  return true;
}

export function AdminConsole({ viewer }: { viewer: Viewer }) {
  const [view, setView] = useState<ViewKey>("dashboard");
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [query, setQuery] = useState("");
  const [data, setData] = useState<AdminData | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [notice, setNotice] = useState("");
  const [error, setError] = useState("");

  async function logout(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setBusy(true);
    try {
      const response = await fetch("/auth/logout", { method: "POST", redirect: "manual" });
      if (response.type !== "opaqueredirect" && !response.ok) throw new Error("Uitloggen is niet gelukt. Probeer het opnieuw.");
      window.location.replace("/login?logged_out=1");
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Uitloggen is niet gelukt.");
      setBusy(false);
    }
  }

  async function loadData() {
    setLoading(true);
    setError("");
    try {
      const response = await fetch("/api/admin", { cache: "no-store" });
      if (redirectIfSessionExpired(response)) return;
      const payload = (await response.json()) as AdminData & { error?: string };
      if (!response.ok) throw new Error(payload.error || "De beheerdata kon niet worden geladen.");
      setData(payload);
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Onbekende fout");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    let cancelled = false;
    fetch("/api/admin", { cache: "no-store" })
      .then(async (response) => {
        if (redirectIfSessionExpired(response)) return;
        const payload = (await response.json()) as AdminData & { error?: string };
        if (!response.ok) throw new Error(payload.error || "De beheerdata kon niet worden geladen.");
        return payload;
      })
      .then((payload) => {
        if (!cancelled && payload) setData(payload);
      })
      .catch((caught: unknown) => {
        if (!cancelled) setError(caught instanceof Error ? caught.message : "Onbekende fout");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });

    return () => { cancelled = true; };
  }, []);

  async function mutate(action: string, values: Record<string, unknown>, success: string) {
    setBusy(true);
    setError("");
    setNotice("");
    try {
      const response = await fetch("/api/admin", {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify({ action, ...values }),
      });
      if (redirectIfSessionExpired(response)) return;
      const payload = (await response.json()) as { error?: string; message?: string; warning?: string };
      if (!response.ok) throw new Error(payload.error || "De wijziging kon niet worden opgeslagen.");
      setNotice(payload.message || success);
      await loadData();
      if (payload.warning) setError(payload.warning);
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : "Onbekende fout");
    } finally {
      setBusy(false);
    }
  }

  const search = query.trim().toLowerCase();
  const filteredUsers = useMemo(
    () => data?.users.filter((user) => `${user.display_name} ${user.email} ${user.access_group}`.toLowerCase().includes(search)) ?? [],
    [data, search],
  );
  const filteredIps = useMemo(
    () => data?.ipRules.filter((rule) => `${rule.cidr} ${rule.label} ${rule.scope}`.toLowerCase().includes(search)) ?? [],
    [data, search],
  );
  const filteredCards = useMemo(
    () => data?.cardPolicies.filter((policy) => `${policy.card_id} ${policy.card_name} ${policy.audience}`.toLowerCase().includes(search)) ?? [],
    [data, search],
  );

  function navigate(next: ViewKey) {
    setView(next);
    setQuery("");
    setSidebarOpen(false);
  }

  const pageLabel = navItems.find((item) => item.key === view)?.label ?? "Overzicht";

  return (
    <div className="consoleShell">
      <aside className={`sidebar ${sidebarOpen ? "sidebarOpen" : ""}`}>
        <div className="brand">
          <span className="brandMark" aria-hidden="true">I</span>
          <div>
            <strong>ICARUS</strong>
            <span>CONTROL</span>
          </div>
        </div>

        <div className="environmentCard">
          <span className="pulseDot" />
          <div>
            <small>OMGEVING</small>
            <strong>Productie</strong>
          </div>
          <span className="environmentTag">LIVE</span>
        </div>

        <nav aria-label="Hoofdnavigatie">
          <p className="navCaption">OPERATIONS</p>
          {navItems.map((item) => (
            <button
              key={item.key}
              className={view === item.key ? "navItem active" : "navItem"}
              onClick={() => navigate(item.key)}
              type="button"
            >
              <span className="navIcon" aria-hidden="true">{item.icon}</span>
              <span>{item.label}</span>
              {item.key === "cards" && <span className="navBadge">7</span>}
            </button>
          ))}
        </nav>

        <div className="sidebarFooter">
          <div className="systemStatus"><span /> Alle systemen operationeel</div>
          <small>Rules 1.8.4 · Catalogus 2026.08</small>
        </div>
      </aside>

      <button
        className={`sidebarBackdrop ${sidebarOpen ? "visible" : ""}`}
        onClick={() => setSidebarOpen(false)}
        aria-label="Navigatie sluiten"
        type="button"
      />

      <main className="mainContent">
        <header className="topbar">
          <div className="titleGroup">
            <button className="menuButton" type="button" onClick={() => setSidebarOpen(true)} aria-label="Navigatie openen">☰</button>
            <div>
              <span>GAME OPERATIONS</span>
              <h1>{pageLabel}</h1>
            </div>
          </div>
          <label className="searchBox">
            <span aria-hidden="true">⌕</span>
            <input
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder="Zoek spelers, IP-adressen of kaarten..."
              aria-label="Doorzoek het adminpanel"
            />
            <kbd>⌘ K</kbd>
          </label>
          <div className="topActions">
            <button className="iconButton" type="button" onClick={() => void loadData()} aria-label="Gegevens vernieuwen">↻</button>
            <div className="viewer">
              <span className="avatar">{viewer.name.slice(0, 2).toUpperCase()}</span>
              <div><strong>{data?.actor.name ?? viewer.name}</strong><small>{data?.actor.role ?? "Beheerder"}</small></div>
            </div>
            <form action="/auth/logout" method="post" onSubmit={logout}>
              <button className="tableAction" style={{ minHeight: 44, padding: "0 12px", fontSize: 11 }} disabled={busy} type="submit">Uitloggen</button>
            </form>
          </div>
        </header>

        <div className="contentArea">
          {(notice || error) && (
            <div className={error ? "toast errorToast" : "toast"} role="status">
              <span>{error ? "!" : "✓"}</span>{error || notice}
              <button type="button" onClick={() => { setNotice(""); setError(""); }} aria-label="Melding sluiten">×</button>
            </div>
          )}

          {loading && !data ? (
            <LoadingState />
          ) : error && !data ? (
            <EmptyState title="Geen verbinding met de beheerlaag" detail={error} action={() => void loadData()} />
          ) : data ? (
            <>
              {view === "dashboard" && <Dashboard data={data} navigate={navigate} />}
              {view === "users" && <UsersView users={filteredUsers} settings={data.gameSettings} busy={busy} mutate={mutate} />}
              {view === "ranked" && <RankedPanel search={query} canManage={["Super Admin", "Admin", "Moderator"].includes(data.actor.role)} />}
              {view === "network" && <NetworkView rules={filteredIps} busy={busy} mutate={mutate} />}
              {view === "cards" && <CardsView policies={filteredCards} busy={busy} mutate={mutate} />}
              {view === "releases" && <ReleasesView releases={data.releases} busy={busy} mutate={mutate} />}
              {view === "audit" && <AuditView events={data.auditEvents} />}
            </>
          ) : null}
        </div>
      </main>
    </div>
  );
}

function Dashboard({ data, navigate }: { data: AdminData; navigate: (view: ViewKey) => void }) {
  const stats = [
    { label: "Spelers online", value: formatNumber(data.metrics.onlinePlayers), delta: "+8.4%", tone: "mint", action: "users" as ViewKey },
    { label: "Live duels", value: formatNumber(data.metrics.liveDuels), delta: "+3.1%", tone: "cyan", action: "dashboard" as ViewKey },
    { label: "Kaarten catalogus", value: formatNumber(data.metrics.catalogCards), delta: "100% geladen", tone: "violet", action: "cards" as ViewKey },
    { label: "Openstaande wijzigingen", value: String(data.metrics.pendingChanges), delta: "actie nodig", tone: "amber", action: "releases" as ViewKey },
  ];
  const activeUsers = data.users.filter((user) => user.status === "active").length;
  const activeIps = data.ipRules.filter((rule) => rule.status === "active").length;

  return (
    <section className="dashboardView" aria-label="Dashboard">
      <div className="statGrid">
        {stats.map((stat, index) => (
          <button className={`statCard ${stat.tone}`} key={stat.label} type="button" onClick={() => navigate(stat.action)}>
            <div><span>{stat.label}</span><i>0{index + 1}</i></div>
            <strong>{stat.value}</strong>
            <small><b>↗</b> {stat.delta}</small>
            <div className="miniTrend">{activityBars.slice(index * 4, index * 4 + 9).map((height, bar) => <span key={bar} style={{ height: `${Math.max(18, height)}%` }} />)}</div>
          </button>
        ))}
      </div>

      <div className="dashboardGrid">
        <article className="panel activityPanel">
          <PanelHeader eyebrow="LIVE TELEMETRIE" title="Duelactiviteit" action="Afgelopen 24 uur" />
          <div className="activitySummary">
            <div><small>Matches vandaag</small><strong>13.278</strong><span>+12.5% tegenover gisteren</span></div>
            <div className="legend"><span><i className="legendMint" /> Ranked</span><span><i className="legendBlue" /> Casual</span></div>
          </div>
          <div className="barChart" aria-label="Duelactiviteit per uur">
            {activityBars.map((height, index) => <span key={index} style={{ height: `${height}%` }}><i /></span>)}
          </div>
          <div className="chartAxis"><span>00:00</span><span>06:00</span><span>12:00</span><span>18:00</span><span>24:00</span></div>
        </article>

        <article className="panel healthPanel">
          <PanelHeader eyebrow="INFRASTRUCTUUR" title="Serverstatus" action="Realtime" />
          <div className="healthScore">
            <div className="scoreRing"><strong>99.98</strong><span>% uptime</span></div>
            <div><span className="healthyPill">Gezond</span><p>Alle regio’s reageren binnen de afgesproken waarden.</p></div>
          </div>
          <div className="regionList">
            {["Amsterdam", "Frankfurt", "London", "Montréal"].map((region, index) => (
              <div className="regionRow" key={region}>
                <span><i />{region}</span><div><b style={{ width: `${regionBars[index]}%` }} /></div><small>{18 + index * 7} ms</small>
              </div>
            ))}
          </div>
        </article>

        <article className="panel accessPanel">
          <PanelHeader eyebrow="TOEGANG" title="Beveiligingsstatus" action="Beheren" onAction={() => navigate("network")} />
          <div className="accessGauge"><span style={{ "--gauge": "86%" } as CSSProperties}><b>{activeIps}</b><small>actieve regels</small></span></div>
          <div className="securityFacts">
            <div><span>Gebruikers actief</span><strong>{activeUsers}/{data.users.length}</strong></div>
            <div><span>Geblokkeerde pogingen</span><strong className="dangerText">23</strong></div>
            <div><span>Admin MFA</span><strong className="successText">Verplicht</strong></div>
          </div>
        </article>

        <article className="panel cardPanel">
          <PanelHeader eyebrow="CONTENT CONTROL" title="Kaartbeschikbaarheid" action="Alle kaarten" onAction={() => navigate("cards")} />
          <div className="policyStats">
            <div><strong>13.782</strong><span>Publiek</span><i className="publicBar" /></div>
            <div><strong>31</strong><span>Testers</span><i className="testerBar" /></div>
            <div><strong>7</strong><span>Verborgen</span><i className="hiddenBar" /></div>
          </div>
          <div className="recentPolicies">
            {data.cardPolicies.slice(0, 4).map((policy) => (
              <div key={policy.id}><span className="cardRune">{policy.card_name.slice(0, 1)}</span><p><strong>{policy.card_name}</strong><small>ID {policy.card_id}</small></p><Status value={policy.availability} /></div>
            ))}
          </div>
        </article>

        <article className="panel releasePanel">
          <PanelHeader eyebrow="DEPLOYMENTS" title="Release-pipeline" action="Release maken" onAction={() => navigate("releases")} />
          <div className="releaseTimeline">
            {data.releases.slice(0, 4).map((release, index) => (
              <div key={release.id} className="releaseStep">
                <span className={release.status === "live" ? "releaseNode live" : "releaseNode"}>{index + 1}</span>
                <div><strong>{release.version}</strong><small>{release.kind} · {release.environment}</small></div>
                <Status value={release.status} />
              </div>
            ))}
          </div>
        </article>
      </div>
    </section>
  );
}

function UsersView({ users, settings, busy, mutate }: { users: GameUser[]; settings: GameSetting[]; busy: boolean; mutate: (action: string, values: Record<string, unknown>, success: string) => Promise<void> }) {
  const [newRole, setNewRole] = useState("Player");
  const playerAccounts = users.filter(user => ["Player", "Tester"].includes(user.role));
  const internalAccounts = users.filter(user => !["Player", "Tester"].includes(user.role));
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const values = Object.fromEntries(new FormData(form));
    await mutate("create-user", values, `${values.displayName} is toegevoegd.`);
    form.reset();
  }

  async function updateSettings(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const values = Object.fromEntries(new FormData(form));
    await mutate("update-game-settings", values, "Game-toegang en spelersinstellingen zijn bijgewerkt.");
  }

  async function resetPassword(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const values = Object.fromEntries(new FormData(form));
    await mutate("reset-user-password", values, "Het wachtwoord is veilig opnieuw ingesteld.");
    form.reset();
  }

  async function sendPasswordLink(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const values = Object.fromEntries(new FormData(event.currentTarget));
    await mutate("send-user-password-link", values, "De wachtwoordlink is verstuurd.");
  }

  return (
    <section className="workspaceGrid">
      <article className="panel tablePanel widePanel">
        <PanelHeader eyebrow="ACCOUNT CONTROL" title="Gamegebruikers" action={`${users.length} resultaten`} />
        <div className="tableScroller"><table><thead><tr><th>Gebruiker</th><th>Groep</th><th>Rol</th><th>Game-toegang</th><th>Account</th><th>Laatst actief</th><th /></tr></thead><tbody>
          {users.map((user) => {
            const gameSetting = settings.find((setting) => setting.user_id === user.id && setting.game_id === "master-duel");
            return <tr key={user.id}>
            <td><div className="identityCell"><span>{initials(user.display_name)}</span><div><strong>{user.display_name}</strong><small>{user.email}</small></div></div></td>
            <td>{user.access_group}</td><td>{user.role}</td><td><Status value={gameSetting?.can_play ? "allowed" : "denied"} /><small className="cellMeta">{gameSetting?.matchmaking_region ?? "niet toegewezen"}</small></td><td><Status value={user.status} /></td><td>{relativeDate(user.last_seen)}</td>
            <td><button className="tableAction" disabled={busy} type="button" onClick={() => void mutate("toggle-user", { id: user.id, status: user.status === "active" ? "blocked" : "active" }, user.status === "active" ? "Gebruiker geblokkeerd." : "Gebruiker geactiveerd.")}>{user.status === "active" ? "Blokkeren" : "Activeren"}</button></td>
          </tr>;})}
        </tbody></table></div>
      </article>
      <article className="panel formPanel">
        <PanelHeader eyebrow="NIEUW ACCOUNT" title="Gebruiker toevoegen" />
        <form onSubmit={submit}>
          <FormField label="Naam" name="displayName" placeholder="Bijv. Renée Tester" required />
          <FormField label="E-mailadres" name="email" type="email" placeholder="speler@example.com" required />
          {newRole === "Moderator" ? <FormField label="Intern moderatorwachtwoord" name="accessCode" type="password" placeholder="12–128 tekens" required minLength={12} maxLength={128} /> : <p className="formHint">De speler ontvangt een e-maillink en kiest zelf een blijvend wachtwoord. Tot die tijd is aanmelden niet mogelijk.</p>}
          <label>Toegangsgroep<select name="accessGroup" defaultValue="Closed Alpha"><option>Closed Alpha</option><option>Internal QA</option><option>Creator Program</option><option>Public</option></select></label>
          <label>Rol<select name="role" value={newRole} onChange={event => setNewRole(event.target.value)}><option>Player</option><option>Tester</option><option>Moderator</option></select></label>
          <FormField label="Game-ID" name="gameId" placeholder="master-duel" required mono />
          <label>Matchmakingregio<select name="matchmakingRegion" defaultValue="eu-west"><option value="eu-west">Europa West</option><option value="us-east">Verenigde Staten Oost</option><option value="asia-pacific">Azië-Pacific</option></select></label>
          <FormField label="Deckslots" name="deckSlots" type="number" placeholder="20" required />
          <button className="primaryButton" disabled={busy} type="submit">{busy ? "Opslaan..." : newRole === "Moderator" ? "Moderator toevoegen" : "Speler toevoegen en uitnodigen"}</button>
        </form>
        <div className="formDivider" />
        <PanelHeader eyebrow="GAME ENTITLEMENT" title="Toegang en settings" />
        <form onSubmit={updateSettings}>
          <label>Gebruiker<select name="userId" required defaultValue=""><option value="" disabled>Selecteer een gebruiker</option>{users.map((user) => <option key={user.id} value={user.id}>{user.display_name}</option>)}</select></label>
          <FormField label="Game-ID" name="gameId" placeholder="master-duel" required mono />
          <label>Mag spelen<select name="canPlay" defaultValue="true"><option value="true">Ja</option><option value="false">Nee</option></select></label>
          <label>Matchmakingregio<select name="matchmakingRegion" defaultValue="eu-west"><option value="eu-west">Europa West</option><option value="us-east">Verenigde Staten Oost</option><option value="asia-pacific">Azië-Pacific</option></select></label>
          <label>Ranked beschikbaar<select name="rankedEnabled" defaultValue="true"><option value="true">Ja</option><option value="false">Nee</option></select></label>
          <FormField label="Deckslots" name="deckSlots" type="number" placeholder="20" required />
          <button className="primaryButton" disabled={busy} type="submit">{busy ? "Opslaan..." : "Settings bijwerken"}</button>
        </form>
        <div className="formDivider" />
        <PanelHeader eyebrow="SPELERSWACHTWOORD" title="Wachtwoordlink versturen" />
        <p className="formHint">De speler stelt zelf een eerste of nieuw wachtwoord in via een beveiligde e-maillink. Een bestaand wachtwoord blijft geldig totdat de speler de link gebruikt.</p>
        <form onSubmit={sendPasswordLink}>
          <label>Speler<select name="userId" required defaultValue=""><option value="" disabled>Selecteer een speler</option>{playerAccounts.map((user) => <option key={user.id} value={user.id}>{user.display_name} · {user.email}</option>)}</select></label>
          <button className="primaryButton" disabled={busy} type="submit">{busy ? "Versturen..." : "Wachtwoordlink versturen"}</button>
        </form>
        {internalAccounts.length > 0 && <>
        <div className="formDivider" />
        <PanelHeader eyebrow="INTERN ACCOUNT" title="Wachtwoord herstellen" />
        <p className="formHint">Afzonderlijke interne reset voor accounts die niet voor openbaar spelersherstel in aanmerking komen.</p>
        <form onSubmit={resetPassword}>
          <label>Gebruiker<select name="userId" required defaultValue=""><option value="" disabled>Selecteer een gebruiker</option>{internalAccounts.map((user) => <option key={user.id} value={user.id}>{user.display_name} · {user.email}</option>)}</select></label>
          <FormField label="Nieuw wachtwoord" name="accessCode" type="password" placeholder="12–128 tekens" required minLength={12} maxLength={128} />
          <FormField label="Herhaal nieuw wachtwoord" name="accessCodeConfirmation" type="password" placeholder="Nogmaals invoeren" required minLength={12} maxLength={128} />
          <button className="primaryButton" disabled={busy} type="submit">{busy ? "Resetten..." : "Wachtwoord resetten"}</button>
        </form>
        </>}
      </article>
    </section>
  );
}

function NetworkView({ rules, busy, mutate }: { rules: IpRule[]; busy: boolean; mutate: (action: string, values: Record<string, unknown>, success: string) => Promise<void> }) {
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const values = Object.fromEntries(new FormData(form));
    await mutate("create-ip-rule", values, `${values.cidr} staat nu op de whitelist.`);
    form.reset();
  }

  return (
    <section className="workspaceGrid">
      <article className="panel tablePanel widePanel">
        <PanelHeader eyebrow="NETWORK POLICY" title="IP-whitelist" action={`${rules.filter((rule) => rule.status === "active").length} actief`} />
        <div className="tableScroller"><table><thead><tr><th>Adres / bereik</th><th>Label</th><th>Scope</th><th>Omgeving</th><th>Verloopt</th><th>Status</th><th /></tr></thead><tbody>
          {rules.map((rule) => <tr key={rule.id}><td><code>{rule.cidr}</code></td><td>{rule.label}</td><td>{rule.scope}</td><td>{rule.environment}</td><td>{rule.expires_at ? shortDate(rule.expires_at) : "Nooit"}</td><td><Status value={rule.status} /></td><td><button className="tableAction" disabled={busy} type="button" onClick={() => void mutate("toggle-ip-rule", { id: rule.id, status: rule.status === "active" ? "disabled" : "active" }, "Netwerkregel bijgewerkt.")}>{rule.status === "active" ? "Pauzeren" : "Activeren"}</button></td></tr>)}
        </tbody></table></div>
      </article>
      <article className="panel formPanel">
        <PanelHeader eyebrow="NIEUWE REGEL" title="Adres toestaan" />
        <form onSubmit={submit}>
          <FormField label="IPv4, IPv6 of CIDR" name="cidr" placeholder="203.0.113.24/32" required mono />
          <FormField label="Label" name="label" placeholder="QA-kantoor Amsterdam" required />
          <label>Scope<select name="scope" defaultValue="Gameplay"><option>Gameplay</option><option>Admin</option><option>API</option></select></label>
          <label>Omgeving<select name="environment" defaultValue="Production"><option>Production</option><option>Staging</option><option>Development</option></select></label>
          <FormField label="Einddatum (optioneel)" name="expiresAt" type="date" />
          <button className="primaryButton" disabled={busy} type="submit">{busy ? "Opslaan..." : "Whitelistregel toevoegen"}</button>
          <p className="formHint">Accountrechten blijven altijd leidend; een toegestaan IP-adres is een extra beveiligingslaag.</p>
        </form>
      </article>
    </section>
  );
}

function CardsView({ policies, busy, mutate }: { policies: CardPolicy[]; busy: boolean; mutate: (action: string, values: Record<string, unknown>, success: string) => Promise<void> }) {
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const values = Object.fromEntries(new FormData(form));
    await mutate("create-card-policy", values, `${values.cardName} is aan content control toegevoegd.`);
    form.reset();
  }

  return (
    <section className="workspaceGrid">
      <article className="panel tablePanel widePanel">
        <PanelHeader eyebrow="CONTENT CONTROL" title="Kaartbeschikbaarheid" action="Catalogus 13.820" />
        <div className="tableScroller"><table><thead><tr><th>Kaart</th><th>Regulation</th><th>Doelgroep</th><th>Beschikbaarheid</th><th>Van kracht</th></tr></thead><tbody>
          {policies.map((policy) => <tr key={policy.id}>
            <td><div className="cardIdentity"><span>{policy.card_name.slice(0, 1)}</span><div><strong>{policy.card_name}</strong><small>ID {policy.card_id}</small></div></div></td>
            <td>{policy.regulation}</td>
            <td><select className="inlineSelect" value={policy.audience} disabled={busy} onChange={(event) => void mutate("update-card-policy", { id: policy.id, audience: event.target.value, availability: policy.availability }, "Doelgroep bijgewerkt.")}><option>Everyone</option><option>Closed Alpha</option><option>Internal QA</option><option>Creators</option><option>Nobody</option></select></td>
            <td><select className={`inlineSelect statusSelect ${statusClass(policy.availability)}`} value={policy.availability} disabled={busy} onChange={(event) => void mutate("update-card-policy", { id: policy.id, audience: policy.audience, availability: event.target.value }, "Kaartbeschikbaarheid bijgewerkt.")}><option>public</option><option>testers</option><option>hidden</option><option>disabled</option></select></td>
            <td>{shortDate(policy.effective_from)}</td>
          </tr>)}
        </tbody></table></div>
      </article>
      <article className="panel formPanel">
        <PanelHeader eyebrow="NIEUWE POLICY" title="Kaart toevoegen" />
        <form onSubmit={submit}>
          <FormField label="Database-ID" name="cardId" placeholder="89631139" required mono />
          <FormField label="Kaartnaam" name="cardName" placeholder="Blue-Eyes White Dragon" required />
          <label>Beschikbaarheid<select name="availability" defaultValue="testers"><option value="public">Publiek</option><option value="testers">Alleen testers</option><option value="hidden">Verborgen</option><option value="disabled">Uitgeschakeld</option></select></label>
          <label>Doelgroep<select name="audience" defaultValue="Closed Alpha"><option>Everyone</option><option>Closed Alpha</option><option>Internal QA</option><option>Creators</option><option>Nobody</option></select></label>
          <button className="primaryButton" disabled={busy} type="submit">{busy ? "Opslaan..." : "Policy toevoegen"}</button>
        </form>
      </article>
    </section>
  );
}

function ReleasesView({ releases, busy, mutate }: { releases: Release[]; busy: boolean; mutate: (action: string, values: Record<string, unknown>, success: string) => Promise<void> }) {
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const values = Object.fromEntries(new FormData(form));
    await mutate("create-release", values, `Release ${values.version} is als concept aangemaakt.`);
    form.reset();
  }

  return (
    <section className="workspaceGrid">
      <article className="panel releaseBoard widePanel">
        <PanelHeader eyebrow="VERSION CONTROL" title="Release-pipeline" action="Rollback beschikbaar" />
        <div className="releaseColumns">
          {["draft", "staging", "live"].map((status) => <div className="releaseColumn" key={status}><h3>{status === "draft" ? "Concept" : status === "staging" ? "Staging" : "Productie"}<span>{releases.filter((release) => release.status === status).length}</span></h3>
            {releases.filter((release) => release.status === status).map((release) => <article className="releaseCard" key={release.id}><div><span>{release.kind}</span><Status value={release.status} /></div><strong>{release.version}</strong><p>{release.notes}</p><small>{release.environment} · {shortDate(release.created_at)}</small>{release.status !== "live" && <button type="button" disabled={busy} onClick={() => void mutate("promote-release", { id: release.id, currentStatus: release.status }, release.status === "draft" ? "Release naar staging verplaatst." : "Release staat live.")}>{release.status === "draft" ? "Naar staging" : "Publiceren"} →</button>}</article>)}
          </div>)}
        </div>
      </article>
      <article className="panel formPanel">
        <PanelHeader eyebrow="NIEUWE RELEASE" title="Versie voorbereiden" />
        <form onSubmit={submit}>
          <FormField label="Versie" name="version" placeholder="v1.9.0" required mono />
          <label>Type<select name="kind" defaultValue="Content"><option>Client</option><option>Server</option><option>Content</option><option>Rules</option></select></label>
          <label>Doelomgeving<select name="environment" defaultValue="Production"><option>Production</option><option>Staging</option></select></label>
          <label>Release-opmerking<textarea name="notes" placeholder="Nieuwe kaartpool en stabiliteitsverbeteringen" required /></label>
          <button className="primaryButton" disabled={busy} type="submit">{busy ? "Opslaan..." : "Releaseconcept maken"}</button>
        </form>
      </article>
    </section>
  );
}

function AuditView({ events }: { events: AuditEvent[] }) {
  return (
    <section className="panel auditPanel">
      <PanelHeader eyebrow="SECURITY & COMPLIANCE" title="Onveranderbaar activiteitenlog" action={`${events.length} recente gebeurtenissen`} />
      <div className="auditTimeline">
        {events.map((event) => <article key={event.id}><span className={`auditIcon ${event.severity}`}>{event.severity === "critical" ? "!" : event.severity === "warning" ? "△" : "✓"}</span><div className="auditContent"><div><strong>{event.action}</strong><Status value={event.severity} /></div><p>{event.detail}</p><small><b>{event.actor}</b> · {event.target} · {relativeDate(event.created_at)}</small></div></article>)}
      </div>
    </section>
  );
}

function PanelHeader({ eyebrow, title, action, onAction }: { eyebrow: string; title: string; action?: string; onAction?: () => void }) {
  return <header className="panelHeader"><div><span>{eyebrow}</span><h2>{title}</h2></div>{action && (onAction ? <button type="button" onClick={onAction}>{action} →</button> : <small>{action}</small>)}</header>;
}

function Status({ value }: { value: string }) {
  return <span className={`status ${statusClass(value)}`}><i />{labelStatus(value)}</span>;
}

function FormField({ label, name, placeholder, type = "text", required = false, mono = false, minLength, maxLength }: { label: string; name: string; placeholder?: string; type?: string; required?: boolean; mono?: boolean; minLength?: number; maxLength?: number }) {
  return <label>{label}<input className={mono ? "monoInput" : ""} name={name} placeholder={placeholder} type={type} required={required} minLength={minLength} maxLength={maxLength} autoComplete={type === "password" ? "new-password" : undefined} /></label>;
}

function LoadingState() {
  return <div className="loadingGrid" role="status" aria-label="Beheerdata laden">{Array.from({ length: 8 }, (_, index) => <span key={index} />)}</div>;
}

function EmptyState({ title, detail, action }: { title: string; detail: string; action: () => void }) {
  return <section className="emptyState"><span>!</span><h2>{title}</h2><p>{detail}</p><button className="primaryButton" type="button" onClick={action}>Opnieuw proberen</button></section>;
}

function initials(name: string) {
  return name.split(/\s+/).slice(0, 2).map((part) => part[0]).join("").toUpperCase();
}

function formatNumber(value: number) {
  return new Intl.NumberFormat("nl-NL").format(value);
}

function shortDate(value: string) {
  return new Intl.DateTimeFormat("nl-NL", { day: "2-digit", month: "short", year: "numeric" }).format(new Date(value));
}

function relativeDate(value: string) {
  const minutes = Math.max(1, Math.round((Date.now() - new Date(value).getTime()) / 60000));
  if (minutes < 60) return `${minutes} min geleden`;
  if (minutes < 1440) return `${Math.round(minutes / 60)} uur geleden`;
  return `${Math.round(minutes / 1440)} dagen geleden`;
}

function statusClass(value: string) {
  const normalized = value.toLowerCase();
  if (["active", "live", "public", "info", "allowed"].includes(normalized)) return "positive";
  if (["staging", "testers", "warning"].includes(normalized)) return "warning";
  if (["blocked", "disabled", "hidden", "critical", "denied"].includes(normalized)) return "negative";
  return "neutral";
}

function labelStatus(value: string) {
  const labels: Record<string, string> = { active: "Actief", blocked: "Geblokkeerd", disabled: "Uit", allowed: "Toegestaan", denied: "Geweigerd", public: "Publiek", testers: "Testers", hidden: "Verborgen", draft: "Concept", staging: "Staging", live: "Live", info: "Info", warning: "Let op", critical: "Kritiek" };
  return labels[value.toLowerCase()] ?? value;
}
