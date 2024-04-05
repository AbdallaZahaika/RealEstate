namespace RealEstate.Models
{
    using System.ComponentModel.DataAnnotations;
    public class City
    {
        public int Id { get; set; }
        [Required]
        public int RegionId { get; set; }
        [Required]
        public string Title { get; set; }
        public int IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; }
    }
}
