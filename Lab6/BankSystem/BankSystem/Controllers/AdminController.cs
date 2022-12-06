using BankSystem.Data;
using BankSystem.Models.AdminModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.Controllers
{
    public class AdminController : Controller
    {
        [Authorize(Roles = "admin")]
        public IActionResult Profile()
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select user_name, email, phone_number, first_name, last_name, patronimic " +
                $"from users where email = '{User.Identity!.Name}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var model = new ProfileModel
            {
                Email = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString(),
                FirstName = dataReader.GetValue(dataReader.GetOrdinal("first_name")).ToString(),
                LastName = dataReader.GetValue(dataReader.GetOrdinal("last_name")).ToString(),
                Patronymic = dataReader.GetValue(dataReader.GetOrdinal("patronimic")).ToString(),
                PhoneNumber = dataReader.GetValue(dataReader.GetOrdinal("phone_number")).ToString()
            };

            return View(model);
        }
    }
}
