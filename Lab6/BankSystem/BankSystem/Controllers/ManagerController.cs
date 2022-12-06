using BankSystem.Data;
using BankSystem.Models.BankModels;
using BankSystem.Models.Entities;
using BankSystem.Models.ManagerModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DbConnection = BankSystem.Data.DbConnection;

namespace BankSystem.Controllers
{
    public class ManagerController : Controller
    {
        [Authorize(Roles = "manager")]
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

        [Authorize(Roles = "manager")]
        public IActionResult Bank()
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select banks.id, banks.name, banks.percent_for_deposit, banks.percent_for_credit " +
                $"from managers inner join banks on banks.id = managers.bank_id where managers.id = " +
                $"(select id from users where email = '{User.Identity!.Name}')";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var model = new BankDetailModel
            {
                Id = dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!,
                Name = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!,
                DepositPercent = dataReader.GetValue(dataReader.GetOrdinal("percent_for_deposit")).ToString()!,
                CreditPercent = dataReader.GetValue(dataReader.GetOrdinal("percent_for_credit")).ToString()!,
            };

            return View(model);
        }

        [Authorize(Roles = "manager")]
        public IActionResult Clients()
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select users.id, users.first_name, users.last_name, users.patronimic, users.phone_number, " +
                $"clients.passport_series, clients.passport_number_series, clients.passport_identification_number " +
                $"from bankclients inner join clients on clients.id = bankclients.client_id inner join users on " +
                $"users.id = clients.id where bankclients.bank_id = (select id from banks where id = " +
                $"(select bank_id from managers where id = (select id from users where email = '{User.Identity!.Name}')))";
            var dataReader = command.ExecuteReader();

            var clients = new List<Client>();

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

                var command2 = DbConnection.getCommand();
                command2.CommandText = $"select client_id from blacklistclients where client_id = '{client.Id}' and " +
                    $"black_list_id = (select id from blacklists where bank_id = " +
                    $"(select bank_id from managers where id = " +
                    $"(select id from users where email = '{User.Identity!.Name}')))";
                var dataReader2 = command2.ExecuteReader();

                if (dataReader2.Read())
                {
                    client.InBlackList = true;
                }
                else
                {
                    client.InBlackList = false;
                }

