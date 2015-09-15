using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleData;
using Tonic.MVVM;

namespace ExampleViewModel
{
    public class ArtistViewModel : EntityViewModel<Artist, ExampleContext>
    {
        public ArtistViewModel(Func<ExampleContext> Context) : base(Context) { }
    }
}
