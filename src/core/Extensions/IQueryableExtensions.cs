using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace core.Extensions;

public static class IQueryableExtensions
{

    public static async Task<ImmutableArray<TSource>> ToImmutableArrayAsync<TSource>(
       [NotNull] this IQueryable<TSource> source,
       int initialCapacity = 10,
       CancellationToken cancellationToken = default)
    {
        var builder = ImmutableArray.CreateBuilder<TSource>(initialCapacity);

        await foreach (var element in source.AsAsyncEnumerable()
                                            .WithCancellation(cancellationToken))
        {

            builder.Add(element);
        }
        if(builder.Capacity == builder.Count)
        {
            return builder.MoveToImmutable();
        }

        return builder.ToImmutable();
    }

}
