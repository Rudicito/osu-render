using System.Drawing;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Overlays.Notifications;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens;
using OsuRender.BeatmapDownloader;
using OsuRender.BeatmapDownloader.Mino;
using OsuRender.Player;

namespace OsuRender
{
    public partial class OsuRenderGame : OsuGameBase
    {
        private FrameworkConfigManager config = null!;
        private IBeatmapDownloader beatmapDownloader = null!;
        private OsuScreenStack screenStack = null!;

        [Cached]
        private readonly Options options;

        [Cached]
        private readonly RecordManager recordManager;

        public OsuRenderGame(Options options)
        {
            this.options = options;
            recordManager = new RecordManager();

            API = new DummyAPIAccess();
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            host.MaximumDrawHz = 0;

            config = (FrameworkConfigManager)host.Dependencies.Get(typeof(FrameworkConfigManager));

            config.SetValue(FrameworkSetting.ExecutionMode, ExecutionMode.SingleThread);
            config.SetValue(FrameworkSetting.WindowMode, WindowMode.Windowed);
            config.SetValue(FrameworkSetting.WindowedSize, new Size(options.Width, options.Height));

            LocalConfig.GetBindable<bool>(OsuSetting.ReplaySettingsOverlay).Value = false;
            LocalConfig.GetBindable<bool>(OsuSetting.GameplayLeaderboard).Value = false;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Add(recordManager);

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
                Ruleset.Value = score.ScoreInfo.Ruleset;
                SelectedMods.Value = score.ScoreInfo.Mods;
                screenStack.Push(new RenderPlayerLoader(score));
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
