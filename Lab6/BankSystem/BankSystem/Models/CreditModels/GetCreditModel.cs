using BankSystem.Models.Entities;

namespace BankSystem.Models.CreditModels
{
    public class GetCreditModel
    {
        public List<Balance> Balances { get; set; }
        public string CreditId { get; set; }
        public string BalanceId { get; set; }
    }
}
