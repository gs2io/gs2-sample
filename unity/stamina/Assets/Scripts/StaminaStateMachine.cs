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
    public class StaminaStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            Initialize,
            GetPropertiesProcessing,
            Idle,
            StaminaStore,
            StaminaBuyProcessing,
            GetMoneyProductsProcessing,
            MoneyStore,
            MoneyBuyProcessing,
            ConsumeProgress,
            Error,
        }

        public enum Trigger
        {
            InitializeSucceed,
            InitializeFailed,
            GetPropertiesSucceed,
            GetPropertiesFailed,
            OpenMoneyStore,
            OpenStaminaStore,
            Purchase,
            SelectProduct,
            BuySucceed,
            BuyFailed,
            GetProductsSucceed,
            GetProductsFailed,
            ConsumeStamina,
            ConsumeStaminaSucceed,
            ConsumeStaminaFailed,
            Back,
            ConfirmError,
        }
        
        /// <summary>
        /// スタミナコントローラー
        /// </summary>
        public StaminaController staminaController;

        /// <summary>
        /// 課金通貨コントローラー
        /// </summary>
        public MoneyController moneyController;

        /// <summary>
        /// スタミナ
        /// </summary>
        public EzStamina _stamina;

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
            yield return staminaController.gs2Client.Initialize(
                r =>
                {
                    result = r;
                }
            );
            
            if (result.Error != null)
            {
                staminaController.gs2StaminaSetting.onError.Invoke(
                    result.Error
                );
                animator.SetTrigger(Trigger.InitializeFailed.ToString());
                yield break;
            }

            yield return moneyController.gs2Client.Initialize(
                r =>
                {
                    result = r;
                }
            );
            
            if (result.Error != null)
            {
                staminaController.gs2StaminaSetting.onError.Invoke(
                    result.Error
                );
                animator.SetTrigger(Trigger.InitializeFailed.ToString());
                yield break;
            }
            
            animator.SetTrigger(Trigger.InitializeSucceed.ToString());
        }

        /// <summary>
        /// 課金通貨のウォレット/スタミナを取得
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator GetProperties(
            Animator animator
        )
        {
            {
                AsyncResult<EzGetResult> result = null;
                yield return moneyController.GetWallet(
                    r => result = r
                );
                if (result.Error != null)
                {
                    animator.SetTrigger(Trigger.GetPropertiesFailed.ToString());
                    yield break;
                }
                
                _wallet = result.Result.Item;
            }
            {
                AsyncResult<EzGetStaminaResult> result = null;
                yield return staminaController.GetStamina(
                    r => result = r
                );
                    
                if (result.Error != null)
                {
                    animator.SetTrigger(Trigger.GetPropertiesFailed.ToString());
                    yield break;
                }
                _stamina = result.Result.Item;
            }
            
            animator.SetTrigger(Trigger.GetPropertiesSucceed.ToString());
        }

        /// <summary>
        /// 課金通貨を購入
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator BuyStamina(
            Animator animator
        )
        {
            yield return staminaController.Buy(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.BuySucceed.ToString()
                        : Trigger.BuyFailed.ToString());
                }
            );
        }

        /// <summary>
        /// スタミナを消費（非推奨 サービス間の連携（スタンプシート）を経由してスタミナ値を操作するほうが望ましい）
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="consumeValue"></param>
        /// <returns></returns>
        private IEnumerator ConsumeStamina(
            Animator animator,
            int consumeValue
        )
        {
            yield return staminaController.ConsumeStamina(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.ConsumeStaminaSucceed.ToString()
                        : Trigger.ConsumeStaminaFailed.ToString());
                },
                consumeValue
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
            yield return moneyController.ListProducts(
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
        private IEnumerator BuyMoney(
            Animator animator,
            Product product
        )
        {
            yield return moneyController.Buy(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.BuySucceed.ToString()
                        : Trigger.BuyFailed.ToString());
                },
                selectProduct
            );
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            State? newState = null;
            if (stateInfo.IsName(State.Initialize.ToString()))
            {
                staminaController.StartCoroutine(
                    Initialize(
                        animator
                    )
                );
            }
            if (stateInfo.IsName(State.GetPropertiesProcessing.ToString()))
            {
                staminaController.StartCoroutine(
                    GetProperties(
                        animator
                    )
                );
            }
            if (stateInfo.IsName(State.StaminaBuyProcessing.ToString()))
            {
                staminaController.StartCoroutine(
                    BuyStamina(
                        animator
                    )
                );
            }
            if (stateInfo.IsName(State.GetMoneyProductsProcessing.ToString()))
            {
                staminaController.StartCoroutine(
                    ListProducts(
                        animator
                    )
                );
            }
            if (stateInfo.IsName(State.MoneyBuyProcessing.ToString()))
            {
                staminaController.StartCoroutine(
                    BuyMoney(
                        animator,
                        selectProduct
                    )
                );
            }
            if (stateInfo.IsName(State.ConsumeProgress.ToString()))
            {
                staminaController.StartCoroutine(
                    ConsumeStamina(
                        animator,
                        10
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
