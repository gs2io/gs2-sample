using System;
using System.Collections;
using Gs2.Sample.Core;
using Gs2.Unity.Gs2Money.Model;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Money
{
    public class MoneyStatusWidgetStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            Initialize,
            GetWalletProcessing,
            Idle,
        }

        public enum Trigger
        {
            InitializeComplete,
            GetWalletSucceed,
            GetWalletFailed,
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
        /// コントローラー
        /// </summary>
        public MoneyController controller = new MoneyController();

        /// <summary>
        /// ウォレット
        /// </summary>
        public EzWallet wallet;

        /// <summary>
        /// 
        /// </summary>
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
        public IEnumerator InitializeTask()
        {
            _animator.SetTrigger(Trigger.InitializeComplete.ToString());
            yield break;
        }

        /// <summary>
        /// 課金通貨のウォレットを取得
        /// </summary>
        /// <returns></returns>
        private IEnumerator GetWalletTask()
        {
            yield return controller.GetWallet(
                r =>
                {
                    wallet = r.Result.Item;
                    
                    _animator.SetTrigger(r.Error == null
                        ? Trigger.GetWalletSucceed.ToString()
                        : Trigger.GetWalletFailed.ToString());
                }
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _animator = animator;
            
            if (stateInfo.IsName(State.Initialize.ToString()))
            {
                controller.Initialize();
                controller.gs2MoneySetting.StartCoroutine(
                    InitializeTask()
                );
            }
            if (stateInfo.IsName(State.GetWalletProcessing.ToString()))
            {
                controller.gs2MoneySetting.StartCoroutine(
                    GetWalletTask()
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

        /// <summary>
        /// 
        /// </summary>
        public bool Close()
        {
            if (_state == State.Idle)
            {
                _animator.SetTrigger(Trigger.Close.ToString());
                return true;
            }

            return false;
        }
    }
}
