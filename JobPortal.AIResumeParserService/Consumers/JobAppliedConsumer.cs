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
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<JobAppliedConsumer> _logger;
        private readonly IConfiguration _configuration;

        public JobAppliedConsumer(IGeminiService geminiService, IHttpClientFactory httpClientFactory, ILogger<JobAppliedConsumer> logger, IConfiguration configuration)
        {
            _geminiService = geminiService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task Consume(ConsumeContext<JobAppliedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("AI Service: Analyzing Resume for AppId: {ApplicationId}, Resume: {ResumeUrl}", msg.ApplicationId, msg.ResumeUrl);

            try 
            {
                // 1. Define Job Description (could be fetched from JobService)
                string jobDescription = "Looking for a full-stack developer with experience in .NET, React, and SQL Server.";

                // 2. Call Gemini for Analysis
                var analysis = await _geminiService.AnalyzeResumeAsync(msg.ResumeUrl, jobDescription);
                _logger.LogInformation("AI Service: Analysis complete for AppId: {ApplicationId}. Score: {Score}", msg.ApplicationId, analysis.Score);

                // 3. Callback to ApplicationService to update score
                var updateRequest = new
                {
                    applicationId = msg.ApplicationId,
                    atsScore = analysis.Score,
                    aiSummary = analysis.Summary
                };

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(updateRequest, options);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var appServiceUrl = _configuration["ServiceUrls:ApplicationService"]
                    ?? throw new InvalidOperationException("ServiceUrls:ApplicationService is missing in configuration.");
                
                // Ensure internal communication uses HTTP (ACA internal ingress is usually HTTP)
                if (appServiceUrl.StartsWith("https://") && !appServiceUrl.Contains("localhost"))
                {
                    appServiceUrl = appServiceUrl.Replace("https://", "http://");
                }
                
                // Ensure base URL doesn't have double slashes when combined
                var baseUrl = appServiceUrl.TrimEnd('/');
                var callbackUrl = $"{baseUrl}/api/AiCallback/update-score";
                
                _logger.LogInformation("AI Service: Sending callback to {CallbackUrl} with payload: {Payload}", callbackUrl, json);
                
                try 
                {
                    // Use the named "GeminiClient" which has SSL bypass to talk to internal application-service
                    var httpClient = _httpClientFactory.CreateClient("GeminiClient");
                    var response = await httpClient.PostAsync(callbackUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("AI Service: Successfully updated score for AppId: {ApplicationId}", msg.ApplicationId);
                    }
                    else
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        _logger.LogError("AI Service: Failed to update ApplicationService. Status: {StatusCode}, Error: {ErrorBody}, URL: {CallbackUrl}", response.StatusCode, errorBody, callbackUrl);
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError(ex, "AI Service: Network error while calling ApplicationService at {CallbackUrl}. Is the URL correct and reachable?", callbackUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI Service: Critical error processing AppId: {ApplicationId}", msg.ApplicationId);
            }
        }
    }
}
