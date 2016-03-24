using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Xamarin.Forms;
using Scouty.Models.Local;
using Scouty.Utility;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Scouty.UI
{
	public partial class SelectDefensePage : ContentPage
	{
		static readonly Logger logger = new Logger (typeof(SelectDefensePage));
		public ObservableCollection<GroupedDefense> GroupedDefenses { get; set; }
		public DefenseSelectionUI Selection { get; set; }

		public event Action<RobotPerformance> PerformanceCreated;
		public event Action NavigatedTo;

		public Team Team { get; }
		public int MatchNumber { get; }
		public MatchType MatchType { get; }
		public string EventCode { get; }
		public IList<Team> OtherAlliance { get; }

		public SelectDefensePage (Team team, int matchNumber, MatchType type, string eventCode, IList<Team> otherAlliance)
		{
			InitializeComponent ();

			Selection = new DefenseSelectionUI ();
			SelectedDefensesTwoThree.BindingContext = Selection;
			SelectedDefensesFourFive.BindingContext = Selection;

			Team = team;
			MatchNumber = matchNumber;
			MatchType = type;
			EventCode = eventCode;
			OtherAlliance = otherAlliance;

			// Generate GroupedDefenses
			var a = new List<DefenseType> () {
				DefenseType.Portcullis,
				DefenseType.ChevalDeFrise
			};
			var b = new List<DefenseType> () {
				DefenseType.Moat,
				DefenseType.Ramparts
			};
			var c = new List<DefenseType> () {
				DefenseType.SallyPort,
				DefenseType.Drawbridge
			};
			var d = new List<DefenseType> () {
				DefenseType.RockWall,
				DefenseType.RoughTerrain
			};

			GroupedDefenses = new ObservableCollection<GroupedDefense> () {
				new GroupedDefense (a, DefenseCategory.A),
				new GroupedDefense (b, DefenseCategory.B),
				new GroupedDefense (c, DefenseCategory.C),
				new GroupedDefense (d, DefenseCategory.D)
			};
			Defenses.ItemsSource = GroupedDefenses;

			// Setup Submit Button
			ToolbarItems.Add(new ToolbarItem("Next", null, SubmitClicked));

			// Setup all the other buttons
			DefenseTwoButton.Clicked += async (object sender, EventArgs e) => await ChangeDefense(2);
			DefenseThreeButton.Clicked += async (object sender, EventArgs e) => await ChangeDefense(3);
			DefenseFourButton.Clicked += async (object sender, EventArgs e) => await ChangeDefense(4);
			DefenseFiveButton.Clicked += async (object sender, EventArgs e) => await ChangeDefense(5);

			Defenses.ItemSelected += DefenseSelected;
		}

		async void DefenseSelected (object sender, SelectedItemChangedEventArgs e)
		{
			var selectedItem = e.SelectedItem as DefenseUI;

			// Dont care here
			if (selectedItem == null)
				return;

			Defenses.SelectedItem = null;

			// Figure out which slot to add it to
			if (Selection.SlotTwo == null)
				Selection.SlotTwo = selectedItem.Defense;
			else if (Selection.SlotThree == null)
				Selection.SlotThree = selectedItem.Defense;
			else if (Selection.SlotFour == null)
				Selection.SlotFour = selectedItem.Defense;
			else if (Selection.SlotFive == null)
				Selection.SlotFive = selectedItem.Defense;
			else {
				await DisplayAlert("Error", "Already filled all defenses, select one to remove it", "OK");
				return;
			}

			// Get the category
			var category = selectedItem.Defense.GetCategory();

			// Remove it's group from the list if its not a practice match or just remove it
			if (MatchType == MatchType.Practice) {
				GroupedDefenses
					.First (x => x.Category == category)
					.Remove (GroupedDefenses.First (x => x.Category == category)
						.First (x => x.Defense == selectedItem.Defense));
			} else {
				GroupedDefenses.Remove (GroupedDefenses.First (x => x.Category == category));
			}
		}

		async void SubmitClicked ()
		{
			if (!Selection.SelectedDefenses ()) {
				await DisplayAlert ("Error", "Must select all defenses", "OK");
				return;
			}
			// Simply navigate to a team performance page
			var tcs = new TaskCompletionSource<RobotPerformance>();

			var page = new PerformancePage (Team, MatchNumber, MatchType, EventCode, Selection.GetDefensesSelected().ToList(), OtherAlliance);
			page.PerformanceCreated += tcs.SetResult;
			NavigatedTo += () => {
				if (!tcs.Task.IsCanceled || !tcs.Task.IsCompleted)
					tcs.TrySetCanceled ();
			};

			// Lets try to create the performance
			await Navigation.PushModalAsync(page);
			try {
				var perf = await tcs.Task;
				await Navigation.PopModalAsync();
				PerformanceCreated?.Invoke(perf);
			} catch(OperationCanceledException){
				// Dont care here
			} catch (Exception e){
				logger.Error ("This souldnt happen", e);
			}
		}

		async Task ChangeDefense(int slot){
			// Get what slot it is
			DefenseType? type = null;
			if (slot == 2)
				type = Selection.SlotTwo;
			else if (slot == 3)
				type = Selection.SlotThree;
			else if (slot == 4)
				type = Selection.SlotFour;
			else if (slot == 5)
				type = Selection.SlotFive;

			// Dont care if theres nothing in there
			if (type == null)
				return;

			// Cast it into a non-null type
			var defense = (DefenseType)type;

			// Display Action Sheet
			var destroyAction = "Remove " + defense.GetDefenseTypeString();
			var action = await DisplayActionSheet ("Slot " + slot, "Nothing", destroyAction, "");

			if (action == destroyAction) {
				// Remove it from that slot
				if (slot == 2)
					Selection.SlotTwo = null;
				else if (slot == 3)
					Selection.SlotThree = null;
				else if (slot == 4)
					Selection.SlotFour = null;
				else if (slot == 5)
					Selection.SlotFive = null;

				// Add it back in
				if (MatchType == MatchType.Practice) {
					GroupedDefenses.First (x => x.Category == defense.GetCategory ()).Add (new DefenseUI (defense));
				} else {
					var grp = new GroupedDefense (new List<DefenseType> () { defense, defense.GetSisterDefense () }, defense.GetCategory ());
					GroupedDefenses.Add (grp);
				}
			}
		}
	}

	public class GroupedDefense : ObservableCollection<DefenseUI> {
		public string LongName { get; set; }
		public string ShortName { get; set; }

		public DefenseCategory Category { get; set; }

		public GroupedDefense(IEnumerable<DefenseType> defenses, DefenseCategory category){
			LongName = "Category " + category;
			ShortName = category.ToString ();
			Category = category;

			foreach (var defense in defenses) {
				if (!defense.IsInCategory (category))
					throw new ArgumentException ("Defenses dont match category given");

				Add (new DefenseUI (defense));
			}
		}
	}

	public class DefenseUI {
		public DefenseType Defense { get; set; }
		public string Name { get; set; }

		public DefenseUI(DefenseType type){
			Defense = type;
			Name = type.GetDefenseTypeString ();
		}
	}

	public class DefenseSelectionUI : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Gets the slot one Defense, Always Low Bar.
		/// </summary>
		/// <value>The slot one.</value>
		public DefenseType SlotOne { get { return DefenseType.LowBar; } }

		public DefenseType? _slotTwo = null;
		/// <summary>
		/// Gets or sets the slot two.
		/// </summary>
		/// <value>The slot two.</value>
		public DefenseType? SlotTwo { get { return _slotTwo; } 
			set { 
				if (value != _slotTwo) {
					_slotTwo = value;
					OnPropertyChanged ("SlotTwo");
					OnPropertyChanged ("DefenseTwo");
				}
			}
		}
		public DefenseType? _slotThree = null;
		/// <summary>
		/// Gets or sets the slot three.
		/// </summary>
		/// <value>The slot three.</value>
		public DefenseType? SlotThree { get { return _slotThree; } 
			set { 
				if (value != _slotThree) {
					_slotThree = value;
					OnPropertyChanged ("SlotThree");
					OnPropertyChanged ("DefenseThree");
				}
			}
		}
		public DefenseType? _slotFour = null;
		/// <summary>
		/// Gets or sets the slot four.
		/// </summary>
		/// <value>The slot four.</value>
		public DefenseType? SlotFour { get { return _slotFour; } 
			set { 
				if (value != _slotFour) {
					_slotFour = value;
					OnPropertyChanged ("SlotFour");
					OnPropertyChanged ("DefenseFour");
				}
			}
		}
		public DefenseType? _slotFive = null;
		/// <summary>
		/// Gets or sets the slot five.
		/// </summary>
		/// <value>The slot five.</value>
		public DefenseType? SlotFive { get { return _slotFive; } 
			set { 
				if (value != _slotFive) {
					_slotFive = value;
					OnPropertyChanged ("SlotFive");
					OnPropertyChanged ("DefenseFive");
				}
			}
		}

		/// <summary>
		/// Gets the defense one.
		/// </summary>
		/// <value>The defense one.</value>
		public string DefenseOne { get { return "Slot One: Low Bar"; } }

		/// <summary>
		/// Gets the defense two.
		/// </summary>
		/// <value>The defense two.</value>
		public string DefenseTwo { 
			get { 
				return "Slot Two: " + (_slotTwo == null ? "" : ((DefenseType)_slotTwo).GetDefenseTypeString());
			} 
		}

		/// <summary>
		/// Gets the defense three.
		/// </summary>
		/// <value>The defense three.</value>
		public string DefenseThree { 
			get { 
				return "Slot Three: " + (_slotThree == null ? "" : ((DefenseType)_slotThree).GetDefenseTypeString());
			} 
		}

		/// <summary>
		/// Gets the defense four.
		/// </summary>
		/// <value>The defense four.</value>
		public string DefenseFour { 
			get { 
				return "Slot Four: " + (_slotFour == null ? "" : ((DefenseType)_slotFour).GetDefenseTypeString());
			} 
		}

		/// <summary>
		/// Gets the defense five.
		/// </summary>
		/// <value>The defense five.</value>
		public string DefenseFive { 
			get { 
				return "Slot Five: " + (_slotFive == null ? "" : ((DefenseType)_slotFive).GetDefenseTypeString());
			} 
		}

		public void OnPropertyChanged(string propertyName){
			PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
		}

		/// <summary>
		/// Gets the defenses selected.
		/// </summary>
		/// <returns>The defenses selected.</returns>
		public IEnumerable<DefenseType> GetDefensesSelected(){
			if (!SelectedDefenses ())
				throw new InvalidOperationException ("Cannot get selected defenses when some aren't selected");

			return new List<DefenseType> () { 
				SlotOne,
				(DefenseType)SlotTwo,
				(DefenseType)SlotThree,
				(DefenseType)SlotFour,
				(DefenseType)SlotFive
			};
		}

		/// <summary>
		/// Selecteds the defenses.
		/// </summary>
		/// <returns><c>true</c>, if defenses was selecteded, <c>false</c> otherwise.</returns>
		public bool SelectedDefenses(){
			return _slotTwo != null && _slotThree != null && _slotFour != null && _slotFive != null;
		}
	}
}

