namespace BankSystem.Models.Entities
{
    public class Bank
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double DepositPercent { get; set; }
        public double CreditPercent { get; set; }
    }
}
