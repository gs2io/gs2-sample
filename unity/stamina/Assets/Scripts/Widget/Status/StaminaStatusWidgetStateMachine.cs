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
    public class StaminaStatusWidgetStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            Initialize,
            GetStaminaProcessing,
            Idle,
        }

        public enum Trigger
        {
            InitializeComplete,
            GetStaminaSucceed,
            GetStaminaFailed,
            Refresh,
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
        /// スタミナコントローラー
        /// </summary>
        public StaminaController controller = new StaminaController();

        /// <summary>
        /// スタミナ
        /// </summary>
        public EzStamina stamina;

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
        /// 初期化処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator InitializeTask()
        {
            _animator.SetTrigger(Trigger.InitializeComplete.ToString());
            yield break;
        }

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

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _animator = animator;

            if (stateInfo.IsName(State.Initialize.ToString()))
            {
                controller.Initialize();
                controller.gs2StaminaSetting.StartCoroutine(
                    InitializeTask()
                );
            }
            if (stateInfo.IsName(State.GetStaminaProcessing.ToString()))
            {
                controller.gs2StaminaSetting.StartCoroutine(
                    GetStaminaTask()
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
        public bool Refresh()
        {
            _animator.SetTrigger(Trigger.Refresh.ToString());
            return true;
        }
    }
}
