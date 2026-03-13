using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DoanWeb.Controllers
{
    [Route("chatbot")]
    public class ChatBotController : Controller
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public ChatBotController(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _http = httpFactory.CreateClient();
            _apiKey = config["OpenAI:ApiKey"];
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] UserMessage msg)
        {
            if (msg == null || string.IsNullOrWhiteSpace(msg.Message))
                return Json(new { reply = "Vui lòng nhập tin nhắn!" });

            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var body = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "Bạn là chatbot hỗ trợ tìm việc cho website Jobify." },
                    new { role = "user", content = msg.Message }
                }
            };

            var response = await _http.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                return Json(new { reply = $"Lỗi API: {err}" });
            }

            var result = await response.Content.ReadAsStringAsync();
            string reply = "Bot không trả lời được";

            using var doc = JsonDocument.Parse(result);
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var message = choices[0].GetProperty("message");
                reply = message.GetProperty("content").GetString();
            }

            return Json(new { reply });
        }
    }

    public class UserMessage
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
