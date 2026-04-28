using JobPortal.Shared.Events;
using JobPortal.AIResumeParserService.Services;
using MassTransit;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace JobPortal.AIResumeParserService.Consumers
{
    public class JobAppliedConsumer : IConsumer<JobAppliedEvent>
    {
        private readonly IGeminiService _geminiService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<JobAppliedConsumer> _logger;
        private readonly IConfiguration _configuration;

        public JobAppliedConsumer(IGeminiService geminiService, HttpClient httpClient, ILogger<JobAppliedConsumer> logger, IConfiguration configuration)
        {
            _geminiService = geminiService;
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<JobAppliedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation($"AI Service: Analyzing Resume for AppId: {msg.ApplicationId}");

            // TODO: Fetch real Job Description from JobService via HTTP or include in Event
            string jobDescription = "Looking for a full-stack .NET and React developer.";

            // 2. Call Gemini for Analysis
            var analysis = await _geminiService.AnalyzeResumeAsync(msg.ResumeUrl, jobDescription);

            // 3. Callback to ApplicationService to update score
            var updateRequest = new
            {
                ApplicationId = msg.ApplicationId,
                AtsScore = analysis.Score,
                AiSummary = analysis.Summary
            };

            var json = JsonConvert.SerializeObject(updateRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var appServiceUrl = _configuration["ServiceUrls:ApplicationService"];
            var response = await _httpClient.PostAsync($"{appServiceUrl}/api/aicallback/update-score", content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"AI Service: Successfully updated score for AppId: {msg.ApplicationId}");
            }
            else
            {
                _logger.LogError($"AI Service: Failed to update ApplicationService for AppId: {msg.ApplicationId} at {appServiceUrl}");
            }
        }
    }
}
