using Application.Interfaces.External;
using Domain.Enums;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infrastructure.Services.AI
{
    public class OpenAIIncidentAnalyzer : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<OpenAIIncidentAnalyzer> _logger;

        public OpenAIIncidentAnalyzer(HttpClient httpClient, IOptions<OpenAISettings> options, ILogger<OpenAIIncidentAnalyzer> logger)
        {
            _httpClient = httpClient;
            _apiKey = options.Value.ApiKey;
            _logger = logger;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<IncidentAIResult> AnalyzeIncidentMediaAsync(string mediaUrl)
        {
            //var systemPrompt = """
            //    You are an AI assistant that classifies emergency incidents from media content.
            //    Analyze the image or video carefully and respond strictly in JSON format like this:
            //    {
            //        "title": "short title of what you see",
            //        "type": "Fire | Security | Medical | NaturalDisaster | Accident | Other | Unknown",
            //        "confidence": 0.0 - 1.0
            //    }
            //""";

            var systemPrompt = """
                You are an expert emergency-incident classifier. Your job is to analyze a single media URL (image or short video/audio) and return a SINGLE JSON object ONLY — no extra commentary, no code fences, no explanation.

                JSON schema (strict):
                {
                  "title": "string (short, 12-25 words) - concise description of the main incident",
                  "type": "Fire | Security | Medical | NaturalDisaster | Accident | Other | Unknown",
                  "confidence": number between 0.0 and 1.0 (probability the classification is correct),
                  "evidence": "string (1-2 short sentences) - the main visual cues used to decide",
                  "recommended_action": "string (one short sentence) - what responders should consider first"
                }

                Rules:
                1) Always follow the schema exactly and use those field names (case-insensitive).
                2) Keep `title` concise and factual — avoid speculation.
                3) `confidence` must be a number (0.0 - 1.0). If uncertain, set confidence < 0.6 and be explicit in `evidence`.
                4) `evidence` must mention the visual features that led to the decision (e.g., 'large flames, heavy smoke, crowd running').
                5) `recommended_action` should be short and actionable (e.g., 'Dispatch fire unit, secure perimeter').
                6) If media is corrupted, inaccessible, or clearly not useful for classification, return:
                   {
                     "title": "Unable to classify",
                     "type": "Unknown",
                     "confidence": 0.0,
                     "evidence": "media inaccessible or not informative",
                     "recommended_action": "Request clearer media or manual review"
                   }
                7) Do NOT include any extra keys or metadata beyond the schema above.
                8) Output must be valid JSON parsable by standard JSON parsers.
                """;

            //var userPrompt = $"Analyze this media and determine what type of emergency incident it shows:\n{mediaUrl}";

            var userPrompt = $$"""
                Analyze this media URL and produce the JSON object per the system instructions above.

                Media URL: {{mediaUrl}}

                Context: This media may be a photo or a short video/audio. Prioritize obvious, high-confidence visual signals. If the media clearly shows more than one incident type, choose the dominant one and mention the ambiguity in the "evidence" field.

                Example valid output (do not include the example in your output; it is only illustrative):
                {
                  "title": "Two-storey building on fire with heavy smoke",
                  "type": "Fire",
                  "confidence": 0.92,
                  "evidence": "Visible large flames at roofline, dense black smoke, bystanders evacuating",
                  "recommended_action": "Dispatch fire brigade, establish safety perimeter"
                }

                Now analyze the provided media and return the JSON object only.
                """;

            var payload = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.2,
                max_tokens = 350
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI API error: {StatusCode} - {Body}", response.StatusCode, errorBody);
                throw new Exception($"OpenAI API error ({response.StatusCode}): {errorBody}");
            }

            var raw = await response.Content.ReadAsStringAsync();

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(raw);
            }
            catch (JsonException jex)
            {
                _logger.LogError(jex, "Failed to parse OpenAI API response: {Raw}", raw);
                throw new Exception("Invalid JSON received from OpenAI.");
            }

            if (!doc.RootElement.TryGetProperty("choices", out var choices) || choices.GetArrayLength() == 0)
                throw new Exception("No response choices returned by OpenAI API");

            var message = choices[0].GetProperty("message");
            var aiText = message.GetProperty("content").GetString()?.Trim() ?? "";

            _logger.LogInformation("AI raw output: {Text}", aiText);

            if (aiText.StartsWith("```"))
            {
                var parts = aiText.Split("```", StringSplitOptions.RemoveEmptyEntries);
                aiText = parts.Length > 0 ? parts.Last().Trim() : aiText;
            }

            int start = aiText.IndexOf('{');
            int end = aiText.LastIndexOf('}');
            if (start < 0 || end <= start)
                throw new Exception("No valid JSON object found in AI output");

            var jsonOnly = aiText.Substring(start, end - start + 1);

            try
            {
                //var result = JsonSerializer.Deserialize<IncidentAIResult>(
                //    jsonOnly,
                //    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var result = JsonSerializer.Deserialize<IncidentAIResult>(
                jsonOnly,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                });

                if (result is null)
                    throw new Exception("Deserialized result was null");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse AI output JSON");
                return new IncidentAIResult("Unable to classify", IncidentType.Unknown, 0.0);
            }

            //var chatRequest = new ChatRequest(
            //    messages: new[]
            //    {
            //        new Message(Role.System, systemPrompt),
            //        new Message(Role.User, userPrompt)
            //    },
            //    model: "gpt-4o-mini", // works with vision/multimodal content
            //    temperature: 0.2
            //);

            //var response = await _client.ChatEndpoint.GetCompletionAsync(chatRequest);
            //var content = response.FirstChoice.Message.Content[0].Text.Trim();

            //try
            //{
            //    using var doc = JsonDocument.Parse(content);
            //    var root = doc.RootElement;

            //    var title = root.GetProperty("title").GetString() ?? "Unknown";
            //    var typeStr = root.GetProperty("type").GetString() ?? "Unknown";
            //    var confidence = root.GetProperty("confidence").GetDouble();

            //    Enum.TryParse(typeStr, true, out IncidentType incidentType);

            //    return new IncidentAIResult(title, incidentType, confidence);
            //}
            //catch
            //{
            //    // fallback if the AI output isn’t valid JSON
            //    return new IncidentAIResult("Unable to classify", IncidentType.Unknown, 0.0);
            //}
        }
    }
}