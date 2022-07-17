using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Services {
    public class Example : MonoBehaviour {
        [SerializeField] private EventService _eventService;

        IEnumerator Start() {
            //registration
            var serverConnect = new ConnectManager();
            _eventService.init(serverConnect);

            //settings
            _eventService.cooldownBeforeSendInSeconds = 1;
            _eventService.serverUrl = "0.0.0.0";

            //using
            yield return new WaitForSeconds(3f);
            _eventService.trackEvent("openGameTime", DateTime.Now.ToLongTimeString());

            while (true) {
                yield return new WaitForSeconds(Random.Range(0, 5));
                _eventService.trackEvent("countOfGold", Random.Range(1, 100).ToString());
            }
            // ReSharper disable once IteratorNeverReturns
        }

        [ContextMenu("ReadPlayerPrefs")]
        private void readPlayerPrefs() {
            Debug.Log($"playerPrefs contains {PlayerPrefs.GetString("eventData")}");
        }

        [ContextMenu("DeletePlayerPrefs")]
        private void deletePlayerPrefs() {
            PlayerPrefs.DeleteAll();
        }
    }
}