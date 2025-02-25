using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SkiServiceManagement.Models;
using SkiServiceManagement.Data;
using System;
using MongoDB.Driver;

namespace SkiServiceManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuftragController(MangoDBContext context) : ControllerBase
    {
        private readonly MangoDBContext _context = context;

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuftragById(int id)
        {
            var auftrag = await _context.Serviceauftraege.Find(Builders<Serviceauftrag>.Filter.Eq("Id", id)).FirstOrDefaultAsync();
            if (auftrag == null)
                return NotFound();

            return Ok(auftrag);
        }

        [HttpPost("bookings")]
        public async Task<IActionResult> GetMyAuftraege(GetMyBookings myBookingsRequest) {
            if (string.IsNullOrEmpty(myBookingsRequest.RefreshToken)) {
                return Unauthorized();
            }

            var benutzer = await _context.Benutzer.Find(Builders<Benutzer>.Filter.Eq("RefreshToken", myBookingsRequest.RefreshToken)).FirstOrDefaultAsync();
            if (benutzer == null) {
                return Unauthorized();
            }

            var auftraege = await _context.Serviceauftraege.Find(Builders<Serviceauftrag>.Filter.Eq("BenutzerId", benutzer.Id.ToString())).ToListAsync();

            return Ok(auftraege);
        }

        [HttpPost("book")]
        public async Task<IActionResult> CreateAuftrag(BookingRequest bookingRequest)
        {
            if (string.IsNullOrEmpty(bookingRequest.RefreshToken)) {
                return Unauthorized();
            }

            if (!bookingRequest.HasService())
            {
                return BadRequest(new { message = "Bitte füllen Sie alle erforderlichen Felder aus." });
            }

            bookingRequest.PickupDate = CalculatePickupDate(bookingRequest.Prioritaet);

            var benutzer = await _context.Benutzer.Find(Builders<Benutzer>.Filter.Eq("RefreshToken", bookingRequest.RefreshToken)).FirstOrDefaultAsync();
            if (benutzer == null)
            {
                return Unauthorized();
            }

            var auftrag = new Serviceauftrag
            {
                BenutzerId = benutzer.Id.ToString(),
                Prioritaet = bookingRequest.Prioritaet,
                HasRaceService = bookingRequest.HasRaceService,
                HasWaxService = bookingRequest.HasWaxService,
                HasSkinService = bookingRequest.HasSkinService,
                HasBindingService = bookingRequest.HasBindingService,
                PickupDate = bookingRequest.PickupDate
            };

            await _context.Serviceauftraege.InsertOneAsync(auftrag);


            return CreatedAtAction(nameof(GetAuftragById), new { id = auftrag.Id }, auftrag);
        }

        private static DateTime CalculatePickupDate(string priority)
        {
            int daysToAdd = priority switch
            {
                "Tief" => 12,
                "Standard" => 7,
                "Express" => 5,
                _ => 7
            };

            DateTime pickupDate = DateTime.Now.AddDays(daysToAdd);

            // Wochenenden überspringen
            while (pickupDate.DayOfWeek == DayOfWeek.Saturday || pickupDate.DayOfWeek == DayOfWeek.Sunday)
            {
                pickupDate = pickupDate.AddDays(1);
            }

            return pickupDate;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuftrag(int id, BookingRequest updateRequest)
        {
            var filter = Builders<Serviceauftrag>.Filter.Eq("Id", id);

            var update = Builders<Serviceauftrag>.Update
                .Set("Prioritaet", updateRequest.Prioritaet)
                .Set("IsRaceService", updateRequest.HasRaceService)
                .Set("IsWaxService", updateRequest.HasWaxService)
                .Set("IsEdgeService", updateRequest.HasSkinService)
                .Set("IsBaseService", updateRequest.HasBindingService);

            var result = await _context.Serviceauftraege.UpdateOneAsync(filter, update);
            if (!result.IsAcknowledged || result.ModifiedCount == 0)
                return NotFound();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuftrag(int id)
        {
            var auftrag = await _context.Serviceauftraege.DeleteOneAsync(Builders<Serviceauftrag>.Filter.Eq("Id", id));

            if (!auftrag.IsAcknowledged || auftrag.DeletedCount == 0)
                return NotFound();

            return NoContent();
        }
    }
}