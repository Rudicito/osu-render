// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace OsuRender
{
    public partial class RecordManager : Component
    {
        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private Options options { get; set; } = null!;

        public bool SkipIntro => options.DisableIntro;

        public bool SkipResultsScreen => options.DisableResults;

        public void StartRecording()
        {
            //todo: implement
            Logger.Log("Recording started", LoggingTarget.Runtime, LogLevel.Important);
        }

        public void StopRecording()
        {
            //todo: implement
            Logger.Log("Recording stopped", LoggingTarget.Runtime, LogLevel.Important);
            host.Exit();
        }
    }
}
