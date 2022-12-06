namespace BankSystem.Models.Entities
{
    public class Deposit
    {
        public string Id { get; set; }
        public decimal Money { get; set; }
        public double Percent { get; set; }
        public string Bank { get; set; }
        public DateTime CreationTime { get; set; }
        public bool Blocked { get; set; }
        public bool Freezed { get; set; }
    }
}
