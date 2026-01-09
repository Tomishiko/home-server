DROP TABLE IF EXISTS invites;
-- Enable UUID generation if not already

-- Table: invites
CREATE TABLE invites (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    token_hash BYTEA NOT NULL UNIQUE,
    role VARCHAR(50) NULL,
    created_by BIGINT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    expires_at TIMESTAMPTZ NOT NULL,
    used_at TIMESTAMPTZ NULL,
    used_by BIGINT NULL
);

CREATE UNIQUE INDEX idx_invites_token_hash ON invites(token_hash)
WHERE used_at IS NULL;

CREATE INDEX idx_invites_expires_at ON invites(expires_at);
