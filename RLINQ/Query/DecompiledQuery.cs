using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Tonic
{
    class DecompiledQuery<T> : IQueryable<T>
    {
        public DecompiledQuery(IQueryable<T> original)
        {
            this.original = original;
            this.Provider = new DecompiledQuery<T>.QProv(original);
        }
        private readonly IQueryable<T> original;
        internal class QProv : IQueryProvider
        {
            public QProv(IQueryable<T> original)
            {
                this.original = original;
            }
            private readonly IQueryable<T> original;
            public IQueryable CreateQuery(Expression expression)
            {
                throw new NotSupportedException();
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new DecompiledQuery<TElement>(original.Provider.CreateQuery<TElement>(expression.Decompile()));
            }

            public object Execute(Expression expression)
            {
                return original.Provider.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return original.Provider.Execute<TResult>(expression);
            }
        }

        public Type ElementType
        {
            get
            {
                return original.ElementType;
            }
        }

        public Expression Expression
        {
            get
            {
                return original.Expression;
            }
        }

        public IQueryProvider Provider
        {
            get; private set;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return original.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
