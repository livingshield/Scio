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

public class OpenAIService : IAIService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;

    public OpenAIService(IConfiguration configuration, HttpClient httpClient)
    {
        _apiKey = configuration["AI:OpenAIApiKey"] ?? "";
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(10); // Don't hang for too long
    }

    public async Task<AIAnalysisResult> AnalyzeMessageAsync(string goalDescription, string studentNickname, string messageContent, int currentProgress, int targetValue)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            // Fallback mock logic if API key is missing
            return new AIAnalysisResult { IsProgress = false, NewProgressValue = currentProgress };
        }

        try
        {
            var prompt = $@"
Cíl úkolu pro studenta: {goalDescription}
Jméno studenta: {studentNickname}
Aktuální stav pokroku studenta: {currentProgress} z {targetValue} (cílová hodnota)

Student právě poslal tuto zprávu:
""{messageContent}""

Tvým úkolem je objektivně, ale s pochopením pro kreativitu, vyhodnotit tuto zprávu.
- isRelevant: Je zpráva k tématu? (Např. student píše báseň, přemýšlí o zadání, klade dotaz k tématu). Pokud student plní kreativní úkol jako je psaní básně nebo eseje, je to VŽDY relevantní.
- isProgress: Dokazuje zpráva, že student udělal reálný kus práce nebo úkol dokončil? (Např. napsal sloku básně, vyřešil příklad, odevzdal finální text).
- newProgressValue: Pokud je to pokrok, navrhni novou hodnotu. Pokud je úkol splněn (např. odevzdaná celá báseň), nastav hodnotu na {targetValue}.
- studentFeedback: Krátká věta pro studenta (česky). Musí být povzbudivá. Pokud udělal chybu, nekritizuj, ale naváděj.

Odpověz POUZE ve formátu JSON:
{{
  ""isProgress"": bool,
  ""isRelevant"": bool,
  ""newProgressValue"": int,
  ""feedback"": ""technická poznámka pro učitele"",
  ""studentFeedback"": ""povzbuzení pro studenta""
}}";

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "Jsi empatický asistent učitele. Tvým cílem je rozpoznat snahu studenta a jeho pokrok k cíli. Nejsi příliš přísný na formát, hledáš podstatu splnění úkolu. Úkoly mohou být matematické i kreativní (např. psaní textů)." },
                    new { role = "user", content = prompt }
                },
                response_format = new { type = "json_object" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            var resultText = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            return JsonSerializer.Deserialize<AIAnalysisResult>(resultText!) ?? new AIAnalysisResult();
        }
        catch (Exception ex)
        {
            return new AIAnalysisResult { IsProgress = false, NewProgressValue = currentProgress, Feedback = $"Chyba AI: {ex.Message}" };
        }
    }
}
