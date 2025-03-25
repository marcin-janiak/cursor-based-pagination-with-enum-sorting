using System.Reflection;
using HotChocolate.Internal;
using HotChocolate.Types.Pagination;

namespace Host;

public class CustomPagingProvider : CursorPagingProvider
{
    private static readonly MethodInfo _createHandler =
        typeof(CustomPagingProvider).GetMethod(
            nameof(CreateHandlerInternal),
            BindingFlags.Static | BindingFlags.NonPublic)!;

    public override bool CanHandle(IExtendedType source)
        => source.IsArrayOrList;

    protected override CursorPagingHandler CreateHandler(
        IExtendedType source,
        PagingOptions options)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        return (CursorPagingHandler)_createHandler
            .MakeGenericMethod(source.ElementType?.Source ?? source.Source)
            .Invoke(null, [options,])!;
    }

    private static CustomCursorPagingHandler<TEntity> CreateHandlerInternal<TEntity>(
        PagingOptions options)
        => new(options);
}