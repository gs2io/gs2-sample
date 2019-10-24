﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
  using Gs2.Sample.Quest.Internal;
  using Gs2.Unity.Gs2Quest.Model;
using Gs2.Unity.Gs2Quest.Result;
using UnityEngine;
using UnityEngine.Events;

 namespace Gs2.Sample.Quest
{
    public class PlayGameWidgetStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            StartProcessing,
            PlayGame,
            SendCompleteResult,
            SendFailedResult,
        }

        public enum Trigger
        {
            StartQuestSucceed,
            StartQuestFailed,
            SuccessQuest,
            FailedQuest,
            SendResultSucceed,
            SendResultFailed,
        }
        
        /// <summary>
        /// アニメーター
        /// </summary>
        private Animator _animator;

        /// <summary>
        /// 現在のステータス
        /// </summary>
        private State _state;

        /// <summary>
        /// 
        /// </summary>
        private EzProgress _progress;

        /// <summary>
        /// スタミナコントローラー
        /// </summary>
        public QuestController controller = new QuestController();

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
        /// クエストを開始
        /// </summary>
        /// <returns></returns>
        public IEnumerator StartTask()
        {
            var selectedQuestGroup = GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().selectedQuestGroup;
            var selectedQuest = GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().selectedQuest;
            var progress = GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().progress;

            if (progress == null)
            {
                AsyncResult<EzProgress> result = null;
                yield return controller.QuestStart(
                    r => result = r,
                    selectedQuestGroup,
                    selectedQuest
                );

                if (result.Error != null)
                {
                    _animator.SetTrigger(Trigger.StartQuestFailed.ToString());
                    yield break;
                }

                _progress = result.Result;
            }
            else
            {
                _progress = progress;
            }

            _animator.SetTrigger(Trigger.StartQuestSucceed.ToString());
        }

        /// <summary>
        /// クエストを取得
        /// </summary>
        /// <returns></returns>
        public IEnumerator EndTask(
            List<EzReward> rewards,
            bool isComplete
        )
        {
            {
                AsyncResult<object> result = null;
                yield return controller.QuestEnd(
                    r => result = r,
                    _progress,
                    rewards,
                    isComplete
                );

                if (result.Error != null)
                {
                    _animator.SetTrigger(Trigger.SendResultFailed.ToString());
                    yield break;
                }
            }

            _animator.SetTrigger(Trigger.SendResultSucceed.ToString());
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _animator = animator;

            if (stateInfo.IsName(State.StartProcessing.ToString()))
            {
                controller.Initialize();
                controller.gs2QuestSetting.StartCoroutine(
                    StartTask()
                );
            }
            if (stateInfo.IsName(State.SendCompleteResult.ToString()))
            {
                controller.gs2QuestSetting.StartCoroutine(
                    EndTask(
                        _progress.Rewards,
                        true
                    )
                );
            }
            if (stateInfo.IsName(State.SendFailedResult.ToString()))
            {
                controller.gs2QuestSetting.StartCoroutine(
                    EndTask(
                        _progress.Rewards,
                        false
                    )
                );
            }

            foreach (State state in Enum.GetValues(typeof(State)))
            {
                if (stateInfo.IsName(state.ToString()))
                {
                    _state = state;
                    onChangeState.Invoke(animator, state);
                    break;
                }
            }
        }

        public bool Complete()
        {
            if (_state == State.PlayGame)
            {
                _animator.SetTrigger(Trigger.SuccessQuest.ToString());
                return true;
            }

            return false;
        }

        public bool Failed()
        {
            if (_state == State.PlayGame)
            {
                _animator.SetTrigger(Trigger.FailedQuest.ToString());
                return true;
            }

            return false;
        }
    }
}
