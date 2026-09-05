namespace NCMIS.Models
{
    public class OfflineDump
    {

        [key]
        public int Id { get; set; }                // Primary key
        public string ModuleName { get; set; } = ""; // e.g. "BeneficiaryProfile"
        public string DataJson { get; set; } = "";   // Raw JSON data
        public bool IsSynced { get; set; } = false;  // Whether synced with server

        public int UserId { get; set; }

        public int WorkFlowTrackerId { get; set; }
        public int ApplicaitonId { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompleteDate { get; set; }

        public string DeviceMetaData { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
