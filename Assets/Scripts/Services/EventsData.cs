using System;
using System.Collections.Generic;

namespace Services {
    [Serializable]
    public class EventsData {
        public List<EventData> events = new List<EventData>();
    }
}