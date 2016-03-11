using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;

using Xamarin.Forms;
using Scouty.Models.Local;
using System.Collections.ObjectModel;
using Scouty.Utility;

namespace Scouty.UI
{
	public partial class CreateMatchPage : ContentPage
	{
		readonly Logger logger = new Logger(typeof(CreateMatchPage));
		/// <summary>
		/// Red Alliance Model
		/// </summary>
		/// <value>The red alliance model.</value>
		public AllianceModel RedAllianceModel { get; set; }

		/// <summary>
		/// Blue alliance model
		/// </summary>
		/// <value>The blue alliance model.</value>
		public AllianceModel BlueAllianceModel { get; set; }

		/// <summary>
		/// Teams in the event
		/// </summary>
		/// <value>The teams.</value>
		ObservableCollection<Team> Teams { get; set; }

		/// <summary>
		/// Number of the match
		/// </summary>
		/// <value>The match number.</value>
		public int MatchNumber { get; }

		/// <summary>
		/// The Type of the match
		/// </summary>
		/// <value>The type.</value>
		public MatchType Type { get; set; }

		/// <summary>
		/// Occurs when match created.
		/// </summary>
		public event Action<Match> MatchCreated;

		public CreateMatchPage (string eventCode, int year, int matchNumber)
		{
			InitializeComponent ();

			Title = "Create Match " + matchNumber;
			MatchNumber = matchNumber;

			// Query the DB for the Teams at the event
			var db = Database.LocalDatabase.Database;

			var teams = db.QueryEventTeams (eventCode, year, true).OrderBy(x => x.TeamNumber);
			Teams = new ObservableCollection<Team> (teams);

			TeamsList.ItemsSource = Teams;

			Type = MatchType.Practice;

			// Create Alliance Models
			RedAllianceModel = new AllianceModel ();
			BlueAllianceModel = new AllianceModel ();
			RedAlliance.BindingContext = RedAllianceModel;
			BlueAlliance.BindingContext = BlueAllianceModel;

			MatchTypeButton.Clicked += MatchTypeButton_Clicked;
			ToolbarItems.Add(new ToolbarItem("Create", null, MatchCreate));
			TeamsList.ItemSelected += TeamSelected;
		}

		public void MatchCreate() {
			// Lets make sure there's 6 teams selected
			if (BlueAllianceModel.ActualTeamOne == null || BlueAllianceModel.ActualTeamTwo == null || BlueAllianceModel.ActualTeamThree == null)
				return;
			if (RedAllianceModel.ActualTeamOne == null || RedAllianceModel.ActualTeamTwo == null || RedAllianceModel.ActualTeamThree == null)
				return;

			MatchCreated?.Invoke (new Match (){ 
				BlueOne = BlueAllianceModel.ActualTeamOne.TeamNumber,
				BlueTwo = BlueAllianceModel.ActualTeamTwo.TeamNumber,
				BlueThree = BlueAllianceModel.ActualTeamThree.TeamNumber,
				RedOne = RedAllianceModel.ActualTeamOne.TeamNumber,
				RedTwo = RedAllianceModel.ActualTeamTwo.TeamNumber,
				RedThree = RedAllianceModel.ActualTeamThree.TeamNumber,
				MatchNumber = MatchNumber,
				MatchType = Type,
				StartTime = DateTime.Now
			});
		}

		/// <summary>
		/// Switches between match types
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		void MatchTypeButton_Clicked (object sender, EventArgs e)
		{
			if (Type == MatchType.Practice) {
				MatchTypeButton.Text = "Qualification Match";
				Type = MatchType.Qualification;
			} else if (Type == MatchType.Qualification) {
				MatchTypeButton.Text = "Octo-Final Match";
				Type = MatchType.OctoFinal;
			} else if (Type == MatchType.OctoFinal) {
				MatchTypeButton.Text = "Quarter Final Match";
				Type = MatchType.QuarterFinal;
			} else if (Type == MatchType.QuarterFinal) {
				MatchTypeButton.Text = "Semi-final Match";
				Type = MatchType.SemiFinal;
			} else if (Type == MatchType.SemiFinal) {
				MatchTypeButton.Text = "Final Match";
				Type = MatchType.Final;
			} else {
				MatchTypeButton.Text = "Practice Match";
				Type = MatchType.Practice;
			}
		}

		/// <summary>
		/// Occurs when a team is selected
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		async void TeamSelected (object sender, SelectedItemChangedEventArgs e)
		{
			var team = e.SelectedItem as Team;
			if (team == null)
				return;
			var action = await DisplayActionSheet ($"Which alliance would you like to add {team.TeamNumber} to?", "None", null, "Red", "Blue");

			if (action == "Red") {
				// Get which one is null on Red...
				if (RedAllianceModel.ActualTeamOne == null)
					RedAllianceModel.ActualTeamOne = team;
				else if (RedAllianceModel.ActualTeamTwo == null)
					RedAllianceModel.ActualTeamTwo = team;
				else
					RedAllianceModel.ActualTeamThree = team;
			} else if (action == "Blue") {
				// Get which one is null on blue...
				if (BlueAllianceModel.ActualTeamOne == null)
					BlueAllianceModel.ActualTeamOne = team;
				else if (BlueAllianceModel.ActualTeamTwo == null)
					BlueAllianceModel.ActualTeamTwo = team;
				else
					BlueAllianceModel.ActualTeamThree = team;
			} else {
				// Nothing selected
				logger.Info("Nothing selected");
				TeamsList.SelectedItem = null;
				return;
			}

			Teams.Remove (team);
		}


	}

	public class AllianceModel : INotifyPropertyChanged {
		#region INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		Team teamOne;
		public Team ActualTeamOne { get { return teamOne; }
			set { 
				if (teamOne != value) {
					teamOne = value;
					OnPropertyChanged ("ActualTeamOne");
					OnPropertyChanged ("TeamOne");
				}
			}
		}
		public string TeamOne {
			get { 
				if (teamOne != null)
					return "Slot One: "+ teamOne.TeamNumber;
				else
					return "Slot One is Empty";
			}
		}

		Team teamTwo;
		public Team ActualTeamTwo { get { return teamTwo; } set {
				if (teamTwo != value){
					teamTwo = value;
					OnPropertyChanged("ActualTeamTwo");
					OnPropertyChanged("TeamTwo");
				}
			}
		}
		public string TeamTwo {
			get { 
				if (teamTwo != null)
					return "Slot Two: " + teamTwo.TeamNumber;
				else
					return "Slot Two is Empty";
			}
		}

		Team teamThree;
		public Team ActualTeamThree {
			get {return teamThree; }
			set {
				if (teamThree != value){
					teamThree = value;
					OnPropertyChanged ("ActualTeamThree");
					OnPropertyChanged ("TeamThree");
				}
			}
		}
		public string TeamThree {
			get { 
				if (teamThree != null)
					return "Slot Three: " + teamThree.TeamNumber;
				else
					return "Slot Three is Empty";
			}
		}

		public void OnPropertyChanged(string property){
			PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (property));
		}
	}
}

