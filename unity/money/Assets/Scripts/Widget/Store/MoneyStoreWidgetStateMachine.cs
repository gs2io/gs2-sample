﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Money
{
    public class MoneyStoreWidgetStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            GetProductsProcessing,
            SelectProduct,
            BuyProcessing,
        }

        public enum Trigger
        {
            GetProductsSucceed,
            GetProductsFailed,
            SelectProduct,
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
        /// コントローラー
        /// </summary>
        public MoneyController controller = new MoneyController();

        /// <summary>
        /// 販売中の課金通貨
        /// </summary>
        public List<Product> products = new List<Product>();

        /// <summary>
        /// 購入メニューで選択した課金通貨
        /// </summary>
        public Product selectProduct;

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
        /// 販売中の課金通貨一覧を取得
        /// </summary>
        /// <returns></returns>
        private IEnumerator ListProductsTask()
        {
            yield return controller.ListProducts(
                r =>
                {
                    products = r.Result;
                    
                    _animator.SetTrigger(r.Error == null
                        ? Trigger.GetProductsSucceed.ToString()
                        : Trigger.GetProductsFailed.ToString());
                }
            );
        }

        /// <summary>
        /// 課金通貨を購入
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
                },
                selectProduct
            );

            controller.gs2MoneySetting.onBuy.Invoke(selectProduct);
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

            if (stateInfo.IsName(State.GetProductsProcessing.ToString()))
            {
                controller.Initialize();
                controller.gs2MoneySetting.StartCoroutine(
                    ListProductsTask()
                );
            }
            if (stateInfo.IsName(State.BuyProcessing.ToString()))
            {
                controller.gs2MoneySetting.StartCoroutine(
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
        public bool Select(
            Product product
        )
        {
            selectProduct = product;
            _animator.SetTrigger(Trigger.SelectProduct.ToString());
            
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Close()
        {
            if (_state == State.SelectProduct)
            {
                _animator.SetTrigger(Trigger.Close.ToString());
                return true;
            }

            return false;
        }
    }
}
