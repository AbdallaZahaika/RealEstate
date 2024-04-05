namespace RealEstate.Models
{
    using System.ComponentModel.DataAnnotations;
    using static RealEstate.Models.Property;

    public class Property
    {
        public int Id { get; set; }
        [Required]
        public int OwnerLevel { get; set; }
        [Required]
        public int OwnerUserId { get; set; }
        [Required]
        public int ProviderId { get; set; }
        public int? CustomerId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public double PropertySize { get; set; }
        [Required]
        public int NumberOfFloors { get; set; }
        [Required]
        public int FloorNumber { get; set; }
        [Required]
        public PropertyTypes Type { get; set; }
        [Required]
        public int NumberOfRooms { get; set; }
        [Required]
        public int NumberOfBathrooms { get; set; }
        public string Note { get; set; }
        [Required]
        public ServiceTypes ServiceType { get; set; }
        [Required]
        public double FullTotal { get; set; }
        [Required]
        public PropertyStatus Status { get; set; }
        [Required]
        public int RegionId { get; set; }
        [Required]
        public int CityId { get; set; }
        [Required]
        public bool WithMaintenanceContract { get; set; }
        public int IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; }

        public enum PropertyTypes
        {
            OrdinaryHouse,
            Villa,
            InBuilding
        }
        public enum ServiceTypes
        {
            Rent,
            Sale
        }

        public enum PropertyStatus
        {
            Confirmed,
            Canceled,
            Pending
        }

        public class PropertyViewDetailsModel
        {
            public int Id { get; set; }
            public string Note { get; set; }
            public string Title { get; set; }
            public string MainImage { get; set; }
            public int NumberOfFloors { get; set; }
            public int FloorNumber { get; set; }
            public List<string> Images { get; set; }
            public Property.PropertyTypes Type { get; set; }
            public Property.ServiceTypes ServiceType { get; set; }
            public int NumberOfRooms { get; set; }
            public int NumberOfBathrooms { get; set; }
            public double PropertySize { get; set; }
            public Property.PropertyStatus Status { get; set; }
            public string WithMaintenanceContract { get; set; }
            public string Region { get; set; }
            public string City { get; set; }
            public double FullTotal { get; set; }
            public string StatusColor { get; set; }
            public bool IsHotDeal { get; set; }
            public bool haveContract { get; set; }
        }

        public class PropertyDataTableRowDataModal
        {
            public int Id { get; set; }
            public string OwnerName { get; set; }
            public string OwnerType { get; set; }
            public string Title { get; set; }
            public string MainImage { get; set; }
            public Property.PropertyTypes Type { get; set; }
            public Property.ServiceTypes ServiceType { get; set; }
            public int NumberOfRooms { get; set; }
            public int NumberOfBathrooms { get; set; }
            public double PropertySize { get; set; }
            public Property.PropertyStatus Status { get; set; }
            public string WithMaintenanceContract { get; set; }
            public string Region { get; set; }
            public string City { get; set; }
            public double FullTotal { get; set; }
            public string StatusColor { get; set; }
            public bool IsHotDeal { get; set; }
        }

    }


}
