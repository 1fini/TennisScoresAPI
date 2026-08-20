# Match Analytics v1

`GET /api/Matches/{matchId}/stats` returns both player sides for matches in
progress and completed matches. It derives values from the current Point
projections, so a successfully undone point is not included.

Metric attribution is explicit:

- total points, aces, and winners belong to the point winner;
- double faults, unforced errors, and forced errors belong to the other player,
  because the stored point identifies the winner rather than the player who
  committed the error;
- points served and returned are derived by replaying point order and server
  transitions through the deterministic scoring engine.

`serviceContextAvailable` states whether serving context could be reconstructed
from the match projection. When it is false, `pointsServed` and
`pointsReturned` are `null` for both players; the API does not guess those
values. Other supported metrics remain available.
