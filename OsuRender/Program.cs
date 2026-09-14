using CommandLine;
using osu.Framework;
using osu.Framework.Platform;

namespace OsuRender
{
    public static class Program
    {
        [STAThread]
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args)
                .MapResult(
                    o =>
                    {
                        using DesktopGameHost host = Host.GetSuitableDesktopHost(@"osu-render");
                        host.Run(new OsuRenderGame(o));
                        return 0;
                    },
                    _ => 1
                );
        }
    }

    public class Options
    {
        [Value(0, MetaName = "replay", Required = true, HelpText = "Path of the replay file.")]
        public string ReplayFile { get; set; } = null!;

        [Option('b', "beatmap", Required = false, HelpText = "Path of the beatmap file.")]
        public string? BeatmapFile { get; set; }

        [Option('w', "width", Default = 1280, Required = false, HelpText = "The width of the render.")]
        public int Width { get; set; }

        [Option('h', "height", Default = 720, Required = false, HelpText = "The height of the render.")]
        public int Height { get; set; }

        [Option("disable-audio", Default = false, Required = false, HelpText = "Disable audio.")]
        public bool DisableAudio { get; set; }

        [Option("disable-intro", Default = false, Required = false, HelpText = "Disable the intro screen.")]
        public bool DisableIntro { get; set; }

        [Option("disable-results", Default = false, Required = false, HelpText = "Disable the results screen.")]
        public bool DisableResults { get; set; }
    }
}
