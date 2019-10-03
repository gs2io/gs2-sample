﻿using System;
using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Sample.Core;
using Gs2.Unity.Gs2Realtime.Model;
using Gs2.Unity.Gs2Realtime;
using Gs2.Unity.Gs2Realtime.Result;
using UnityEngine;
using UnityEngine.Events;

namespace Scenes.Realtime
{
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
        /// マッチメイキングコントローラー
        /// </summary>
        public RealtimeController controller;

        /// <summary>
        /// 
        /// </summary>
        public RelayRealtimeSession session;

        /// <summary>
        /// 
        /// </summary>
        public EzRoom room;
        
        [System.Serializable]
        public class ChangeStateEvent : UnityEvent<Animator, State>
        {
        }

        /// <summary>
        /// ステートが変化した時に呼び出されるイベント
        /// </summary>
        [SerializeField]
        public ChangeStateEvent onChangeState = new ChangeStateEvent();

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
        /// GS2-Realtime のルーム情報を取得
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator GetRoom(
            Animator animator
        )
        {
            var request = Gs2Util.LoadGlobalGameObject<RealtimeRequest>("RealtimeRequest");

            if (!string.IsNullOrEmpty(request.ipAddress))
            {
                room = new EzRoom
                {
                    Name = request.gatheringId,
                    IpAddress = request.ipAddress,
                    Port = request.port,
                    EncryptionKey = request.encryptionKey,
                };
                animator.SetTrigger(Trigger.GetRoomSucceed.ToString());
                yield break;
            }
            while (true)
            {
                yield return new WaitForSeconds(0.5f);
                
                AsyncResult<EzGetRoomResult> result = null;
                yield return controller.GetRoom(
                    r => { result = r; }
                );
            
                if (result.Error != null)
                {
                    animator.SetTrigger(Trigger.GetRoomFailed.ToString());
                    yield break;
                }

                if (!string.IsNullOrEmpty(result.Result.Item.IpAddress))
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
            yield return controller.ConnectRoom(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.ConnectRoomSucceed.ToString()
                        : Trigger.ConnectRoomFailed.ToString()
                    );
                },
                room.IpAddress,
                room.Port,
                room.EncryptionKey
            );
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
            yield return controller.SyncPlayerProfiles(
                r =>
                {
                    animator.SetTrigger(Trigger.SyncPlayerProfilesSucceed.ToString());
                }
            );
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            State? newState = null;
            if (stateInfo.IsName(State.Initialize.ToString()))
            {
                // 初期化処理
                controller.StartCoroutine(
                    Initialize(
                        animator
                    )
                );
            }
            else if (stateInfo.IsName(State.GetRoom.ToString()))
            {
                // 
                controller.StartCoroutine(
                    GetRoom(
                        animator
                    )
                );
            }
            else if (stateInfo.IsName(State.ConnectRoom.ToString()))
            {
                // 
                controller.StartCoroutine(
                    ConnectRoom(
                        animator
                    )
                );
            }
            else if (stateInfo.IsName(State.SyncPlayerProfiles.ToString()))
            {
                // 
                controller.StartCoroutine(
                    SyncPlayerProfiles(
                        animator
                    )
                );
            }
            foreach (State state in Enum.GetValues(typeof(State)))
            {
                if (stateInfo.IsName(state.ToString()))
                {
                    newState = state;
                    break;
                }
            }

            // ステート変化を通知
            if (newState.HasValue)
            {
                onChangeState.Invoke(animator, newState.Value);
            }
        }
    }
}
