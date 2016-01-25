using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using ScoutingModels.Data;
using ScoutingModels.Scrubber;
using Xamarin.Forms;

namespace ScoutingApp.UI
{
	public partial class HomeTeamPage : ContentPage
	{
	    private BlueAllianceClient _client;
	    private AsyncLock _isLoading;

        public ObservableCollection<Team> Teams { get; set; }
         
		public HomeTeamPage ()
		{
			InitializeComponent ();
            _client = new BlueAllianceClient();
            _isLoading = new AsyncLock();
            Teams = new ObservableCollection<Team>();
		    TeamListView.ItemsSource = Teams;
            TeamListView.ItemAppearing += OnTeamAppearing;
		}

	    private async void OnTeamAppearing(object sender, ItemVisibilityEventArgs itemVisibilityEventArgs)
	    {
	        var team = itemVisibilityEventArgs.Item as Team;
	        var lastTeam = Teams.OrderByDescending(x => x.Number).FirstOrDefault();
	        if (lastTeam != null && lastTeam.Number == team.Number)
	        {
	            int page = Teams.Count/500 + 1;
                var tokenSource = new CancellationTokenSource(75);
	            try
	            {
	                using (await _isLoading.LockAsync(tokenSource.Token))
	                {
                        var dlteams = (await _client.GetTeams(page)).OrderBy(x => x.Number);
                        foreach (var dlteam in dlteams)
                        {
                            Teams.Add(dlteam);
                        }
                    }
	            }
	            catch (OperationCanceledException e)
	            {
	                System.Console.WriteLine("Operation Canceled....");
	            }
	            finally
	            {
                    tokenSource.Dispose();
	            }
	        }
	    }

	    protected override async void OnAppearing()
	    {
	        base.OnAppearing();

	        using (await _isLoading.LockAsync())
	        {
	            var dlteams = (await _client.GetTeams(0)).OrderBy(x => x.Number);
	            foreach (var dlteam in dlteams)
	            {
	                Teams.Add(dlteam);
	            }
	        }
	    }
    }
}
