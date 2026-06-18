using System.Collections.Concurrent;
using core.DomainExceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;

namespace Data.Infra;

public class DatabaseExceptionInterceptor : SaveChangesInterceptor
{
    private static readonly ConcurrentDictionary<(string Table, string Key), string> _metadataCache = new();

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        EvaluateAndThrowException(eventData.Context, eventData.Exception);
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        EvaluateAndThrowException(eventData.Context, eventData.Exception);
        base.SaveChangesFailed(eventData);
    }

    private static void EvaluateAndThrowException(DbContext? context, Exception? exception)
    {
        if (context != null && exception is DbUpdateException dbEx && dbEx.InnerException is PostgresException pgEx)
        {
            throw pgEx.SqlState switch
            {

                "23505" => BuildUniqueViolationException(context, pgEx, dbEx),

                _ => new InfrastructureException($"Unhandled DB error ({pgEx.SqlState}): {pgEx.MessageText}", dbEx)
            };
        }
    }

    private static Exception BuildUniqueViolationException(DbContext context, PostgresException pgEx, DbUpdateException dbEx)
    {
        string propName = ResolveNameFromConstraint(context, pgEx.TableName, pgEx.ConstraintName);
        return new DuplicateException($"The value provided for '{propName}' already exists.", dbEx);
    }

    // --- METADATA RESOLVER ENGINE ---

    private static string ResolveNameFromColumn(DbContext context, string tableName, string columnName)
    {
        if (string.IsNullOrEmpty(columnName)) return "Unknown Field";

        return _metadataCache.GetOrAdd((tableName, columnName), key =>
        {
            var entityType = context.Model.GetEntityTypes().FirstOrDefault(e => e.GetTableName() == key.Table);
            if (entityType == null) return key.Key;

            var storeObject = StoreObjectIdentifier.Table(key.Table, entityType.GetSchema());
            var property = entityType.GetProperties().FirstOrDefault(p => p.GetColumnName(storeObject) == key.Key);

            return property?.Name ?? key.Key; // Returns "TokenHash"
        });
    }

    private static string ResolveNameFromConstraint(DbContext context, string tableName, string constraintName)
    {
        if (string.IsNullOrEmpty(constraintName)) return "Unknown Field";

        return _metadataCache.GetOrAdd((tableName, constraintName), key =>
        {
            var entityType = context.Model.GetEntityTypes().FirstOrDefault(e => e.GetTableName() == key.Table);
            if (entityType == null) return key.Key;

            // Look through unique indexes to match the Postgres constraint identifier
            var index = entityType.GetIndexes().FirstOrDefault(i => i.GetDatabaseName() == key.Key);
            if (index != null)
            {
                // Join properties in case it's a composite unique index (e.g., "TenantId, Email")
                return string.Join(", ", index.Properties.Select(p => p.Name)); // Returns "TokenHash"
            }

            return key.Key;
        });
    }
}
