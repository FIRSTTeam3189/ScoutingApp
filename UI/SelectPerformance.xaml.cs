using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Scouty.Models.Local;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Scouty.UI
{
	public partial class SelectPerformance : ContentPage
	{
		public ObservableCollection<Team> Teams;

		public event Action NavigatedTo;

		public int MatchNumber { get; }

		public SelectPerformance (IEnumerable<Team> teams, int matchNumber)
		{
			InitializeComponent ();

			Teams = new ObservableCollection<Team> (teams);
			TeamsList.ItemsSource = Teams;

			TeamsList.ItemSelected += TeamSelected;
			MatchNumber = matchNumber;
		}

		async void TeamSelected (object sender, SelectedItemChangedEventArgs e)
		{
			// Simply navigate to a team performance page
			var tcs = new TaskCompletionSource<RobotPerformance>();

			var page = new PerformancePage (e.SelectedItem as Team, MatchNumber);
			page.PerformanceCreated += tcs.SetResult;
			NavigatedTo += () => {if (tcs.Task.IsCanceled != false) tcs.SetCanceled(); };

			// Lets try to create the performance
			await Navigation.PushAsync(page);
			try {
				var perf = await tcs.Task;
			} catch (OperationCanceledException){
				// We dont care about this
			}
		}

		protected override void OnAppearing ()
		{
			base.OnAppearing ();

			NavigatedTo?.Invoke ();
		}
	}
}

