
using System;

namespace SkiServiceManagement.Models
{
    public class BookingRequest
    {
        public string Prioritaet { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public bool HasRaceService { get; set; }
        public bool HasWaxService { get; set; }
        public bool HasSkinService { get; set; }
        public bool HasBindingService { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime PickupDate { get; set; }
        public bool HasService()
        {
            return HasRaceService || HasWaxService || HasSkinService || HasBindingService;
        }
    }
}