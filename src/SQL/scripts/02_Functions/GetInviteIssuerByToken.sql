CREATE OR REPLACE FUNCTION GetInviteIssuerByToken(IN token BYTEA)
RETURNS SETOF users
LANGUAGE sql
STABLE
AS $$
    SELECT * FROM users
    WHERE users.id = (SELECT invites.created_by FROM invites
                      WHERE invites.token_hash = token
                      AND invites.expires_at > now());

$$;
