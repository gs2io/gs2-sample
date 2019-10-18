﻿using System;
 using Gs2.Sample.Core;
 using Gs2.Sample.Money.Internal;
 using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Money
{
    public class MoneyStatusWidget : MonoBehaviour
    {
        /// <summary>
        /// ウォレットの残高表示
        /// </summary>
        [SerializeField]
        public Text walletValue;

        /// <summary>
        /// ステートマシン
        /// </summary>
        private MoneyStatusWidgetStateMachine _stateMachine;

        private void Start()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'MoneyStatusWidgetStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " +
                    "ステートをコントロールする 'MoneyStatusWidgetStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                );
            }

            _stateMachine = animator.GetBehaviour<MoneyStatusWidgetStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'MoneyStatusWidgetStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " +
                    "ステートをコントロールする 'MoneyStatusWidgetStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                );
            }

            _stateMachine.controller.Initialize();
            _stateMachine.onChangeState.AddListener(
                (_, state) =>
                {
                    InActiveAll();
                    GetMenuGameObject(state).SetActive(true);
                }
            );

            var originalWalletValueText = walletValue.text;
            _stateMachine.controller.gs2MoneySetting.onGetWallet.AddListener(
                wallet =>
                {
                    if (walletValue != null)
                    {
                        walletValue.text =
                            originalWalletValueText
                                .Replace("{wallet_value}", (wallet.Free + wallet.Paid).ToString());
                    }
                }
            );
            _stateMachine.controller.gs2MoneySetting.onBuy.AddListener(
                wallet =>
                {
                    _stateMachine.Refresh();
                }
            );
            walletValue.text = originalWalletValueText
                .Replace("{wallet_value}", "---");

            // 画面の初期状態を設定
            InActiveAll();
        }

        /// <summary>
        /// メニューパネルをすべて非表示にする
        /// </summary>
        private void InActiveAll()
        {
            foreach (MoneyStatusWidgetStateMachine.State state in Enum.GetValues(
                typeof(MoneyStatusWidgetStateMachine.State)))
            {
                GetMenuGameObject(state).SetActive(false);
            }
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(MoneyStatusWidgetStateMachine.State state)
        {
            switch (state)
            {
                default:
                    return transform.Find("Panel").gameObject;
            }
        }

        public void ClickToOpenStore()
        {
            GameObject.Find("Gs2MoneyInternalSetting").GetComponent<Gs2MoneyInternalSetting>().onOpenStore.Invoke();
        }
    }
}