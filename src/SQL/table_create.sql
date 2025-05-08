DROP TABLE IF exists logs;
DROP TABLE IF exists files;
DROP TABLE IF exists users;
DROP TABLE IF exists roles;

CREATE TABLE roles(
    id bigint not null primary key generated always as identity,
    name varchar(256) not null
);

CREATE TABLE users(
    id bigint not null generated always as identity primary key,
    uname varchar(256) not null,
    password varchar(256) not null,
    role_id bigint references roles (id)
);

CREATE TABLE logs(
    id bigint not null primary key generated always as identity,
    user_id bigint references users (id),
    username varchar(256),
    eventname text,
    time timestamp
);


CREATE TABLE files(
    id bigint not null primary key generated always as identity,
    name varchar(256) not null,
    location varchar(256) not null,
    size bigint,
    ext varchar(10) not null,
    owner bigint references users(id)

);

