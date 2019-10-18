﻿using System;
using UnityEngine;
using UnityEngine.UI;
using Gs2.Sample.Stamina.Internal;

 namespace Gs2.Sample.Stamina
{
    public class StaminaStoreWidget : MonoBehaviour
    {
        /// <summary>
        /// ステートマシン
        /// </summary>
        private StaminaStoreWidgetStateMachine _stateMachine;

        /// <summary>
        /// スタミナの購入ボタン
        /// </summary>
        public Text buyStaminaButton;

        private string _originalBuyStaminaButtonText = null;

        private void Start()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'StaminaStoreWidgetStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'StaminaStoreWidgetStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }
            _stateMachine = animator.GetBehaviour<StaminaStoreWidgetStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'StaminaStoreWidgetStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'StaminaStoreWidgetStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }

            _stateMachine.controller.Initialize();
            _originalBuyStaminaButtonText = buyStaminaButton.text;
            _stateMachine.controller.gs2StaminaSetting.onGetStamina.AddListener(
                stamina =>
                {
                    buyStaminaButton.text = _originalBuyStaminaButtonText
                        .Replace("{gem_num}", "5")
                        .Replace("{current_stamina}", stamina.Value.ToString())
                        .Replace("{recovered_stamina}", (stamina.Value + 10).ToString());
                }
            );
            _stateMachine.onChangeState.AddListener(
                (_, state) =>
                {
                    InActiveAll();
                    GetMenuGameObject(state).SetActive(true);
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
            foreach (StaminaStoreWidgetStateMachine.State state in Enum.GetValues(typeof(StaminaStoreWidgetStateMachine.State)))
            {
                GetMenuGameObject(state).SetActive(false);
            }
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(StaminaStoreWidgetStateMachine.State state)
        {
            switch (state)
            {
                case StaminaStoreWidgetStateMachine.State.Store:
                    return transform.Find("Store").gameObject;
                default:
                    return transform.Find("Processing").gameObject;
            }
        }

        /// <summary>
        /// 購入する
        /// </summary>
        public void ClickToBuy()
        {
            _stateMachine.Purchase();
        }

        /// <summary>
        /// 閉じる
        /// </summary>
        public void ClickToClose()
        {
            var gs2StaminaInternalSetting = GameObject.Find("Gs2StaminaInternalSetting").GetComponent<Gs2StaminaInternalSetting>();
            gs2StaminaInternalSetting.onCloseStore.Invoke(
                this
            );
        }
    }
}