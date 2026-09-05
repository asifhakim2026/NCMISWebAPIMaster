using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace NCMIS.Models
{
    
    public class PersonAddress
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "PersonId is required.")]
        public int PersonId { get; set; } // ✅ Foreign Key to PersonalInfo

        [NotMapped]
        public virtual PersonalInfo PersonalInfo { get; set; } // ✅ Navigation Property

        [Required(ErrorMessage = "Village or city is required.")]
        [StringLength(100, ErrorMessage = "Village or city cannot exceed 100 characters.")]
        public string VillageOrCity { get; set; }

        [Required(ErrorMessage = "Location Type is required.")]
        [StringLength(100, ErrorMessage = "Location Type cannot exceed 100 characters.")]
        public string LocationType { get; set; }

        [Required(ErrorMessage = "Union Council is required.")]
        [StringLength(100, ErrorMessage = "Union Council cannot exceed 100 characters.")]
        public string UnionCouncil { get; set; }

        [Required(ErrorMessage = "Taluqa/Tehsil is required.")]
        [StringLength(100, ErrorMessage = "Taluqa/Tehsil cannot exceed 100 characters.")]
        public string TaluqaTehsil { get; set; }

        [Required(ErrorMessage = "District is required.")]
        [StringLength(100, ErrorMessage = "District cannot exceed 100 characters.")]
        public string District { get; set; }

        [Required(ErrorMessage = "Number of rooms is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of rooms must be at least 1.")]
        public int NumberofRooms { get; set; }

        public int NumberofLivingRooms { get; set; }

        public int NumberOfHall { get; set; }

        public int NumberofKitchen { get; set; }

        [Required(ErrorMessage = "Type of house is required.")]
        public string TypeofHouse { get; set; }

        [Required(ErrorMessage = "Ownership status is required.")]
        public string IsOwnedRented { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Monthly Rent must be a positive value.")]
        public decimal? MonthlyRent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Deposit must be a positive value.")]
        public decimal? Deposit { get; set; }

        [Required(ErrorMessage = "Address Type is required.")]
        public string AddressType { get; set; } // Current, Permanent

        [Required(ErrorMessage = "Address is required.")]
        public string AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; } // Optional

        [Required(ErrorMessage = "City is required.")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required.")]
        public string State { get; set; }

        [Required(ErrorMessage = "Country is required.")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Postal Code is required.")]
        [RegularExpression(@"^\d{5,10}$", ErrorMessage = "Postal Code must be between 5 and 10 digits.")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Latitude is required.")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90 degrees.")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required.")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180 degrees.")]
        public double Longitude { get; set; }


        public string HouseOrFlatNumber { get; set; }
    
    
     

        [Required]
        public Guid AddressGuid { get; set; } = Guid.NewGuid(); // ✅ Generates a new GUID by default

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "CreatedBy is required.")]
        public string CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
 
       public string? UpdatedBy { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
