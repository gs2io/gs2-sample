using System;
using UnityEngine;

namespace Gs2.Sample.Core
{
    public class Gs2Util
    {
        public static T LoadGlobalResource<T>() where T: MonoBehaviour
        {
            var requestGameObject = GameObject.FindWithTag(typeof(T).Name);
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

                request = gameObject.GetComponent<T>();
                request.tag = typeof(T).Name;
            }
            return request;
        }

    }
}