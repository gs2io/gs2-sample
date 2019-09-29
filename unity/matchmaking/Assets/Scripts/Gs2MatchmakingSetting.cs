using System.Collections.Generic;
using Gs2.Core.Exception;
using Gs2.Unity.Gs2Matchmaking.Model;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Matchmaking
{
    [System.Serializable]
    public class JoinPlayerEvent : UnityEvent<EzGathering,  string>
    {
    }

    [System.Serializable]
    public class LeavePlayerEvent : UnityEvent<EzGathering,  string>
    {
    }

    [System.Serializable]
    public class UpdateJoinedPlayerIdsEvent : UnityEvent<EzGathering, List<string>>
    {
    }

    [System.Serializable]
    public class MatchmakingCompleteEvent : UnityEvent<EzGathering, List<string>>
    {
    }
    
    [System.Serializable]
    public class MatchmakingCancelEvent : UnityEvent<EzGathering>
    {
    }

    [System.Serializable]
    public class ErrorEvent : UnityEvent<Gs2Exception>
    {
    }

    public class Gs2MatchmakingSetting : MonoBehaviour
    {
        /// <summary>
        /// GS2-Matchmaking のネームスペース名
        /// </summary>
        [SerializeField]
        public string matchmakingNamespaceName;
        
        /// <summary>
        /// 新しいプレイヤーがギャザリングに参加したとき
        /// </summary>
        [SerializeField]
        public JoinPlayerEvent onJoinPlayer = new JoinPlayerEvent();
        
        /// <summary>
        /// プレイヤーがギャザリングから離脱したとき
        /// </summary>
        [SerializeField]
        public LeavePlayerEvent onLeavePlayer = new LeavePlayerEvent();
        
        /// <summary>
        /// 参加中のプレイヤー一覧が更新されたとき
        /// </summary>
        [SerializeField]
        public UpdateJoinedPlayerIdsEvent onUpdateJoinedPlayerIds = new UpdateJoinedPlayerIdsEvent();

        /// <summary>
        /// マッチメイキングが完了したとき
        /// </summary>
        [SerializeField]
        public MatchmakingCompleteEvent onMatchmakingComplete = new MatchmakingCompleteEvent();

        /// <summary>
        /// マッチメイキングをキャンセルしたとき
        /// </summary>
        [SerializeField]
        public MatchmakingCancelEvent onMatchmakingCancel = new MatchmakingCancelEvent();

        /// <summary>
        /// エラー発生時に発行されるイベント
        /// </summary>
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();

    }
}