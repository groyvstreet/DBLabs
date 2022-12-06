using System.ComponentModel.DataAnnotations;

namespace BankSystem.Models.DepositModels
{
    public class CreateDepositModel
    {
        [Required(ErrorMessage = "Не указана сумма")]
        [RegularExpression(@"[0](?:[.][0-9][1-9]|[.][1-9])?|[1-9]+[0-9]{0,9}(?:[.][0-9][1-9]|[.][1-9])?", ErrorMessage = "Некорректный ввод")]
        public string Money { get; set; }

        public string BalanceName { get; set; }
        public string? BankId { get; set; }
    }
}
