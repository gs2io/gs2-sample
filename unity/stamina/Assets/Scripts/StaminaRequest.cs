using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.Stamina
{
    [System.Serializable]
    public class StaminaRequest : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public GameSession gameSession;

        private void Start()
        {
            DontDestroyOnLoad (this);
        }
    }
}