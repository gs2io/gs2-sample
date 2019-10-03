using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.AccountTakeOver
{
    [System.Serializable]
    public class AccountTakeOverRequest : MonoBehaviour
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