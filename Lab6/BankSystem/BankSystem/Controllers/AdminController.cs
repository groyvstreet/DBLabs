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

        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult CreateManager(string bankId)
        {
            ViewBag.BankId = bankId;

            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult CreateManager(CreateManagerModel model)
        {
            ViewBag.BankId = model.BankId;

            if (ModelState.IsValid)
            {
                var command = DbConnection.getCommand();
                command.CommandText = $"select id from users where user_name = '{model.UserName}'";
                var dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    ModelState.AddModelError("", "Указанное имя пользователя уже занято");

                    return View(model);
                }

                command = DbConnection.getCommand();
                command.CommandText = $"select id from users where email = '{model.Email}'";
                dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    ModelState.AddModelError("", "На указанный электронный адрес уже зарегистрирован пользователь");

                    return View(model);
                }

                var id = Guid.NewGuid();

                command = DbConnection.getCommand();
                command.CommandText = $"insert into users values('{id}', '{model.UserName}', '{model.Password}', " +
                    $"'{model.Email}', '{model.PhoneNumber}', '{model.FirstName}', '{model.LastName}', '{model.Patronymic}', " +
                    $"(select id from roles where name = 'manager'))";
                command.ExecuteReader();

                command = DbConnection.getCommand();
                command.CommandText = $"insert into managers values('{id}', '{model.BankId}')";
                command.ExecuteReader();

                command = DbConnection.getCommand();
                command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                    $"'Администратор {User.Identity.Name} создал менеджера {model.Email} в системе.')";
                command.ExecuteReader();

                return RedirectToAction("Banks", "Bank");
            }

            return View(model);
        }
    }
}
