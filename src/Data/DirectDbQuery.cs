using core.Domain;
using core.Interfaces;
using Npgsql;
using NpgsqlTypes;

namespace Data.Core;

///<inheritdoc/>
public class DirectDbQuery : IDirectDbQuery
{
    private readonly NpgsqlDataSource _source;


    public DirectDbQuery(NpgsqlDataSource source)
    {
        _source = source;
    }

    public async Task<int> UpdateFileUploadState(ReadOnlyMemory<FileUploadStateBackupContext> batch,
                                            CancellationToken ct)
    {
        if (batch.IsEmpty)
        {
            return 0;
        }

        var length = batch.Length;


        var ids = new Guid[length];
        var bitfields = new int[length];
        var partsWritten = new int[length];

        var span = batch.Span;
        for (int i = 0; i < length; i++)
        {
            ids[i] = span[i].Id;
            bitfields[i] = (int)span[i].Bitfield;
            partsWritten[i] = span[i].PartsWritten;
        }



        const string sql = "SELECT batch_update_file_upload_state($1, $2, $3);";
        await using var cmd = _source.CreateCommand(sql);

        cmd.Parameters.AddWithValue(ids);
        cmd.Parameters.AddWithValue(bitfields);
        cmd.Parameters.AddWithValue(partsWritten);


        var result = await cmd.ExecuteScalarAsync(ct);

        return result is int count ? count : 0;


    }







    [Obsolete("Used with old parts bitfield tracking", true)]
    public ValueTask UpdateFileUploadStateLegacy(ReadOnlySpan<FileStateBackupContextLegacy> batch)
    {
        if (batch.IsEmpty) return ValueTask.CompletedTask;

        using var conn = new NpgsqlConnection();
        conn.Open();

        using var transaction = conn.BeginTransaction();
        try
        {
            using (var tempTableCmd = new NpgsqlCommand("CREATE TEMP TABLE temp_file_updates (id UUID, parts INT, bits BYTEA) ON COMMIT DROP",
                                                        conn,
                                                        transaction))
            {

                tempTableCmd.ExecuteNonQuery();
            }

            const string copySQL = "COPY temp_file_updates (id, parts, bits) FROM STDIN (FORMAT BINARY)";
            using (var writer = conn.BeginBinaryImport(copySQL))
            {
                for (int i = 0; i < batch.Length; i++)
                {
                    writer.StartRow();
                    writer.Write(batch[i].Id, NpgsqlDbType.Uuid);
                    writer.Write(batch[i].PartsWritten, NpgsqlDbType.Integer);
                    writer.Write(batch[i].Bitfield, NpgsqlDbType.Bytea);
                }
                writer.Complete();
            }

            const string updateSql = @"
                UPDATE file_upload_state
                SET parts_written = t.parts,
                    parts_bitfield = t.bits
                FROM temp_file_updates t
                WHERE file_upload_state.id = t.id";
            using (var updateCmd = new NpgsqlCommand(updateSql, conn, transaction))
            {
                updateCmd.ExecuteNonQuery();
            }

            transaction.Commit();
            return ValueTask.CompletedTask;

        }
        catch (Exception ex)
        {
            transaction.Rollback();
            return ValueTask.FromException(ex);
        }


    }

}
