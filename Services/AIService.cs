using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScioApp.Services;

public class AIAnalysisResult
{
    [JsonPropertyName("isProgress")]
    public bool IsProgress { get; set; }

    [JsonPropertyName("isRelevant")]
    public bool IsRelevant { get; set; }

    [JsonPropertyName("newProgressValue")]
    public int NewProgressValue { get; set; }

    [JsonPropertyName("feedback")]
    public string? Feedback { get; set; }

    [JsonPropertyName("studentFeedback")]
    public string? StudentFeedback { get; set; }
}

public interface IAIService
{
    Task<AIAnalysisResult> AnalyzeMessageAsync(string goalDescription, string studentNickname, string messageContent, int currentProgress, int targetValue);
}

public class GeminiAIService : IAIService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public GeminiAIService(IConfiguration configuration, HttpClient httpClient)
    {
        _apiKey = configuration["AI:GeminiApiKey"] ?? "";
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<AIAnalysisResult> AnalyzeMessageAsync(string goalDescription, string studentNickname, string messageContent, int currentProgress, int targetValue)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return new AIAnalysisResult { IsProgress = false, NewProgressValue = currentProgress, IsRelevant = true };
        }

        try
        {
            var systemInstruction = "Jsi empatický asistent učitele. Tvým cílem je rozpoznat snahu studenta a jeho pokrok k cíli. Nejsi příliš přísný na formát, hledáš podstatu splnění úkolu. Úkoly mohou být matematické i kreativní (např. psaní básní, textů).";
            
            var prompt = $@"
Cíl úkolu pro studenta: {goalDescription}
Jméno studenta: {studentNickname}
Aktuální stav pokroku studenta: {currentProgress} z {targetValue} (cílová hodnota)

Student právě poslal tuto zprávu:
""{messageContent}""

Tvým úkolem je vyhodnotit tuto zprávu a odpovědět POUZE ve formátu JSON:
{{
  ""isProgress"": bool (pravda, pokud zpráva posouvá studenta k cíli nebo dokazuje jeho splnění),
  ""isRelevant"": bool (pravda, pokud se student věnuje úkolu, i když to není přímý pokrok - např. klade dotazy k tématu, píše první sloku básně apod.),
  ""newProgressValue"": int (navrhni novou hodnotu celkového pokroku),
  ""feedback"": ""technická poznámka pro učitele v češtině"",
  ""studentFeedback"": ""krátká povzbudivá věta přímo pro studenta v češtině""
}}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = systemInstruction + "\n\n" + prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    response_mime_type = "application/json"
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
            var response = await _httpClient.PostAsJsonAsync(url, requestBody);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            var resultText = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return JsonSerializer.Deserialize<AIAnalysisResult>(resultText!) ?? new AIAnalysisResult();
        }
        catch (Exception ex)
        {
            return new AIAnalysisResult { 
                IsProgress = false, 
                NewProgressValue = currentProgress, 
                IsRelevant = true,
                Feedback = $"Chyba Gemini API: {ex.Message}" 
            };
        }
    }
}
