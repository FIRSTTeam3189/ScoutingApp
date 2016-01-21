using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScoutingApp.Data;
using ScoutingApp.Models;
using ScoutingModels.Data;
using ScoutingModels.Extentions;
using ScoutingModels.Scrubber;
using Xamarin.Forms;

namespace ScoutingApp.UI
{
	public partial class HomePage : TabbedPage
	{
        private BlueAllianceClient _client = new BlueAllianceClient();
        public HomePageModel Model { get; set; }

        HomeEventPage _eventPage = new HomeEventPage() {Title = "Events"};
        HomeTeamPage _teamPage = new HomeTeamPage() {Title = "Teams"};
        HomeMyTeamPage _myTeamPage = new HomeMyTeamPage() {Title = "My Team"};

		public HomePage ()
		{
			InitializeComponent ();  
            Children.Add(_eventPage);
            Children.Add(_teamPage);
            Children.Add(_myTeamPage);  
		}

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            Model = new HomePageModel();
            var events = new List<Event>(await _client.GetEvents(2016));
            var eventsWithStartDate = events.Where(x => x.StartDate != null).ToList();
            var eventsWithoutStartDate = events.Where(x => x.StartDate == null);            
            var soonestWeek = (eventsWithStartDate.OrderBy(x => x.StartDate).First().StartDate ?? DateTime.Now).GetWeekNumber();
             
            Model.EventGroupings =
                new ObservableCollection<EventGrouping>(eventsWithStartDate.Select((e) => new { IndexPlace = (e.StartDate ?? DateTime.Now).GetWeekNumber() - soonestWeek, Event = e })
                    .GroupBy(x => x.IndexPlace)
                    .Select(x => new EventGrouping(x.Key, x.ToList().Select(y => y.Event))).OrderBy(x => x.Week));

            Model.EventGroupings.Add(new EventGrouping(0, eventsWithoutStartDate));
            

            _eventPage.SetItemSource(Model.EventGroupings);
        }
    }
}
