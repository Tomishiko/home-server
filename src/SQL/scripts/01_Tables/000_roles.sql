DROP TABLE IF exists roles;

CREATE TABLE roles(
    id bigint not null primary key generated always as identity,
    name varchar(255) not null
);
