using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using OllamaSharp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// =======================
// 1. SETUP SERVICES
// =======================
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

// A. Aktifkan Session (PENTING BUAT LOGIN)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(60); // Login tahan 60 menit
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

// B. Setup Ollama (Ganti model sesuai yg kamu punya, misal: "riobot" atau "llama3.2")
var ollamaUri = new Uri("http://127.0.0.1:11434");
var ollamaClient = new OllamaApiClient(ollamaUri);
ollamaClient.SelectedModel = "riobot";
builder.Services.AddSingleton<IOllamaApiClient>(ollamaClient);

// C. Setup Rate Limiter
builder.Services.AddRateLimiter(options =>
{
	options.AddFixedWindowLimiter("fixed", opt => {
		opt.PermitLimit = 10;
		opt.Window = TimeSpan.FromSeconds(10);
	});
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// D. Setup Kunci Keamanan (JWT)
var jwtKey = "KunciRahasiaSuperAmanYangHarusPanjangBanget123!";
var keyBytes = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(opt => {
	opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt => {
	opt.RequireHttpsMetadata = false;
	opt.SaveToken = true;
	opt.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
		ValidateIssuer = false,
		ValidateAudience = false
	};
});
builder.Services.AddAuthorization();

var app = builder.Build();

// =======================
// 2. MIDDLEWARE (URUTAN PENTING)
// =======================
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // <--- WAJIB ADA SEBELUM AUTH
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// =======================
// 3. ENDPOINTS API
// =======================

// A. Endpoint Login (Username: admin, Pass: 123)
app.MapPost("/login", (UserLogin user) =>
{
	if (user.Username == "rionelson" && user.Password == "bewbew")
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.Username) }),
			Expires = DateTime.UtcNow.AddHours(1),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
		};
		return Results.Ok(new { Token = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor)) });
	}
	return Results.Unauthorized();
});

// B. Endpoint Chat
app.MapPost("/chat", async (ChatRequest request, IOllamaApiClient ollama) =>
{
	try
	{
		var sb = new StringBuilder();
		await foreach (var stream in ollama.GenerateAsync(request.Message))
		{
			sb.Append(stream?.Response);
		}
		return Results.Ok(new { Response = sb.ToString() });
	}
	catch (Exception ex)
	{
		return Results.Problem($"Ollama Error: {ex.Message}");
	}
})
.RequireAuthorization() // Wajib punya Token
.RequireRateLimiting("fixed");

app.Run();

// DATA MODELS
public record UserLogin(string Username, string Password);
public record ChatRequest(string Message);