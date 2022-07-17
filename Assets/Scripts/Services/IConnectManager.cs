using System;
using System.Collections;

namespace Services {
    public interface IConnectManager {
        public IEnumerator postCorutine(string serverUrl, string json, Action onSuccess, Action onError);
    }
}