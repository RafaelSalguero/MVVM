using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.Patterns.PagedQuery.Composer
{
    /// <summary>
    /// On a compositing expression, a constant expression with an instance of this object represents the original query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class QueryPlaceholder<T> : IQueryable<T>, IQueryComposerReference
    {
        private string id;
        public QueryPlaceholder(IQueryComposer Creator)
        {
            id = Guid.NewGuid().ToString();
            this.Creator = Creator;
        }
        private readonly IQueryComposer Creator;
        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public Type ElementType
        {
            get { throw new NotImplementedException(); }
        }

        public Expression Expression
        {
            get { throw new NotImplementedException(); }
        }

        public IQueryProvider Provider
        {
            get { throw new NotImplementedException(); }
        }

        public IQueryComposer Composer
        {
            get { return Creator; }
        }

        public override string ToString()
        {
            return typeof(T) + ":" + id.ToString();
        }
    }
}
