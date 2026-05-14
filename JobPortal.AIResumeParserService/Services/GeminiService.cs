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
        private readonly ILogger<GeminiService> _logger;
        private readonly IConfiguration _configuration;

        public GeminiService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key is missing in configuration.");
        }

        public async Task<(int Score, string Summary)> AnalyzeResumeAsync(string resumeUrl, string jobDescription)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_apiKey))
                {
                    _logger.LogWarning("Gemini API Key is empty. Using mock data.");
                    return GetMockData();
                }

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

                // Google Gemini API Endpoint
                var httpClient = _httpClientFactory.CreateClient();
                using var response = await httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Gemini API call failed. Status: {StatusCode}, Error: {Error}", response.StatusCode, error);
                    return GetMockData();
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(resultJson)!;
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
                _logger.LogError(ex, "Error calling Gemini API. Falling back to mock data."); 
                return GetMockData();
            }
        }

        private (int Score, string Summary) GetMockData()
        {
            return (Random.Shared.Next(65, 98), "AI Summary (Mock): Candidate has strong matching skills in .NET, React, and Microservices architecture.");
        }

        private async Task<string> ExtractTextFromPdfAsync(string resumeUrl)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(resumeUrl)) return string.Empty;

                var httpClient = _httpClientFactory.CreateClient();
                
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
