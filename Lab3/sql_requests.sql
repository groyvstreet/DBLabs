CREATE DATABASE banksystemdb;

DROP TABLE IF EXISTS balances, bankclients, banks, blacklists, clients, creditpayments, credits,
depositreplenishments, deposits, managers, roles, transfers, users, blacklistclients CASCADE;

CREATE TABLE roles
(
	id uuid PRIMARY KEY,
	name varchar(50) NOT NULL
);

CREATE TABLE users
(
	id uuid PRIMARY KEY,
	user_name varchar(1000) NOT NULL,
	password varchar(1000) NOT NULL,
	email varchar(1000) NOT NULL,
	phone_number varchar(12) NOT NULL,
	first_name varchar(100) NOT NULL,
	last_name varchar(100) NOT NULL,
	patronimic varchar(100) NOT NULL,
	role_id uuid REFERENCES roles(id) NOT NULL
);

CREATE INDEX user_id_index ON users (id);

CREATE TABLE banks
(
	id uuid PRIMARY KEY,
	name varchar(100) NOT NULL,
	percent_for_deposit real NOT NULL,
	percent_for_credit real NOT NULL
);

CREATE TABLE managers
(
	id uuid PRIMARY KEY REFERENCES users(id),
	bank_id uuid REFERENCES banks(id) NOT NULL
);

CREATE TABLE blacklists
(
	id uuid PRIMARY KEY,
	bank_id uuid REFERENCES banks(id) NOT NULL UNIQUE
);

CREATE TABLE clients
(
	id uuid PRIMARY KEY REFERENCES users(id),
	passport_series varchar(50) NOT NULL,
	passport_number_series varchar(50) NOT NULL,
	passport_identification_number varchar(50) NOT NULL,
	is_approved boolean NOT NULL
);

CREATE TABLE blacklistclients
(
	black_list_id uuid REFERENCES blacklists(id) NOT NULL,
	client_id uuid REFERENCES clients(id) NOT NULL,
	blacklisting_date date NOT NULL,
	reason text NOT NULL
);

CREATE TABLE bankclients
(
	bank_id uuid REFERENCES banks(id) NOT NULL,
	client_id uuid REFERENCES clients(id) NOT NULL
);

CREATE TABLE balances
(
    id uuid PRIMARY KEY,
    name varchar(100) NOT NULL,
    money decimal NOT NULL,
    client_id uuid REFERENCES clients(id) NOT NULL,
    bank_id uuid REFERENCES banks(id) NOT NULL
);

CREATE INDEX client_id_index1 ON balances (client_id);

CREATE TABLE transfers
(
    id uuid PRIMARY KEY,
    money decimal NOT NULL,
    date_time timestamp NOT NULL,
    from_balance_id uuid REFERENCES balances(id) NOT NULL,
    to_balance_id uuid REFERENCES balances(id) NOT NULL
);

CREATE INDEX from_balance_id_index ON transfers (from_balance_id);
CREATE INDEX to_balance_id_index ON transfers (to_balance_id);

CREATE TABLE deposits
(
    id uuid PRIMARY KEY,
    money decimal NOT NULL,
    percent real NOT NULL,
    creation_date date NOT NULL,
    is_blocked boolean NOT NULL,
    is_freezed boolean NOT NULL,
    client_id uuid REFERENCES clients(id) NOT NULL,
    bank_id uuid REFERENCES banks(id) NOT NULL
);

CREATE INDEX client_id_index2 ON deposits (client_id);

CREATE TABLE credits
(
    id uuid PRIMARY KEY,
    money decimal NOT NULL,
    percent real NOT NULL,
    money_with_percent decimal NOT NULL,
    money_payed decimal NOT NULL,
    creation_date date NOT NULL,
    payment_date date NOT NULL,
    is_approved boolean NOT NULL,
    client_id uuid REFERENCES clients(id) NOT NULL,
    bank_id uuid REFERENCES banks(id) NOT NULL
);

