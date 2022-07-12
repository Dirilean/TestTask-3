using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Services {
    public class EventService : MonoBehaviour {
        private string _serverUrl;

        private int _cooldownBeforeSendInSeconds = 10;
        private WaitForSeconds _cooldownBeforeSend;
        private IEnumerator _currentIenumerator;

        private List<(string, string)> _events = new List<(string, string)>();

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
        
        public string serverUrl {
            get { return _serverUrl;}
            set {
                if (string.IsNullOrWhiteSpace(value)) {
                    Debug.LogWarning("value must not be empty"); 
                    return;
                }

                _serverUrl = value;
            }
        }

        /// <summary>
        /// Сохранить ивент для отправки </summary>
        /// <param name="type">тип ивента</param>
        /// <param name="data">данные ивента</param>
        public void trackEvent(string type, string data) {
            _events.Add((type, data));
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

            (string, string)[] sendedEvents = _events.ToArray();
            string json = JsonUtility.ToJson(sendedEvents);
            StartCoroutine(postCorutine(json, () => removeSendedEvents(sendedEvents), startSendingIfNotStarted));
            _currentIenumerator = null;
        }

        private void removeSendedEvents((string, string)[] sendedEvents) {
            foreach (var sendedEvent in sendedEvents) {
                _events.Remove(sendedEvent);
            }
        }

        /// <summary>
        /// Отправка post запроса </summary>
        private IEnumerator postCorutine(string json, Action onSuccess, Action onError) {
            Debug.Log("send: " + json);
            UnityWebRequest www = UnityWebRequest.Put(_serverUrl, json);
            www.method = "POST";
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");

            yield return www.SendWebRequest();

            Debug.Log(www.downloadHandler.text);

            if (www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
                onError.Invoke();
            }
            else {
                onSuccess.Invoke();
            }
        }
    }
}