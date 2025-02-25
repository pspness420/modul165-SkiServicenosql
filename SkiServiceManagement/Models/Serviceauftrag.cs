using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SkiServiceManagement.Models
{
    public class Serviceauftrag
    {
        public ObjectId Id { get; set; }
        public string BenutzerId { get; set; }
        public string Prioritaet { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public bool HasRaceService { get; set; }
        public bool HasWaxService { get; set; }
        public bool HasSkinService { get; set; }
        public bool HasBindingService { get; set; }
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime PickupDate { get; set; }
        public string Status { get; set; } = "Offen";
    }
}
