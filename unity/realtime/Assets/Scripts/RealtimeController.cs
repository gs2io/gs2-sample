using System.Collections;
using Google.Protobuf;
using Gs2.Core;
using Gs2.Sample.Core;
using Gs2.Unity.Gs2Realtime.Result;
using Gs2.Unity.Gs2Realtime;
using Scenes.Menu;
using UnityEngine;
using UnityEngine.Events;

namespace Scenes.Realtime
{
    public class RealtimeController : MonoBehaviour
    {
        /// <summary>
        /// GS2-Realtime の設定値
        /// </summary>
        [SerializeField]
        public Gs2RealtimeSetting gs2RealtimeSetting;

        /// <summary>
        /// Gs2Client
        /// </summary>
        [SerializeField]
        public Gs2Client gs2Client;

        private void Validate()
        {
            if (!gs2RealtimeSetting)
            {
                gs2RealtimeSetting = Gs2Util.LoadGlobalGameObject<Gs2RealtimeSetting>("Gs2Settings");
            }

            if (!gs2Client)
            {
                gs2Client = Gs2Util.LoadGlobalGameObject<Gs2Client>("Gs2Settings");
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            Validate();
        }

        /// <summary>
        /// GS2-Realtime のギャザリング情報から GS2-Realtime のルーム情報を取得
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator GetRoom(
            UnityAction<AsyncResult<EzGetRoomResult>> callback
        )
        {
            Validate();

            var request = Gs2Util.LoadGlobalGameObject<RealtimeRequest>("RealtimeRequest");

            AsyncResult<EzGetRoomResult> result = null;
            yield return gs2Client.client.Realtime.GetRoom(
                r => { result = r; },
                gs2RealtimeSetting.realtimeNamespaceName,
                request.gatheringId
            );
            
            if (result.Error != null)
            {
                gs2RealtimeSetting.onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }

            gs2RealtimeSetting.onGetRoom.Invoke(result.Result.Item);

            callback.Invoke(result);
        }

        /// <summary>
        /// GS2-Realtime のルームを作成
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <param name="encryptionKey"></param>
        /// <returns></returns>
        public IEnumerator ConnectRoom(
            UnityAction<AsyncResult<RealtimeSession>> callback,
            string ipAddress,
            int port,
            string encryptionKey
        )
        {
            var request = Gs2Util.LoadGlobalGameObject<RealtimeRequest>("RealtimeRequest");

            var session = new RelayRealtimeSession(
                request.gameSession.AccessToken.token,
                ipAddress,
                port,
                encryptionKey,
                ByteString.CopyFrom()
            );
            
            session.OnRelayMessage += message =>
            {
                gs2RealtimeSetting.onRelayMessage.Invoke(message);
            }; 
            session.OnJoinPlayer += player =>
            {
                gs2RealtimeSetting.onJoinPlayer.Invoke(player);
            };
            session.OnLeavePlayer += player =>
            {
                gs2RealtimeSetting.onLeavePlayer.Invoke(player);
            };
            session.OnGeneralError += args => 
            {
                gs2RealtimeSetting.onGeneralError.Invoke(args);
            };
            session.OnError += error =>
            {
                gs2RealtimeSetting.onRelayError.Invoke(error);
            };
            session.OnUpdateProfile += player =>
            {
                gs2RealtimeSetting.onUpdateProfile.Invoke(player);
            };
            session.OnClose += args =>
            {
                gs2RealtimeSetting.onClose.Invoke(args);
            };

            AsyncResult<bool> result = null;
            yield return session.Connect(
                this,
                r =>
                {
                    result = r;
                }
            );

            if (session.Connected)
            {
                callback.Invoke(
                    new AsyncResult<RealtimeSession>(session, null)
                );
            }
            else
            {
                callback.Invoke(
                    new AsyncResult<RealtimeSession>(null, result.Error)
                );
            }
        }

        /// <summary>
        /// 他プレイヤーの座標情報を同期
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator SyncPlayerProfiles(
            UnityAction<AsyncResult<object>> callback
        )
        {
            callback.Invoke(null);
            yield break;
        }
    }
}