                clients.Add(client);
            }

            return View(clients);
        }

        [Authorize(Roles = "manager")]
        public IActionResult Transfers(string clientId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select balances.name, users.email, banks.name as bank, transfers.money, " +
                $"transfers.date_time, transfers.to_balance_id from transfers " +
                $"left join balances on balances.id = transfers.from_balance_id " +
                $"left join users on users.id = balances.client_id " +
                $"left join banks on banks.id = balances.bank_id " +
                $"where balances.client_id = '{clientId}'";
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
                $"where balances.client_id = '{clientId}'";
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

        [Authorize(Roles = "manager")]
        public IActionResult Credits(string clientId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select * from credits where client_id = '{clientId}' and bank_id = " +
                $"(select id from banks where id = (select bank_id from managers where id = " +
                $"(select id from users where email = '{User.Identity!.Name}')))";
            var dataReader = command.ExecuteReader();

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
                    Approved = bool.Parse(dataReader.GetValue(dataReader.GetOrdinal("is_approved")).ToString()!)
                };

                credits.Add(credit);
            }

            return View(credits);
        }

        [Authorize(Roles = "manager")]
        public IActionResult ApproveCredit(string? creditId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select creation_date, payment_date from credits where id = '{creditId}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var payment = DateTime.Parse(dataReader.GetValue(dataReader.GetOrdinal("payment_date")).ToString()!);
            var creation = DateTime.Parse(dataReader.GetValue(dataReader.GetOrdinal("creation_date")).ToString()!);

            var months = (payment.Year - creation.Year) * 12 + (payment.Month - creation.Month);

            var now = DateTime.Now;

            command = DbConnection.getCommand();
            command.CommandText = $"update credits set is_approved = true, " +
                $"creation_date = '{now.Date}', " +
                $"payment_date = '{now.AddMonths(months).Date}' " +
                $"where id = '{creditId}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select email from users where id = (select client_id from credits where id = '{creditId}')";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var clientEmail = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = (select bank_id from credits where id = '{creditId}')";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Менеджер {User.Identity.Name} подтвердил заявку клиента {clientEmail} на кредит в банке {bankName}.')";
            command.ExecuteReader();

            return RedirectToAction("Clients", "Manager");
        }

        [Authorize(Roles = "manager")]
        public IActionResult RejectCredit(string? creditId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select email from users where id = (select client_id from credits where id = '{creditId}')";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var clientEmail = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = (select bank_id from credits where id = '{creditId}')";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Менеджер {User.Identity.Name} отклонил заявку клиента {clientEmail} на кредит в банке {bankName}.')";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"delete from credits where id = '{creditId}'";
            command.ExecuteReader();

            return RedirectToAction("Clients", "Manager");
        }

        [Authorize(Roles = "manager")]
        public IActionResult Balances(string clientId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select balances.id, balances.money, balances.name, banks.name as bank from balances inner join " +
                $"banks on balances.bank_id = banks.id where balances.client_id = '{clientId}' and balances.bank_id = " +
                $"(select id from banks where id = (select bank_id from managers where id = (select id from users where email = '{User.Identity!.Name}')))";
            var dataReader = command.ExecuteReader();

            var balances = new List<Balance>();

            while (dataReader.Read())
            {
                var balance = new Balance
                {
                    Id = dataReader.GetValue(dataReader.GetOrdinal("id")).ToString()!,
                    Money = decimal.Parse(dataReader.GetValue(dataReader.GetOrdinal("money")).ToString()!),
                    Bank = dataReader.GetValue(dataReader.GetOrdinal("bank")).ToString()!,
                    Name = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!
                };

                balances.Add(balance);
            }

            return View(balances);
        }

        [Authorize(Roles = "manager")]
        public IActionResult Deposits(string clientId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"select deposits.id, deposits.money, deposits.percent, deposits.creation_date, " +
                $"deposits.is_blocked, deposits.is_freezed, banks.name as bank from deposits inner join " +
                $"banks on deposits.bank_id = banks.id where deposits.client_id = '{clientId}' and deposits.bank_id = " +
                $"(select id from banks where id = (select bank_id from managers where id = (select id from users where email = '{User.Identity!.Name}')))";
            var dataReader = command.ExecuteReader();

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

            return View(deposits);
        }

        [Authorize(Roles = "manager")]
        public IActionResult Block(string? depositId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"update deposits set is_blocked = true where id = '{depositId}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select email from users where id = (select client_id from deposits where id = '{depositId}')";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var clientEmail = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = (select bank_id from deposits where id = '{depositId}')";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Менеджер {User.Identity.Name} заблокировал вклад клиента {clientEmail} в банке {bankName}.')";
            command.ExecuteReader();

            return RedirectToAction("Clients", "Manager");
        }

        [Authorize(Roles = "manager")]
        public IActionResult Unblock(string? depositId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"update deposits set is_blocked = false where id = '{depositId}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select email from users where id = (select client_id from deposits where id = '{depositId}')";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var clientEmail = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = (select bank_id from deposits where id = '{depositId}')";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Менеджер {User.Identity.Name} разблокировал вклад клиента {clientEmail} в банке {bankName}.')";
            command.ExecuteReader();

            return RedirectToAction("Clients", "Manager");
        }

        [Authorize(Roles = "manager")]
        public IActionResult Freeze(string? depositId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"update deposits set is_freezed = true where id = '{depositId}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select email from users where id = (select client_id from deposits where id = '{depositId}')";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var clientEmail = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = (select bank_id from deposits where id = '{depositId}')";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Менеджер {User.Identity.Name} заморозил вклад клиента {clientEmail} в банке {bankName}.')";
            command.ExecuteReader();

            return RedirectToAction("Clients", "Manager");
        }

        [Authorize(Roles = "manager")]
        public IActionResult Unfreeze(string? depositId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"update deposits set is_freezed = false where id = '{depositId}'";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select email from users where id = (select client_id from deposits where id = '{depositId}')";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var clientEmail = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = (select bank_id from deposits where id = '{depositId}')";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Менеджер {User.Identity.Name} разморозил вклад клиента {clientEmail} в банке {bankName}.')";
            command.ExecuteReader();

            return RedirectToAction("Clients", "Manager");
        }

        [Authorize(Roles = "manager")]
        public IActionResult InsertToBlackList(string? clientId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"insert into blacklistclients values(" +
                $"(select id from blacklists where bank_id = " +
                $"(select bank_id from managers where id = (select id from users where email = '{User.Identity!.Name}'))), " +
                $"'{clientId}', '{DateTime.Now.Date}', 'Просрочен кредит')";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select email from users where id = '{clientId}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var clientEmail = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = (select bank_id from managers where id = (select id from users where email = '{User.Identity.Name}'))";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Менеджер {User.Identity.Name} поместил клиента {clientEmail} в черный список банка {bankName}.')";
            command.ExecuteReader();

            return RedirectToAction("Clients", "Manager");
        }

        [Authorize(Roles = "manager")]
        public IActionResult DeleteFromBlackList(string? clientId)
        {
            var command = DbConnection.getCommand();
            command.CommandText = $"delete from blacklistclients where client_id = '{clientId}' and " +
                    $"black_list_id = (select id from blacklists where bank_id = " +
                    $"(select bank_id from managers where id = " +
                    $"(select id from users where email = '{User.Identity!.Name}')))";
            command.ExecuteReader();

            command = DbConnection.getCommand();
            command.CommandText = $"select email from users where id = '{clientId}'";
            var dataReader = command.ExecuteReader();
            dataReader.Read();

            var clientEmail = dataReader.GetValue(dataReader.GetOrdinal("email")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"select name from banks where id = (select bank_id from managers where id = (select id from users where email = '{User.Identity.Name}'))";
            dataReader = command.ExecuteReader();
            dataReader.Read();

            var bankName = dataReader.GetValue(dataReader.GetOrdinal("name")).ToString()!;

            command = DbConnection.getCommand();
            command.CommandText = $"insert into logs values('{Guid.NewGuid()}', '{DateTime.Now}', " +
                $"'Менеджер {User.Identity.Name} убрал клиента {clientEmail} из черного списка банка {bankName}.')";
            command.ExecuteReader();

            return RedirectToAction("Clients", "Manager");
        }
    }
}
