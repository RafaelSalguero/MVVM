using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Tonic.Patterns.PagedQuery.EF;
namespace PagedQueryTest.ViewModel
{

    [PropertyChanged.ImplementPropertyChanged]
    public class QueryTest
    {
        public QueryTest()
        {
            Log = new ObservableCollection<string>();
        }
        public ICommand PagedQuery
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    using (var C = new Model.G5())
                    {
                        Results = C.MOV_VentasDhola.ToPagedQuery().OrderByDescending(x => x.IdConsecutivo);
                    }
                });
            }
        }

        public ICommand AsyncPagedQuery
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    using (var C = new Model.G5())
                    {
                        var query = C.MOV_VentasDhola.ToPagedQueryAsync().OrderByDescending(x => x.IdConsecutivo);
                        
                        Results = query;
                    }
                });
            }
        }


        public ICommand SimpleQuery
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    using (var C = new Model.G5())
                    {
                        Results = C.MOV_VentasDhola.OrderByDescending(x => x.IdConsecutivo).ToList();
                    }
                });
            }
        }


        public ICommand NullQuery
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    Results = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                });
            }
        }
        public ObservableCollection<string> Log
        {
            get;
            private set;
        }

        public IEnumerable<Model.MOV_VentasD> Results
        {
            get;
            set;
        }
    }
}
