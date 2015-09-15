using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExampleData;
using Tonic.MVVM;

namespace ExampleViewModel
{
    public class MainViewModel : CommandsViewModel
    {
        #region Dependencies
        readonly IView ViewContainer;
        readonly Func<ExampleContext> Context;
        public MainViewModel(IView ViewContainer, Func<ExampleContext> Context)
        {
            this.ViewContainer = ViewContainer;
            this.Context = Context;
        }

        #endregion 

        public string Title
        {
            get
            {
                return "Ventana principal";
            }
        }

        public void ShowArtist()
        {
            var V = new ArtistViewModel(Context);
            ViewContainer.ShowDialog(V);
        }
    }
}
