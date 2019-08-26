using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Core.Model;
using Gs2.Gs2Auth.Model;
using Gs2.Gs2Matchmaking.Model;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Gs2Gateway.Result;
using Gs2.Unity.Gs2Matchmaking.Model;
using Gs2.Unity.Gs2Matchmaking.Result;
using Gs2.Unity.Gs2Realtime;
using Gs2.Unity.Gs2Realtime.Model;
using Gs2.Unity.Gs2Realtime.Result;
using Gs2.Unity.Util;
using LitJson;
using Scenes.AccountMenu.Model;
using Scenes.AccountMenu.Repository;
using Scenes.MatchmakingMenu;
using Scenes.MatchmakingMenu.Model;
using Scenes.Menu;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes.Realtime
{
    public delegate void ChangeStateHandler(RealtimeStateMachine.State state);
    public delegate void ErrorHandler(Gs2Exception error);
    public delegate void DisconnectHandler();
    
    public class RealtimeStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            Initialize,
            GetRoom,
            ConnectRoom,
            SyncPlayerProfiles,
            Main,
            Disconnected,
            Error,
        }

        public enum Trigger
        {
            InitializeSucceed,
            InitializeFailed,
            GetRoomSucceed,
            GetRoomFailed,
            ConnectRoomSucceed,
            ConnectRoomFailed,
            SyncPlayerProfilesSucceed,
            Disconnect,
            ConfirmDisconnect,
            ConfirmError,
        }

        /// <summary>
        /// ギャザリング情報
        /// </summary>
        private EzGathering _gathering;

        /// <summary>
        /// 
        /// </summary>
        public RelayRealtimeSession session;
        
        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<uint, Gs2.Gs2Realtime.Message.Player> players = new Dictionary<uint, Gs2.Gs2Realtime.Message.Player>();

        /// <summary>
        /// ルーム情報
        /// </summary>
        public EzRoom room;

        /// <summary>
        /// GS2クライアント
        /// </summary>
        private Gs2.Unity.Client _client;

        /// <summary>
        /// GS2プロファイル
        /// </summary>
        private Gs2.Unity.Util.Profile _profile;

        /// <summary>
        /// GS2の設定値
        /// </summary>
        private Gs2RealtimeSetting _setting;

        /// <summary>
        /// ゲームセッション
        /// </summary>
        private GameSession _gameSession;

        /// <summary>
        /// 
        /// </summary>
        private MonoBehaviour _monoBehaviour;

        /// <summary>
        /// 
        /// </summary>
        public event ChangeStateHandler OnChangeState;

        /// <summary>
        /// エラー発生時に発行されるイベント
        /// </summary>
        public event ErrorHandler OnError;

        /// <summary>
        /// 
        /// </summary>
        public event OnRelayMessageHandler OnRelayMessage;
        
        /// <summary>
        /// 
        /// </summary>
        public event OnJoinPlayerHandler OnJoinPlayer;
        
        /// <summary>
        /// 
        /// </summary>
        public event OnLeavePlayerHandler OnLeavePlayer;
        
        /// <summary>
        /// 
        /// </summary>
        public event OnUpdateProfileHandler OnUpdateProfile;
        
        /// <summary>
        /// 
        /// </summary>
        public event OnCloseHandler OnClose;
        
        /// <summary>
        /// 
        /// </summary>
        public bool initialized;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="profile"></param>
        /// <param name="setting"></param>
        /// <param name="gameSession"></param>
        /// <param name="gathering"></param>
        /// <param name="monoBehaviour"></param>
        /// <param name="staticRoom"></param>
        public void Initialize(
            Gs2.Unity.Client client,
            Gs2.Unity.Util.Profile profile,
            Gs2RealtimeSetting setting,
            GameSession gameSession,
            EzGathering gathering,
            MonoBehaviour monoBehaviour,
            EzRoom staticRoom = null
        )
        {
            _client = client;
            _profile = profile;
            _setting = setting;
            _gameSession = gameSession;
            _gathering = gathering;
            room = staticRoom;
            _monoBehaviour = monoBehaviour;

            initialized = true;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator Initialize(
            Animator animator
        )
        {
            animator.SetTrigger(Trigger.InitializeSucceed.ToString());
            yield break;
        }

        /// <summary>
        /// GS2-Matchmaking のギャザリング情報から GS2-Realtime のルーム情報を取得
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator GetRoom(
            Animator animator
        )
        {
            if (room != null)
            {
                animator.SetTrigger(Trigger.GetRoomSucceed.ToString());
                yield break;
            }
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                
                AsyncResult<EzGetRoomResult> result = null;
                yield return _client.Realtime.GetRoom(
                    r => { result = r; },
                    _setting.realtimeNamespaceName,
                    _gathering.Name
                );
            
                if (result.Error != null)
                {
                    if (OnError != null)
                    {
                        OnError.Invoke(
                            result.Error
                        );
                    }

                    animator.SetTrigger(Trigger.GetRoomFailed.ToString());
                    yield break;
                }

                if (result.Result.Item.IpAddress != null)
                {
                    room = result.Result.Item;
                    break;
                }
            }

            animator.SetTrigger(Trigger.GetRoomSucceed.ToString());
        }

        /// <summary>
        /// GS2-Realtime のルームに接続
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator ConnectRoom(
            Animator animator
        )
        {
            session = new RelayRealtimeSession(
                _gameSession.AccessToken.token,
                room.IpAddress,
                room.Port,
                room.EncryptionKey,
                ByteString.CopyFrom()
            );
            
            session.OnRelayMessage += OnRelayMessage; 
            session.OnJoinPlayer += player =>
            {
                players[player.ConnectionId] = player;
                if (OnJoinPlayer != null)
                {
                    OnJoinPlayer.Invoke(player);
                }
            };
            session.OnLeavePlayer += player =>
            {
                players.Remove(player.ConnectionId);
                if (OnLeavePlayer != null)
                {
                    OnLeavePlayer.Invoke(player);
                }
            };
            session.OnGeneralError += args => 
            {
                if (OnError != null)
                {
                    OnError.Invoke(
                        new UnknownException(args.Message)
                    );
                }
            };
            session.OnError += error =>
            {
                if (OnError != null)
                {
                    OnError.Invoke(
                        new UnknownException(error.Message)
                    );
                }
            };
            session.OnUpdateProfile += player =>
            {
                if (players.ContainsKey(player.ConnectionId))
                {
                    players[player.ConnectionId].Profile = player.Profile;
                }
                else
                {
                    players[player.ConnectionId] = player;
                    if (OnJoinPlayer != null)
                    {
                        OnJoinPlayer.Invoke(player);
                    }
                }

                if (OnUpdateProfile != null)
                {
                    OnUpdateProfile.Invoke(
                        player
                    );
                }
            };
            session.OnClose += args =>
            {
                animator.SetTrigger(Trigger.Disconnect.ToString());
                if (OnClose != null)
                {
                    OnClose.Invoke(
                        args
                    );
                }
            };

            AsyncResult<bool> result = null;
            yield return session.Connect(
                _monoBehaviour,
                r =>
                {
                    result = r;
                }
            );
            if (result.Error != null)
            {
                if (OnError != null)
                {
                    OnError.Invoke(result.Error);
                }
                animator.SetTrigger(Trigger.ConnectRoomFailed.ToString());
                yield break;
            }

            if (!session.Connected)
            {
                animator.SetTrigger(Trigger.ConnectRoomFailed.ToString());
                yield break;
            }
            animator.SetTrigger(Trigger.ConnectRoomSucceed.ToString());

        }

        /// <summary>
        /// 他プレイヤーの座標情報を同期
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator SyncPlayerProfiles(
            Animator animator
        )
        {
            animator.SetTrigger(Trigger.SyncPlayerProfilesSucceed.ToString());
            yield break;
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            State? newState = null;
            if (stateInfo.IsName(State.Initialize.ToString()))
            {
                // 初期化処理
                newState = State.Initialize;
            }
            else if (stateInfo.IsName(State.GetRoom.ToString()))
            {
                // 
                newState = State.GetRoom;
                _monoBehaviour.StartCoroutine(
                    GetRoom(
                        animator
                    )
                );
            }
            else if (stateInfo.IsName(State.ConnectRoom.ToString()))
            {
                // 
                newState = State.ConnectRoom;
                _monoBehaviour.StartCoroutine(
                    ConnectRoom(
                        animator
                    )
                );
            }
            else if (stateInfo.IsName(State.SyncPlayerProfiles.ToString()))
            {
                // 
                newState = State.SyncPlayerProfiles;
                _monoBehaviour.StartCoroutine(
                    SyncPlayerProfiles(
                        animator
                    )
                );
            }
            else if (stateInfo.IsName(State.Main.ToString()))
            {
                // 
                newState = State.Main;
            }
            else if (stateInfo.IsName(State.Disconnected.ToString()))
            {
                // エラー描画
                newState = State.Disconnected;
            }
            else if (stateInfo.IsName(State.Error.ToString()))
            {
                // エラー描画
                newState = State.Error;
            }

            if (!newState.HasValue)
            {
                if (OnError != null)
                {
                    OnError.Invoke(
                        new UnknownException("unknown state")
                    );
                }
            }

            if (OnChangeState != null)
            {
                // ステート変化を通知
                OnChangeState.Invoke(newState.Value);
            }
        }

        private void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            if (animatorStateInfo.IsName(State.Initialize.ToString()))
            {
                if (initialized)
                {
                    _monoBehaviour.StartCoroutine(
                        Initialize(animator)
                    );
                }
            }
        }
    }
}
