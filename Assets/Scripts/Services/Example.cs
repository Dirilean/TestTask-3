using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Services {
    public class Example : MonoBehaviour {
        [SerializeField] private EventService eventService;
        IEnumerator Start() {
            //registration
            var serverConnect = new ConnectManager();
            eventService.init(serverConnect);
            
            //settings
            eventService.cooldownBeforeSendInSeconds = 1;
            eventService.serverUrl = "0.0.0.0";
            
            //using
            yield return new WaitForSeconds(3f);
            eventService.trackEvent("openGameTime",DateTime.Now.ToLongTimeString());

            while (true) {
                yield return new WaitForSeconds(Random.Range(0,5));
                eventService.trackEvent("countOfGold",Random.Range(1,100).ToString());
            }
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