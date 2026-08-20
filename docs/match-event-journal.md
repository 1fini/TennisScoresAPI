# Match event journal

`MatchEvents` is an append-only audit journal that complements the existing
`Match`, `Set`, `Game`, and `Point` projections. Those relational projections
remain the operational read model; this is not a full event-sourcing rewrite.

## Stored envelope

Each row has a stable event id, match id, per-match sequence, stable event type,
event version, JSON payload, and occurrence timestamp. The database enforces a
unique `(MatchId, Sequence)` index. The repository exposes append and ordered
read operations only.

The initial event registry is:

| Type | Version | Purpose |
| --- | --- | --- |
| `point-won` | 1 | Score-changing point, point type, server, set, and game |
| `point-undone` | 1 | Audit record for the projection point removed by Undo Last Point |
| `game-won` | 1 | Completed game and winner |
| `set-won` | 1 | Completed set and winner |
| `match-won` | 1 | Completed match and winner |
| `server-changed` | 1 | Previous and next serving player |

For one scoring command, `point-won` is emitted first, followed by any
game/set/match completion events from inner to outer. `server-changed` is last
and describes the server for the next point. Event type strings and payload
meaning are compatibility contracts. A breaking payload change requires a new
event version and an explicit deserialization branch; existing rows must not be
rewritten in place.

Undo never deletes or rewrites journal rows. It recalculates the score by
replaying the retained point projections, updates the relational projections,
and appends `point-undone` with the original point identity and timestamp.

## Production migration gate

The journal migration is additive and contains no backfill. It may be built,
tested, and merged, but it must not be applied to the Raspberry Pi production
database until the real PostgreSQL backup and restore procedure has been
validated end to end. Historical matches are intentionally not synthesized by
this migration.
