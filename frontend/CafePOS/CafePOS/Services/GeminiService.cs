using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CafePOS.Services
{
    public interface IGeminiService
    {
        Task<string> GetDrinkRecommendationsAsync(double temperature, List<string> availableProducts);
    }

    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        public GeminiService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetDrinkRecommendationsAsync(double temperature, List<string> availableProducts)
        {
            try
            {
                var prompt = CreatePrompt(temperature, availableProducts);
                
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                System.Diagnostics.Debug.WriteLine($"[GeminiService] Request body: {jsonContent}");
                
                var url = $"{API_URL}?key={_apiKey}";
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine("[GeminiService] Sending POST request...");
                var response = await _httpClient.PostAsync(url, content);
                System.Diagnostics.Debug.WriteLine("[GeminiService] POST request completed.");
                
                System.Diagnostics.Debug.WriteLine($"[GeminiService] API Response Status: {response.StatusCode}");
                var responseString = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[GeminiService] API Response Body: {responseString}");
                
                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("[GeminiService] Response indicates failure.");
                    return $"Lỗi khi gọi API: {response.StatusCode} - {responseString}";
                }

                System.Diagnostics.Debug.WriteLine("[GeminiService] Deserializing response...");
                var responseObject = JsonSerializer.Deserialize<GeminiResponse>(responseString);
                System.Diagnostics.Debug.WriteLine("[GeminiService] Deserialization complete.");
                
                var responseText = responseObject?.candidates?[0]?.content?.parts?[0]?.text;
                System.Diagnostics.Debug.WriteLine($"[GeminiService] Extracted Text: {responseText ?? "NULL"}");
                
                return responseText ?? "Không thể lấy được gợi ý.";
            }
            catch (Exception ex)
            {
                // Log the full exception details
                System.Diagnostics.Debug.WriteLine($"[GeminiService] Exception in GetDrinkRecommendationsAsync: {ex.ToString()}"); 
                return $"Lỗi khi lấy gợi ý: {ex.Message}";
            }
        }

        private string CreatePrompt(double temperature, List<string> allAvailableProductNames)
        {
            var sb = new StringBuilder();
            string weatherContext;
            string instruction;

            // Define temperature thresholds and corresponding contexts/instructions
            if (temperature < 23)
            {
                weatherContext = $"lạnh ({temperature:F1}°C)";
                // Be more explicit about including simple hot options
                instruction = "Hãy ưu tiên gợi ý đồ uống nóng hoặc ấm từ danh sách (ví dụ: cà phê nóng, trà nóng, hoặc nước nóng nếu có). Tránh gợi ý đồ uống có đá hoặc chỉ uống lạnh.";
            }
            else if (temperature > 28)
            {
                weatherContext = $"nóng ({temperature:F1}°C)";
                instruction = "Hãy ưu tiên gợi ý đồ uống lạnh, mát lạnh, hoặc có đá từ danh sách. Tránh gợi ý đồ uống chỉ phục vụ nóng.";
            }
            else // Mild weather (18-28°C)
            {
                weatherContext = $"ôn hòa ({temperature:F1}°C)";
                // Suggest considering both hot and cold, letting AI decide suitability
                instruction = "Hãy gợi ý những đồ uống nóng hoặc lạnh phù hợp nhất từ danh sách cho thời tiết này.";
            }

            sb.AppendLine($"Thời tiết hiện tại là {weatherContext}.");
            sb.AppendLine("Danh sách TẤT CẢ sản phẩm có sẵn trong quán cà phê:");
            sb.AppendLine();
            sb.AppendLine(string.Join(Environment.NewLine, allAvailableProductNames));
            sb.AppendLine();
            sb.AppendLine(instruction); // Add the specific instruction based on weather
            sb.AppendLine();
            sb.AppendLine("Chỉ liệt kê tên các sản phẩm phù hợp nhất (khoảng 3-5 món), mỗi tên trên một dòng.");
            sb.AppendLine("Không thêm bất kỳ giải thích, mô tả, hay định dạng nào khác.");

            return sb.ToString();
        }
    }

    class GeminiResponse
    {
        public Candidate[] candidates { get; set; }
        public PromptFeedback promptFeedback { get; set; }
    }

    class Candidate
    {
        public Content content { get; set; }
        public string finishReason { get; set; }
        public int index { get; set; }
    }

    class Content
    {
        public Part[] parts { get; set; }
        public string role { get; set; }
    }

    class Part
    {
        public string text { get; set; }
    }

    class PromptFeedback
    {
        public SafetyRating[] safetyRatings { get; set; }
    }

    class SafetyRating
    {
        public string category { get; set; }
        public string probability { get; set; }
    }
} 