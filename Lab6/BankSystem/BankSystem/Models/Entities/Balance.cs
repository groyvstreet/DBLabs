using System.ComponentModel.DataAnnotations;

namespace BankSystem.Models.Entities
{
    public class Balance
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Money { get; set; }
        public string Bank { get; set; }
    }
}
