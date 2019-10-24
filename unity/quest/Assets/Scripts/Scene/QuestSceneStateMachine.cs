﻿using System;
 using System.Collections;
 using Gs2.Core;
 using Gs2.Core.Exception;
 using Gs2.Sample.Money;
using Gs2.Sample.Stamina;
using Gs2.Sample.Money.Internal;
 using Gs2.Sample.Quest.Internal;
 using Gs2.Sample.Stamina.Internal;
 using Gs2.Unity.Gs2Quest.Model;
 using Gs2.Unity.Gs2Quest.Result;
 using UnityEngine;
using UnityEngine.Events;

 namespace Gs2.Sample.Quest
{
    public class QuestSceneStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            Initialize,
            CheckCurrentProgress,
            SelectQuest,
            MoneyStore,
            StaminaStore,
            PlayQuest,
            Error,
        }

        public enum Trigger
        {
            OpenSelectQuest,
            OpenMoneyStatus,
            OpenStaminaStatus,
            ExistsProgress,
            NotExistsProgress,
            OpenMoneyStore,
            OpenStaminaStore,
            StartQuest,
            EndQuest,
            Back,
            Error,
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
        /// スタミナコントローラー
        /// </summary>
        public QuestController questController = new QuestController();

        /// <summary>
        /// スタミナコントローラー
        /// </summary>
        public StaminaController staminaController = new StaminaController();

        /// <summary>
        /// 課金通貨コントローラー
        /// </summary>
        public MoneyController moneyController = new MoneyController();

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
        public IEnumerator CheckCurrentProgressTask()
        {
            AsyncResult<EzGetProgressResult> result = null;
            yield return questController.GetProgress(
                r => result = r
            );

            if (result.Error != null)
            {
                if (result.Error is NotFoundException)
                {
                    _animator.SetTrigger(Trigger.NotExistsProgress.ToString());
                    yield break;
                }
                else
                {
                    _animator.SetTrigger(Trigger.Error.ToString());
                    yield break;
                }
            }

            GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().selectedQuestGroup = result.Result.QuestGroup;
            GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().selectedQuest = result.Result.Quest;
            GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().progress = result.Result.Item;
            GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().onStartQuest.Invoke(
                result.Result.QuestGroup,
                result.Result.Quest
            );
            _animator.SetTrigger(Trigger.ExistsProgress.ToString());
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _animator = animator;

            if (stateInfo.IsName(State.Initialize.ToString()))
            {
                questController.Initialize();
                moneyController.Initialize();
                staminaController.Initialize();
                GameObject.Find("Gs2StaminaInternalSetting").GetComponent<Gs2StaminaInternalSetting>().onOpenStatus.Invoke();
                GameObject.Find("Gs2MoneyInternalSetting").GetComponent<Gs2MoneyInternalSetting>().onOpenStatus.Invoke();
                GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().onOpenSelectQuest.Invoke();
            }

            if (stateInfo.IsName(State.CheckCurrentProgress.ToString()))
            {
                questController.gs2QuestSetting.StartCoroutine(
                    CheckCurrentProgressTask()
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

        public bool OpenSelectQuest()
        {
            if (_state == State.Initialize || _state == State.PlayQuest || _state == State.Error)
            {
                _animator.SetBool(Trigger.OpenSelectQuest.ToString(), true);
                return true;
            }

            return false;
        }

        public bool CloseSelectQuest()
        {
            if (_state == State.SelectQuest || _state == State.CheckCurrentProgress)
            {
                _animator.SetBool(Trigger.OpenSelectQuest.ToString(), false);
                return true;
            }

            return false;
        }

        public bool OpenMoneyStatus()
        {
            if (_state == State.Initialize)
            {
                _animator.SetBool(Trigger.OpenMoneyStatus.ToString(), true);
                return true;
            }

            return false;
        }

        public bool OpenStaminaStatus()
        {
            if (_state == State.Initialize)
            {
                _animator.SetBool(Trigger.OpenStaminaStatus.ToString(), true);
                return true;
            }

            return false;
        }

        public bool CloseMoneyStatus()
        {
            return false;
        }

        public bool CloseStaminaStatus()
        {
            return false;
        }

        public bool OpenMoneyStore()
        {
            if (_state == State.SelectQuest)
            {
                _animator.SetTrigger(Trigger.OpenMoneyStore.ToString());
                return true;
            }
            return false;
        }

        public bool OpenStaminaStore()
        {
            if (_state == State.SelectQuest || _state == State.Error)
            {
                _animator.SetTrigger(Trigger.OpenStaminaStore.ToString());
                return true;
            }
            return false;
        }

        public bool CloseMoneyStore()
        {
            if (_state == State.MoneyStore || _state == State.Error)
            {
                _animator.SetTrigger(Trigger.Back.ToString());
                return true;
            }
            return false;
        }

        public bool CloseStaminaStore()
        {
            if (_state == State.StaminaStore || _state == State.Error)
            {
                _animator.SetTrigger(Trigger.Back.ToString());
                return true;
            }
            return false;
        }

        public bool PlayGame()
        {
            if (_state == State.SelectQuest || _state == State.CheckCurrentProgress || _state == State.Error)
            {
                CloseSelectQuest();
                _animator.SetTrigger(Trigger.StartQuest.ToString());
                return true;
            }
            return false;
        }
        
        public bool EndGame()
        {
            if (_state == State.PlayQuest || _state == State.Error)
            {
                _animator.SetTrigger(Trigger.EndQuest.ToString());
                return true;
            }
            return false;
        }
    }
}
