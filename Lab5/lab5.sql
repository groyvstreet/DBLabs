-- update credit
create or replace function add_credit_payment() returns trigger language plpgsql as $$
declare
	money decimal;
begin
    money = NEW.money_payed - OLD.money_payed;
    insert into creditpayments values (gen_random_uuid(), money, NOW()::timestamp, OLD.id);

    return NEW;
end;
$$

create trigger trigger_credit
after update on credits for each row execute procedure add_credit_payment();

-- update deposit
create or replace function add_deposit_replenishment() returns trigger language plpgsql as $$
declare
	money decimal;
begin
    money = NEW.money - OLD.money;
    insert into depositreplenishments values (gen_random_uuid(), money, NOW()::timestamp, OLD.id);

    return NEW;
end;
$$

create trigger trigger_deposit
after update on deposits for each row execute procedure add_deposit_replenishment();

-- insert transfer
create or replace function update_balances() returns trigger language plpgsql as $$
declare
	transfer_money decimal;
	from_balance_money decimal;
	to_balance_money decimal;
begin
    transfer_money = NEW.money;
	from_balance_money = (select balances.money from balances where id = NEW.from_balance_id);
	to_balance_money = (select balances.money from balances where id = NEW.to_balance_id);

    if from_balance_money >= transfer_money then
        from_balance_money = from_balance_money - transfer_money;
		update balances set money = from_balance_money where id = NEW.from_balance_id;
        to_balance_money = to_balance_money + transfer_money;
		update balances set money = to_balance_money where id = NEW.to_balance_id;
    	return NEW;
	end if;

    delete from transfers where id = NEW.id;
    return OLD;
end;
$$

create trigger trigger_transfer
after insert on transfers for each row execute procedure update_balances();

-- procedure add transfer
create or replace procedure add_transfer(money decimal, from_user_name varchar(1000), from_balance_name varchar(100), to_user_name varchar(1000), to_balance_name varchar(100)) language plpgsql
as $$
declare
    from_balance_id uuid;
    to_balance_id uuid;
begin
    from_balance_id = (select id from balances where name = from_balance_name and client_id =
        (select id from users where user_name = from_user_name));

    to_balance_id = (select id from balances where name = to_balance_name and client_id =
        (select id from users where user_name = to_user_name));

    insert into transfers values(gen_random_uuid(), money, NOW()::timestamp, from_balance_id, to_balance_id);
end;
$$
