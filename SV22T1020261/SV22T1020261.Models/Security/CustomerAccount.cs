namespace SV22T1020261.Models.Security
{
    public class CustomerAccount
    {
        public int CustomerID { get; set; }   // FK
        public string CustomerName { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool IsLocked { get; set; }
    }
}
