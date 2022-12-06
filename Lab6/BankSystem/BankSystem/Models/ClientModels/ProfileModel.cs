using BankSystem.Models.Entities;

namespace BankSystem.Models.ClientModels
{
    public class ProfileModel
    {
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Patronymic { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PassportSeries { get; set; }
        public string? PassportNumber { get; set; }
        public string? IdentificationNumber { get; set; }
        public List<Deposit> Deposits { get; set; }
        public DateTime NowTime { get; set; }
        public List<Balance> Balances { get; set; }
        public List<Credit> Credits { get; set; }
        public bool Approved { get; set; }
    }
}
