namespace NCMIS.Models
{
    public class LoginAudit
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string IPAddress { get; set; }
        public bool Success { get; set; }
        public DateTime AttemptTime { get; set; }


        // ✅ New fields
        public string DeviceType { get; set; }
        public string OS { get; set; }
        public string Browser { get; set; }
        public string UserAgent { get; set; }
    }
}
