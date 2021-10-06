using System.Collections.Generic;
using Gs2.Core.Exception;
using Gs2.Gs2Realtime.Message;
using Gs2.Unity.Gs2Realtime;
using Gs2.Unity.Gs2Realtime.Model;
using UnityEngine;
using UnityEngine.Events;
using Gs2.Util.WebSocketSharp;

namespace Scenes.Menu
{
    public class Gs2RealtimeSetting : MonoBehaviour
    {
        [System.Serializable]
        public class RelayMessageEvent : UnityEvent<RelayBinaryMessage>
        {
        }
    
        [System.Serializable]
        public class GetRoomEvent : UnityEvent<EzRoom>
        {
        }

        [System.Serializable]
        public class JoinPlayerEvent : UnityEvent<Player>
        {
        }

        [System.Serializable]
        public class LeavePlayerEvent : UnityEvent<Player>
        {
        }
        
        [System.Serializable]
        public class UpdateProfileEvent : UnityEvent<Player>
        {
        }
        
        [System.Serializable]
        public class ErrorEvent : UnityEvent<Gs2Exception>
        {
        }

        [System.Serializable]
        public class RelayErrorEvent : UnityEvent<Error>
        {
        }

        [System.Serializable]
        public class GeneralErrorEvent : UnityEvent<ErrorEventArgs>
        {
        }
        
        [System.Serializable]
        public class CloseEvent : UnityEvent<CloseEventArgs>
        {
        }

        /// <summary>
        /// GS2-Realtime のネームスペース名
        /// </summary>
        public string realtimeNamespaceName;

        /// <summary>
        /// 
        /// </summary>
        public RelayMessageEvent onRelayMessage = new RelayMessageEvent();
        
        /// <summary>
        /// 
        /// </summary>
        public GetRoomEvent onGetRoom = new GetRoomEvent();

        /// <summary>
        /// 
        /// </summary>
        public JoinPlayerEvent onJoinPlayer = new JoinPlayerEvent();

        /// <summary>
        /// 
        /// </summary>
        public LeavePlayerEvent onLeavePlayer = new LeavePlayerEvent();

        /// <summary>
        /// 
        /// </summary>
        public UpdateProfileEvent onUpdateProfile = new UpdateProfileEvent();
        
        /// <summary>
        /// 
        /// </summary>
        public CloseEvent onClose = new CloseEvent();
        
        /// <summary>
        /// 
        /// </summary>
        public RelayErrorEvent onRelayError = new RelayErrorEvent();
        
        /// <summary>
        /// 
        /// </summary>
        public GeneralErrorEvent onGeneralError = new GeneralErrorEvent();
        
        /// <summary>
        /// 
        /// </summary>
        public ErrorEvent onError = new ErrorEvent();

    }
}