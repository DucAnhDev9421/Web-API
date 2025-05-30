using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.Text.RegularExpressions;

namespace webApi.Services
{
    public interface IYouTubeService
    {
        Task<string> GetVideoDurationAsync(string videoUrl);
    }

    public class YouTubeApiService : IYouTubeService
    {
        private readonly Google.Apis.YouTube.v3.YouTubeService _youtubeService;
        private readonly IConfiguration _configuration;

        public YouTubeApiService(IConfiguration configuration)
        {
            _configuration = configuration;
            _youtubeService = new Google.Apis.YouTube.v3.YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = _configuration["YouTube:ApiKey"]
            });
        }

        public async Task<string> GetVideoDurationAsync(string videoUrl)
        {
            try
            {
                // Extract video ID from URL
                string videoId = ExtractVideoId(videoUrl);
                if (string.IsNullOrEmpty(videoId))
                {
                    throw new ArgumentException("Invalid YouTube URL");
                }

                // Get video details
                var videoRequest = _youtubeService.Videos.List("contentDetails");
                videoRequest.Id = videoId;

                var videoResponse = await videoRequest.ExecuteAsync();
                if (videoResponse.Items == null || !videoResponse.Items.Any())
                {
                    throw new Exception("Video not found");
                }

                // Parse duration
                string duration = videoResponse.Items[0].ContentDetails.Duration;
                return FormatDuration(duration);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting video duration: {ex.Message}");
            }
        }

        private string ExtractVideoId(string url)
        {
            string pattern = @"(?:youtube\.com\/(?:[^\/]+\/.+\/|(?:v|e(?:mbed)?)\/|.*[?&]v=)|youtu\.be\/)([^""&?\/\s]{11})";
            Match match = Regex.Match(url, pattern);
            return match.Success ? match.Groups[1].Value : null;
        }

        private string FormatDuration(string duration)
        {
            // Parse ISO 8601 duration format (PT1H2M10S)
            TimeSpan timeSpan = System.Xml.XmlConvert.ToTimeSpan(duration);
            
            if (timeSpan.Hours > 0)
            {
                return $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            else
            {
                return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
        }
    }
} 