using JobPortal.Shared.Events;
using JobPortal.AIResumeParserService.Services;
using MassTransit;
using System.Text;
using System.Text.Json;
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
            _logger.LogInformation($"AI Service: Analyzing Resume for AppId: {msg.ApplicationId}, Resume: {msg.ResumeUrl}");

            try 
            {
                // 1. Define Job Description (could be fetched from JobService)
                string jobDescription = "Looking for a full-stack developer with experience in .NET, React, and SQL Server.";

                // 2. Call Gemini for Analysis
                var analysis = await _geminiService.AnalyzeResumeAsync(msg.ResumeUrl, jobDescription);
                _logger.LogInformation($"AI Service: Analysis complete for AppId: {msg.ApplicationId}. Score: {analysis.Score}");

                // 3. Callback to ApplicationService to update score
                var updateRequest = new
                {
                    ApplicationId = msg.ApplicationId,
                    AtsScore = analysis.Score,
                    AiSummary = analysis.Summary
                };

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(updateRequest, options);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var appServiceUrl = _configuration["ServiceUrls:ApplicationService"] ?? "http://localhost:5243";
                _logger.LogInformation($"AI Service: Sending callback to {appServiceUrl}/api/aicallback/update-score");
                
                var response = await _httpClient.PostAsync($"{appServiceUrl}/api/aicallback/update-score", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"AI Service: Successfully updated score for AppId: {msg.ApplicationId}");
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"AI Service: Failed to update ApplicationService. Status: {response.StatusCode}, Error: {errorBody}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"AI Service: Critical error processing AppId: {msg.ApplicationId}");
            }
        }
    }
}
