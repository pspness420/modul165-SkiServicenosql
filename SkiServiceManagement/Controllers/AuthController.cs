using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SkiServiceManagement.Models;
using SkiServiceManagement.Data;
using System.Collections.Generic;
using System;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;

namespace SkiServiceManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MangoDBContext _context;
        private readonly TokenService _tokenService;

        public AuthController(MangoDBContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(Benutzer benutzer)
        {
            // Benutzername aus Vorname und Nachname zusammensetzen
            if (string.IsNullOrWhiteSpace(benutzer.Benutzername))
            {
                if (string.IsNullOrWhiteSpace(benutzer.Vorname) || string.IsNullOrWhiteSpace(benutzer.Nachname)) 
                {
                    return BadRequest("Vorname und Nachname sind erforderlich, um den Benutzernamen zu erstellen.");
                }
                benutzer.Benutzername = $"{benutzer.Vorname} {benutzer.Nachname}";
            }

            // Überprüfung auf Duplikate

            if ((await _context.Benutzer.FindAsync(Builders<Benutzer>.Filter.Eq("Benutzername", benutzer.Benutzername))).Any())
                return BadRequest("Ein Benutzer mit diesem Namen existiert bereits.");

            if (await _context.Benutzer.Find(Builders<Benutzer>.Filter.Eq("Email", benutzer.Email)).AnyAsync())
                return BadRequest("Ein Benutzer mit dieser E-Mail existiert bereits.");

            // Passwort hashen
            benutzer.Passwort = BCrypt.Net.BCrypt.HashPassword(benutzer.Passwort, workFactor: 10);

            await _context.Benutzer.InsertOneAsync(benutzer);

            return Ok("Benutzer erfolgreich registriert.");
        }

        // Login per Benutzername oder Email
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            // Eingabedaten validieren
            if (string.IsNullOrWhiteSpace(request.username) || string.IsNullOrWhiteSpace(request.password))
            {
                return BadRequest("Benutzername und Passwort sind erforderlich.");
            }

            // Benutzer validieren
            var user = await _context.Benutzer.Find(Builders<Benutzer>.Filter.Eq("Email", request.username)).FirstOrDefaultAsync();
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.password, user.Passwort))
            {
                System.Console.WriteLine("Benutzer nicht gefunden oder Passwort falsch. Benutzer: " + user + " Passwort: " + request.password);
                return Unauthorized("Ungültige Anmeldeinformationen.");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Benutzername),
                new(ClaimTypes.Role, user.Rolle)
            };

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // RefreshToken in der Datenbank speichern
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.Now.AddDays(7); // RefreshToken ist 7 Tage gültig
            var filter = Builders<Benutzer>.Filter.Eq("Email", user.Email);
            await _context.Benutzer.ReplaceOneAsync(filter, user);

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout([FromBody] string refreshToken)
        {
            // Refresh-Token aus der Datenbank entfernen
            var user = _context.Benutzer.Find(Builders<Benutzer>.Filter.Eq("RefreshToken", refreshToken)).FirstOrDefault();
            if (user == null)
            {
                return NotFound("RefreshToken nicht gefunden.");
            }

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            _context.Benutzer.ReplaceOne(Builders<Benutzer>.Filter.Eq("Email", user.Email), user);

            return Ok(new { message = "Erfolgreich abgemeldet." });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh(TokenRefreshRequest request)
        {
            // Eingabedaten validieren
            if (string.IsNullOrWhiteSpace(request.AccessToken) || string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest("AccessToken und RefreshToken sind erforderlich.");
            }

            var user = _context.Benutzer.Find(Builders<Benutzer>.Filter.Eq("RefreshToken", request.RefreshToken)).FirstOrDefault();

            if (user == null || user.RefreshTokenExpiry <= DateTime.Now)
            {
                return Unauthorized("Ungültiger oder abgelaufener RefreshToken.");
            }

            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
            {
                return Unauthorized("Ungültiges AccessToken.");
            }

            var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.Now.AddDays(7);
            _context.Benutzer.ReplaceOne(Builders<Benutzer>.Filter.Eq("Email", user.Email), user);

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
    }
}
