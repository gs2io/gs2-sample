﻿using System;
using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
 using Gs2.Sample.Core;
 using Gs2.Sample.Money.Internal;
 using Gs2.Unity.Gs2Account.Model;
 using Gs2.Unity.Gs2Money.Model;
 using Gs2.Unity.Gs2Money.Result;
 using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Money
{
    public class MoneySceneStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            Initialize,
            Idle,
            Store,
            Error,
        }

        public enum Trigger
        {
            OpenStatus,
            OpenStore,
            CloseStore,
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
        /// コントローラー
        /// </summary>
        public readonly MoneyController controller = new MoneyController();

        /// <summary>
        /// 
        /// </summary>
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

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _animator = animator;
            
            if (stateInfo.IsName(State.Initialize.ToString()))
            {
                controller.Initialize();
                GameObject.Find("Gs2MoneyInternalSetting").GetComponent<Gs2MoneyInternalSetting>().onOpenStatus.Invoke();
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

        public bool OpenStatus()
        {
            if (_state == State.Initialize)
            {
                _animator.SetTrigger(Trigger.OpenStatus.ToString());
                return true;
            }

            return false;
        }

        public bool CloseStatus()
        {
            return false;
        }
        
        public bool OpenStore()
        {
            if (_state == State.Idle)
            {
                _animator.SetTrigger(Trigger.OpenStore.ToString());
                return true;
            }

            return false;
        }

        public bool CloseStore()
        {
            if (_state == State.Store)
            {
                _animator.SetTrigger(Trigger.CloseStore.ToString());
                return true;
            }

            return false;
        }
    }
}
