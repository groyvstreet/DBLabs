-- 1.
select * from balances where money > 11000 and client_id = (select id from users where user_name = 'kraken')
select * from deposits where percent > 5 and client_id = (select id from users where user_name = 'kraken') and bank_id = (select id from banks where name = 'CCG')
select * from transfers where from_balance_id =
	(select id from balances where name = 'balance1' and client_id =
	 	(select id from users where user_name = 'kraken')
    )

-- 2.
-- inner
select users.user_name, balances.name, balances.money
from balances
inner join users on balances.client_id = users.id

-- left
select users.user_name, balances.name, balances.money
from users
left join balances on users.id = balances.client_id

-- right
select users.user_name, balances.name, balances.money
from balances
right join users on balances.client_id = users.id

-- full
select users.user_name, balances.name, balances.money
from balances
full join users on balances.client_id = users.id

-- cross
select * from balances cross join users

-- self
select a.id, a.name, a.money 
from balances a, balances b 
where a.money < b.money and b.name = 'pokemon_balance'

-- 3.
-- group by
select count(client_id), bank_id
from bankclients
group by bank_id

-- having
select sum(money), client_id
from balances
group by client_id
having sum(money) > 46000

-- union
select date_time, money from creditpayments
union all
select date_time, money from depositreplenishments

-- 4.
-- exists
select user_name
from users
where exists (select id from balances where balances.client_id = users.id)

-- insert into select

-- case
select money,
case
	when money = 10000 then 'bad'
	when money = 11000 then 'not bad'
	when money = 12000 then 'good'
	when money = 13000 then 'better'
	when money = 20000 then 'the best'
	else 'undefined'
end as message
from balances

-- explain
explain select * from users where user_name = 'kraken'
