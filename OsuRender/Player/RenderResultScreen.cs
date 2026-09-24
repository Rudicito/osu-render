// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace OsuRender.Player
{
    public partial class RenderResultScreen : SoloResultsScreen
    {
        [Resolved]
        private RecordManager recordManager { get; set; } = null!;

        private const double open_panel_delay = 2500;
        private const double quit_delay = open_panel_delay + 4000;

        public RenderResultScreen(ScoreInfo score) : base(score)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Scheduler.AddDelayed(() => StatisticsPanel.ToggleVisibility(), open_panel_delay);

            if (!recordManager.SkipResultsScreen)
                Scheduler.AddDelayed(() => recordManager.StopRecording(), quit_delay);
        }

        protected override Task<ScoreInfo[]> FetchScores() => Task.FromResult<ScoreInfo[]>([]);

        protected override Task<ScoreInfo[]> FetchNextPage(int direction) => Task.FromResult<ScoreInfo[]>([]);
    }
}
