using System.Collections;
using UnityEngine;

namespace VoiceMod.Utilities
{
    class SharedCoroutineStarter : MonoBehaviour
    {
        private static SharedCoroutineStarter _instance;

        public static new Coroutine StartCoroutine(IEnumerator routine)
        {
            _instance ??= new GameObject("Shared Coroutine Starter").AddComponent<SharedCoroutineStarter>();

            return ((MonoBehaviour)_instance).StartCoroutine(routine);
        }
    }
}
