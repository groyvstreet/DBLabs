using BankSystem.Data;
using BankSystem.Models.DepositModels;
using BankSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DbConnection = BankSystem.Data.DbConnection;

namespace BankSystem.Controllers
{
    public class DepositController : Controller
    {
        [HttpGet]
        [Authorize(Roles = "client")]
        public IActionResult Create(string bankId)
        {
            ViewBag.BankId = bankId;

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "client")]
        public IActionResult Create(CreateDepositModel model)
        {
            ViewBag.BankId = model.BankId;

            if (ModelState.IsValid)
            {
                var modelMoney = decimal.Parse(model.Money.Replace(".", ","));

                if (modelMoney < 1000)
                {
                    ModelState.AddModelError("", "Минимальная сумма - 1000");
                    return View(model);
                }

                if (modelMoney > 1000000)
                {
                    ModelState.AddModelError("", "Максимальная сумма пополнения за раз - 1000000");
                    return View(model);
                }

                var deposit = new Deposit
                {
                    Money = modelMoney
                };

                var command = DbConnection.getCommand();
                command.CommandText = $"select balances.id, balances.money from balances inner join users on " +
                    $"users.id = balances.client_id where balances.name = '{model.BalanceName}' and " +
                    $"users.email = '{User.Identity!.Name}'";
                var dataReader = command.ExecuteReader();
                
                if (!dataReader.Read())
                {
                    ModelState.AddModelError("", "Указанный счет не найден");
                    return View(model);
                }

                var balanceId = dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!;
                var balanceMoney = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!);

                if (balanceMoney < modelMoney)
                {
                    ModelState.AddModelError("", "На указанном счете недостаточно средств");
                    return View(model);
                }

                command = DbConnection.getCommand();
                command.CommandText = $"select percent_for_deposit from banks where id = '{model.BankId}'";
                dataReader = command.ExecuteReader();
                dataReader.Read();

                var bankPercent = double.Parse(dataReader.GetValue(dataReader.GetOrdinal("percent_for_deposit")).ToString()!);

                command = DbConnection.getCommand();
                command.CommandText = $"insert into deposits values('{Guid.NewGuid()}', " +
                    $"{modelMoney.ToString().Replace(',', '.')}, " +
                    $"{bankPercent.ToString().Replace(',', '.')}, " +
                    $"'{DateTime.Now.Date}', " +
                    $"false, " +
                    $"false, " +
                    $"(select id from users where email = '{User.Identity!.Name}'), " +
                    $"'{model.BankId}')";
                command.ExecuteReader();

                command = DbConnection.getCommand();
                command.CommandText = $"update balances set money = {(balanceMoney - modelMoney).ToString().Replace(',', '.')} " +
                    $"where id = '{balanceId}'";
                command.ExecuteReader();

                command = DbConnection.getCommand();
                command.CommandText = $"select banks.name as bank from balances inner join banks on banks.id = balances.bank_id " +
                    $"where balances.id = '{balanceId}'";
                dataReader = command.ExecuteReader();
                dataReader.Read();

                var balanceBankName = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!;

                command = DbConnection.getCommand();
                command.CommandText = $"select name from banks where id = '{model.BankId}'";
                dataReader = command.ExecuteReader();
                dataReader.Read();

                var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

                command = DbConnection.getCommand();
                command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                    $"'Клиент {User.Identity.Name} открыл вклад с начальной суммой {model.Money} в банке {bankName} со счета {model.BalanceName} в банке {balanceBankName}.')";
                command.ExecuteReader();

                return RedirectToAction("Profile", "Client");
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "client")]
        public IActionResult Add(string? depositId)
        {
            ViewBag.DepositId = depositId;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "client")]
        public IActionResult Add(AddDepositModel model)
        {
            var modelMoney = decimal.Parse(model.Money.Replace(".", ","));

            if (modelMoney < 0.01m)
            {
                ModelState.AddModelError("", "Минимальная сумма - 0.01");
                ViewBag.DepositId = model.Id;
                return View(model);
            }

            if (modelMoney > 1000000)
            {
                ModelState.AddModelError("", "Максимальная сумма пополнения за раз - 1000000");
                ViewBag.DepositId = model.Id;
                return View(model);
            }

            var command = DbConnection.getCommand();
            command.CommandText = $"select money from deposits where id = '{model.Id}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!);

            money += modelMoney;

            command = DbConnection.getCommand();
            command.CommandText = $"update deposits set money = {money.ToString().Replace(',', '.')} where id = '{model.Id}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = (select bank_id from deposits where id = '{model.Id}')";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Клиент {User.Identity.Name} пополнил вклад на {modelMoney} в банке {bankName}.')";
            command.ExecuteReader();

            return RedirectToAction("Profile", "Client");
        }

        [HttpGet]
        [Authorize(Roles = "client")]
        public IActionResult Transfer(string? depositId)
        {
            ViewBag.DepositId = depositId;

            var command = DbConnection.getCommand();
            command.CommandText = $"select deposits.id, deposits.money, deposits.percent, deposits.creation_date, " +
                $"deposits.is_blocked, deposits.is_freezed, banks.name as bank from deposits " +
                $"inner join banks on banks.id = deposits.bank_id " +
                $"where deposits.id = '{depositId}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

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

            command = DbConnection.getCommand();
            command.CommandText = $"select deposits.id, deposits.money, deposits.percent, deposits.creation_date, " +
                $"deposits.is_blocked, deposits.is_freezed, banks.name as bank from deposits inner join " +
                $"banks on deposits.bank_id = banks.id inner join users on deposits.client_id = users.id " +
                $"where users.email = '{User.Identity!.Name}' and banks.name = '{deposit.Bank}' and deposits.id <> '{deposit.Id}'";
            dataReader = command.ExecuteReader();

            var deposits = new List<Deposit>();

            while (dataReader.Read())
            {
                var temp = new Deposit
                {
                    Id = dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!,
                    Money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!),
                    Percent = double.Parse(dataReader.GetValue(dataReader.GetOrdinal("percent")).ToString()!),
                    Bank = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!,
                    CreationTime = DateTime.Parse(dataReader.GetValue(dataReader.GetOrdinal("creation_date")).ToString()!),
                    Blocked = bool.Parse(dataReader.GetValue(dataReader.GetOrdinal("is_blocked")).ToString()!),
                    Freezed = bool.Parse(dataReader.GetValue(dataReader.GetOrdinal("is_freezed")).ToString()!)
                };

                deposits.Add(temp);
            }

            if (deposits.Count == 0)
            {
                return RedirectToAction("Profile", "Client");
            }

            double moneyForGetting = Math.Round(((DateTime.Now - deposit.CreationTime) /
                (new DateTime(
                    deposit.CreationTime.Year + (DateTime.Now.Year - deposit.CreationTime.Year + 1),
                    deposit.CreationTime.Month,
                    deposit.CreationTime.Day)
                - deposit.CreationTime) * (deposit.Percent) + 100) * (double)deposit.Money / 100, 2, MidpointRounding.ToPositiveInfinity);

            ViewBag.Money = (decimal)moneyForGetting;

            var model = new TransferDepositModel
            {
                Deposits = deposits,
                Bank = deposit.Bank,
                IdFrom = depositId!,
                Money = (decimal)moneyForGetting
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "client")]
        public IActionResult Transfer(TransferDepositModel model)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select money from deposits where id = '{model.IdTo}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!);

            money += model.Money;

            command = DbConnection.getCommand();
            command.CommandText = $"update deposits set money = {money.ToString().Replace(',', '.')} where id = '{model.IdTo}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"delete from deposits where id = '{model.IdFrom}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = (select bank_id from deposits where id = '{model.IdTo}')";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Клиент {User.Identity.Name} пополнил вклад на {model.Money} в банке {bankName} через другой вклад.')";
            command.ExecuteReader();

            return RedirectToAction("Profile", "Client");
        }

        [HttpGet]
        [Authorize(Roles = "client")]
        public IActionResult Get(string? depositId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select money, percent, bank_id, creation_date from deposits where id = '{depositId}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!);
            var percent = double.Parse(dataReader.GetValue(dataReader.GetOrdinal("percent")).ToString()!);
            var bankId = dataReader.GetValue(dataReader.GetOrdinal("bank_id")).ToString();
            var creationDate = DateTime.Parse(dataReader.GetValue(dataReader.GetOrdinal("creation_date")).ToString()!);
            
            double moneyForGetting = Math.Round(((DateTime.Now - creationDate) /
                (new DateTime(
                    creationDate.Year + (DateTime.Now.Year - creationDate.Year + 1),
                    creationDate.Month,
                    creationDate.Day)
                - creationDate) * (percent) + 100) * (double)money / 100, 2, MidpointRounding.ToPositiveInfinity);

            ViewBag.DepositId = depositId;
            ViewBag.Money = moneyForGetting;

            command = DbConnection.getCommand();
            command.CommandText = $"select balances.id, balances.name, balances.money, banks.name as bank from balances inner join " +
                $"banks on balances.bank_id = banks.id inner join users on balances.client_id = users.id where " +
                $"users.email = '{User.Identity!.Name}' and balances.bank_id = '{bankId}'";
            dataReader = command.ExecuteReader();

            var balances = new List<Balance>();

            while (dataReader.Read())
            {
                var balance = new Balance
                {
                    Name = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!,
                    Money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!),
                    Bank = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!,
                    Id = dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!
                };

                balances.Add(balance);
            }

            var model = new GetDepositModel
            {
                Balances = balances,
                Money = (decimal)moneyForGetting
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "client")]
        public IActionResult Get(GetDepositModel model)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select money from balances where id = '{model.BalanceId}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!);

            money += model.Money;

            command = DbConnection.getCommand();
            command.CommandText = $"update balances set money = {money.ToString().Replace(',', '.')} where id = '{model.BalanceId}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"delete from deposits where id = '{model.DepositId}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select balances.name, banks.name as bank from balances inner join banks on banks.id = balances.bank_id " +
                $"where balances.id = '{model.BalanceId}'";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!;
            var balanceName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Клиент {User.Identity.Name} снял деньги с вклада на сумму {model.Money} на счет {balanceName} в банке {bankName}.')";
            command.ExecuteReader();

            return RedirectToAction("Profile", "Client");
        }

        [Authorize(Roles = "client")]
        public IActionResult DepositReplenishments(string depositId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select money, date_time from depositreplenishments where deposit_id = '{depositId}'";
            var dataReader = command.ExecuteReader();

            var replenishments = new List<DepositReplenishment>();

            while (dataReader.Read())
            {
                var replenishment = new DepositReplenishment
                {
                    DateTime = dataReader.GetValue(dataReader.GetOrdinal("date_time")).ToString()!,
                    Money = dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!
                };

                replenishments.Add(replenishment);
            }

            return View(replenishments);
        }
    }
}
