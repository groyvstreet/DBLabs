namespace BankSystem.Models.Entities
{
    public class Credit
    {
        public string Id { get; set; }
        public decimal Money { get; set; }
        public decimal MoneyWithPercent { get; set; }
        public decimal MoneyPayed { get; set; }
        public double Percent { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime PaymentTime { get; set; }
        public bool Approved { get; set; }
        public bool Getted { get; set; }
    }
}
