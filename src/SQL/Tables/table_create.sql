DROP TABLE IF exists logs;
DROP TABLE IF exists files;
DROP TABLE IF exists users;
DROP TABLE IF exists roles;

CREATE TABLE roles(
    id bigint not null primary key generated always as identity,
    name varchar(255) not null
);

CREATE TABLE users(
    id bigint not null generated always as identity primary key,
    uname varchar(255) not null,
    password varchar(255) not null,
    email varchar(255),
    role_id bigint references roles (id)
);

CREATE TABLE logs(
    id bigint not null primary key generated always as identity,
    username varchar(255),
    eventname text,
    time timestamptz
);


CREATE TABLE files(
    id bigint not null primary key generated always as identity,
    uuid varchar(255) not null,
    name varchar(255) not null,
    size bigint,
    ext varchar(10) not null,
    owner_id bigint references users(id) on delete set NULL,
    shared boolean,
    is_deleted boolean

);
CREATE INDEX idx_files_owner_id ON files(owner_id);
