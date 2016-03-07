﻿using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using Xamarin.Forms;
using Scouty.Models.Local;
using Scouty.Utility;

namespace Scouty.UI
{
	public partial class ConfirmPerformancePage : ContentPage
	{
		static Logger logger = new Logger (typeof(ConfirmPerformancePage));
		public ObservableCollection<GroupedRobotEvent> Groups { get; set; }
		public ConfirmPerformancePage (IEnumerable<RobotEvent> events)
		{
			InitializeComponent ();

			var groups = events.GroupBy (x => x.EventTime, (k, ev) => new GroupedRobotEvent (k, ev));

			Groups = new ObservableCollection<GroupedRobotEvent> ();
			foreach (var grp in groups)
				Groups.Add (grp);
			
			EventList.ItemsSource = Groups;
			EventList.ItemSelected += SelectedEvent;
			ToolbarItems.Add (new ToolbarItem ("Submit", null, Confirm));
		}

		void Confirm(){
			
		}

		public async void SelectedEvent(object sender, SelectedItemChangedEventArgs e){
			var selectedEvent = e.SelectedItem as RobotEventUI;

			EventList.SelectedItem = null;

			if (selectedEvent != null) {
				var eventTimesFull = new List<EventTimeUI> () { new EventTimeUI(EventTime.Auto), new EventTimeUI(EventTime.Final), new EventTimeUI(EventTime.Teleop) }
					.Where(x => x.Time != selectedEvent.Event.EventTime)
					.ToArray();

				var eventTimes = eventTimesFull.Select (x => x.Name).ToArray ();
				
				// Lets display an action sheet for this
				var title = $"What would you like to do about {selectedEvent.Type} during {selectedEvent.Event.EventTime.ToString()}";
				var selectedAction = await DisplayActionSheet (title, "Nothing", "Destroy", eventTimes);

				// See what the user selected
				if (selectedAction == "Nothing") {
					logger.Info ("Nothing Selected");
				} else if (selectedAction == "Destroy") {
					// Remove the event
					Groups.First (x => x.Time == selectedEvent.Event.EventTime).Remove (selectedEvent);
				} else {
					var selectedTime = eventTimesFull.FirstOrDefault (x => x.Name == selectedAction);
					if (selectedTime != null){
						// Move the group over to its new time group
						Groups.First (x => x.Time == selectedEvent.Event.EventTime).Remove (selectedEvent);
						selectedEvent.Event.EventTime = selectedTime.Time;
						var newGrp = Groups.FirstOrDefault (x => x.Time == selectedTime.Time);

						if (newGrp == null) {
							newGrp = new GroupedRobotEvent (selectedTime.Time, new List<RobotEvent> () { selectedEvent.Event });
							Groups.Add (newGrp);
						} else {
							newGrp.Add (selectedEvent);
						}
					}
				}
			}
		}
	}

	public class EventTimeUI{
		public const string AUTONOMOUS_NAME = "Change To Autonomous";
		public const string FINAL_NAME = "Change To Final";
		public const string TELEOP_NAME = "Change To Teleop";
		public EventTime Time { get; set; }
		public string Name { get; set; }

		public EventTimeUI(EventTime time){
			Time = time;

			if (time == EventTime.Auto) {
				Name = AUTONOMOUS_NAME;
			} else if (time == EventTime.Final)
				Name = FINAL_NAME;
			else
				Name = TELEOP_NAME;
		}
	}

	public class GroupedRobotEvent : ObservableCollection<RobotEventUI> {
		public string ShortName { get; set; }
		public string LongName { get; set; }
		public EventTime Time { get; set; }

		public GroupedRobotEvent(EventTime time, IEnumerable<RobotEvent> events){
			foreach (var ev in events) {
				Add (new RobotEventUI (ev));
				if (time != ev.EventTime)
					throw new ArgumentException ("Mismatch of event time!");
			}

			Time = time;

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
