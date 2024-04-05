namespace RealEstate.Models
{
    using System.ComponentModel.DataAnnotations;
    public class MaintenanceContract
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public int PropertyId { get; set; }
        public string Note { get; set; }
        [Required]
        public double FullTotal { get; set; }
        [Required]
        public MaintenanceContractStatus Status { get; set; }
        [Required]
        public int ProviderId { get; set; }
        [Required]
        public int CustomerId { get; set; }
        public int IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; }


        public class MaintenanceContractDataTableRowDataModal
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Note { get; set; }
            public double FullTotal { get; set; }
            public MaintenanceContractStatus Status { get; set; }
            public string Property { get; set; }
            public string Provider { get; set; }
            public string Customer { get; set; }
            public string StatusColor { get; set; }
        }

        public enum MaintenanceContractStatus
        {
            Done,
            InProcessing,
            Canceled,
            Pending
        }

    }
}
