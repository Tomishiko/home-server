DROP TABLE IF EXISTS file_upload_state;

CREATE TABLE file_upload_state (
    id UUID PRIMARY KEY,
    fingerprint BYTEA NOT NULL,
    parts_bitfield BYTEA,
    parts_written INTEGER NOT NULL,
    metadata JSONB NOT NULL
);
CREATE UNIQUE INDEX idx_file_hash ON file_upload_state(fingerprint);
