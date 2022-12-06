namespace BankSystem.Models.BankModels
{
    public class BankDetailModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DepositPercent { get; set; }
        public string CreditPercent { get; set; }
        public bool IsClientRegistered { get; set; }
        public bool IsClientInBlackList { get; set; }
    }
}
