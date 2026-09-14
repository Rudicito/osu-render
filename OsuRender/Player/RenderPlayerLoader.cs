// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Scoring;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;

namespace OsuRender.Player
{
    public partial class RenderPlayerLoader : PlayerLoader
    {
        [Resolved]
        private RecordManager recordManager { get; init; } = null!;

        public readonly ScoreInfo Score;

        public RenderPlayerLoader(Score score)
            : base(() => new RenderPlayer(score))
        {
            if (score.Replay == null)
                throw new ArgumentException($"{nameof(score)} must have a non-null {nameof(score.Replay)}.", nameof(score));

            Score = score.ScoreInfo;
            WindowShouldBeActiveForGameplayStart = false;
        }

        protected override bool ReadyForGameplay => true;

        [BackgroundDependencyLoader]
        private void load()
        {
            PlayerSettings.Expire();
        }

        protected override void OnPlayerLoaded()
        {
            base.OnPlayerLoaded();

            if (recordManager.SkipIntro)
                CurrentPlayer!.OnGameplayStarted += () => recordManager.StartRecording();
            else
                recordManager.StartRecording();

            if (recordManager.SkipResultsScreen)
                CurrentPlayer!.OnShowingResults += () => recordManager.StopRecording();
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
        }

        protected override void LogoExiting(OsuLogo logo)
        {
        }
    }
}
