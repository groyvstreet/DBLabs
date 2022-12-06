using System.ComponentModel.DataAnnotations;

namespace BankSystem.Models.BalanceModels
{
    public class AddBalanceModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Не указана сумма")]
        [RegularExpression(@"[0](?:[.][0-9][1-9]|[.][1-9])?|[1-9]+[0-9]{0,9}(?:[.][0-9][1-9]|[.][1-9])?", ErrorMessage = "Некорректный ввод")]
        public string Money { get; set; }
    }
}
