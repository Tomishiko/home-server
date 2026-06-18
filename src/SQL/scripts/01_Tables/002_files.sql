DROP TABLE IF exists files;

CREATE TABLE files(
    id bigint not null primary key generated always as identity,
    uuid varchar(255) not null,
    name varchar(255) not null,
    size bigint,
    ext varchar(10) not null,
    owner_id bigint references users(id) on delete set NULL,
    shared boolean NOT NULL DEFAULT false,
    is_deleted boolean NOT NULL DEFAULT false

);
CREATE INDEX idx_files_owner_id ON files(owner_id);
