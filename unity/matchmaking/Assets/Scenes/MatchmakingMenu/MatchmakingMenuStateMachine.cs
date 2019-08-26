﻿using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Core.Model;
using Gs2.Gs2Matchmaking.Model;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Gs2Gateway.Result;
using Gs2.Unity.Gs2Matchmaking.Model;
using Gs2.Unity.Gs2Matchmaking.Result;
using Gs2.Unity.Util;
using LitJson;
using Scenes.AccountMenu.Model;
using Scenes.AccountMenu.Repository;
using Scenes.MatchmakingMenu.Model;
using TMPro;
using UnityEngine;

namespace Scenes.MatchmakingMenu
{
    public delegate void ChangeStateHandler(MatchmakingMenuStateMachine.State state);
    public delegate void JoinPlayerHandler(EzGathering gathering, string userId);
    public delegate void LeavePlayerHandler(EzGathering gathering, string userId);
    public delegate void UpdateJoinedPlayerIdsHandler(EzGathering gathering, List<string> joinedPlayerIds);
    public delegate void MatchmakingCompleteHandler(EzGathering gathering, List<string> joinedPlayerIds);
    public delegate void ErrorHandler(Gs2Exception error);
    
    public class MatchmakingMenuStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            Initialize,
            MainMenu,
            CreateGatheringMenu,
            CreateGathering,
            JoinGathering,
            Matchmaking,
            CancelMatchmaking,
            MatchmakingComplete,
            GatheringNotFound,
            Error,
        }

        public enum Trigger
        {
            InitializeSucceed,
            InitializeFailed,
            SelectCreateGathering,
            SubmitCapacity,
            CreateGatheringSucceed,
            CreateGatheringFailed,
            SelectJoinGathering,
            JoinGatheringSucceed,
            JoinGatheringFailed,
            GatheringNotFound,
            MatchmakingSucceed,
            SelectCancelMatchmaking,
            CancelMatchmakingSucceed,
            CancelMatchmakingFailed,
            ResultCallback,
            ConfirmError,
        }

        /// <summary>
        /// ギャザリング情報
        /// </summary>
        public EzGathering gathering;

        /// <summary>
        /// 
        /// </summary>
        private bool _matchmakingComplete;

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
        private Gs2MatchmakingSetting _setting;

        /// <summary>
        /// 
        /// </summary>
        private readonly List<string> _joinedPlayerIds = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        private GameSession _gameSession;

        /// <summary>
        /// 
        /// </summary>
        private MonoBehaviour _monoBehaviour;

        /// <summary>
        /// 
        /// </summary>
        public int capacity;

        /// <summary>
        /// 
        /// </summary>
        public event ChangeStateHandler OnChangeState;

        /// <summary>
        /// 新しいプレイヤーがギャザリングに参加したとき
        /// </summary>
        public event JoinPlayerHandler OnJoinPlayer;
        
        /// <summary>
        /// プレイヤーがギャザリングから離脱したとき
        /// </summary>
        public event LeavePlayerHandler OnLeavePlayer;
        
        /// <summary>
        /// 参加中のプレイヤー一覧が更新されたとき
        /// </summary>
        public event UpdateJoinedPlayerIdsHandler OnUpdateJoinedPlayerIds;

        /// <summary>
        /// マッチメイキングが完了したとき
        /// </summary>
        public event MatchmakingCompleteHandler OnMatchmakingComplete;

        /// <summary>
        /// エラー発生時に発行されるイベント
        /// </summary>
        public event ErrorHandler OnError;

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
        /// <param name="monoBehaviour"></param>
        public void Initialize(
            Gs2.Unity.Client client,
            Gs2.Unity.Util.Profile profile,
            Gs2MatchmakingSetting setting,
            GameSession gameSession,
            MonoBehaviour monoBehaviour
        )
        {
            _client = client;
            _profile = profile;
            _setting = setting;
            _gameSession = gameSession;
            _monoBehaviour = monoBehaviour;

            initialized = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void PushNotificationHandler(NotificationMessage message)
        {
            Debug.Log(message.issuer);
            if (message.issuer.StartsWith("Gs2Matchmaking:"))
            {
                if (message.issuer.EndsWith(":Join"))
                {
                    var notification = JsonMapper.ToObject<JoinNotification>(message.payload);
                    _joinedPlayerIds.Add(notification.joinUserId);
                    if (OnJoinPlayer != null)
                    {
                        OnJoinPlayer.Invoke(gathering, notification.joinUserId);
                    }
                    if (OnUpdateJoinedPlayerIds != null)
                    {
                        OnUpdateJoinedPlayerIds(gathering, _joinedPlayerIds);
                    }
                }
                else if (message.issuer.EndsWith(":Leave"))
                {
                    var notification = JsonMapper.ToObject<LeaveNotification>(message.payload);
                    _joinedPlayerIds.Remove(notification.leaveUserId);
                    if (OnLeavePlayer != null)
                    {
                        OnLeavePlayer.Invoke(gathering, notification.leaveUserId);
                    }
                    if (OnUpdateJoinedPlayerIds != null)
                    {
                        OnUpdateJoinedPlayerIds(gathering, _joinedPlayerIds);
                    }
                }
                else if (message.issuer.EndsWith(":Complete"))
                {
                    _matchmakingComplete = true;
                    if (OnMatchmakingComplete != null)
                    {
                        if (gathering != null)
                        {
                            // Joinと同時にマッチメイキングが成立する場合
                            // DoMatchmaking の応答より先にマッチメイキング完了通知が届くことがある
                            OnMatchmakingComplete.Invoke(gathering, _joinedPlayerIds);
                        }
                    }
                }
            }
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
            _profile.Gs2Session.OnNotificationMessage += PushNotificationHandler;
            animator.SetTrigger(Trigger.InitializeSucceed.ToString());
            yield break;
        }

        /// <summary>
        /// 誰でもいいので参加者を募集するギャザリングを新規作成
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator SimpleMatchmakingCreateGathering(
            Animator animator,
            int capacity
        )
        {
            AsyncResult<EzCreateGatheringResult> result = null;
            yield return _client.Matchmaking.CreateGathering(
                r => { result = r; },
                _gameSession,
                _setting.matchmakingNamespaceName,
                new EzPlayer
                {
                    RoleName = "default"
                },
                new List<EzCapacityOfRole>
                {
                    new EzCapacityOfRole
                    {
                        RoleName = "default",
                        Capacity = capacity
                    },
                },
                new List<string>(),
                new List<EzAttributeRange>()
            );
            
            if (result.Error != null)
            {
                if (OnError != null)
                {
                    OnError.Invoke(
                        result.Error
                    );
                }

                animator.SetTrigger(Trigger.CreateGatheringFailed.ToString());
                yield break;
            }

            _matchmakingComplete = false;
            _joinedPlayerIds.Clear();
            gathering = result.Result.Item;
            _joinedPlayerIds.Add(_gameSession.AccessToken.userId);

            if (OnUpdateJoinedPlayerIds != null)
            {
                OnUpdateJoinedPlayerIds.Invoke(gathering, _joinedPlayerIds);
            }

            animator.SetTrigger(Trigger.CreateGatheringSucceed.ToString());
        }

        /// <summary>
        /// 既存のギャザリングに参加する
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator SimpleMatchmakingJoinGathering(
            Animator animator
        )
        {
            AsyncResult<EzDoMatchmakingResult> result = null;
            string contextToken = null;
            while (true)
            {
                yield return _client.Matchmaking.DoMatchmaking(
                    r => { result = r; },
                    _gameSession,
                    _setting.matchmakingNamespaceName,
                    new EzPlayer
                    {
                        RoleName = "default"
                    },
                    contextToken
                );
            
                if (result.Error != null)
                {
                    if (OnError != null)
                    {
                        OnError.Invoke(
                            result.Error
                        );
                    }

                    if (result.Error is NotFoundException)
                    {
                        animator.SetTrigger(Trigger.GatheringNotFound.ToString());
                    }
                    else
                    {
                        animator.SetTrigger(Trigger.JoinGatheringFailed.ToString());
                    }
                    yield break;
                }

                if (result.Result.Item != null)
                {
                    gathering = result.Result.Item;
                    if (!_matchmakingComplete)
                    {
                        animator.SetTrigger(Trigger.JoinGatheringSucceed.ToString());
                    }
                    else
                    {
                        if (OnMatchmakingComplete != null)
                        {
                            OnMatchmakingComplete.Invoke(gathering, _joinedPlayerIds);
                        }
                        animator.SetTrigger(Trigger.MatchmakingSucceed.ToString());
                    }
                    yield break;
                }

                contextToken = result.Result.MatchmakingContextToken;
            }
        }

        /// <summary>
        /// マッチメイキングをキャンセルしたとき
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator CancelMatchmaking(
            Animator animator
        )
        {
            AsyncResult<EzCancelMatchmakingResult> result = null;
            yield return _client.Matchmaking.CancelMatchmaking(
                r => { result = r; },
                _gameSession,
                _setting.matchmakingNamespaceName,
                gathering.Name
            );
        
            if (result.Error != null)
            {
                if (OnError != null)
                {
                    OnError.Invoke(
                        result.Error
                    );
                }

                animator.SetTrigger(Trigger.CancelMatchmakingFailed.ToString());
                yield break;
            }

            animator.SetTrigger(Trigger.CancelMatchmakingSucceed.ToString());
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            State? newState = null;
            if (stateInfo.IsName(State.Initialize.ToString()))
            {
                // 初期化処理
                newState = State.Initialize;
            }
            else if (stateInfo.IsName(State.MainMenu.ToString()))
            {
                // 
                newState = State.MainMenu;
            }
            else if (stateInfo.IsName(State.CreateGatheringMenu.ToString()))
            {
                // 
                newState = State.CreateGatheringMenu;
            }
            else if (stateInfo.IsName(State.CreateGathering.ToString()))
            {
                // 
                newState = State.CreateGathering;
                _monoBehaviour.StartCoroutine(
                    SimpleMatchmakingCreateGathering(
                        animator,
                        capacity
                    )
                );
            }
            else if (stateInfo.IsName(State.JoinGathering.ToString()))
            {
                // 
                newState = State.JoinGathering;
                _monoBehaviour.StartCoroutine(
                    SimpleMatchmakingJoinGathering(
                        animator
                    )
                );
            }
            else if (stateInfo.IsName(State.Matchmaking.ToString()))
            {
                // 
                newState = State.Matchmaking;
            }
            else if (stateInfo.IsName(State.CancelMatchmaking.ToString()))
            {
                // 
                newState = State.CancelMatchmaking;
                
                _monoBehaviour.StartCoroutine(
                    CancelMatchmaking(
                        animator
                    )
                );
            }
            else if (stateInfo.IsName(State.MatchmakingComplete.ToString()))
            {
                // 
                newState = State.MatchmakingComplete;
                animator.SetTrigger(Trigger.ResultCallback.ToString());
            }
            else if (stateInfo.IsName(State.GatheringNotFound.ToString()))
            {
                // 
                newState = State.GatheringNotFound;
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
