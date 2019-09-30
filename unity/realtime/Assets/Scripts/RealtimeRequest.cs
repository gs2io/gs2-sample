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
        /// ゲームサーバ固有のID
        /// </summary>
        public string gatheringId;

        /// <summary>
        /// ゲームサーバのIPアドレス
        /// </summary>
        public string ipAddress;

        /// <summary>
        /// ゲームサーバの待ち受けポート
        /// </summary>
        public int port;

        /// <summary>
        /// ゲームサーバとの通信に使用する暗号鍵s
        /// </summary>
        public string encryptionKey;

        private void Start()
        {
            DontDestroyOnLoad (this);
        }
    }
}