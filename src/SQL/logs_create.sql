DROP TABLE IF exists logs;
CREATE TABLE logs(
    id bigint not null primary key generated always as identity,
    username varchar(255),
    eventname text,
    time timestamptz
);
