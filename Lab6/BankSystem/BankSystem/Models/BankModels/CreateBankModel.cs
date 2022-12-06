using System.ComponentModel.DataAnnotations;

namespace BankSystem.Models.BankModels
{
    public class CreateBankModel
    {
        [Required(ErrorMessage = "Не указано имя банка")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Не указан процент для вклада")]
        public double DepositPercent { get; set; }

        [Required(ErrorMessage = "Не указан процент для кредита")]
        public double CreditPercent { get; set; }
    }
}
