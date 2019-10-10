using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.Money
{
    [System.Serializable]
    public class MoneyRequest : MonoBehaviour
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