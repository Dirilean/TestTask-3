using System.Collections;
using NUnit.Framework;
using Services;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests {
    public class TestEventManager {
        private EventService _eventService;

        [UnitySetUp]
        public IEnumerator setup() {
            var gameObject = Object.Instantiate(new GameObject());
            _eventService = gameObject.AddComponent<EventService>();
            
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator tearDown() {
            yield return null;
            _eventService = null;
        }

        [UnityTest]
        public IEnumerator createService() {
            Assert.IsTrue(_eventService!=null,"Сервис не создался");
            yield return null;
        }

        [Test]
        public void setupCooldown() {
            _eventService.cooldownBeforeSendInSeconds = 15;
            Assert.IsTrue(_eventService.cooldownBeforeSendInSeconds == 15, "Валидный кулдаун не установился");

            _eventService.cooldownBeforeSendInSeconds = -10;
            Assert.IsFalse(_eventService.cooldownBeforeSendInSeconds <= 0, "Установился неприемлимый кулдаун");

            _eventService.cooldownBeforeSendInSeconds = -10;
            Assert.IsFalse(_eventService.cooldownBeforeSendInSeconds <= 0, "Установился неприемлимый кулдаун");
        }
        
        [UnityTest]
        public IEnumerator setupServerURL() {
            _eventService.serverUrl = "http://google.com";
            Assert.IsTrue(_eventService.serverUrl == "http://google.com", "Валидный url не установился");
            
            _eventService.serverUrl = "";
            Assert.IsFalse(string.IsNullOrWhiteSpace(_eventService.serverUrl), "Установился неприемлимый url");

            _eventService.serverUrl = " ";
            Assert.IsFalse(string.IsNullOrWhiteSpace(_eventService.serverUrl), "Установился неприемлимый url");
            
            _eventService.serverUrl = null;
            Assert.IsFalse(string.IsNullOrWhiteSpace(_eventService.serverUrl), "Установился неприемлимый url");
            yield return null;
        }
    }
}
