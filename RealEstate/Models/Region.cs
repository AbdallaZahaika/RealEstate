namespace RealEstate.Models
{
    using System.ComponentModel.DataAnnotations;
    public class Region
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public double DefultPrice { get; set; }
        public int IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; }
    }
}
