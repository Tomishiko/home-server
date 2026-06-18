CREATE OR REPLACE FUNCTION remove_user_by_id(p_user_id bigint, p_issuer_name varchar(255))
RETURNS users
LANGUAGE plpgsql
AS $$
DECLARE
    v_user_row users%ROWTYPE;
BEGIN
    DELETE FROM users
    WHERE id = p_user_id
    RETURNING * INTO v_user_row;

    IF NOT FOUND THEN
        RAISE EXCEPTION 'User with ID % not found', p_user_id;
    END IF;

    INSERT INTO logs(username, event_name, created_at)
    VALUES (
        p_issuer_name,
        format('Deleted user: %s (Email: %s)', v_user_row.uname, v_user_row.email),
        CURRENT_TIMESTAMP
    );

    RETURN v_user_row;
END
$$;
