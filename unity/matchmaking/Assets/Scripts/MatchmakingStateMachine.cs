﻿using System;
﻿using System.Collections;
using Gs2.Core;
using Gs2.Core.Exception;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Matchmaking
{
    public class MatchmakingStateMachine : StateMachineBehaviour
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
        /// マッチメイキングコントローラー
        /// </summary>
        public MatchmakingController controller;

        /// <summary>
        /// 自分を含む募集人数
        /// </summary>
        public int capacity;

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
            AsyncResult<object> result = null;
            yield return controller.gs2Client.Initialize(
                r =>
                {
                    result = r;
                }
            );
            
            if (result.Error != null)
            {
                controller.gs2MatchmakingSetting.onError.Invoke(
                    result.Error
                );
                animator.SetTrigger(Trigger.InitializeFailed.ToString());
                yield break;
            }
            
            controller.gs2MatchmakingSetting.onMatchmakingComplete.AddListener(
                (gathering, joinUserIds) =>
                {
                    animator.SetTrigger(Trigger.MatchmakingSucceed.ToString());
                }
            );

            animator.SetTrigger(Trigger.InitializeSucceed.ToString());
        }

        /// <summary>
        /// 誰でもいいので参加者を募集するギャザリングを新規作成
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator SimpleMatchmakingCreateGathering(
            Animator animator
        )
        {
            yield return controller.SimpleMatchmakingCreateGathering(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.CreateGatheringSucceed.ToString()
                        : Trigger.CreateGatheringFailed.ToString());
                },
                capacity
            );
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
            yield return controller.SimpleMatchmakingJoinGathering(
                r =>
                {
                    if (r.Error is NotFoundException)
                    {
                        animator.SetTrigger(Trigger.GatheringNotFound.ToString());
                    }
                    else
                    {
                        animator.SetTrigger(r.Error == null
                            ? Trigger.JoinGatheringSucceed.ToString()
                            : Trigger.JoinGatheringFailed.ToString());
                    }
                }
            );
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
            yield return controller.CancelMatchmaking(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.CancelMatchmakingSucceed.ToString()
                        : Trigger.CancelMatchmakingFailed.ToString());
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
            else if (stateInfo.IsName(State.CreateGathering.ToString()))
            {
                // 
                controller.StartCoroutine(
                    SimpleMatchmakingCreateGathering(
                        animator
                    )
                );
            }
            else if (stateInfo.IsName(State.JoinGathering.ToString()))
            {
                // 
                controller.StartCoroutine(
                    SimpleMatchmakingJoinGathering(
                        animator
                    )
                );
            }
            else if (stateInfo.IsName(State.CancelMatchmaking.ToString()))
            {
                // 
                controller.StartCoroutine(
                    CancelMatchmaking(
                        animator
                    )
                );
            }
            else if (stateInfo.IsName(State.MatchmakingComplete.ToString()))
            {
                // 
                animator.SetTrigger(Trigger.ResultCallback.ToString());
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
