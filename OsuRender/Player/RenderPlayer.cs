// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace OsuRender.Player
{
    public partial class RenderPlayer : ReplayPlayer
    {
        public RenderPlayer(Score score, PlayerConfiguration? configuration = null) : base(score, configuration)
        {
        }

        public RenderPlayer(Func<IBeatmap, IReadOnlyList<Mod>, Score> createScore, PlayerConfiguration? configuration = null) : base(createScore, configuration)
        {
        }

        protected override ResultsScreen CreateResults(ScoreInfo score)
            => new RenderResultScreen(score);
    }
}
