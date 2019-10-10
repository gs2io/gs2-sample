﻿using System;
using Gs2.Sample.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Money
{
    public class MoneyScene : MonoBehaviour
    {
        /// <summary>
        /// マッチメイキング操作をするためのコントローラー
        /// </summary>
        public MoneyController controller;

        /// <summary>
        /// ストア表示する際に商品を並べるビューポート
        /// </summary>
        public GameObject productViewPort;

        /// <summary>
        /// ウォレットの残高表示
        /// </summary>
        public Text walletValue;

        /// <summary>
        /// 発生したエラー
        /// </summary>
        [SerializeField]
        public Text errorMessage;

        /// <summary>
        /// ステートマシン
        /// </summary>
        private MoneyStateMachine _stateMachine;

        private void Start()
        {
            controller.Initialize();
            
            if (controller.gs2MoneySetting == null)
            {
                throw new InvalidProgramException("'Gs2MoneySetting' is not null.");
            }
            if (controller.gs2Client == null)
            {
                controller.gs2Client = Gs2Util.LoadGlobalGameObject<Gs2Client>("Gs2Client");
                if (controller.gs2Client == null)
                {
                    throw new InvalidProgramException(
                        "Unable to find GS2 Client" +
                        "You need to set GS2 Client on 'MoneyController' or place a GameObject named 'Gs2Client' in the scene." +
                        "Please check README.md for details." +
                        " / " +
                        "GS2 Client を見つけられません。" +
                        "'MoneyController' に GS2 Client を設定するか、'Gs2Client' という名前の GameObject をシーン内に配置する必要があります。" +
                        "詳しくは README.md をご確認ください。"
                    );
                }
            }

            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'MoneyStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'MoneyStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }
            _stateMachine = animator.GetBehaviour<MoneyStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'MoneyStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'MoneyStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }

            _stateMachine.controller = controller;
            _stateMachine.onChangeState.AddListener(
                (_, state) =>
                {
                    InActiveAll();
                    GetMenuGameObject(state).SetActive(true);
                }
            );

            var originalWalletValueText = walletValue.text;
            controller.gs2MoneySetting.onGetWallet.AddListener(
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
            
            controller.gs2MoneySetting.onGetProducts.AddListener(
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
                                        ClickToBuy(product);
                                    }
                                );
                            }
                        }
                    }
                }
            );
            
            controller.gs2MoneySetting.onError.AddListener(
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
            foreach (MoneyStateMachine.State state in Enum.GetValues(typeof(MoneyStateMachine.State)))
            {
                GetMenuGameObject(state).SetActive(false);
            }
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(MoneyStateMachine.State state)
        {
            switch (state)
            {
                case MoneyStateMachine.State.Idle:
                    return transform.Find("MainMenu").gameObject;
                case MoneyStateMachine.State.SelectProduct:
                    return transform.Find("Store").gameObject;
                case MoneyStateMachine.State.Error:
                    return transform.Find("Error").gameObject;
                default:
                    return transform.Find("Processing").gameObject;
            }
        }

        /// <summary>
        /// ストアを開く
        /// </summary>
        public void ClickToOpenStore()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(MoneyStateMachine.Trigger.OpenStore.ToString());
        }

        /// <summary>
        /// 購入する
        /// </summary>
        public void ClickToBuy(Product product)
        {
            var stateMachine = GetComponent<Animator>();
            _stateMachine.selectProduct = product;
            stateMachine.SetTrigger(MoneyStateMachine.Trigger.SelectProduct.ToString());
        }

        /// <summary>
        /// 戻る
        /// </summary>
        public void ClickToBack()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(MoneyStateMachine.Trigger.Back.ToString());
        }

        /// <summary>
        /// エラー内容の確認ボタンをクリック
        /// </summary>
        public void ClickToConfirmError()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(MoneyStateMachine.Trigger.ConfirmError.ToString());
        }
    }
}