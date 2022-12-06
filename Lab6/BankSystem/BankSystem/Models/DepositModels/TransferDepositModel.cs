using BankSystem.Models.Entities;

namespace BankSystem.Models.DepositModels
{
    public class TransferDepositModel
    {
        public List<Deposit> Deposits { get; set; }
        public string Bank { get; set; }
        public string IdFrom { get; set; }
        public string IdTo { get; set; }
        public decimal Money { get; set; }
    }
}
