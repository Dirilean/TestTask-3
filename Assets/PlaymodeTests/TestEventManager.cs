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
            yield return null;

            _mockConnectionManager = new Mock<IConnectManager>();
            var gameObject = Object.Instantiate(new GameObject());
            _eventService = gameObject.AddComponent<EventService>();
            _eventService.init(_mockConnectionManager.Object);

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator tearDown() {
            yield return null;
            PlayerPrefs.DeleteAll();
            _mockConnectionManager = null;
            _eventService = null;
        }

        [Test]
        public void createService() {
            Assert.IsTrue(_eventService != null, "Сервис не создался");
        }

        /// <summary>
        /// Установка валидных и невалидных кулдаунов для отправки сообщений </summary>
        [Test]
        public void setupCooldown() {
            _eventService.cooldownBeforeSendInSeconds = 15;
            Assert.IsTrue(_eventService.cooldownBeforeSendInSeconds == 15, "Валидный кулдаун не установился");

            _eventService.cooldownBeforeSendInSeconds = -10;
            Assert.IsFalse(_eventService.cooldownBeforeSendInSeconds <= 0, "Установился неприемлимый кулдаун");

            _eventService.cooldownBeforeSendInSeconds = -10;
            Assert.IsFalse(_eventService.cooldownBeforeSendInSeconds <= 0, "Установился неприемлимый кулдаун");
        }

        /// <summary>
        /// Установка валидных и невалидных url </summary>
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

        /// <summary>
        /// Отправка на отслеживание ивентов с одинаковым ключом </summary>
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

        /// <summary>
        /// Отправка ивентов с задержками превышающими и не превышающими кулдаун с успешным ответом</summary>
        [UnityTest]
        public IEnumerator sendDataWithDelayWithSuccess() {
            int countSending = 0;
            var sendingJsons = new List<string>();

            _mockConnectionManager.Setup(connectManager =>
                    connectManager.postCorutine("0.0.0.0", It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<Action>()))
                .Callback((string serverUrl, string json, Action onSuccess, Action onError) => {
                    countSending++;
                    Debug.Log($"mock try send json: {json}");
                    sendingJsons.Add(json);
                    onSuccess.Invoke();
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
            Assert.IsTrue(
                sendingJsons[0] ==
                "{\"events\":[{\"type\":\"levelStart\",\"data\":\"level:3\"},{\"type\":\"levelStart\",\"data\":\"level:4\"}]}",
                $"Сообщение {sendingJsons[0]} сформировалось неверно");

            //отправлен второй запрос
            _eventService.trackEvent("use", "scroll:4");
            yield return new WaitForSeconds(0.1f);

            //прошло 0,1 сек
            Assert.IsTrue(countSending == 1, "второе сообщение отправилось раньше времени");
            yield return new WaitForSeconds(1f);

            //прошло 2.1 сек
            Assert.IsTrue(countSending == 2, "второе сообщение не отправилось");
            Assert.IsTrue(sendingJsons[1] == "{\"events\":[{\"type\":\"use\",\"data\":\"scroll:4\"}]}",
                $"Сообщение {sendingJsons[1]} сформировалось неверно");
        }

        /// <summary>
        ///  Отправка ивентов с задержками превышающими и не превышающими кулдаун с возвращенной ошибкой от сервера </summary>
        [UnityTest]
        public IEnumerator sendDataWithDelayWithError() {
            int countSending = 0;
            var sendingJsons = new List<string>();

            _mockConnectionManager.Setup(connectManager =>
                    connectManager.postCorutine("0.0.0.0", It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<Action>()))
                .Callback((string serverUrl, string json, Action onSuccess, Action onError) => {
                    countSending++;
                    Debug.Log($"mock try send json: {json}");
                    sendingJsons.Add(json);
                    onError.Invoke();
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
            Assert.IsTrue(
                sendingJsons[0] ==
                "{\"events\":[{\"type\":\"levelStart\",\"data\":\"level:3\"},{\"type\":\"levelStart\",\"data\":\"level:4\"}]}",
                $"Сообщение {sendingJsons[0]} сформировалось неверно");

            //отправлен второй запрос
            _eventService.trackEvent("use", "scroll:4");
            yield return new WaitForSeconds(0.1f);

            //прошло 0,1 сек
            Assert.IsTrue(countSending == 1, "второе сообщение отправилось раньше времени");
            yield return new WaitForSeconds(1f);

            //прошло 2.1 сек
            Assert.IsTrue(countSending == 2, "второе сообщение не отправилось");
            Assert.IsTrue(
                sendingJsons[1] ==
                "{\"events\":[{\"type\":\"levelStart\",\"data\":\"level:3\"},{\"type\":\"levelStart\",\"data\":\"level:4\"},{\"type\":\"use\",\"data\":\"scroll:4\"}]}",
                $"Сообщение {sendingJsons[1]} сформировалось неверно");
        }

        /// <summary>
        ///  Отправка сохраненных ивентов после рестарта менеджера </summary>
        [UnityTest]
        public IEnumerator sendSavedDataAfterRestart() {
            int countSending = 0;
            var sendingJsons = new List<string>();

            _mockConnectionManager.Setup(connectManager =>
                    connectManager.postCorutine("0.0.0.0", It.IsAny<string>(), It.IsAny<Action>(), It.IsAny<Action>()))
                .Callback((string serverUrl, string json, Action onSuccess, Action onError) => {
                    countSending++;
                    Debug.Log($"mock try send json: {json}");
                    sendingJsons.Add(json);
                    onSuccess.Invoke();
                }).Returns(mockedPostCorutine
                );

            _eventService.cooldownBeforeSendInSeconds = 1;
            _eventService.serverUrl = "0.0.0.0";

            _eventService.trackEvent("levelStart", "level:3");
            yield return null;

            //deleting eventService
            GameObject.DestroyImmediate(_eventService.gameObject);
            _eventService = null;
            yield return null;

            //create eventService again
            var gameObject = Object.Instantiate(new GameObject());
            _eventService = gameObject.AddComponent<EventService>();
            _eventService.init(_mockConnectionManager.Object);
            _eventService.cooldownBeforeSendInSeconds = 1;
            _eventService.serverUrl = "0.0.0.0";

            yield return new WaitForSeconds(1.1f);
            Assert.IsTrue(countSending == 1, "первое сообщение не отправилось спустя 1,1сек");
            Assert.IsTrue(
                sendingJsons[0] ==
                "{\"events\":[{\"type\":\"levelStart\",\"data\":\"level:3\"}]}",
                $"Сообщение {sendingJsons[0]} сформировалось неверно");
        }

        private IEnumerator mockedPostCorutine() {
            yield return null;
        }
    }
}
