﻿using System;
using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Unity.Gs2Account.Model;
 using Gs2.Unity.Gs2Money.Model;
 using Gs2.Unity.Gs2Money.Result;
 using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Money
{
    public class MoneyStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            Initialize,
            GetWalletProcessing,
            Idle,
            GetProductsProcessing,
            SelectProduct,
            BuyProcessing,
            Error,
        }

        public enum Trigger
        {
            InitializeSucceed,
            InitializeFailed,
            GetWalletSucceed,
            GetWalletFailed,
            OpenStore,
            GetProductsSucceed,
            GetProductsFailed,
            SelectProduct,
            BuySucceed,
            BuyFailed,
            Back,
            ConfirmError,
        }
        
        /// <summary>
        /// マッチメイキングコントローラー
        /// </summary>
        public MoneyController controller;

        /// <summary>
        /// 販売中の課金通貨
        /// </summary>
        private EzWalletDetail _wallet;

        /// <summary>
        /// 販売中の課金通貨
        /// </summary>
        private List<Product> _products = new List<Product>();

        /// <summary>
        /// 購入メニューで選択した課金通貨
        /// </summary>
        public Product selectProduct;

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
                controller.gs2MoneySetting.onError.Invoke(
                    result.Error
                );
                animator.SetTrigger(Trigger.InitializeFailed.ToString());
                yield break;
            }
            
            animator.SetTrigger(Trigger.InitializeSucceed.ToString());
        }

        /// <summary>
        /// 課金通貨のウォレットを取得
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator GetWallet(
            Animator animator
        )
        {
            yield return controller.GetWallet(
                r =>
                {
                    _wallet = r.Result.Item;
                    
                    animator.SetTrigger(r.Error == null
                        ? Trigger.GetWalletSucceed.ToString()
                        : Trigger.GetWalletFailed.ToString());
                }
            );
        }

        /// <summary>
        /// 販売中の課金通貨一覧を取得
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator ListProducts(
            Animator animator
        )
        {
            yield return controller.ListProducts(
                r =>
                {
                    _products = r.Result;
                    
                    animator.SetTrigger(r.Error == null
                        ? Trigger.GetProductsSucceed.ToString()
                        : Trigger.GetProductsFailed.ToString());
                }
            );
        }

        /// <summary>
        /// 課金通貨を購入
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        private IEnumerator Buy(
            Animator animator,
            Product product
        )
        {
            yield return controller.Buy(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.BuySucceed.ToString()
                        : Trigger.BuyFailed.ToString());
                },
                product
            );
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            State? newState = null;
            if (stateInfo.IsName(State.Initialize.ToString()))
            {
                controller.StartCoroutine(
                    Initialize(
                        animator
                    )
                );
            }
            if (stateInfo.IsName(State.GetWalletProcessing.ToString()))
            {
                controller.StartCoroutine(
                    GetWallet(
                        animator
                    )
                );
            }
            if (stateInfo.IsName(State.GetProductsProcessing.ToString()))
            {
                controller.StartCoroutine(
                    ListProducts(
                        animator
                    )
                );
            }
            if (stateInfo.IsName(State.BuyProcessing.ToString()))
            {
                controller.StartCoroutine(
                    Buy(
                        animator,
                        selectProduct
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
