using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tonic.EF
{
    public class OrphanManager
    {
        class NavProp
        {
            Type SetType
            {
                get; set;
            }
            PropertyInfo NavigationProperty { get; set; }
        }
        private List<NavProp> deleteOrphans = new List<NavProp>();

        
    }
}
