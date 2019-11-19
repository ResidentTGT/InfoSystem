using System;
using System.Linq;
using System.Linq.Expressions;

namespace Company.Common.Extensions
{
    public static class EntityFramework
    {
        public static IQueryable<T> AddQuery<T>(this IQueryable<T> query, Expression<Func<T, bool>> filter, bool condition)
        {
            return condition ? query.Where(filter) : query;
        }
    }
}
