using System.Collections;
using UnityEngine;

namespace Services {
    public class EventService : MonoBehaviour {
        const string EVENT_DATA_KEY = "eventData";

        private string _serverUrl;
        private int _cooldownBeforeSendInSeconds = 10;
        private IConnectManager _serverConnect;
        private string _eventsDataJson;
        private EventsData _eventsData = new EventsData();

        private WaitForSeconds _cooldownBeforeSend;
        private IEnumerator _sendingAfterCooldown;

        /// <summary>
        /// Инициализация сервиса </summary>
        /// <param name="serverConnect">Менеджер связи с сервером</param>
        public void init(IConnectManager serverConnect) {
            _serverConnect = serverConnect;
            loadUnsentEvents();
            if (_eventsData.events.Count > 0) startSendingIfNotStarted();
        }

        /// <summary>
        /// URL для отправки ивентов аналитики </summary>
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
        /// Минимальное время повтора отправки очереди из скопившихся ивентов </summary>
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
        /// Записать ивент</summary>
        /// <param name="type">тип ивента</param>
        /// <param name="data">данные ивента</param>
        public void trackEvent(string type, string data) {
            _eventsData.events.Add(new EventData(type, data));
            updateJsonOfEventsData();

            PlayerPrefs.SetString(EVENT_DATA_KEY, _eventsDataJson);
            startSendingIfNotStarted();

            Debug.Log($"track event: {type}:{data}");
        }

        private void startSendingIfNotStarted() {
            if (_sendingAfterCooldown == null) {
                _sendingAfterCooldown = sendEventsAfterCooldown();
                StartCoroutine(_sendingAfterCooldown);
            }
        }

        private IEnumerator sendEventsAfterCooldown() {
            yield return _cooldownBeforeSend;

            string sendingEvents = (string) _eventsDataJson.Clone();
            IEnumerator postCorutine = _serverConnect.postCorutine(serverUrl, _eventsDataJson,
                () => removeSuccessfullySendedEvents(sendingEvents), startSendingIfNotStarted);
            StartCoroutine(postCorutine);
            _sendingAfterCooldown = null;
        }

        private void removeSuccessfullySendedEvents(string jsonSendedEvents) {
            EventsData sendedEventsData = JsonUtility.FromJson<EventsData>(jsonSendedEvents);
            foreach (var sendedEvent in sendedEventsData.events) {
                _eventsData.events.Remove(sendedEvent);
            }

            updateJsonOfEventsData();
        }

        private void loadUnsentEvents() {
            string unsentEventsJson = PlayerPrefs.GetString(EVENT_DATA_KEY);
            if (string.IsNullOrEmpty(unsentEventsJson)) return;
            var eventsData = JsonUtility.FromJson<EventsData>(unsentEventsJson);
            _eventsData.events.AddRange(eventsData.events);
            _eventsDataJson = unsentEventsJson;
        }

        private void updateJsonOfEventsData() {
            _eventsDataJson = JsonUtility.ToJson(_eventsData);
        }
    }
}

