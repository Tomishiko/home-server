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
    username varchar(256),
    eventname text,
    time timestamp
);

CREATE TABLE upload_history(
    file_name varchar(256) not null,
    uploader varchar(256) not null,
    date timestamp
);

CREATE TABLE files(
    id bigint not null primary key generated always as identity,
    uuid varchar(256) not null,
    name varchar(256) not null,
    size bigint,
    ext varchar(10) not null,
    owner_id bigint references users(id),
    shared boolean,
    is_deleted boolean

);

