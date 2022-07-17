using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Services {
    public class ConnectManager : IConnectManager {
        //public Action<string> postCorutineSend;

        /// <summary>
        /// Отправка post запроса </summary>
        public IEnumerator postCorutine(string serverUrl, string json, Action onSuccess, Action onError) {
            //postCorutineSend?.Invoke(json);
            UnityWebRequest www = UnityWebRequest.Put(serverUrl, json);
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