# Master Duel clean-room completion map

Source baseline: `Master_Duel_clean-room_reconstructiespecificatie_NL.docx`, reference build 2.8.0, reviewed on 2026-08-07.

This map distinguishes implemented behavior from partial behavior, deliberate product differences, and missing systems. It does not claim pixel parity or legal permission to distribute third-party card data, art, marks, audio, or scripts.

## Delivery phases

| Specification phase | Current status | Evidence in this project | Highest-priority gap |
| --- | --- | --- | --- |
| 0. License and clean room | Partial | Original runtime UI/theme work exists, but the repository also contains imported card databases, pictures, Lua scripts, and archives. | Complete provenance register and distribution decision for every third-party asset/data source. |
| 1. Rules kernel | Partial | `CleanDuelGame` has zones, turn phases, card identity, prompts, battle, all requested summon families, specific responses, seeded RNG, a monotonic event stream, and deterministic state hashes. | Replace card-specific response branches with generic priority/action windows, LIFO chains, command envelopes, action tokens, and replayable commands. |
| 2. Duel vertical slice | Partial | Landscape field, card detail, hand, log, phase control, local AI, choice prompts, structured results, surrender confirmation, and Play Again exist. | Reconnect/snapshot recovery, battle-step completeness, input parity, settings/help, animation/audio pacing, and a full result/reward model. |
| 3. Online foundation | Missing | No auth, authoritative remote runtime, matchmaking, rooms, presence, spectate, or network replay service was found. | Define an authoritative match boundary before adding public multiplayer. |
| 4. Collection and decks | Mostly missing | A searchable card database and fixed Blue-Eyes recipe exist. | Deck library/editor, shared validator, revisions, ownership, collection, crafting preview/commit, and ledger. |
| 5. Meta and live ops | Missing | No shop, pack transaction, missions, pass, events, notices, gifts, Solo graph, or versioned content rollout was found. | Build only after the authoritative duel and account/economy boundaries are stable. |
| 6. Polish and compliance | Early | A clean-room visual theme and responsive 16:9 layout are in progress. | Accessibility, controller/touch/keyboard parity, localization expansion, original audio/VFX, platform QA, privacy, and compliance review. |

## Core rules completion

| Requirement family | Status | Notes |
| --- | --- | --- |
| Stable card identity, owner/controller, public/hidden zones | Partial | Runtime IDs and owner/controller exist. A general movement history and viewer-filtered state projection do not. |
| Draw/Standby/Main/Battle/Main 2/End flow | Partial | Six phases and first-turn Battle restriction exist. Full Battle Step/Damage Step windows and optional-trigger ordering are not generic. |
| Normal/Flip/Special/Ritual/Fusion/Synchro/Xyz/Pendulum/Link | Implemented for the current card pool | Legal choices are filtered for the fixed recipe. Generic material/replacement rules and the full card pool remain incomplete. |
| Priority, action windows, chains, spell speed, SEGOC | Partial | Specific quick-effect/negation paths exist. There is no generic both-pass window, arbitrary LIFO chain stack, or SEGOC ordering engine. |
| Deterministic RNG and audit log | In progress | Duel starts accept a captured seed; same seed and inputs produce the same state hash; a full event stream is retained. Replayable command transcripts and content-bundle pinning are still missing. |
| Authoritative command loop | Missing | UI currently calls mutable game methods directly; there is no match ID/state sequence/action token/request ID validation envelope. |
| Termination | Partial | LP zero, simultaneous draw, deck-out, card-effect win, surrender, and structured result reasons exist. Disconnect/server-cancel handlers are modeled but not wired. A gameplay timer is deliberately excluded. |

## Deliberate product decision: no gameplay timer

The game will not impose a two-minute turn or duel timer. No timer countdown, timer-expiry command, or timer-loss result is implemented. This is an explicit product difference from the reference specification, not an open defect.

Long-running online sessions still need separate operational policies for disconnect detection, abandoned sessions, and server cleanup. Those policies must not appear to the player as a two-minute gameplay clock.

## Implemented vertical slice: auditable surrender and result

### Requirement card

- User story: during any active duel with no other prompt open, the local player can open the Duel Menu, choose Surrender Duel, review a destructive confirmation, cancel safely, or confirm the loss.
- Legal window: active duel only; the surrender menu does not replace another unresolved choice prompt.
- State transition: confirmation closes prompts, ends the duel once, records `Surrender`, assigns the rival as winner and the local player as loser, and blocks further duel actions.
- Presentation: the result overlay reads structured winner/reason data and offers Play Again.
- Observability: the confirmation, cancellation, and match-end messages remain in the full monotonic event stream; the visible log remains a short projection.
- Determinism: `StartNewDuel(seed)` resets RNG and produces a stable state hash independent of global runtime card IDs.

### Acceptance evidence

- Same seed and same inputs yield identical opening deck/hand order and identical deterministic state hash.
- State sequence equals the number of retained full event records.
- Cancelling surrender leaves the duel active with no result.
- Confirming surrender yields one structured Rival win with reason `Surrender` and no pending prompt.
- Play Again clears the result and starts a new auditable match state.

## Next implementation order

1. Generic command envelope with match ID, actor, expected state sequence, action token, request ID, and idempotent result.
2. Generic priority/action-window and LIFO chain model, then migrate existing quick-effect branches onto it.
3. Replayable command transcript plus golden replay hash and content/ruleset version pinning.
4. Battle Step/Damage Step matrix and generic trigger ordering.
5. Reconnect snapshots without a gameplay timer.
6. Shared deck validator before building the Deck Editor UI.
7. Account/deck revisions, collection and ledger before crafting, packs, rewards, or live events.
