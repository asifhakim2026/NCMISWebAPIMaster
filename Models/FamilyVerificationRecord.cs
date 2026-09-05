namespace NCMIS.Models
{
    public class FamilyVerificationRecord
    {
        public int Id { get; set; }
        public int? PersonId { get; set; }

        public int? FamilyId { get; set; }
        public string VerifiedDataJson { get; set; } = string.Empty;
        public string SignedBy { get; set; } = string.Empty;
        public string SignatureImagePath { get; set; } = string.Empty;
        public DateTime VerifiedOn { get; set; }

        public string CreatedBy { get; set; }
    }
}
