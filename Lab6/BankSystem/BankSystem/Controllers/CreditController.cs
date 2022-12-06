using BankSystem.Data;
using BankSystem.Models.CreditModels;
using BankSystem.Models.DepositModels;
using BankSystem.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankSystem.Controllers
{
    public class CreditController : Controller
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
        public IActionResult Create(CreateCreditModel model)
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

                var command = DbConnection.getCommand();
                command.CommandText = $"select percent_for_credit from banks where id = '{model.BankId}'";
                var dataReader = command.ExecuteReader();
                dataReader.Read();

                var bankPercent = double.Parse(dataReader.GetValue(dataReader.GetOrdinal("percent_for_credit")).ToString()!);

                command = DbConnection.getCommand();
                command.CommandText = $"insert into credits values('{Guid.NewGuid()}', " +
                    $"{modelMoney.ToString().Replace(',', '.')}, " +
                    $"{bankPercent.ToString().Replace(',', '.')}, " +
                    $"{(modelMoney * (decimal)((100.0 + bankPercent) / 100.0)).ToString().Replace(',', '.')}, " +
                    $"0.0, " +
                    $"'{DateTime.Now.Date}', " +
                    $"'{DateTime.Now.Date.AddMonths(model.Months)}', " +
                    $"false, " +
                    $"false, " +
                    $"(select id from users where email = '{User.Identity!.Name}'), " +
                    $"'{model.BankId}')";
                command.ExecuteReader();

                command = DbConnection.getCommand();
                command.CommandText = $"select name from banks where id = '{model.BankId}'";
                dataReader = command.ExecuteReader();
                dataReader.Read();

                var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

                command = DbConnection.getCommand();
                command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                    $"'Клиент {User.Identity.Name} подал заявку на кредит на сумму {model.Money} в банке {bankName}.')";
                command.ExecuteReader();

                return RedirectToAction("Profile", "Client");
            }

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "client")]
        public IActionResult PayAll(string? creditId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select balances.id, balances.name, balances.money, banks.name as bank from balances inner join " +
                $"banks on balances.bank_id = banks.id inner join users on balances.client_id = users.id where " +
                $"users.email = '{User.Identity!.Name}'";
            var dataReader = command.ExecuteReader();

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

            ViewBag.CreditId = creditId;

            var model = new PayCreditModel
            {
                Balances = balances
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "client")]
        public IActionResult PayAll(PayCreditModel model)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select * from credits where id = '{model.CreditId}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var credit = new Credit
            {
                Id = model.CreditId,
                Money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!),
                MoneyWithPercent = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money_with_percent")).ToString()!),
                MoneyPayed = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money_payed")).ToString()!),
                Percent = double.Parse(dataReader.GetValue(dataReader.GetOrdinal("percent")).ToString()!),
                CreationTime = DateTime.Parse(dataReader.GetValue(dataReader.GetOrdinal("creation_date")).ToString()!),
                PaymentTime = DateTime.Parse(dataReader.GetValue(dataReader.GetOrdinal("payment_date")).ToString()!),
                Approved = bool.Parse(dataReader.GetValue(dataReader.GetOrdinal("is_approved")).ToString()!)
            };

            var moneyToPay = credit.MoneyWithPercent - credit.MoneyPayed;

            command = DbConnection.getCommand();
            command.CommandText = $"select money from balances where id = '{model.BalanceId}'";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var balanceMoney = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!);

            if (balanceMoney < moneyToPay)
            {
                ViewBag.CreditId = model.CreditId;

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
                        Money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!),
                        Bank = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!,
                        Id = dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!
                    };

                    balances.Add(balance);
                }

                var newModel = new PayCreditModel
                {
                    Balances = balances
                };

                return View(newModel);
            }

            credit.MoneyPayed += moneyToPay;

            command = DbConnection.getCommand();
            command.CommandText = $"update credits set money_payed = '{credit.MoneyPayed.ToString().Replace(',', '.')}' where id = '{model.CreditId}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = (select bank_id from credits where id = '{credit.Id}')";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Клиент {User.Identity.Name} внес платеж {moneyToPay} за кредит на сумму {credit.Money} в банке {bankName}.')";
            command.ExecuteReader();

            if (credit.MoneyPayed >= credit.MoneyWithPercent)
            {
                command = DbConnection.getCommand();
                command.CommandText = $"delete from credits where id = '{model.CreditId}'";
                command.ExecuteReader();

                command = DbConnection.getCommand();
                command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                    $"'Клиент {User.Identity.Name} погасил кредит на сумму {credit.Money} в банке {bankName}.')";
                command.ExecuteReader();
            }

            return RedirectToAction("Profile", "Client");
        }

        [HttpGet]
        [Authorize(Roles = "client")]
        public IActionResult Pay(string? creditId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select balances.id, balances.name, balances.money, banks.name as bank from balances inner join " +
                $"banks on balances.bank_id = banks.id inner join users on balances.client_id = users.id where " +
                $"users.email = '{User.Identity!.Name}'";
            var dataReader = command.ExecuteReader();

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

            ViewBag.CreditId = creditId;

            var model = new PayCreditModel
            {
                Balances = balances
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "client")]
        public IActionResult Pay(PayCreditModel model)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select * from credits where id = '{model.CreditId}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var credit = new Credit
            {
                Id = model.CreditId,
                Money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!),
                MoneyWithPercent = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money_with_percent")).ToString()!),
                MoneyPayed = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money_payed")).ToString()!),
                Percent = double.Parse(dataReader.GetValue(dataReader.GetOrdinal("percent")).ToString()!),
                CreationTime = DateTime.Parse(dataReader.GetValue(dataReader.GetOrdinal("creation_date")).ToString()!),
                PaymentTime = DateTime.Parse(dataReader.GetValue(dataReader.GetOrdinal("payment_date")).ToString()!),
                Approved = bool.Parse(dataReader.GetValue(dataReader.GetOrdinal("is_approved")).ToString()!)
            };

            var months = (credit.PaymentTime.Year - credit.CreationTime.Year) * 12 + (credit.PaymentTime.Month - credit.CreationTime.Month);
            var moneyToPay = credit.MoneyWithPercent / months;

            command = DbConnection.getCommand();
            command.CommandText = $"select money from balances where id = '{model.BalanceId}'";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var balanceMoney = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!);

            if (balanceMoney < moneyToPay)
            {
                ViewBag.CreditId = model.CreditId;

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
                        Money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!),
                        Bank = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!,
                        Id = dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!
                    };

                    balances.Add(balance);
                }

                var newModel = new PayCreditModel
                {
                    Balances = balances
                };

                return View(newModel);
            }

            credit.MoneyPayed += moneyToPay;

            command = DbConnection.getCommand();
            command.CommandText = $"update credits set money_payed = '{credit.MoneyPayed.ToString().Replace(',', '.')}' where id = '{model.CreditId}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = (select bank_id from credits where id = '{credit.Id}')";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Клиент {User.Identity.Name} внес платеж {moneyToPay} за кредит на сумму {credit.Money} в банке {bankName}.')";
            command.ExecuteReader();

            if (credit.MoneyPayed >= credit.MoneyWithPercent)
            {
                command = DbConnection.getCommand();
                command.CommandText = $"delete from credits where id = '{model.CreditId}'";
                command.ExecuteReader();

                command = DbConnection.getCommand();
                command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                    $"'Клиент {User.Identity.Name} погасил кредит на сумму {credit.Money} в банке {bankName}.')";
                command.ExecuteReader();
            }

            return RedirectToAction("Profile", "Client");
        }

        [Authorize(Roles = "client")]
        public IActionResult CreditPayments(string creditId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select money, date_time from creditpayments where credit_id = '{creditId}'";
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

        [HttpGet]
        [Authorize(Roles = "client")]
        public IActionResult Get(string? creditId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select balances.id, balances.name, balances.money, banks.name as bank from balances inner join " +
                $"banks on balances.bank_id = banks.id inner join users on balances.client_id = users.id where " +
                $"users.email = '{User.Identity!.Name}'";
            var dataReader = command.ExecuteReader();

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

            ViewBag.CreditId = creditId;

            var model = new GetCreditModel
            {
                Balances = balances
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "client")]
        public IActionResult Get(GetCreditModel model)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select * from credits where id = '{model.CreditId}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var credit = new Credit
            {
                Id = model.CreditId,
                Money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!),
                MoneyWithPercent = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money_with_percent")).ToString()!),
                MoneyPayed = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money_payed")).ToString()!),
                Percent = double.Parse(dataReader.GetValue(dataReader.GetOrdinal("percent")).ToString()!),
                CreationTime = DateTime.Parse(dataReader.GetValue(dataReader.GetOrdinal("creation_date")).ToString()!),
                PaymentTime = DateTime.Parse(dataReader.GetValue(dataReader.GetOrdinal("payment_date")).ToString()!),
                Approved = bool.Parse(dataReader.GetValue(dataReader.GetOrdinal("is_approved")).ToString()!)
            };

            command = DbConnection.getCommand();
            command.CommandText = $"select money from balances where id = '{model.BalanceId}'";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var balanceMoney = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!);

            balanceMoney += credit.Money;

            command = DbConnection.getCommand();
            command.CommandText = $"update credits set is_getted = true where id = '{model.CreditId}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"update balances set money = {balanceMoney.ToString().Replace(',', '.')} where id = '{model.BalanceId}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = (select bank_id from credits where id = '{credit.Id}')";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"select balances.name, banks.name as bank from balances inner join banks on banks.id = balances.bank_id " +
                $"where balances.id = '{model.BalanceId}'";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var balanceBankName = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!;
            var balanceName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Клиент {User.Identity.Name} получил сумму {credit.Money} за кредит в банке {bankName} на счет {balanceName} в банке {balanceBankName}.')";
            command.ExecuteReader();

            return RedirectToAction("Profile", "Client");
        }
    }
}
