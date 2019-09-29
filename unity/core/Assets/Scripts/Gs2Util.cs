using System;
using UnityEngine;

namespace Gs2.Sample.Core
{
    public class Gs2Util
    {
        public static T LoadGlobalResource<T>(string fixedName = null) where T: MonoBehaviour
        {
            var requestGameObject = GameObject.Find(typeof(T).Name);
            var request = requestGameObject == null ? null : requestGameObject.GetComponent<T>();
            if (request == null)
            {
                var prefab = Resources.Load<GameObject>("Prefabs/" + typeof(T).Name);
                if (prefab == null)
                {
                    throw new InvalidProgramException("'" + typeof(T).Name + ".prefab' is not found.");
                }

                var gameObject = MonoBehaviour.Instantiate<GameObject>(prefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
                if (gameObject == null)
                {
                    throw new InvalidProgramException("'" + typeof(T).Name + ".prefab' is not found.");
                }

                if (fixedName != null)
                {
                    gameObject.name = fixedName;
                }

                request = gameObject.GetComponent<T>();
            }
            return request;
        }

        public static T LoadGlobalGameObject<T>(string gameObjectName) where T: MonoBehaviour
        {
            var requestGameObject = GameObject.Find(gameObjectName);
            return requestGameObject == null ? null : requestGameObject.GetComponent<T>();
        }

    }
}