﻿using System;
 using Gs2.Core.Util;
 using Gs2.Sample.Core;
 using Gs2.Sample.Money;
 using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Stamina
{
    public class StaminaScene : MonoBehaviour
    {
        /// <summary>
        /// マッチメイキング操作をするためのコントローラー
        /// </summary>
        public StaminaController staminaController;

        /// <summary>
        /// マッチメイキング操作をするためのコントローラー
        /// </summary>
        public MoneyController moneyController;

        /// <summary>
        /// スタミナの現在値表示
        /// </summary>
        public Text staminaValue;

        /// <summary>
        /// スタミナの次回回復時刻表示
        /// </summary>
        public Text nextRecoverCountDown;

        /// <summary>
        /// ストア表示する際に商品を並べるビューポート
        /// </summary>
        public GameObject productViewPort;

        /// <summary>
        /// ウォレットの残高表示
        /// </summary>
        public Text walletValue;

        /// <summary>
        /// スタミナの購入ボタン
        /// </summary>
        public Text buyStaminaButton;

        /// <summary>
        /// 発生したエラー
        /// </summary>
        [SerializeField]
        public Text errorMessage;

        /// <summary>
        /// ステートマシン
        /// </summary>
        private StaminaStateMachine _stateMachine;

        private string _originalBuyStaminaButtonText = null;

        private void Start()
        {
            staminaController.Initialize();
            moneyController.Initialize();
            
            if (staminaController.gs2StaminaSetting == null)
            {
                throw new InvalidProgramException("'Gs2StaminaSetting' is not null.");
            }
            if (moneyController.gs2MoneySetting == null)
            {
                throw new InvalidProgramException("'Gs2MoneySetting' is not null.");
            }
            if (staminaController.gs2Client == null)
            {
                staminaController.gs2Client = Gs2Util.LoadGlobalGameObject<Gs2Client>("Gs2Client");
                if (staminaController.gs2Client == null)
                {
                    throw new InvalidProgramException(
                        "Unable to find GS2 Client" +
                        "You need to set GS2 Client on 'StaminaController' or place a GameObject named 'Gs2Client' in the scene." +
                        "Please check README.md for details." +
                        " / " +
                        "GS2 Client を見つけられません。" +
                        "'StaminaController' に GS2 Client を設定するか、'Gs2Client' という名前の GameObject をシーン内に配置する必要があります。" +
                        "詳しくは README.md をご確認ください。"
                    );
                }
                moneyController.gs2Client = Gs2Util.LoadGlobalGameObject<Gs2Client>("Gs2Client");
            }

            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'StaminaStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct staminaController is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'StaminaStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }
            _stateMachine = animator.GetBehaviour<StaminaStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'StaminaStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct staminaController is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'StaminaStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }

            _stateMachine.staminaController = staminaController;
            _stateMachine.moneyController = moneyController;
            _stateMachine.onChangeState.AddListener(
                (_, state) =>
                {
                    InActiveAll();
                    GetMenuGameObject(state).SetActive(true);
                }
            );

            var originalStaminaValueText = staminaValue.text;
            staminaController.gs2StaminaSetting.onGetStamina.AddListener(
                stamina =>
                {
                    if (staminaValue != null)
                    {
                        staminaValue.text =
                            originalStaminaValueText
                                .Replace("{current_stamina}", stamina.Value.ToString())
                                .Replace("{max_stamina}", stamina.MaxValue.ToString());
                    }
                }
            );
            staminaValue.text = originalStaminaValueText
                .Replace("{current_stamina}", "--")
                .Replace("{max_stamina}", "--");
            nextRecoverCountDown.text = "--:--";
            
            var originalWalletValueText = walletValue.text;
            moneyController.gs2MoneySetting.onGetWallet.AddListener(
                wallet =>
                {
                    if (walletValue != null)
                    {
                        walletValue.text =
                            originalWalletValueText.Replace("{wallet_value}", (wallet.Free + wallet.Paid).ToString());
                    }
                }
            );
            walletValue.text = originalWalletValueText.Replace("{wallet_value}", "---");

            _originalBuyStaminaButtonText = buyStaminaButton.text;
            
            moneyController.gs2MoneySetting.onGetProducts.AddListener(
                products =>
                {
                    if (productViewPort != null)
                    {
                        for (int i = 0; i < productViewPort.transform.childCount; i++)
                        {
                            Destroy(productViewPort.transform.GetChild(i).gameObject);
                        }

                        foreach (var product in products)
                        {
                            var productView = Gs2Util.LoadGlobalResource<ProductView>();
                            productView.transform.SetParent(productViewPort.transform);
                            productView.Initialize(product);
                            productView.transform.localScale = new Vector3(1, 1, 1);
                            if (!productView.Sold)
                            {
                                productView.transform.GetComponentInChildren<Button>().onClick.AddListener(
                                    () =>
                                    {
                                        ClickToBuyMoney(product);
                                    }
                                );
                            }
                        }
                    }
                }
            );

            staminaController.gs2StaminaSetting.onError.AddListener(
                e =>
                {
                    if (errorMessage != null)
                    {
                        errorMessage.text = e.Message;
                    }
                }
            );

            // 画面の初期状態を設定
            InActiveAll();
        }

        /// <summary>
        /// メニューパネルをすべて非表示にする
        /// </summary>
        private void InActiveAll()
        {
            foreach (StaminaStateMachine.State state in Enum.GetValues(typeof(StaminaStateMachine.State)))
            {
                GetMenuGameObject(state).SetActive(false);
            }
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(StaminaStateMachine.State state)
        {
            switch (state)
            {
                case StaminaStateMachine.State.Idle:
                    return transform.Find("MainMenu").gameObject;
                case StaminaStateMachine.State.MoneyStore:
                    return transform.Find("MoneyStore").gameObject;
                case StaminaStateMachine.State.StaminaStore:
                    return transform.Find("StaminaStore").gameObject;
                case StaminaStateMachine.State.Error:
                    return transform.Find("Error").gameObject;
                default:
                    return transform.Find("Processing").gameObject;
            }
        }

        private void Update()
        {
            if (_stateMachine._stamina != null)
            {
                if (_stateMachine._stamina.NextRecoverAt == 0)
                {
                    nextRecoverCountDown.text = "--:--";
                }
                else
                {
                    var timeSpan = UnixTime.FromUnixTime(_stateMachine._stamina.NextRecoverAt) - DateTime.UtcNow;
                    if (timeSpan.Ticks < 0)
                    {
                        if (_stateMachine._stamina.Value >= _stateMachine._stamina.MaxValue)
                        {
                            _stateMachine._stamina.Value = _stateMachine._stamina.MaxValue;
                            _stateMachine._stamina.NextRecoverAt = 0;
                        }
                        else
                        {
                            _stateMachine._stamina.Value += _stateMachine._stamina.RecoverValue;
                            _stateMachine._stamina.NextRecoverAt += _stateMachine._stamina.RecoverIntervalMinutes * 60 * 1000;
                            
                            timeSpan = UnixTime.FromUnixTime(_stateMachine._stamina.NextRecoverAt) - DateTime.UtcNow;
                        }
                        
                        staminaController.gs2StaminaSetting.onGetStamina.Invoke(_stateMachine._stamina);
                    }
                    nextRecoverCountDown.text = $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
                }
            }
        }

        /// <summary>
        /// ストアを開く
        /// </summary>
        public void ClickToOpenMoneyStore()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(StaminaStateMachine.Trigger.OpenMoneyStore.ToString());
        }

        /// <summary>
        /// ストアを開く
        /// </summary>
        public void ClickToOpenStaminaStore()
        {
            var stateMachine = GetComponent<Animator>();
            
            buyStaminaButton.text = _originalBuyStaminaButtonText
                .Replace("{gem_num}", "5")
                .Replace("{current_stamina}", _stateMachine._stamina.Value.ToString())
                .Replace("{recovered_stamina}", (_stateMachine._stamina.Value + 10).ToString());

            stateMachine.SetTrigger(StaminaStateMachine.Trigger.OpenStaminaStore.ToString());
        }

        /// <summary>
        /// 購入ボタン
        /// </summary>
        public void ClickToBuyStamina()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(StaminaStateMachine.Trigger.Purchase.ToString());
        }

        /// <summary>
        /// 購入する
        /// </summary>
        public void ClickToBuyMoney(Product product)
        {
            var stateMachine = GetComponent<Animator>();
            _stateMachine.selectProduct = product;
            stateMachine.SetTrigger(StaminaStateMachine.Trigger.SelectProduct.ToString());
        }

        /// <summary>
        /// スタミナを消費
        /// </summary>
        public void ClickToConsumeStamina()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(StaminaStateMachine.Trigger.ConsumeStamina.ToString());
        }

        /// <summary>
        /// 戻る
        /// </summary>
        public void ClickToBack()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(StaminaStateMachine.Trigger.Back.ToString());
        }

        /// <summary>
        /// エラー内容の確認ボタンをクリック
        /// </summary>
        public void ClickToConfirmError()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(StaminaStateMachine.Trigger.ConfirmError.ToString());
        }
    }
}