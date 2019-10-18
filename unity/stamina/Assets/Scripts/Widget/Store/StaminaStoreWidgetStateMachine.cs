﻿using System;
using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Sample.Money;
using Gs2.Unity.Gs2Account.Model;
 using Gs2.Unity.Gs2Money.Model;
 using Gs2.Unity.Gs2Money.Result;
 using Gs2.Unity.Gs2Stamina.Model;
using Gs2.Unity.Gs2Stamina.Result;
using UnityEngine;
using UnityEngine.Events;
 using UnityEngine.UI;

 namespace Gs2.Sample.Stamina
{
    public class StaminaStoreWidgetStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            GetStaminaProcessing,
            Store,
            BuyProcessing,
        }

        public enum Trigger
        {
            GetStaminaSucceed,
            GetStaminaFailed,
            Purchase,
            BuySucceed,
            BuyFailed,
            Close,
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
        /// スタミナ
        /// </summary>
        public EzStamina stamina;

        /// <summary>
        /// コントローラー
        /// </summary>
        public StaminaController controller = new StaminaController();

        [Serializable]
        public class ChangeStateEvent : UnityEvent<Animator, State>
        {
        }

        /// <summary>
        /// ステートが変化した時に呼び出されるイベント
        /// </summary>
        [SerializeField]
        public ChangeStateEvent onChangeState = new ChangeStateEvent();

        /// <summary>
        /// スタミナを取得
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetStaminaTask()
        {
            AsyncResult<EzGetStaminaResult> result = null;
            yield return controller.GetStamina(
                r => result = r
            );
                
            if (result.Error != null)
            {
                _animator.SetTrigger(Trigger.GetStaminaFailed.ToString());
                yield break;
            }
            stamina = result.Result.Item;
        
            _animator.SetTrigger(Trigger.GetStaminaSucceed.ToString());
        }

        /// <summary>
        /// スタミナを購入
        /// </summary>
        /// <returns></returns>
        private IEnumerator BuyTask()
        {
            yield return controller.Buy(
                r =>
                {
                   _animator.SetTrigger(r.Error == null
                        ? Trigger.BuySucceed.ToString()
                        : Trigger.BuyFailed.ToString());
                }
            );
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _animator = animator;

            if (stateInfo.IsName(State.GetStaminaProcessing.ToString()))
            {
                controller.Initialize();
                controller.gs2StaminaSetting.StartCoroutine(
                    GetStaminaTask()
                );
            }
            if (stateInfo.IsName(State.BuyProcessing.ToString()))
            {
                controller.gs2StaminaSetting.StartCoroutine(
                    BuyTask()
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
        
        /// <summary>
        /// 
        /// </summary>
        public bool Purchase()
        {
            if (_state == State.Store)
            {
                _animator.SetTrigger(Trigger.Purchase.ToString());
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Close()
        {
            if (_state == State.Store)
            {
                _animator.SetTrigger(Trigger.Close.ToString());
                return true;
            }

            return false;
        }
    }
}
