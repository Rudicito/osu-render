// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Buffers;
using System.Buffers.Text;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using osu.Framework.Logging;

namespace OsuRender.BeatmapDownloader.Mino
{
    public class MinoBeatmapDownloader : IBeatmapDownloader
    {
        private const string url = "https://catboy.best/";
        private const float rate_limit_ratio = 0.1f;

        private static readonly HttpClient http_client = new();

        public MinoBeatmapDownloader()
        {
            http_client.DefaultRequestHeaders.UserAgent.ParseAdd("osu-render/1.0");
        }

        public async Task<Beatmapset> GetBeatmapset(string hash)
        {
            // Get rate limits
            var rateLimits = await getRateLimits();

            if (rateLimits.Remaining / (float)rateLimits.Limit < rate_limit_ratio)
            {
                var delay = rateLimits.Reset - DateTime.UtcNow + new TimeSpan(0, 0, 2);

                if (delay > TimeSpan.Zero)
                {
                    Logger.Log($"Rate limits reached, waiting reset (in {delay.TotalSeconds:F0} seconds)", LoggingTarget.Runtime, LogLevel.Important);
                    await Task.Delay(delay);
                }
            }

            // Find beatmap id
            var beatmap = await getBeatmap(hash);

            // Download beatmap
            return await getBeatmapset(beatmap.ParentSetId);
        }

        private async Task<RateLimits> getRateLimits()
        {
            using var rateLimitsMessage = await http_client.GetAsync(url + "api/ratelimits");
            rateLimitsMessage.EnsureSuccessStatusCode();

            var rateLimits = await JsonSerializer.DeserializeAsync<RateLimits>
            (
                await rateLimitsMessage.Content.ReadAsStreamAsync()
            );

            if (rateLimits == null)
                throw new BeatmapDownloaderException.UnexpectedResponse();

            var context = new ValidationContext(rateLimits);

            try
            {
                Validator.ValidateObject(rateLimits, context, true);
            }
            catch (ValidationException e)
            {
                throw new BeatmapDownloaderException.UnexpectedResponse(e.Message, e);
            }

            return rateLimits;
        }

        private async Task<Beatmap> getBeatmap(string hash)
        {
            using var beatmapMessage = await http_client.GetAsync(url + "api/md5/" + hash);

            if (beatmapMessage.StatusCode == HttpStatusCode.NotFound)
                throw new BeatmapDownloaderException.BeatmapNotFoundException();

            beatmapMessage.EnsureSuccessStatusCode();

            var beatmap = await JsonSerializer.DeserializeAsync<Beatmap>
            (
                await beatmapMessage.Content.ReadAsStreamAsync()
            );

            if (beatmap == null)
                throw new BeatmapDownloaderException.UnexpectedResponse();

            var context = new ValidationContext(beatmap);

            try
            {
                Validator.ValidateObject(beatmap, context, true);
            }
            catch (ValidationException e)
            {
                throw new BeatmapDownloaderException.UnexpectedResponse(e.Message, e);
            }

            return beatmap;
        }

        private async Task<Beatmapset> getBeatmapset(int beatmapSetId)
        {
            using var beatmapsetResponse = await http_client.GetAsync(url + "d/" + beatmapSetId);
            beatmapsetResponse.EnsureSuccessStatusCode();

            if (beatmapsetResponse.StatusCode == HttpStatusCode.NotFound)
                throw new BeatmapDownloaderException.UnexpectedResponse();

            var memoryStream = new MemoryStream();
            await beatmapsetResponse.Content.CopyToAsync(memoryStream);

            memoryStream.Position = 0;

            string filename = beatmapsetResponse.Content.Headers.ContentDisposition?.FileName ?? "beatmap.osz";

            return new Beatmapset
            {
                Stream = memoryStream,
                FileName = filename
            };
        }
    }

    #region Json converter

    internal class Beatmap
    {
        [JsonPropertyName("ParentSetID")]
        [Range(1, int.MaxValue)]
        public int ParentSetId { get; init; }
    }

    internal class RateLimits : IValidatableObject
    {
        [JsonPropertyName("limit")]
        [Range(1, int.MaxValue)]
        public int Limit { get; init; }

        [JsonPropertyName("remaining")]
        [Range(0, int.MaxValue)]
        public int Remaining { get; init; }

        [JsonPropertyName("reset")]
        [JsonConverter(typeof(DateTimeRfc1123JsonConverter))]
        public DateTime Reset { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Remaining > Limit)
            {
                yield return new ValidationResult("Remaining can't be greater than Limit",
                    [nameof(Limit), nameof(Remaining)]);
            }
        }
    }

    /// <remarks>
    /// Copied from <see href="https://learn.microsoft.com/en-us/dotnet/standard/datetime/system-text-json-support"/>,
    /// method : <c>DateTimeConverterForCustomStandardFormatR()</c>
    /// </remarks>
    public class DateTimeRfc1123JsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(DateTime));

            if (Utf8Parser.TryParse(reader.ValueSpan, out DateTime value, out _, 'R'))
            {
                return value;
            }

            throw new FormatException();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // The "R" standard format will always be 29 bytes.
            Span<byte> utf8Date = new byte[29];

            bool result = Utf8Formatter.TryFormat(value, utf8Date, out _, new StandardFormat('R'));
            Debug.Assert(result);

            writer.WriteStringValue(utf8Date);
        }
    }

    #endregion
}
