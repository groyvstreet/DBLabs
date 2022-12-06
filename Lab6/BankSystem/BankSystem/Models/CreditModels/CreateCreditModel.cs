using System.ComponentModel.DataAnnotations;

namespace BankSystem.Models.CreditModels
{
    public class CreateCreditModel
    {
        [Required(ErrorMessage = "Не указана сумма")]
        [RegularExpression(@"[0](?:[.][0-9][1-9]|[.][1-9])?|[1-9]+[0-9]{0,9}(?:[.][0-9][1-9]|[.][1-9])?", ErrorMessage = "Некорректный ввод")]
        public string Money { get; set; }

        [Required(ErrorMessage = "Не указано количество месяцев")]
        [RegularExpression(@"[1-9][0-9]*[0-9]*", ErrorMessage = "Некорректный ввод")]
        public int Months { get; set; }

        public string BankId { get; set; }
    }
}
