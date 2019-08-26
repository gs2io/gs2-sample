using Gs2.Unity.Gs2Matchmaking.Model;
using Gs2.Unity.Gs2Realtime.Model;
using Gs2.Unity.Util;
using UnityEngine;

namespace Scenes.Realtime
{
    public class RealtimeRequest : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public GameSession gameSession;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public EzGathering gathering;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public EzRoom room;

        private void Start()
        {
            DontDestroyOnLoad (this);
        }
    }
}