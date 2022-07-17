using System;
using System.Collections;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Services;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PlaymodeTests {
    public class TestEventManager {
        private EventService _eventService;
        private Mock<IConnectManager> _mockConnectionManager;
        
        [UnitySetUp]
        public IEnumerator setup() {
            _mockConnectionManager = new Mock<IConnectManager>();
            var gameObject = Object.Instantiate(new GameObject());
            _eventService = gameObject.AddComponent<EventService>();
            _eventService.init(_mockConnectionManager.Object);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator tearDown() {
            yield return null;
            _mockConnectionManager = null;
            _eventService = null;
        }

        [Test]
        public void createService() {
            Assert.IsTrue(_eventService != null, "Сервис не создался");
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

        [Test]
        public void setupServerURL() {
            _eventService.serverUrl = "http://google.com";
            Assert.IsTrue(_eventService.serverUrl == "http://google.com", "Валидный url не установился");

            _eventService.serverUrl = "";
            Assert.IsFalse(string.IsNullOrWhiteSpace(_eventService.serverUrl), "Установился неприемлимый url");

            _eventService.serverUrl = " ";
            Assert.IsFalse(string.IsNullOrWhiteSpace(_eventService.serverUrl), "Установился неприемлимый url");

            _eventService.serverUrl = null;
            Assert.IsFalse(string.IsNullOrWhiteSpace(_eventService.serverUrl), "Установился неприемлимый url");
        }

        [UnityTest]
        public IEnumerator sendDataWithSameType() {
            int countSending = 0;
            var sendingJsons = new List<string>();
            _mockConnectionManager.Setup(connectManager =>
                    connectManager.postCorutine("0.0.0.0", It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<Action>()))
                .Callback((string serverUrl, string json, Action onSuccess, Action onError) => {
                    countSending++;
                    Debug.Log(json);
                    sendingJsons.Add(json);
                }).Returns(mockedPostCorutine
                );

            _eventService.cooldownBeforeSendInSeconds = 1;
            _eventService.serverUrl = "0.0.0.0";
            _eventService.trackEvent("levelStart", "level:3");
            _eventService.trackEvent("levelStart", "level:4");


            Assert.IsTrue(countSending == 0, "Сообщение отправилось раньше времени");
            yield return new WaitForSeconds(1.1f);
            Assert.IsTrue(countSending == 1, "Сообщение не отправилось");
            Assert.IsTrue(
                sendingJsons[0] ==
                "{\"events\":[{\"type\":\"levelStart\",\"data\":\"level:3\"},{\"type\":\"levelStart\",\"data\":\"level:4\"}]}",
                $"Сообщение {sendingJsons[0]} сформировалось неверно");
        }
        
        [UnityTest]
        public IEnumerator sendDataWithDelay() {
            int countSending = 0;
            var sendingJsons = new List<string>();
            
            _mockConnectionManager.Setup(connectManager =>
                    connectManager.postCorutine("0.0.0.0", It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<Action>()))
                .Callback((string serverUrl, string json, Action onSuccess, Action onError) => {
                    countSending++;
                    Debug.Log(json);
                    sendingJsons.Add(json);
                }).Returns(mockedPostCorutine
                );

            _eventService.cooldownBeforeSendInSeconds = 1;
            _eventService.serverUrl = "0.0.0.0";

            _eventService.trackEvent("levelStart", "level:3");
            yield return new WaitForSeconds(0.9f);

            Assert.IsTrue(countSending == 0, "первое сообщение отправилось раньше времени");

            _eventService.trackEvent("levelStart", "level:4");
            yield return new WaitForSeconds(0.3f);

            //прошло 1.2 сек
            Assert.IsTrue(countSending == 1, "первое сообщение не отправилось спустя 1,1сек");
            //  Assert.IsTrue(sendingJsons[0] == "{\"events\":[{\"type\":\"levelStart\",\"data\":\"level:3\"},{\"type\":\"levelStart\",\"data\":\"level:4\"}]}", $"Сообщение {sendingJsons[0]} сформировалось неверно");
            
            //отправлен второй запрос
            _eventService.trackEvent("use", "scroll:4");
            yield return new WaitForSeconds(0.1f);

            //прошло 0,1 сек
            Assert.IsTrue(countSending == 1, "второе сообщение отправилось раньше времени");
            yield return new WaitForSeconds(1f);

            //прошло 2.1 сек
            Assert.IsTrue(countSending == 2, "второе сообщение не отправилось");
            //   Debug.Log(sendingJsons[1]);
            //   Assert.IsTrue(sendingJsons[1] == "{\"events\":[{\"use\":\"scroll:4\"}]}", $"Сообщение {sendingJsons[1]} сформировалось неверно");
        }

        private void onError() {
            Debug.Log("error");
        }

        private void onSuccess() {
            Debug.Log("success");
        }
        
        private IEnumerator mockedPostCorutine() {
            yield return null;
        }
    }
}
