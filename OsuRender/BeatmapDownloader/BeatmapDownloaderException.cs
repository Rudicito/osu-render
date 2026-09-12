// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace OsuRender.BeatmapDownloader
{
    public class BeatmapDownloaderException : Exception
    {
        public BeatmapDownloaderException() { }
        public BeatmapDownloaderException(string message) : base(message) { }
        public BeatmapDownloaderException(string message, Exception inner) : base(message, inner) { }

        public class BeatmapNotFoundException : BeatmapDownloaderException
        {
            public BeatmapNotFoundException() { }
            public BeatmapNotFoundException(string message) : base(message) { }
            public BeatmapNotFoundException(string message, Exception inner) : base(message, inner) { }
        }

        public class UnexpectedResponse : BeatmapDownloaderException
        {
            public UnexpectedResponse() { }
            public UnexpectedResponse(string message) : base(message) { }
            public UnexpectedResponse(string message, Exception inner) : base(message, inner) { }
        }
    }
}