CREATE INDEX client_id_index3 ON credits (client_id);

CREATE TABLE creditpayments
(
    id uuid PRIMARY KEY,
    money decimal NOT NULL,
    date_time timestamp NOT NULL,
    credit_id uuid REFERENCES credits(id) NOT NULL
);

CREATE INDEX credit_id_index ON creditpayments (credit_id);

CREATE TABLE depositreplenishments
(
    id uuid PRIMARY KEY,
    money decimal NOT NULL,
    date_time timestamp NOT NULL,
    deposit_id uuid REFERENCES deposits(id) NOT NULL
);

CREATE INDEX deposit_id_index ON depositreplenishments (deposit_id);

DELETE FROM roles WHERE name = 'admin';
DELETE FROM roles WHERE name = 'moderator';
DELETE FROM roles WHERE name = 'manager';
DELETE FROM roles WHERE name = 'client';

INSERT INTO roles VALUES (gen_random_uuid(), 'admin');
INSERT INTO roles VALUES (gen_random_uuid(), 'moderator');
INSERT INTO roles VALUES (gen_random_uuid(), 'manager');
INSERT INTO roles VALUES (gen_random_uuid(), 'client');

INSERT INTO users VALUES(gen_random_uuid(),
						'user_admin',
						'123123',
						'user_admin@gmail.com',
						'375291234567',
						'Shadow',
						'Fiend',
						'ZXC',
						(SELECT id FROM roles WHERE name = 'admin'));

SELECT name FROM roles WHERE id = (SELECT role_id FROM users WHERE first_name = 'Shadow');

INSERT INTO users VALUES(gen_random_uuid(),
						'user_admin2',
						'456456',
						'user_admin2@gmail.com',
						'375299876543',
						'Shadow',
						'Raze',
						'QWE',
						(SELECT id FROM roles WHERE name = 'admin'));

SELECT * FROM users;

INSERT INTO banks VALUES(gen_random_uuid(),
                        'CCG',
                        2.0,
                        3.0);

INSERT INTO banks VALUES(gen_random_uuid(),
                        'BAKUGAN',
                        6.0,
                        7.0);

SELECT * FROM banks;

INSERT INTO users VALUES(gen_random_uuid(),
						'user_manager',
						'!!!!!!',
						'user_manager@gmail.com',
						'xxxxxxxxxx',
						'Anti',
						'Mage',
						'Manta',
						(SELECT id FROM roles WHERE name = 'manager'));

INSERT INTO users VALUES(gen_random_uuid(),
						'user_manager2',
						'******',
						'user_manager2@gmail.com',
						'____________',
						'Night',
						'Stalker',
						'NORM',
						(SELECT id FROM roles WHERE name = 'manager'));

INSERT INTO managers VALUES((SELECT id FROM users WHERE user_name = 'user_manager'),
						(SELECT id FROM banks WHERE name = 'CCG'));

INSERT INTO managers VALUES((SELECT id FROM users WHERE user_name = 'user_manager2'),
						(SELECT id FROM banks WHERE name = 'BAKUGAN'));

INSERT INTO users VALUES(gen_random_uuid(),
						'kraken',
						'kachestvennyi',
						'kraken@gmail.com',
						'-----------',
						'A',
						'B',
						'C',
						(SELECT id FROM roles WHERE name = 'client'));

INSERT INTO clients VALUES((SELECT id FROM users WHERE user_name = 'kraken'),
						'avbjad',
						'pmwby9',
						'2b09[tp',
						true);

INSERT INTO users VALUES(gen_random_uuid(),
						'pokemon',
						'kachestvennyi',
						'pokemon@gmail.com',
						'123670',
						'Chimchar',
						'R',
						'A',
						(SELECT id FROM roles WHERE name = 'client'));

INSERT INTO clients VALUES((SELECT id FROM users WHERE user_name = 'pokemon'),
						'qwe',
						'gdf',
						'w[tp',
						true);

