namespace BankSystem.Models.Entities
{
    public class Transfer
    {
        public string? BankNameFrom { get; set; }
        public string? BankNameTo { get; set; }
        public string? ClientEmailFrom { get; set; }
        public string? ClientEmailTo { get; set; }
        public string? BalanceNameFrom { get; set; }
        public string? BalanceNameTo { get; set; }
        public string? Money { get; set; }
        public string? DateTime { get; set; }
    }
}
