﻿using System;
using System.Collections;
using Gs2.Sample.Money;
using Gs2.Sample.Money.Internal;
using Gs2.Sample.Stamina.Internal;
using UnityEngine;
using UnityEngine.Events;

 namespace Gs2.Sample.Stamina
{
    public class StaminaSceneStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            Initialize,
            Idle,
            ConsumeProgress,
            StaminaStore,
            MoneyStore,
            Error,
        }

        public enum Trigger
        {
            OpenMoneyStatus,
            OpenStaminaStatus,
            OpenMoneyStore,
            OpenStaminaStore,
            ConsumeStamina,
            ConsumeStaminaSucceed,
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
        public StaminaController staminaController = new StaminaController();

        /// <summary>
        /// 課金通貨コントローラー
        /// </summary>
        public MoneyController moneyController = new MoneyController();

        [Serializable]
        public class ChangeStateEvent : UnityEvent<Animator, State>
        {
        }

        [Serializable]
        public class OpenStatusEvent : UnityEvent
        {
        }

        /// <summary>
        /// ステートが変化した時に呼び出されるイベント
        /// </summary>
        [SerializeField]
        public ChangeStateEvent onChangeState = new ChangeStateEvent();

        /// <summary>
        /// スタミナを消費（非推奨 サービス間の連携（スタンプシート）を経由してスタミナ値を操作するほうが望ましい）
        /// </summary>
        /// <param name="consumeValue"></param>
        /// <returns></returns>
        private IEnumerator ConsumeStaminaTask(
            int consumeValue
        )
        {
            yield return staminaController.ConsumeStamina(
                r =>
                {
                    _animator.SetTrigger(r.Error == null
                        ? Trigger.ConsumeStaminaSucceed.ToString()
                        : Trigger.Error.ToString());
                    staminaController.gs2StaminaSetting.onBuy.Invoke();
                },
                consumeValue
            );
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _animator = animator;
            
            if (stateInfo.IsName(State.Initialize.ToString()))
            {
                moneyController.Initialize();
                staminaController.Initialize();
                GameObject.Find("Gs2StaminaInternalSetting").GetComponent<Gs2StaminaInternalSetting>().onOpenStatus.Invoke();
                GameObject.Find("Gs2MoneyInternalSetting").GetComponent<Gs2MoneyInternalSetting>().onOpenStatus.Invoke();
            }
            if (stateInfo.IsName(State.ConsumeProgress.ToString()))
            {
                staminaController.gs2StaminaSetting.StartCoroutine(
                    ConsumeStaminaTask(10)
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

        public bool OpenMoneyStatus()
        {
            if (_state == State.Initialize)
            {
                _animator.SetTrigger(Trigger.OpenMoneyStatus.ToString());
                return true;
            }

            return false;
        }

        public bool OpenStaminaStatus()
        {
            if (_state == State.Initialize)
            {
                _animator.SetTrigger(Trigger.OpenStaminaStatus.ToString());
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
            if (_state == State.Idle)
            {
                _animator.SetTrigger(Trigger.OpenMoneyStore.ToString());
                return true;
            }
            return false;
        }

        public bool OpenStaminaStore()
        {
            if (_state == State.Idle)
            {
                _animator.SetTrigger(Trigger.OpenStaminaStore.ToString());
                return true;
            }
            return false;
        }

        public bool CloseMoneyStore()
        {
            if (_state == State.MoneyStore)
            {
                _animator.SetTrigger(Trigger.Back.ToString());
                return true;
            }
            return false;
        }

        public bool CloseStaminaStore()
        {
            if (_state == State.StaminaStore)
            {
                _animator.SetTrigger(Trigger.Back.ToString());
                return true;
            }
            return false;
        }
        
        public bool ConsumeStamina()
        {
            if (_state == State.Idle)
            {
                _animator.SetTrigger(Trigger.ConsumeStamina.ToString());
                return true;
            }
            return false;
        }
    }
}
