using System;

namespace Services {
    [Serializable]
    public struct EventsData {
        public EventData[] events;

        public EventsData(EventData[] events) {
            this.events = events;
        }
    }
}