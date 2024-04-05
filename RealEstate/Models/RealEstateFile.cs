namespace RealEstate.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    public class RealEstateFile
    {
        public int Id { get; set; }
        [Required]
        public string TableName { get; set; }
        [Required]
        public int TableId { get; set; }
        [Required]
        public string Url { get; set; }
        public int IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; }
    }
}
