using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;

namespace MyAIBackend.Pages
{
	// 👇 INI YANG PENTING: Class-nya harus LoginModel, JANGAN IndexModel
	[IgnoreAntiforgeryToken]
	public class LoginModel : PageModel
	{
		private readonly IHttpClientFactory _clientFactory;
		public LoginModel(IHttpClientFactory clientFactory) => _clientFactory = clientFactory;

		[BindProperty] public string Username { get; set; }
		[BindProperty] public string Password { get; set; }
		public string ErrorMessage { get; set; }

		public void OnGet()
		{
			// Cek kalau user sudah login, lempar ke Chat (Index)
			if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
			{
				Response.Redirect("/Index");
			}
		}

		public async Task<IActionResult> OnPostAsync()
		{
			var client = _clientFactory.CreateClient();
			var baseUrl = $"{Request.Scheme}://{Request.Host}";

			try
			{
				// Kirim Username & Password ke API /login
				var loginData = new { Username = Username, Password = Password };
				var response = await client.PostAsJsonAsync($"{baseUrl}/login", loginData);

				if (response.IsSuccessStatusCode)
				{
					// Ambil Token
					var jsonString = await response.Content.ReadAsStringAsync();
					var token = JsonDocument.Parse(jsonString).RootElement.GetProperty("token").GetString();

					// Simpan Token di Saku (Session)
					HttpContext.Session.SetString("JWToken", token);

					// Sukses! Pindah ke Chat
					return RedirectToPage("/Index");
				}
				else
				{
					ErrorMessage = "Username atau Password salah!";
					return Page();
				}
			}
			catch (Exception ex)
			{
				ErrorMessage = "Error: " + ex.Message;
				return Page();
			}
		}
	}
}