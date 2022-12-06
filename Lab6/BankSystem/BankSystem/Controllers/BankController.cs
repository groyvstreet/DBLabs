using BankSystem.Data;
using BankSystem.Models.BankModels;
using BankSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace BankSystem.Controllers
{
    public class BankController : Controller
    {
        [Authorize(Roles = "admin")]
        public IActionResult Delete(string bankId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = '{bankId}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Администратор {User.Identity.Name} удалил банк {bankName}.')";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"delete from banks where id = '{bankId}'";
            command.ExecuteReader();

            return RedirectToAction("Banks", "Bank");
        }

        [Authorize(Roles = "admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult Create(CreateBankModel model)
        {
            if (ModelState.IsValid)
            {
                var command = DbConnection.getCommand();
                command.CommandText = $"select name from banks where name = '{model.Name}'";
                var dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    ModelState.AddModelError("", "Банк с таким наименованием уже есть");
                    return View(model);
                }

                command = DbConnection.getCommand();
                command.CommandText = $"insert into banks values('{Guid.NewGuid()}', '{model.Name}', {model.DepositPercent.ToString().Replace(',', '.')}, {model.CreditPercent.ToString().Replace(',', '.')})";
                command.ExecuteReader();

                command = DbConnection.getCommand();
                command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                    $"'Администратор {User.Identity.Name} создал банк {model.Name} с процентом для вклада {model.DepositPercent} и процентом для кредита {model.CreditPercent}.')";
                command.ExecuteReader();

                return RedirectToAction("Banks", "Bank");
            }

            return View(model);
        }

        [Authorize(Roles = "admin, manager")]
        [HttpGet]
        public IActionResult Edit(string bankId)
        {
            ViewBag.BankId = bankId;

            return View();
        }

        [Authorize(Roles = "admin, manager")]
        [HttpPost]
        public IActionResult Edit(EditBankModel model)
        {
            ViewBag.BankId = model.BankId;

            if (ModelState.IsValid)
            {
                var command = DbConnection.getCommand();
                command.CommandText = $"select name from banks where name = '{model.Name}' and id <> '{model.BankId}'";
                var dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    ModelState.AddModelError("", "Банк с таким наименованием уже есть");
                    return View(model);
                }

                command = DbConnection.getCommand();
                command.CommandText = $"select * from banks where id = '{model.BankId}'";
                dataReader = command.ExecuteReader();
                dataReader.Read();

                var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;
                var depositPercent = dataReader.GetValue(dataReader.GetOrdinal("percent_for_deposit")).ToString()!;
                var creditPercent = dataReader.GetValue(dataReader.GetOrdinal("percent_for_credit")).ToString()!;

                command = DbConnection.getCommand();
                command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                    $"'Администратор/менеджер {User.Identity.Name} изменил банк {bankName} -> {model.Name}, {depositPercent} -> {model.DepositPercent}, {creditPercent} -> {model.CreditPercent}.')";
                command.ExecuteReader();

                command = DbConnection.getCommand();
                command.CommandText = $"update banks set name = '{model.Name}', percent_for_deposit = {model.DepositPercent.ToString().Replace(',', '.')}, percent_for_credit = {model.CreditPercent.ToString().Replace(',', '.')} where id = '{model.BankId}'";
                command.ExecuteReader();

                if (User.IsInRole("manager"))
                {
                    return RedirectToAction("Bank", "Manager");
                }

                return RedirectToAction("Banks", "Bank");
            }

            return View(model);
        }

        public IActionResult Banks()
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select * from banks";
            var dataReader = command.ExecuteReader();

            var banks = new List<Bank>();

            while (dataReader.Read())
            {
                var bank = new Bank
                {
                    Id = dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!,
                    Name = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!,
                    DepositPercent = double.Parse(dataReader.GetValue(dataReader.GetOrdinal("percent_for_deposit")).ToString()!),
                    CreditPercent = double.Parse(dataReader.GetValue(dataReader.GetOrdinal("percent_for_credit")).ToString()!)
                };

                banks.Add(bank);
            }

            return View(banks);
        }

        public IActionResult Detail(string? bankId)
        {
            ViewBag.BankId = bankId;

            var command = DbConnection.getCommand();
            command.CommandText = $"select * from banks where id = '{bankId}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var model = new BankDetailModel
            {
                Id = bankId!,
                Name = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!,
                DepositPercent = dataReader.GetValue(dataReader.GetOrdinal("percent_for_deposit")).ToString()!,
                CreditPercent = dataReader.GetValue(dataReader.GetOrdinal("percent_for_credit")).ToString()!,
            };

            if (User.Identity!.IsAuthenticated)
            {
                model.IsClientRegistered = IsClientRegistered(bankId!);

                if (User.IsInRole("client"))
                {
                    model.IsClientInBlackList = IsClientInBlackList(bankId!);
                }
            }
            else
            {
                model.IsClientRegistered = false;
            }

            return View(model);
        }

        [Authorize(Roles = "client")]
        public IActionResult Register(string? bankId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select id from users where email = '{User.Identity!.Name}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var clientId = dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into bankclients values('{bankId}', '{clientId}')";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = '{bankId}'";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Клиент {User.Identity.Name} зарегистрировался в банке {bankName}.')";
            command.ExecuteReader();

            return Redirect($"Detail?bankId={bankId}");
        }

        public bool IsClientRegistered(string bankId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select id from users where email = '{User.Identity!.Name}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var clientId = dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"select bank_id from bankclients where bank_id = '{bankId}' and client_id = '{clientId}'";
            dataReader = command.ExecuteReader();
            
            if (dataReader.Read())
            {
                return true;
            }

            return false;
        }

        public bool IsClientInBlackList(string bankId)
        {
            var command2 = DbConnection.getCommand();
            command2.CommandText = $"select client_id from blacklistclients where client_id = (select id from users where email = '{User.Identity!.Name}') and " +
                $"black_list_id = (select id from blacklists where bank_id = '{bankId}')";
            var dataReader2 = command2.ExecuteReader();

            if (dataReader2.Read())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
