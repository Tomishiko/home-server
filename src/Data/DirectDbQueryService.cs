using core.Domain;
using core.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
namespace Data.Core;

public class DirectDbQueryService : IDirectDbQueryService
{
    private readonly string _connectionString;

    public DirectDbQueryService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new Exception("Db connection string is not provided");
    }

    public ValueTask UpdateFileUploadState(ReadOnlySpan<FileStateBackupContext> batch)
    {
        if (batch.IsEmpty) return ValueTask.CompletedTask;

        using var conn = new NpgsqlConnection(_connectionString);
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
        catch(Exception ex)
        {
            transaction.Rollback();
            return ValueTask.FromException(ex);
        }


    }
}
