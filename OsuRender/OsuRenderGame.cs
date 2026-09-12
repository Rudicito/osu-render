using System.Drawing;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game;
using osu.Game.Database;
using osu.Game.Overlays.Notifications;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using OsuRender.BeatmapDownloader;
using OsuRender.BeatmapDownloader.Mino;

namespace OsuRender
{
    public partial class OsuRenderGame : OsuGameBase
    {
        private FrameworkConfigManager config = null!;
        private IBeatmapDownloader beatmapDownloader = null!;
        private readonly Options options;
        private OsuScreenStack screenStack = null!;

        public OsuRenderGame(Options options)
        {
            this.options = options;
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            host.MaximumDrawHz = 0;

            config = (FrameworkConfigManager)host.Dependencies.Get(typeof(FrameworkConfigManager));

            config.SetValue(FrameworkSetting.ExecutionMode, ExecutionMode.SingleThread);
            config.SetValue(FrameworkSetting.WindowMode, WindowMode.Windowed);
            config.SetValue(FrameworkSetting.WindowedSize, new Size(options.Width, options.Height));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapDownloader = new MinoBeatmapDownloader();

            Add(screenStack = new OsuScreenStack { RelativeSizeAxes = Axes.Both });

            Task.Run(main).GetAwaiter().GetResult();
        }

        private async Task main()
        {
            Score score;

            try
            {
                score = await importScore(options.ReplayFile);
            }
            catch (LegacyScoreDecoder.BeatmapNotFoundException notFound)
            {
                var beatmapset = await beatmapDownloader.GetBeatmapset(notFound.Hash);
                await importBeatmap(beatmapset);

                score = await importScore(options.ReplayFile);
            }

            Schedule(() =>
            {
                Beatmap.Value = BeatmapManager.GetWorkingBeatmap(score.ScoreInfo.BeatmapInfo);
                screenStack.Push(new ReplayPlayerLoader(score));
            });
        }

        private async Task<Score> importScore(string file)
        {
            await using var stream = File.OpenRead(file);
            return new DatabasedLegacyScoreDecoder(RulesetStore, BeatmapManager).Parse(stream);
        }

        private async Task importBeatmap(Beatmapset beatmapset)
        {
            await BeatmapManager.Import(new ProgressNotification(), [new ImportTask(beatmapset.Stream, beatmapset.FileName)]);
        }
    }
}
