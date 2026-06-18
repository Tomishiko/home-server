CREATE OR REPLACE VIEW v_valid_token_details

AS

SELECT
    invites.id AS token_id,
    users.id AS issuer_id,
    users.uname AS issuer_name,
    invites.token_hash

FROM invites

INNER JOIN users ON users.id = invites.created_by

WHERE invites.expires_at > now()
  AND invites.used_at IS NULL
  AND users.role_id = 2; --Manager role id = 2
