namespace BankSystem.Models.Entities
{
    public class Client
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
        public string PhoneNumber { get; set; }
        public string PassportSeries { get; set; }
        public string PassportNumber { get; set; }
        public string IdentificationNumber { get; set; }
        public bool InBlackList { get; set; }
    }
}
