## HotChocolate cursor pagination with enum sorting repro

This issue arises in cursor pagination registered via **.AddDbContextCursorPagingProvider()**, specifically when dealing with sorting by enums.

It applies to cursor pagination registered with **.AddDbContextCursorPagingProvider()**

### Issue

Since an **ICursorKeySerializer** for enums does not exist, we created a custom implementation:
[SomeEnumCursorKeySerializer](https://github.com/marcin-janiak/cursor-based-pagination-with-enum-sorting/blob/main/Host/SomeEnumCursorKeySerializer.cs)

However, after performing a paginated request using the "after" variable, we encounter the following error:
```
Expression of type Host.SomeEnum cannot be used for parameter of type System.Object of method Int32 CompareTo(System.Object) (Parameter arg0).
```
Stack Trace:

`` at System.Dynamic.Utils.ExpressionUtils.ValidateOneArgument(MethodBase method, ExpressionType nodeKind, Expression arguments, ParameterInfo pi, String methodParamName, String argumentParamName, Int32 index)\r\n
at System.Linq.Expressions.Expression.Call(Expression instance, MethodInfo method, Expression arg0)\r\n
at System.Linq.Expressions.Expression.Call(Expression instance, MethodInfo method, IEnumerable`1 arguments)\r\n
at GreenDonut.Data.Expressions.ExpressionHelpers.BuildWhereExpression[T](ReadOnlySpan`1 keys, Cursor cursor, Boolean forward)\r\n
at HotChocolate.Data.Pagination.EfQueryableCursorPagingHandler`1.SliceAsync(IResolverContext context, IQueryableExecutable`1 executable, CursorPagingArguments arguments)\r\n
at HotChocolate.Types.Pagination.CursorPagingHandler.HotChocolate.Types.Pagination.IPagingHandler.SliceAsync(IResolverContext context, Object source)\r\n
at HotChocolate.Types.Pagination.PagingMiddleware.InvokeAsync(IMiddlewareContext context)\r\n
at HotChocolate.Execution.Processing.Tasks.ResolverTask.ExecuteResolverPipelineAsync(CancellationToken cancellationToken)\r\n
at HotChocolate.Execution.Processing.Tasks.ResolverTask.TryExecuteAsync(CancellationToken cancellationToken)"``

## Root Cause
This occurs because enums do not implement IComparable<T>, making them incompatible with database-side sorting.

## Attempts to Fix
We modified:
- CursorPagingHandler
- ExpressionsHelper
- EfQueryableCursorPagingProvider

to explicitly call ```IComparable.CompareTo```, but it fails on the database side.


```
"The LINQ expression 'DbSet<User>()
    .OrderBy(u => u.SkillType)
    .Where(u => u.SkillType.CompareTo((object)__value_0) > 0)' could not be translated. 

Additional information: Translation of method 'System.Enum.CompareTo' failed. 

Either rewrite the query in a form that can be translated, or switch to client evaluation explicitly by inserting a call to 'AsEnumerable', 'AsAsyncEnumerable', 'ToList', or 'ToListAsync'."
```

## Potential Workaround

We came up with a proof-of-concept expression converts, which appears to work:

[Handling of keys with enum return type](https://github.com/marcin-janiak/cursor-based-pagination-with-enum-sorting/blob/main/Host/CustomExpressionHelpers.cs#L101-L118)

[Handling handled cursor keys](https://github.com/marcin-janiak/cursor-based-pagination-with-enum-sorting/blob/main/Host/CustomExpressionHelpers.cs#L78-L86)