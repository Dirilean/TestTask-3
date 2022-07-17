using UnityEngine;

namespace Services {
    public class Example {
        void main() {
            //registration
            var serverConnect = new ConnectManager();
            var gameObject = Object.Instantiate(new GameObject());
            var eventService = gameObject.AddComponent<EventService>();
         //   eventService.init(serverConnect);

            //settings
            eventService.cooldownBeforeSendInSeconds = 1;
            eventService.serverUrl = "http://google.com";
            
            //using
            eventService.trackEvent("startLevel","level:3");
        }
    }
}