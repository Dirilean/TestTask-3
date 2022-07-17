using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Services {
    public class EventService : MonoBehaviour {
        private string _serverUrl;
        private int _cooldownBeforeSendInSeconds = 10;
        private WaitForSeconds _cooldownBeforeSend;
        private IEnumerator _currentIenumerator;
        private IConnectManager _serverConnect;
        
        private List<EventData> _events = new List<EventData>();

        public void init(IConnectManager serverConnect) {
            _serverConnect = serverConnect;
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
            _events.Add(new EventData(type, data));
            startSendingIfNotStarted();
        }

        private void startSendingIfNotStarted() {
            if (_currentIenumerator == null) {
                _currentIenumerator = sendEventsAfterCooldown();
                StartCoroutine(_currentIenumerator);
            }
        }

        private IEnumerator sendEventsAfterCooldown() {
            yield return _cooldownBeforeSend;

            var eventsData = new EventsData(_events.ToArray());
            string json = JsonUtility.ToJson(eventsData);
            IEnumerator r = _serverConnect.postCorutine(serverUrl, json, () => removeSendedEvents(eventsData.events),
                startSendingIfNotStarted);
            StartCoroutine(r);
            _currentIenumerator = null;
        }

        private void removeSendedEvents(EventData[] sendedEvents) {
            foreach (var sendedEvent in sendedEvents) {
                _events.Remove(sendedEvent);
            }
        }
        


        [Serializable]
        public struct EventsData {
            public EventData[] events;

            public EventsData(EventData[] events) {
                this.events = events;
            }
        }
        [Serializable]
        public struct EventData {
            public string type;
            public string data;

            public EventData(string type, string data) {
                this.type = type;
                this.data = data;
            }
        }
    }
}

