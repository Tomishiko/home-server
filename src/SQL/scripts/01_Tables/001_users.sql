DROP TABLE IF exists users;

CREATE TABLE users (
    id BIGINT NOT NULL GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    uname VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,        -- Stores hashed pwds
    email VARCHAR(255) NOT NULL UNIQUE,
    role_id INT REFERENCES roles(id) DEFAULT 1
);
