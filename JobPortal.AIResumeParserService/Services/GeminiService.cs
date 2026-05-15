using Newtonsoft.Json;
using System.Text;
using UglyToad.PdfPig;

namespace JobPortal.AIResumeParserService.Services
{
    public interface IGeminiService
    {
        Task<(int Score, string Summary)> AnalyzeResumeAsync(string resumeUrl, string jobDescription);
    }

    public class GeminiService : IGeminiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly ILogger<GeminiService> _logger;
        private readonly IConfiguration _configuration;

        public GeminiService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key is missing in configuration.");
            _model = configuration["Gemini:Model"] ?? "gemini-2.0-flash"; // Default to a newer model
        }

        public async Task<(int Score, string Summary)> AnalyzeResumeAsync(string resumeUrl, string jobDescription)
        {
            try
            {
                // Note: In real world, you'd download the resume from resumeUrl and extract text.
                // For now, we assume the resumeUrl IS the text or we use a mock.
                string resumeText = await ExtractTextFromPdfAsync(resumeUrl);
                
                if (string.IsNullOrWhiteSpace(resumeText))
                {
                    _logger.LogWarning("Failed to extract text from resume or text is empty.");
                    resumeText = "Candidate has not provided a readable resume.";
                } 

                var prompt = $"Act as an ATS (Applicant Tracking System). Analyze this Resume against the Job Description.\n\n" +
                             $"Job Description: {jobDescription}\n" +
                             $"Resume: {resumeText}\n\n" +
                             $"Provide a match score (0-100) and a brief summary of why. " +
                             $"Return only JSON in this format: {{ \"score\": 85, \"summary\": \"Candidate is a good fit with strong .NET skills.\" }}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = prompt } } }
                    }
                };

                var jsonBody = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                _logger.LogInformation("Attempting Gemini API call. Payload length: {Length}", jsonBody.Length);

                // List of models to try as fallbacks
                var modelsToTry = new List<string> { _model, "gemini-2.5-flash", "gemini-flash-latest", "gemini-2.0-flash", "gemini-1.5-pro", "gemini-1.5-flash" }.Distinct().ToList();
                
                var httpClient = _httpClientFactory.CreateClient("GeminiClient");
                HttpResponseMessage response = null;
                string lastError = "";

                foreach (var m in modelsToTry)
                {
                    _logger.LogInformation("Trying Gemini model: {Model}", m);
                    // Always use v1beta for better compatibility across multiple models
                    var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{m}:generateContent?key={_apiKey}";
                    
                    response = await httpClient.PostAsync(apiUrl, content);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Successfully connected using model: {Model}", m);
                        break; // Success! Stop trying other models.
                    }
                    
                    lastError = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Model {Model} failed with Status: {StatusCode}. Moving to next...", m, response.StatusCode);
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    var statusCode = response?.StatusCode ?? System.Net.HttpStatusCode.InternalServerError;
                    _logger.LogError("All Gemini models failed. Last Status: {StatusCode}, Error: {Error}", statusCode, lastError);
                        
                    if (statusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        return (0, "AI Analysis is currently busy (All models exhausted their Free Quota). Please wait a minute and try again.");
                    }

                    return (0, $"AI Analysis failed: All Gemini models failed. Last Status: {statusCode}. Details: {lastError}");
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Gemini API Response received successfully.");
                dynamic result = JsonConvert.DeserializeObject(resultJson)!;
                
                if (result.candidates == null || result.candidates.Count == 0)
                {
                    _logger.LogWarning("Gemini returned no candidates. Response: {Response}", resultJson);
                    return (0, "AI Analysis failed: Gemini did not generate any response content.");
                }

                string textResponse = result.candidates[0].content.parts[0].text;
                
                // Extract JSON from markdown if Gemini wraps it in ```json ... ```
                if (textResponse.Contains("```json"))
                {
                    textResponse = textResponse.Split("```json")[1].Split("```")[0].Trim();
                }
                else if (textResponse.Contains("```"))
                {
                    textResponse = textResponse.Split("```")[1].Split("```")[0].Trim();
                }

                var analysisResult = JsonConvert.DeserializeObject<GeminiAnalysisResponse>(textResponse);
                return (analysisResult?.Score ?? 0, analysisResult?.Summary ?? "AI could not generate a summary.");
            }
            catch (Exception ex) 
            { 
                _logger.LogError(ex, "Error calling Gemini API."); 
                return (0, "AI Analysis failed: A critical error occurred during processing.");
            }
        }

        private async Task<string> ExtractTextFromPdfAsync(string resumeUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(resumeUrl)) return string.Empty;

                var httpClient = _httpClientFactory.CreateClient("GeminiClient");
                
                // If the URL is relative, prepend the ApplicationService base URL
                if (resumeUrl.StartsWith("/"))
                {
                    var appServiceUrl = _configuration["ServiceUrls:ApplicationService"]?.TrimEnd('/');
                    if (!string.IsNullOrEmpty(appServiceUrl))
                    {
                        resumeUrl = $"{appServiceUrl}{resumeUrl}";
                    }
                }

                using var response = await httpClient.GetAsync(resumeUrl);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                ms.Position = 0;
                
                using var document = PdfDocument.Open(ms);
                
                var text = new StringBuilder();
                foreach (var page in document.GetPages())
                {
                    text.AppendLine(page.Text);
                }
                
                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract text from PDF: {ResumeUrl}", resumeUrl);
                return string.Empty;
            }
        }

        private class GeminiAnalysisResponse
        {
            [JsonProperty("score")]
            public int Score { get; set; }
            [JsonProperty("summary")]
            public string Summary { get; set; } = string.Empty;
        }
    }
}
