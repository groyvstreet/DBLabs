using BankSystem.Models.Entities;

namespace BankSystem.Models.CreditModels
{
    public class PayCreditModel
    {
        public List<Balance> Balances { get; set; }
        public string CreditId { get; set; }
        public string BalanceId { get; set; }
    }
}
