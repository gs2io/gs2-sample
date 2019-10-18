using System;
using System.Collections.Generic;
using System.Linq;
using LitJson;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gs2.Sample.Core
{
    public class Gs2Util
    {
        public static T LoadGlobalResource<T>(string fixedName = null) where T: MonoBehaviour
        {
            var requestGameObject = GameObject.Find(typeof(T).Name);
            var request = requestGameObject == null ? null : requestGameObject.GetComponent<T>();
            if (request) return request;
            
            var prefab = Resources.Load<GameObject>("Prefabs/" + typeof(T).Name);
            if (!prefab)
            {
                throw new InvalidProgramException("'" + typeof(T).Name + ".prefab' is not found.");
            }

            var gameObject = Object.Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            if (!gameObject)
            {
                throw new InvalidProgramException("'" + typeof(T).Name + ".prefab' is not found.");
            }

            if (fixedName != null)
            {
                gameObject.name = fixedName;
            }

            request = gameObject.GetComponent<T>();
            return request;
        }

        public static T LoadGlobalGameObject<T>(string gameObjectName) where T: MonoBehaviour
        {
            var requestGameObject = GameObject.Find(gameObjectName);
            return requestGameObject == null ? null : requestGameObject.GetComponent<T>();
        }

        public static JsonData RemovePlaceholder(JsonData json)
        {
            var removeKeys = json.Keys.Where(key => json[key].IsString).Where(key => ((string) json[key])[0] == '#').ToList();
            foreach (var key in removeKeys)
            {
                json[key] = null;
            }

            return json;
        }
    }
}