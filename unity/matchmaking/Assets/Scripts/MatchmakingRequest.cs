using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.Matchmaking
{
    [System.Serializable]
    public class MatchmakingRequest : MonoBehaviour
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