using BankSystem.Models.Entities;

namespace BankSystem.Models.DepositModels
{
    public class GetDepositModel
    {
        public decimal Money { get; set; }
        public List<Balance> Balances { get; set; }
        public string DepositId { get; set; }
        public string BalanceId { get; set; }
    }
}
