using System.Collections;
using UnityEngine;

namespace Services {
    public class EventService : MonoBehaviour {
        const string EVENT_DATA_KEY = "eventData";

        private string _serverUrl;
        private int _cooldownBeforeSendInSeconds = 10;
        private WaitForSeconds _cooldownBeforeSend;
        private IEnumerator _currentIenumerator;
        private IConnectManager _serverConnect;
        private string _eventsDataJson;

        private EventsData _eventsData = new EventsData();

        public void init(IConnectManager serverConnect) {
            _serverConnect = serverConnect;
            loadUnsentEvents();
            if (_eventsData.events.Count > 0) startSendingIfNotStarted();
        }

        public string serverUrl {
            get { return _serverUrl; }
            set {
                if (string.IsNullOrWhiteSpace(value)) {
                    Debug.LogWarning("value must not be empty");
                    return;
                }

                _serverUrl = value;
            }
        }

        /// <summary>
        /// минимальное время повтора отправки очереди из скопившихся ивентов </summary>
        public int cooldownBeforeSendInSeconds {
            get { return _cooldownBeforeSendInSeconds; }
            set {
                if (value <= 0) {
                    Debug.LogWarning("value must not be less than or equal to 0");
                    return;
                }

                _cooldownBeforeSendInSeconds = value;
                _cooldownBeforeSend = new WaitForSeconds(_cooldownBeforeSendInSeconds);
            }
        }

        /// <summary>
        /// Сохранить ивент для отправки </summary>
        /// <param name="type">тип ивента</param>
        /// <param name="data">данные ивента</param>
        public void trackEvent(string type, string data) {
            _eventsData.events.Add(new EventData(type, data));
            
            _eventsDataJson = JsonUtility.ToJson(_eventsData);
            PlayerPrefs.SetString(EVENT_DATA_KEY, _eventsDataJson);
            
            startSendingIfNotStarted();
            
            Debug.Log($"track event: {type}:{data}");
        }

        private void startSendingIfNotStarted() {
            if (_currentIenumerator == null) {
                _currentIenumerator = sendEventsAfterCooldown();
                StartCoroutine(_currentIenumerator);
            }
        }

        private IEnumerator sendEventsAfterCooldown() {
            yield return _cooldownBeforeSend;

            string sendingEvents = (string)_eventsDataJson.Clone();
            IEnumerator r = _serverConnect.postCorutine(serverUrl, _eventsDataJson, () => removeSendedEvents(sendingEvents),
                startSendingIfNotStarted);
            StartCoroutine(r);
            _currentIenumerator = null;
        }

        private void removeSendedEvents(string jsonSendedEvents) {
            EventsData sendedEventsData = JsonUtility.FromJson<EventsData>(jsonSendedEvents);
            foreach (var sendedEvent in sendedEventsData.events) {
                _eventsData.events.Remove(sendedEvent);
            }
        }
        
        private void loadUnsentEvents() {
            string json = PlayerPrefs.GetString(EVENT_DATA_KEY);
            if (string.IsNullOrEmpty(json)) return;
            var eventsData = JsonUtility.FromJson<EventsData>(json);
            _eventsData.events.AddRange(eventsData.events);
            _eventsDataJson = json;
        }
    }
}

