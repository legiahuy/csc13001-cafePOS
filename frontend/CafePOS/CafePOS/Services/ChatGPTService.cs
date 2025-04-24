using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CafePOS.Services
{
    public interface IChatGPTService
    {
        Task<string> GetDrinkRecommendationsAsync(double temperature, List<string> availableProducts);
    }

    public class ChatGPTService : IChatGPTService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string API_URL = "https://api.openai.com/v1/chat/completions";

        public ChatGPTService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");
        }

        public async Task<string> GetDrinkRecommendationsAsync(double temperature, List<string> availableProducts)
        {
            try
            {
                var prompt = CreatePrompt(temperature, availableProducts);
                
                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "You are a drink recommendation expert at a cafe. You understand Vietnamese drinks and culture. Keep responses in Vietnamese."
                        },
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                System.Diagnostics.Debug.WriteLine($"Request body: {jsonContent}");
                
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Add API key to request header
                if (!content.Headers.Contains("Authorization"))
                {
                    content.Headers.Add("Authorization", $"Bearer {_apiKey}");
                }

                var response = await _httpClient.PostAsync(API_URL, content);
                
                // Log the response for debugging
                var responseString = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"API Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"API Response Body: {responseString}");
                
                if (!response.IsSuccessStatusCode)
                {
                    return $"Lỗi khi gọi API: {response.StatusCode} - {responseString}";
                }

                var responseObject = JsonSerializer.Deserialize<ChatGPTResponse>(responseString);
                return responseObject?.choices?[0]?.message?.content ?? "Không thể lấy được gợi ý.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in GetDrinkRecommendationsAsync: {ex}");
                return $"Lỗi khi lấy gợi ý: {ex.Message}";
            }
        }

        private string CreatePrompt(double temperature, List<string> availableProducts)
        {
            return $@"Với nhiệt độ hiện tại là {temperature}°C và danh sách đồ uống có sẵn sau:

{string.Join(", ", availableProducts)}

Hãy gợi ý 2-3 đồ uống phù hợp nhất với thời tiết hiện tại và giải thích ngắn gọn tại sao chọn những đồ uống này. 
Chỉ chọn từ danh sách đồ uống có sẵn.
Trả lời bằng tiếng Việt, ngắn gọn khoảng 2-3 dòng.";
        }
    }

    class ChatGPTResponse
    {
        public Choice[] choices { get; set; }
    }

    class Choice
    {
        public Message message { get; set; }
    }

    class Message
    {
        public string content { get; set; }
    }
} 