INSERT INTO users VALUES(gen_random_uuid(),
						'drago',
						'vfrtgb_+-=',
						'drago@gmail.com',
						'123670',
						'Chimchar',
						'R',
						'A',
						(SELECT id FROM roles WHERE name = 'client'));

INSERT INTO clients VALUES((SELECT id FROM users WHERE user_name = 'drago'),
						'qwe',
						'gdf',
						'w[tp',
						false);

INSERT INTO bankclients VALUES((SELECT id FROM banks WHERE name = 'CCG'),
								(SELECT id FROM users WHERE user_name = 'kraken'));

INSERT INTO bankclients VALUES((SELECT id FROM banks WHERE name = 'BAKUGAN'),
								(SELECT id FROM users WHERE user_name = 'kraken'));

INSERT INTO bankclients VALUES((SELECT id FROM banks WHERE name = 'CCG'),
								(SELECT id FROM users WHERE user_name = 'pokemon'));

INSERT INTO bankclients VALUES((SELECT id FROM banks WHERE name = 'BAKUGAN'),
								(SELECT id FROM users WHERE user_name = 'pokemon'));

INSERT INTO balances VALUES(gen_random_uuid(),
							'kraken_balance',
							10000,
							(SELECT id FROM users WHERE user_name = 'kraken'),
							(SELECT id FROM banks WHERE name = 'CCG'));

INSERT INTO balances VALUES(gen_random_uuid(),
							'pokemon_balance',
							20000,
							(SELECT id FROM users WHERE user_name = 'pokemon'),
							(SELECT id FROM banks WHERE name = 'BAKUGAN'));

INSERT INTO credits VALUES(gen_random_uuid(),
							1000000,
							10,
							1100000,
							0,
							'2021-01-02',
							'2022-01-02',
							true,
							(SELECT id FROM users WHERE user_name = 'kraken'),
							(SELECT id FROM banks WHERE name = 'CCG'));

INSERT INTO credits VALUES(gen_random_uuid(),
							222222,
							20,
							266666.4,
							222,
							'2021-01-02',
							'2022-01-02',
							true,
							(SELECT id FROM users WHERE user_name = 'pokemon'),
							(SELECT id FROM banks WHERE name = 'BAKUGAN'));

INSERT INTO deposits VALUES(gen_random_uuid(),
							999999,
							11,
							'2021-01-02',
							false,
							false,
							(SELECT id FROM users WHERE user_name = 'kraken'),
							(SELECT id FROM banks WHERE name = 'CCG'));

INSERT INTO deposits VALUES(gen_random_uuid(),
							123123,
							5,
							'2021-01-02',
							false,
							false,
							(SELECT id FROM users WHERE user_name = 'pokemon'),
							(SELECT id FROM banks WHERE name = 'BAKUGAN'));

INSERT INTO transfers VALUES(gen_random_uuid(),
							500,
							'2021-01-02 02:06:08',
							(SELECT id FROM balances WHERE name = 'kraken_balance'),
							(SELECT id FROM balances WHERE name = 'pokemon_balance'));

INSERT INTO creditpayments VALUES(gen_random_uuid(),
									222,
									'2021-01-02 02:06:08',
									(SELECT id FROM credits WHERE money = 222222));

INSERT INTO depositreplenishments VALUES(gen_random_uuid(),
										9000,
										'2021-01-02 02:06:08',
										(SELECT id FROM deposits WHERE money = 123123));

INSERT INTO blacklists VALUES(gen_random_uuid(),
								(SELECT id FROM banks WHERE name = 'CCG'));

INSERT INTO blacklistclients VALUES((SELECT id FROM blacklists WHERE bank_id = (SELECT id FROM banks WHERE name = 'CCG')),
									(SELECT id FROM users WHERE user_name = 'kraken'),
									'2024-05-05',
									'Не оплатил кредит, в бан его');
