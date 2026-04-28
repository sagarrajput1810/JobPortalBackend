using Newtonsoft.Json;
using System.Text;

namespace JobPortal.AIResumeParserService.Services
{
    public interface IGeminiService
    {
        Task<(int Score, string Summary)> AnalyzeResumeAsync(string resumeUrl, string jobDescription);
    }

    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key is missing in configuration.");
        }

        public async Task<(int Score, string Summary)> AnalyzeResumeAsync(string resumeUrl, string jobDescription)
        {
            // Note: In real world, you'd download the resume from resumeUrl and extract text.
            // For now, we assume the resumeUrl IS the text or we use a mock.
            string resumeText = "Experienced Developer with .NET and React skills."; 

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
            var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}", content);
            
            if (response.IsSuccessStatusCode)
            {
                var resultJson = await response.Content.ReadAsStringAsync();
                // Parsing logic for Gemini's response (simplified)
                // In reality, you'd need to extract the text part of the response and then parse the JSON inside it.
                
                // MOCKING FOR NOW since we don't have a real API Key in this environment
                return (Random.Shared.Next(60, 95), "AI Summary: Candidate matches 80% of required skills including .NET and Microservices.");
            }

            return (0, "AI Analysis failed.");
        }
    }
}
