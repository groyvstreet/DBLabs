using BankSystem.Data;
using BankSystem.Models.BalanceModels;
using BankSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.Common;
using System.Reflection;
using DbConnection = BankSystem.Data.DbConnection;

namespace BankSystem.Controllers
{
    public class BalanceController : Controller
    {
        [HttpGet]
        [Authorize]
        public IActionResult Open(string bankId)
        {
            ViewBag.BankId = bankId;

            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Open(OpenBalanceModel model)
        {
            ViewBag.BankId = model.BankId;

            var command = DbConnection.getCommand();
            command.CommandText = $"select name from balances inner join users on balances.client_id = users.id " +
                $"where balances.name = '{model.Name}' and users.email = '{User.Identity!.Name}'";
            var dataReader = command.ExecuteReader();

            if (dataReader.Read())
            {
                ModelState.AddModelError("", "Уже открыт счет с таким именем");
                return View(model);
            }

            var modelMoney = decimal.Parse(model.Money.Replace(".", ","));

            if (modelMoney > 1000000)
            {
                ModelState.AddModelError("", "Максимальная сумма пополнения за раз - 1000000");
                return View(model);
            }

            if (modelMoney < 0.01m)
            {
                ModelState.AddModelError("", "Минимальная сумма - 0.01");
                return View(model);
            }

            command = DbConnection.getCommand();
            command.CommandText = $"insert into balances values('{Guid.NewGuid()}', '{model.Name}', " +
                $"{modelMoney.ToString().Replace(',', '.')}, " +
                $"(select id from users where email = '{User.Identity!.Name}'), '{model.BankId}')";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = '{model.BankId}'";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Клиент {User.Identity.Name} открыл счет с именем {model.Name} и начальной суммой {modelMoney} в банке {bankName}.')";
            command.ExecuteReader();

            return RedirectToAction("Profile", "Client");
        }

        [Authorize]
        public IActionResult Close(string? balanceId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select balances.money, balances.name, banks.name as bank from balances " +
                $"inner join banks on banks.id = balances.bank_id " +
                $"where balances.id = '{balanceId}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!;
            var balanceName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;
            var balanceMoney = dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Клиент {User.Identity.Name} закрыл счет с именем {balanceName} и суммой {balanceMoney} в банке {bankName}.')";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"delete from balances where id = '{balanceId}'";
            command.ExecuteReader();

            return RedirectToAction("Profile", "Client");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Add(string? balanceId)
        {
            ViewBag.BalanceId = balanceId;
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Add(AddBalanceModel model)
        {
            var modelMoney = decimal.Parse(model.Money.Replace(".", ","));

            if (modelMoney < 0.01m)
            {
                ModelState.AddModelError("", "Минимальная сумма - 0.01");
                ViewBag.BalanceId = model.Id;
                return View(model);
            }

            if (modelMoney > 1000000)
            {
                ModelState.AddModelError("", "Максимальная сумма пополнения за раз - 1000000");
                ViewBag.BalanceId = model.Id;
                return View(model);
            }

            var command = DbConnection.getCommand();
            command.CommandText = $"select money from balances where id = '{model.Id}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!);

            money += modelMoney;

            command = DbConnection.getCommand();
            command.CommandText = $"update balances set money = {money.ToString().Replace(',', '.')} where id = '{model.Id}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select balances.money, balances.name, banks.name as bank from balances " +
                $"inner join banks on banks.id = balances.bank_id " +
                $"where balances.id = '{model.Id}'";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!;
            var balanceName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Клиент {User.Identity.Name} пополнил счет с именем {balanceName} на сумму {modelMoney} в банке {bankName}.')";
            command.ExecuteReader();

            return RedirectToAction("Profile", "Client");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Transfer(string? balanceId)
        {
            ViewBag.BalanceId = balanceId;
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult Transfer(TransferBalanceModel model)
        {
            decimal modelMoney = decimal.Parse(model.Money.Replace(".", ","));

            if (modelMoney < 0.01m)
            {
                ModelState.AddModelError("", "Минимальная сумма перевода 0.01");
                ViewBag.BalanceId = model.IdFrom;
                return View(model);
            }

            if (modelMoney > 1000000)
            {
                ModelState.AddModelError("", "Максимальная сумма перевода за раз - 1000000");
                ViewBag.BalanceId = model.IdFrom;
                return View(model);
            }

            var command = DbConnection.getCommand();
            command.CommandText = $"select from banks where name = '{model.BankNameTo}'";
            var dataReader = command.ExecuteReader();

            if (!dataReader.Read())
            {
                ModelState.AddModelError("", "Указанный банк отсутствует в системе");
                ViewBag.BalanceId = model.IdFrom;
                return View(model);
            }

            command = DbConnection.getCommand();
            command.CommandText = $"select from users where user_name = '{model.UserNameTo}'";
            dataReader = command.ExecuteReader();

            if (!dataReader.Read())
            {
                ModelState.AddModelError("", "Счет не найден");
                ViewBag.BalanceId = model.IdFrom;
                return View(model);
            }

            command = DbConnection.getCommand();
            command.CommandText = $"select balances.id, balances.money from balances inner join users on balances.client_id = users.id inner " +
                $"join banks on balances.bank_id = banks.id where balances.name = '{model.BalanceNameTo}' and " +
                $"users.user_name = '{model.UserNameTo}' and banks.name = '{model.BankNameTo}'";
            dataReader = command.ExecuteReader();

            if (!dataReader.Read())
            {
                ModelState.AddModelError("", "Счет не найден");
                ViewBag.BalanceId = model.IdFrom;
                return View(model);
            }

            if (model.IdFrom == dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!)
            {
                ModelState.AddModelError("", "Невозможная операция");
                ViewBag.BalanceId = model.IdFrom;
                return View(model);
            }

            command = DbConnection.getCommand();
            command.CommandText = $"select money from balances where id = '{model.IdFrom}'";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            if (decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!) < modelMoney)
            {
                ModelState.AddModelError("", "Недостаточно средств на счете");
                ViewBag.BalanceId = model.IdFrom;
                return View(model);
            }

            command = DbConnection.getCommand();
            command.CommandText = $"select balances.name as balance_name, banks.name as bank_name, users.user_name as user_name from balances " +
                $"inner join banks on balances.bank_id = banks.id inner join users on balances.client_id = users.id where balances.id = '{model.IdFrom}'";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            model.UserNameFrom = dataReader.GetValue(dataReader.GetOrdinal("user_name")).ToString()!;
            model.BalanceNameFrom = dataReader.GetValue(dataReader.GetOrdinal("balance_name")).ToString()!;
            model.BankNameFrom = dataReader.GetValue(dataReader.GetOrdinal("bank_name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"call add_transfer({model.Money}, '{model.UserNameFrom}', '{model.BalanceNameFrom}', '{model.BankNameFrom}', '{model.UserNameTo}', '{model.BalanceNameTo}', '{model.BankNameTo}')";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Клиент {model.UserNameFrom} перевел сумму {model.Money} со счета {model.BalanceNameFrom} в банке {model.BankNameFrom} клиенту {model.UserNameTo} на счет {model.BalanceNameTo} в банке {model.BankNameTo}')";
            command.ExecuteReader();
            
            return RedirectToAction("Profile", "Client");
        }

        [Authorize(Roles = "client")]
        public IActionResult Transfers(string balanceId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select balances.name, users.email, banks.name as bank, transfers.money, " +
                $"transfers.date_time, transfers.to_balance_id from transfers " +
                $"left join balances on balances.id = transfers.from_balance_id " +
                $"left join users on users.id = balances.client_id " +
                $"left join banks on banks.id = balances.bank_id " +
                $"where balances.id = '{balanceId}'";
            var dataReader = command.ExecuteReader();

            var transfers = new List<Transfer>();

            while (dataReader.Read())
            {
                var to_balance_id = dataReader.GetValue(dataReader.GetOrdinal("to_balance_id")).ToString();


                if (to_balance_id.Length == 0)
                {
                    var transfer = new Transfer
                    {
                        BankNameFrom = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!,
                        BankNameTo = null,
                        ClientEmailFrom = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString()!,
                        ClientEmailTo = null,
                        BalanceNameFrom = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!,
                        BalanceNameTo = null,
                        Money = dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!,
                        DateTime = dataReader.GetValue(dataReader.GetOrdinal("date_time")).ToString()!
                    };

                    transfers.Add(transfer);
                }
                else
                {
                    var command2 = DbConnection.getCommand();
                    command2.CommandText = $"select balances.name, users.email, banks.name as bank from balances " +
                        $"inner join users on users.id = balances.client_id " +
                        $"inner join banks on banks.id = balances.bank_id where balances.id = '{to_balance_id}'";
                    var dataReader2 = command2.ExecuteReader();
                    dataReader2.Read();

                    var transfer = new Transfer
                    {
                        BankNameFrom = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!,
                        BankNameTo = dataReader2.GetValue(dataReader2.GetOrdinal("bank")).ToString()!,
                        ClientEmailFrom = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString()!,
                        ClientEmailTo = dataReader2.GetValue(dataReader2.GetOrdinal("email")).ToString()!,
                        BalanceNameFrom = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!,
                        BalanceNameTo = dataReader2.GetValue(dataReader2.GetOrdinal("name")).ToString()!,
                        Money = dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!,
                        DateTime = dataReader.GetValue(dataReader.GetOrdinal("date_time")).ToString()!
                    };

                    transfers.Add(transfer);
                }
            }

            command = DbConnection.getCommand();
            command.CommandText = $"select balances.name, users.email, banks.name as bank, transfers.money, " +
                $"transfers.date_time, transfers.from_balance_id from transfers " +
                $"left join balances on balances.id = transfers.to_balance_id " +
                $"left join users on users.id = balances.client_id " +
                $"left join banks on banks.id = balances.bank_id " +
                $"where balances.id = '{balanceId}'";
            dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                var from_balance_id = dataReader.GetValue(dataReader.GetOrdinal("from_balance_id")).ToString();

                if (from_balance_id.Length == 0)
                {
                    var transfer = new Transfer
                    {
                        BankNameFrom = null,
                        BankNameTo = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!,
                        ClientEmailFrom = null,
                        ClientEmailTo = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString()!,
                        BalanceNameFrom = null,
                        BalanceNameTo = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!,
                        Money = dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!,
                        DateTime = dataReader.GetValue(dataReader.GetOrdinal("date_time")).ToString()!
                    };

                    transfers.Add(transfer);
                }
                else
                {
                    var command2 = DbConnection.getCommand();
                    command2.CommandText = $"select balances.name, users.email, banks.name as bank from balances " +
                        $"inner join users on users.id = balances.client_id " +
                        $"inner join banks on banks.id = balances.bank_id where balances.id = '{from_balance_id}'";
                    var dataReader2 = command2.ExecuteReader();
                    dataReader2.Read();

                    var transfer = new Transfer
                    {
                        BankNameFrom = dataReader2.GetValue(dataReader.GetOrdinal("bank")).ToString()!,
                        BankNameTo = dataReader.GetValue(dataReader2.GetOrdinal("bank")).ToString()!,
                        ClientEmailFrom = dataReader2.GetValue(dataReader.GetOrdinal("email")).ToString()!,
                        ClientEmailTo = dataReader.GetValue(dataReader2.GetOrdinal("email")).ToString()!,
                        BalanceNameFrom = dataReader2.GetValue(dataReader.GetOrdinal("name")).ToString()!,
                        BalanceNameTo = dataReader.GetValue(dataReader2.GetOrdinal("name")).ToString()!,
                        Money = dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!,
                        DateTime = dataReader.GetValue(dataReader.GetOrdinal("date_time")).ToString()!
                    };

                    transfers.Add(transfer);
                }
            }

            return View(transfers);
        }
    }
}
