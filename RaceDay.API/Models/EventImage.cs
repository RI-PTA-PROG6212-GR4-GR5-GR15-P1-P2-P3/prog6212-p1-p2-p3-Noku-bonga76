using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("EventImages")]
    public class EventImage
    {
        [Key]
        public int ImageID { get; set; }

        public int EventID { get; set; }

        public string ImageURL { get; set; } = string.Empty;

        public DateTime UploadedDate { get; set; }

        public Event? Event { get; set; }
    }
}

