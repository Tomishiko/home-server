DROP TABLE IF exists users;

CREATE TABLE users(
    id bigint not null generated always as identity primary key,
    uname varchar(255) not null,
    password varchar(255) not null,
    email varchar(255),
    role_id bigint references roles (id)
);
