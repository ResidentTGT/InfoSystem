using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Company.Pim.Helpers.v2
{
    public class TreeObjectHelper
    {
        public delegate void TreeExecuteDelegate<T>(T item);

        public List<T2> GetPropertyFromTreeObject<T, T2>(ICollection<T> items, Func<T, T2> selector, Expression<Func<T, ICollection<T>>> childsProp)
        {
            var result = new List<T2>();

            if (items != null && items.Any())
                foreach (var item in items)
                {
                    result.Add(selector(item));
                    result.AddRange(GetPropertyFromTreeObject(childsProp.Compile()(item), selector, childsProp));

                }
            return result;
        }

        public T GetHeadOfTreeObject<T>(T item, Func<T,T> parentProperty)
        {

            while (parentProperty(item) != null)
                item = parentProperty(item); 

            return item;
        }
        /// <summary>
        /// Do some anonymous function for each item in tree
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <param name="executive"></param>
        /// <param name="childsProp"></param>
        public void DoForEachInTreeObject<T>(ICollection<T> items, TreeExecuteDelegate<T> executive, Expression<Func<T, ICollection<T>>> childsProp)
        {
            foreach(var item in items)
            {
                executive(item);
                DoForEachInTreeObject(childsProp.Compile()(item), executive, childsProp);
            }
        }
    }
}
