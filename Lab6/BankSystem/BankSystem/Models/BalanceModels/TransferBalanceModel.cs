using BankSystem.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace BankSystem.Models.BalanceModels
{
    public class TransferBalanceModel
    {
        [Required(ErrorMessage = "Не указана сумма")]
        [RegularExpression(@"[0](?:[.][0-9][1-9]|[.][1-9])?|[1-9]+[0-9]{0,9}(?:[.][0-9][1-9]|[.][1-9])?", ErrorMessage = "Некорректный ввод")]
        public string Money { get; set; }

        [Required(ErrorMessage = "Не указано имя пользователя")]
        public string UserNameFrom { get; set; }

        [Required(ErrorMessage = "Не указано название банка")]
        [RegularExpression(@"(?:[A-Za-zА-Яа-я0-9]+(?:\s[A-Za-zА-Яа-я0-9])?)+", ErrorMessage = "Некорректный ввод")]
        public string BankNameFrom { get; set; }

        [Required(ErrorMessage = "Не указано название счета")]
        [RegularExpression(@"(?:[A-Za-zА-Яа-я0-9]+(?:\s[A-Za-zА-Яа-я0-9])?)+", ErrorMessage = "Некорректный ввод")]
        public string BalanceNameFrom { get; set; }

        [Required(ErrorMessage = "Не указано имя пользователя")]
        public string UserNameTo { get; set; }

        [Required(ErrorMessage = "Не указано название банка")]
        [RegularExpression(@"(?:[A-Za-zА-Яа-я0-9]+(?:\s[A-Za-zА-Яа-я0-9])?)+", ErrorMessage = "Некорректный ввод")]
        public string BankNameTo { get; set; }

        [Required(ErrorMessage = "Не указано название счета")]
        [RegularExpression(@"(?:[A-Za-zА-Яа-я0-9]+(?:\s[A-Za-zА-Яа-я0-9])?)+", ErrorMessage = "Некорректный ввод")]
        public string BalanceNameTo { get; set; }

        public string IdFrom { get; set; }
    }
}
