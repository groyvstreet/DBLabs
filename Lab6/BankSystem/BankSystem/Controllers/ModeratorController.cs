using BankSystem.Data;
using BankSystem.Models.Entities;
using BankSystem.Models.ModeratorModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace BankSystem.Controllers
{
    public class ModeratorController : Controller
    {
        [Authorize(Roles = "moderator")]
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

        [Authorize(Roles = "moderator")]
        public IActionResult SignupApplications()
        {
            var clients = new List<Client>();

            var command = DbConnection.getCommand();
            command.CommandText = $"select users.id, users.phone_number, users.first_name, users.last_name, users.patronimic, clients.passport_series, clients.passport_number_series, clients.passport_identification_number " +
                $"from users inner join clients on clients.id = users.id where clients.is_approved = false";
            var dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                var client = new Client
                {
                    Id = dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!,
                    FirstName = dataReader.GetValue(dataReader.GetOrdinal("first_name")).ToString()!,
                    LastName = dataReader.GetValue(dataReader.GetOrdinal("last_name")).ToString()!,
                    Patronymic = dataReader.GetValue(dataReader.GetOrdinal("patronimic")).ToString()!,
                    PhoneNumber = dataReader.GetValue(dataReader.GetOrdinal("phone_number")).ToString()!,
                    PassportSeries = dataReader.GetValue(dataReader.GetOrdinal("passport_series")).ToString()!,
                    PassportNumber = dataReader.GetValue(dataReader.GetOrdinal("passport_number_series")).ToString()!,
                    IdentificationNumber = dataReader.GetValue(dataReader.GetOrdinal("passport_identification_number")).ToString()!
                };

                clients.Add(client);
            }

            return View(clients);
        }

        [Authorize(Roles = "moderator")]
        public IActionResult ApproveSignup(string clientId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"update clients set is_approved = true where id = '{clientId}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select email from users where id = '{clientId}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var clientEmail = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Модератор {User.Identity.Name} подтвердил регистрацию клиента {clientEmail}.')";
            command.ExecuteReader();

            return RedirectToAction("SignupApplications", "Moderator");
        }

        [Authorize(Roles = "moderator")]
        public IActionResult RejectSignup(string clientId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select email from users where id = '{clientId}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var clientEmail = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Модератор {User.Identity.Name} отклонил регистрацию клиента {clientEmail}.')";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"delete from users where id = '{clientId}'";
            command.ExecuteReader();

            return RedirectToAction("SignupApplications", "Moderator");
        }
    }
}
