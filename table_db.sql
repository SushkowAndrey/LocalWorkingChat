--таблица пользователей чата
create table table_users
(
    id  varchar(50) primary key,
    name_user varchar(150) not null unique,
	password   varchar(100)    not null,
	is_active   varchar(10)  DEFAULT 'true',
	is_admin VARCHAR(10) not null DEFAULT 'false'
);

--таблица пользователей онлайн
create table table_users_online
(
	users_id  varchar(50)  primary key,
	FOREIGN KEY (users_id)  REFERENCES table_users (id) ON DELETE RESTRICT,
	date_time   DATETIME  not null
);

--таблица сообщений
create table table_message
(
    id  varchar(50) primary key,
	text_message   text,
	users_id_sender  varchar(50),
	FOREIGN KEY (users_id_sender)  REFERENCES table_users (id) ON DELETE RESTRICT,
	users_id_recipient  varchar(50),
	FOREIGN KEY (users_id_recipient)  REFERENCES table_users (id) ON DELETE RESTRICT,
	date_time   DATETIME   not null,
	type_message  varchar(50),
);

--прикрепленные файлы
create table information_register_message_attached_files
(
    id int auto_increment primary key,
 	name_file   VARCHAR(250)          not null,
	file_pdf MEDIUMBLOB not null,
	id_message    VARCHAR(50) not null,
	FOREIGN KEY (id_message)  REFERENCES table_message (id) ON DELETE CASCADE
); 