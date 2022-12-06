using BankSystem.Data;
using BankSystem.Models.ClientModels;
using BankSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace BankSystem.Controllers
{
    public class ClientController : Controller
    {
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Signup(SignupClientModel model)
        {
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
                    $"(select id from roles where name = 'client'))";
                command.ExecuteReader();

                command = DbConnection.getCommand();
                command.CommandText = $"insert into clients values('{id}', '{model.PassportSeries}', '{model.PassportNumber}', " +
                    $"'{model.IdentificationNumber}', false)";
                command.ExecuteReader();

                command = DbConnection.getCommand();
                command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                    $"'Клиент {model.Email} зарегистрировался в системе.')";
                command.ExecuteReader();

                return RedirectToAction("Login", "User");
            }

            return View(model);
        }

        [Authorize(Roles = "client")]
        public IActionResult Profile()
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select user_name, password, email, phone_number, first_name, last_name, " +
                $"patronimic, passport_series, passport_number_series, passport_identification_number, is_approved " +
                $"from users inner join clients on users.id = clients.id where email = '{User.Identity!.Name}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var model = new ProfileModel
            {
                Email = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString(),
                FirstName = dataReader.GetValue(dataReader.GetOrdinal("first_name")).ToString(),
                LastName = dataReader.GetValue(dataReader.GetOrdinal("last_name")).ToString(),
                Patronymic = dataReader.GetValue(dataReader.GetOrdinal("patronimic")).ToString(),
                PhoneNumber = dataReader.GetValue(dataReader.GetOrdinal("phone_number")).ToString(),
                PassportSeries = dataReader.GetValue(dataReader.GetOrdinal("passport_series")).ToString(),
                PassportNumber = dataReader.GetValue(dataReader.GetOrdinal("passport_number_series")).ToString(),
                IdentificationNumber = dataReader.GetValue(dataReader.GetOrdinal("passport_identification_number")).ToString(),
                NowTime = DateTime.Now,
                Approved = bool.Parse(dataReader.GetValue(dataReader.GetOrdinal("is_approved")).ToString()!)
            };

            command = DbConnection.getCommand();
            command.CommandText = $"select balances.id, balances.name, balances.money, banks.name as bank from balances inner join " +
                $"banks on balances.bank_id = banks.id inner join users on balances.client_id = users.id where " +
                $"users.email = '{User.Identity!.Name}'";
            dataReader = command.ExecuteReader();

            var balances = new List<Balance>();

            while (dataReader.Read())
            {
                var balance = new Balance
                {
                    Name = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!,
                    Money = Decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!),
                    Bank = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!,
                    Id = dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!
                };

                balances.Add(balance);
            }

            command = DbConnection.getCommand();
            command.CommandText = $"select credits.id, credits.money, credits.percent, credits.money_with_percent, " +
                $"credits.money_payed, credits.creation_date, credits.payment_date, credits.is_approved, credits.is_getted, " +
                $"banks.name as bank from credits inner join banks on credits.bank_id = banks.id inner join users " +
                $"on credits.client_id = users.id where users.email = '{User.Identity.Name}'";
            dataReader = command.ExecuteReader();

            var credits = new List<Credit>();

            while (dataReader.Read())
            {
                var credit = new Credit
                {
                    Id = dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!,
                    Money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!),
                    MoneyWithPercent = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money_with_percent")).ToString()!),
                    MoneyPayed = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money_payed")).ToString()!),
                    Percent = double.Parse(dataReader.GetValue(dataReader.GetOrdinal("percent")).ToString()!),
                    CreationTime = DateTime.Parse(dataReader.GetValue(dataReader.GetOrdinal("creation_date")).ToString()!),
                    PaymentTime = DateTime.Parse(dataReader.GetValue(dataReader.GetOrdinal("payment_date")).ToString()!),
                    Approved = bool.Parse(dataReader.GetValue(dataReader.GetOrdinal("is_approved")).ToString()!),
                    Getted = bool.Parse(dataReader.GetValue(dataReader.GetOrdinal("is_getted")).ToString()!)
                };

                credits.Add(credit);
            }

            command = DbConnection.getCommand();
            command.CommandText = $"select deposits.id, deposits.money, deposits.percent, deposits.creation_date, " +
                $"deposits.is_blocked, deposits.is_freezed, banks.name as bank from deposits inner join " +
                $"banks on deposits.bank_id = banks.id inner join users on deposits.client_id = users.id " +
                $"where users.email = '{User.Identity.Name}'";
            dataReader = command.ExecuteReader();

            var deposits = new List<Deposit>();

            while (dataReader.Read())
            {
                var deposit = new Deposit
                {
                    Id = dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!,
                    Money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!),
                    Percent = double.Parse(dataReader.GetValue(dataReader.GetOrdinal("percent")).ToString()!),
                    Bank = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!,
                    CreationTime = DateTime.Parse(dataReader.GetValue(dataReader.GetOrdinal("creation_date")).ToString()!),
                    Blocked = bool.Parse(dataReader.GetValue(dataReader.GetOrdinal("is_blocked")).ToString()!),
                    Freezed = bool.Parse(dataReader.GetValue(dataReader.GetOrdinal("is_freezed")).ToString()!)
                };

                deposits.Add(deposit);
            }

            model.Balances = balances;
            model.Deposits = deposits;
            model.Credits = credits;

            return View(model);
        }
    }
}
