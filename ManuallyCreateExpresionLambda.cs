using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace BambooCore.Utility
{
    public static class SearchByKeyword
    {
        public static IQueryable<T> Search<T>(this IQueryable<T> source, string propertyName, string keyword)
        {
            // ------ function of this class ------
            //create an expression lambda then pass it 
            //to predicate builder for making search function               
            // ------          *****         ------
            var predicateKeyword = PredicateBuilder.New<T>(true);

            if (string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(keyword))
            {
                return source;
            }

            var property = typeof(T).GetProperty(propertyName);

            if (property is null)
            {
                return source;
            }
            //create parameter
            var itemParameter = Expression.Parameter(typeof(T), "item");
            //create property
            var functions = Expression.Property(null, typeof(EF).GetProperty(nameof(EF.Functions)));
            var like = typeof(DbFunctionsExtensions).GetMethod(nameof(DbFunctionsExtensions.Like), new Type[] { functions.Type, typeof(string), typeof(string) });

            Expression expressionProperty = Expression.Property(itemParameter, property.Name);

            if (property.PropertyType != typeof(string))
            {
                expressionProperty = Expression.Call(expressionProperty, typeof(object).GetMethod(nameof(object.ToString), new Type[0]));
            }

            var selector1 = Expression.Call(
                       null,
                       like,
                       functions,
                       expressionProperty,
                       Expression.Constant($"%[^a-z0-9]{keyword}[^a-z0-9]%"));

            var selector2 = Expression.Call(
                       null,
                       like,
                       functions,
                       expressionProperty,
                       Expression.Constant($"{keyword}[^a-z0-9]%"));

            var selector3 = Expression.Call(
                       null,
                       like,
                       functions,
                       expressionProperty,
                       Expression.Constant($"%[^a-z0-9]{keyword}"));

            var selector4 = Expression.Call(
                       null,
                       like,
                       functions,
                       expressionProperty,
                       Expression.Constant($"{keyword}"));

            predicateKeyword.Or(Expression.Lambda<Func<T, bool>>(selector1, itemParameter));
            predicateKeyword.Or(Expression.Lambda<Func<T, bool>>(selector2, itemParameter));
            predicateKeyword.Or(Expression.Lambda<Func<T, bool>>(selector3, itemParameter));
            predicateKeyword.Or(Expression.Lambda<Func<T, bool>>(selector4, itemParameter));
            return source.Where(predicateKeyword);
        }
    }
}
