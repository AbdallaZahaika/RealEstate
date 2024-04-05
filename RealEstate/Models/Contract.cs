namespace RealEstate.Models
{
    using Microsoft.VisualBasic;
    using System.ComponentModel.DataAnnotations;

    public class Contract
    {
        public int Id { get; set; }

        [Required]
        public int PropertyId { get; set; }
        [Required]
        public int ProviderId { get; set; }
        [Required]
        public int CustomerId { get; set; }
        [Required]
        public ContractTypes Type { get; set; }
        [Required]
        public ContractStatus Status { get; set; }
        public string? Note { get; set; }
        public double FullTotal { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; }

        public class ContractDataTableRowDataModal
        {
            public int Id { get; set; }
            public string Note { get; set; }
            public double FullTotal { get; set; }
            public ContractTypes Type { get; set; }
            public ContractStatus Status { get; set; }
            public string Property { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public string Provider { get; set; }
            public string Customer { get; set; }
            public string StatusColor { get; set; }
        }
        public enum ContractTypes
        {
            Rent,
            Sale
        }
        public enum ContractStatus
        {
            Done,
            InProcessing,
            Canceled,
            Pending
        }
    }

}
