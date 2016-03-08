using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xamarin.Forms;
using Scouty.Models.Local;

namespace Scouty.UI
{
	public partial class PerformancePage : ContentPage
	{
		List<RobotEvent> events = new List<RobotEvent> ();

		EventTime currentTime = EventTime.Auto;

		public event Action<RobotPerformance> PerformanceCreated;

		public Team Team { get; }

		public int MatchNumber { get; }

		int makeHigh = 0;
		int highShots = 0;
		/// <summary>
		/// Gets or sets the made high shots.
		/// </summary>
		/// <value>The made high shots.</value>
		public int MadeHighShots {
			get { return makeHigh; }
			set {
				var diff = value - makeHigh;
				makeHigh = value;
				highShots += diff;

				HighLabel.Text = makeHigh + "/" + highShots;
			}
		}

		/// <summary>
		/// Gets or sets the high shots.
		/// </summary>
		/// <value>The high shots.</value>
		public int HighShots {
			get { return highShots; }
			set {
				highShots = value;
				HighLabel.Text = makeHigh + "/" + highShots;
			}
		}

		int lowShots = 0;
		int makeLow = 0;
		/// <summary>
		/// Gets or sets the made low shots.
		/// </summary>
		/// <value>The made low shots.</value>
		public int MadeLowShots {
			get { return makeLow; }
			set { 
				var diff = value - makeLow;
				makeLow = value;
				lowShots += diff;

				LowLabel.Text = makeLow + "/" + lowShots;
			}
		}

		/// <summary>
		/// Gets or sets the low shots.
		/// </summary>
		/// <value>The low shots.</value>
		public int LowShots {
			get { return lowShots; }
			set { 
				lowShots = value;
				LowLabel.Text = makeLow + "/" + lowShots;
			}
		}

		public PerformancePage (Team team, int matchNumber)
		{
			InitializeComponent ();
			Team = team;
			MatchNumber = matchNumber;
			Title = $"Team {team.TeamNumber} : Match {matchNumber}";

			MakeHigh.Clicked += (object sender, EventArgs e) => {
				MadeHighShots++;
				AddEvent(EventType.MakeHigh);
			};

			MissHigh.Clicked += (object sender, EventArgs e) => {
				HighShots++;
				AddEvent(EventType.MissHigh);
			};

			MakeLow.Clicked += (object sender, EventArgs e) => {
				MadeLowShots++;
				AddEvent(EventType.MakeLow);
			};

			MissLow.Clicked += (object sender, EventArgs e) => {
				LowShots++;
				AddEvent(EventType.MissLow);
			};

			// Crossing Defenses
			CrossOneButton.Clicked += (object sender, EventArgs e) => AddCross(CrossOneAmountLabel, EventType.CrossOne);
			CrossTwoButton.Clicked += (object sender, EventArgs e) => AddCross(CrossTwoAmountLabel, EventType.CrossTwo);
			CrossThreeButton.Clicked += (object sender, EventArgs e) => AddCross(CrossThreeAmountLabel, EventType.CrossThree);
			CrossFourButton.Clicked += (object sender, EventArgs e) => AddCross(CrossFourAmountLabel, EventType.CrossFour);
			CrossFiveButton.Clicked += (object sender, EventArgs e) => AddCross(CrossFiveAmountLabel, EventType.CrossFive);

			// Assisting Defenses
			AssistOneButton.Clicked += (object sender, EventArgs e) => AddCross(AssistOneAmountLabel, EventType.AssistOne);
			AssistTwoButton.Clicked += (object sender, EventArgs e) => AddCross(AssistTwoAmountLabel, EventType.AssistTwo);
			AssistThreeButton.Clicked += (object sender, EventArgs e) => AddCross(AssistThreeAmountLabel, EventType.AssistThree);
			AssistFourButton.Clicked += (object sender, EventArgs e) => AddCross(AssistFourAmountLabel, EventType.AssistFour);
			AssistFiveButton.Clicked += (object sender, EventArgs e) => AddCross(AssistFiveAmountLabel, EventType.AssistFive);

			ReachDefenseButton.Clicked += (object sender, EventArgs e) => ToggleEvent (ReachDefenseButton, EventType.ReachDefense);
			ChallengeButton.Clicked += (object sender, EventArgs e) => ToggleEvent (ChallengeButton, EventType.Challenge);
			HangButton.Clicked += (object sender, EventArgs e) => ToggleEvent (HangButton, EventType.Hang);

			// Fouls Button
			FoulButton.Clicked += (object sender, EventArgs e) => AddEvent(EventType.Foul);
			TechnicalFoulButton.Clicked += (object sender, EventArgs e) => AddEvent(EventType.TechnicalFoul);

			// Toggle Button
			ModeButton.Clicked += (object sender, EventArgs e) => {
				if (currentTime == EventTime.Auto){
					currentTime = EventTime.Teleop;
					ModeButton.Text = "Teleop Mode";
					ModeButton.BackgroundColor = Color.Lime;
				} else if (currentTime == EventTime.Teleop){
					currentTime = EventTime.Final;
					ModeButton.Text = "Final Mode";
					ModeButton.BackgroundColor = Color.Fuchsia;
				} else {
					currentTime = EventTime.Auto;
					ModeButton.Text = "AutonomousMode";
					ModeButton.BackgroundColor = Color.Aqua;
				}
			};

			SubmitButton.Clicked += async (object sender, EventArgs e) => await App.Current.MainPage.Navigation.PushAsync(new ConfirmPerformancePage(events, Team,MatchNumber));
		}

		/// <summary>
		/// Toggles an Event in the list and sets button to appropriate color
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="type">Type.</param>
		void ToggleEvent(Button button, EventType type){
			var hasEvent = events.FirstOrDefault (x => x.EventType == type);
			if (hasEvent == null) {
				button.BackgroundColor = Color.Lime;
				AddEvent (type);
			} else {
				button.BackgroundColor = Color.Silver;
				events.Remove (hasEvent);
			}
		}

		/// <summary>
		/// Adds an event and updates the label given
		/// </summary>
		/// <param name="label">Label.</param>
		/// <param name="type">Type.</param>
		void AddCross(Label label, EventType type){
			label.Text = int.Parse (label.Text) + 1 + "";
			AddEvent (type);
		}

		/// <summary>
		/// Adds an Event
		/// </summary>
		/// <param name="type">Type.</param>
		void AddEvent(EventType type){
			events.Add (new RobotEvent{ EventTime = currentTime, EventType = type });
		}
	}
}

