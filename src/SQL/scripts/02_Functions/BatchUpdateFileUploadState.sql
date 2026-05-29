CREATE OR REPLACE FUNCTION batch_update_file_upload_state(
    p_ids UUID[],
    p_bitfields INT[],
    p_writtens INT[]
)
RETURNS INT AS $$
DECLARE
    updated_count INTEGER;
BEGIN
    UPDATE file_upload_state AS f
    SET parts_bitfield = u.new_bitfield,
        parts_written = u.new_written
    FROM UNNEST(p_ids, p_bitfields, p_writtens) AS u(id, new_bitfield, new_written)
    WHERE f.id = u.id;

    GET DIAGNOSTICS updated_count = ROW_COUNT;
    RETURN updated_count;
END;
$$ LANGUAGE plpgsql;
