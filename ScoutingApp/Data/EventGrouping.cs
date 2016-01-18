using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using ScoutingModels.Data;
using ScoutingModels.Test;

namespace ScoutingApp.Data
{
    public class EventGrouping : ObservableCollection<Event>
    {
        public int Week { get; set; }

        public string GroupName
        {
            get
            {
                if (Week == 0)
                {
                    return "Pre-Season";
                }
                return $"Week {Week}";  
            }
        }

        public string ShortName
        {
            get
            {
                if (Week == 0)
                {
                    return "P";
                }
                return $"W{Week}";
            }
        }

        public EventGrouping(int week, IEnumerable<Event> events)
        {
            events.IsNotNull();
            Week = week;
            foreach (var e in events)
            {
                e.IsNotNull();
                Add(e);
            }
        }
    }
}
