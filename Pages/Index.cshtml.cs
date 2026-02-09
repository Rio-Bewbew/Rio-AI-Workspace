using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MyAIBackend.Pages
{
	public class ChatMessage
	{
		public string Sender { get; set; }
		public string Text { get; set; }
		public string Time { get; set; }
	}

	[IgnoreAntiforgeryToken]
	public class IndexModel : PageModel
	{
		private readonly IHttpClientFactory _clientFactory;
		public IndexModel(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

		[BindProperty] public string UserMessage { get; set; } = "";
		public List<ChatMessage> ChatHistory { get; set; } = new List<ChatMessage>();
		public string ErrorMessage { get; set; }

		public IActionResult OnGet()
		{
			var token = HttpContext.Session.GetString("JWToken");
			if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

			LoadHistory();
			return Page();
		}

		// --- HANDLER UNTUK CHAT ---
		public async Task<IActionResult> OnPostAsync()
		{
			var token = HttpContext.Session.GetString("JWToken");
			if (string.IsNullOrEmpty(token)) return RedirectToPage("/Login");

			LoadHistory();
			ChatHistory.Add(new ChatMessage { Sender = "User", Text = UserMessage, Time = DateTime.Now.ToString("HH:mm") });

			var client = _clientFactory.CreateClient();
			var baseUrl = $"{Request.Scheme}://{Request.Host}";

			try
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
				var chatRes = await client.PostAsJsonAsync($"{baseUrl}/chat", new { Message = UserMessage });

				if (chatRes.IsSuccessStatusCode)
				{
					var chatString = await chatRes.Content.ReadAsStringAsync();
					var aiText = JsonDocument.Parse(chatString).RootElement.GetProperty("response").GetString();
					ChatHistory.Add(new ChatMessage { Sender = "AI", Text = aiText, Time = DateTime.Now.ToString("HH:mm") });
					UserMessage = "";
				}
				else if (chatRes.StatusCode == System.Net.HttpStatusCode.Unauthorized)
				{
					return RedirectToPage("/Login");
				}
				else
				{
					ErrorMessage = $"Gagal: {chatRes.StatusCode}";
				}
			}
			catch (Exception ex) { ErrorMessage = "Error: " + ex.Message; }

			SaveHistory();
			return Page();
		}

		// --- HANDLER BARU: LOGOUT ---
		public IActionResult OnPostLogout()
		{
			// Hapus semua data sesi (Token, History, dll)
			HttpContext.Session.Clear();
			// Tendang ke halaman Login
			return RedirectToPage("/Login");
		}

		private void LoadHistory()
		{
			var sessionData = HttpContext.Session.GetString("ChatHistory");
			if (!string.IsNullOrEmpty(sessionData)) ChatHistory = JsonSerializer.Deserialize<List<ChatMessage>>(sessionData);
		}

		private void SaveHistory()
		{
			var json = JsonSerializer.Serialize(ChatHistory);
			HttpContext.Session.SetString("ChatHistory", json);
		}
	}
}