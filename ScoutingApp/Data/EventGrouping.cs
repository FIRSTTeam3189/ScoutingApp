using System;
using System.Collections.Generic;
using System.Text;
using ScoutingModels.Data;
using ScoutingModels.Test;

namespace ScoutingApp.Data
{
    public class EventGrouping
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

        public ICollection<Event> Events { get; set; }

        public EventGrouping(int week, ICollection<Event> events)
        {
            events.IsNotNull();
            Events = events;
            Week = week;
        }
    }
}
