using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScoutingApp.Data;
using ScoutingApp.Models;
using ScoutingModels.Data;
using ScoutingModels.Scrubber;
using Xamarin.Forms;

namespace ScoutingApp.UI
{
	public partial class HomeEventPage : ContentPage
	{

        public HomeEventPage ()
		{
			InitializeComponent ();
		}

	    public void SetItemSource(ObservableCollection<EventGrouping> itemSource)
	    {
	        EventListView.ItemsSource = itemSource;
	    }

        
    }
}


