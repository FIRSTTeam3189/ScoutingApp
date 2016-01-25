using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScoutingApp.Models;
using ScoutingModels.Data;
using ScoutingModels.Scrubber;
using Xamarin.Forms;

namespace ScoutingApp.UI
{
	public partial class HomeMyTeamPage : ContentPage
	{
	    private Team _team;

	    public Team Team
	    {
	        get
	        {
	            return _team;
	        }
	        set
            {
	            if (value != _team)
	            {
	                _team = value;
	                OnPropertyChanged("Team");
	            }
	        }
	    }

        public ObservableCollection<Event> Events { get; set; }

	    public ObservableCollection<EventRanking> EventRankings { get; set; }

	    public HomeMyTeamPage ()
		{
			InitializeComponent ();
		}

	    protected override async void OnAppearing()
	    {
	        base.OnAppearing();

            // TODO: Download Events For Team Later, Rankings Too
            
            var client = new BlueAllianceClient();

	        var team = await client.GetTeam(3189);

	        Team = team;

            Events = new ObservableCollection<Event>()
            {
                new Event()
                {
                    Name = "Sacramento Regional",
                    Location = "Davis, CA"
                },
                new Event()
                {
                    Name = "Medera",
                    Location = "Medera, CA"
                }
            };

            EventRankings = new ObservableCollection<EventRanking>()
            {
                new EventRanking()
                {
                    Name = "Sacramento",
                    Ranking = 1
                },
                new EventRanking()
                {
                    Name = "Medera",
                    Ranking = 2
                }
            };
	        EventsListView.ItemsSource = Events;

	        RankListView.ItemsSource = EventRankings;

	        Layout.BindingContext = this;

	    }
	}
}
