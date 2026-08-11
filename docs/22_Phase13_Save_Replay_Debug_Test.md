# Phase 13 — Save / Replay / Debug / Test

Save 純 GameState：faction、settlement、unit、hero、army、resource、building、tech、objective、clock、random state。

Metadata：SaveVersion、FrameworkVersion、ContentVersion、ScenarioId、Timestamp。

Replay：InitialState + Seed + Commands + Tick。

Debug Console：spawn、kill、damage、give_resource、capture、set_speed、toggle_ai、show_path、show_threat。

Tests：EditMode、PlayMode、AI simulation/soak。

Acceptance：戰鬥中 save→reload 後核心狀態一致。
