using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using Xamarin.Forms;
using Scouty.Models.Local;

namespace Scouty.UI
{
	public partial class ConfirmPerformancePage : ContentPage
	{
		public ObservableCollection<GroupedRobotEvent> Groups { get; set; }
		public ConfirmPerformancePage (IEnumerable<RobotEvent> events)
		{
			InitializeComponent ();

			var groups = events.GroupBy (x => x.EventTime, (k, ev) => new GroupedRobotEvent (k, ev));

			Groups = new ObservableCollection<GroupedRobotEvent> ();
			foreach (var grp in groups)
				Groups.Add (grp);
			
			EventList.ItemsSource = Groups;
		}


	}

	public class GroupedRobotEvent : ObservableCollection<RobotEventUI> {
		public string ShortName { get; set; }
		public string LongName { get; set; }


		public GroupedRobotEvent(EventTime time, IEnumerable<RobotEvent> events){
			foreach (var ev in events) {
				Add (new RobotEventUI (ev));
				if (time != ev.EventTime)
					throw new ArgumentException ("Mismatch of event time!");
			}

			if (time == EventTime.Auto) {
				ShortName = "A";
				LongName = "Autonomous Period";
			} else if (time == EventTime.Final) {
				ShortName = "F";
				LongName = "Final Period";
			} else {
				ShortName = "T";
				LongName = "Teleop Period";
			}
		}
	}

	public class RobotEventUI {
		public RobotEvent Event { get; set; }
		public string Type { get; set; }

		public RobotEventUI(RobotEvent associatedEvent){
			Event = associatedEvent;

			// Get the string assoiciated with the event
			if (associatedEvent.EventType == EventType.AssistFive)
				Type = "Assist on Slot Five";
			else if (associatedEvent.EventType == EventType.AssistFour)
				Type = "Assist on Slot Four";
			else if (associatedEvent.EventType == EventType.AssistThree)
				Type = "Assist on Slot Three";
			else if (associatedEvent.EventType == EventType.AssistTwo)
				Type = "Assist on Slot Two";
			else if (associatedEvent.EventType == EventType.AssistOne)
				Type = "Assist on Slot One";
			else if (associatedEvent.EventType == EventType.BlockedShot)
				Type = "Blocked Shot";
			else if (associatedEvent.EventType == EventType.Challenge)
				Type = "Challenged Defense";
			else if (associatedEvent.EventType == EventType.CrossFive)
				Type = "Crossed Slot Five";
			else if (associatedEvent.EventType == EventType.CrossFour)
				Type = "Crossed Slot Four";
			else if (associatedEvent.EventType == EventType.CrossThree)
				Type = "Crossed Slot Three";
			else if (associatedEvent.EventType == EventType.CrossTwo)
				Type = "Crossed Slot Two";
			else if (associatedEvent.EventType == EventType.CrossOne)
				Type = "Crossed Slot One";
			else if (associatedEvent.EventType == EventType.Foul)
				Type = "FOUL!";
			else if (associatedEvent.EventType == EventType.Hang)
				Type = "Robot Hung";
			else if (associatedEvent.EventType == EventType.MakeHigh)
				Type = "Made a High Goal";
			else if (associatedEvent.EventType == EventType.MissHigh)
				Type = "Missed a High Goal";
			else if (associatedEvent.EventType == EventType.MakeLow)
				Type = "Made a Low Goal";
			else if (associatedEvent.EventType == EventType.MissLow)
				Type = "Missed Low Goal";
			else if (associatedEvent.EventType == EventType.ReachDefense)
				Type = "Reached The Defense";
			else if (associatedEvent.EventType == EventType.TechnicalFoul)
				Type = "TECHNICAL FOUL!!";
			else
				Type = "This is a bug....";
		}
	}
}

