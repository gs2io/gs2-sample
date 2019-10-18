﻿using System;
using Gs2.Core.Util;
using Gs2.Sample.Stamina.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Stamina
{
    public class StaminaStatusWidget : MonoBehaviour
    {
        /// <summary>
        /// スタミナの現在値表示
        /// </summary>
        public Text staminaValue;

        /// <summary>
        /// スタミナの次回回復時刻表示
        /// </summary>
        public Text nextRecoverCountDown;

        /// <summary>
        /// ステートマシン
        /// </summary>
        private StaminaStatusWidgetStateMachine _stateMachine;

        private void Start()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'StaminaStatusWidgetStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " +
                    "ステートをコントロールする 'StaminaStatusWidgetStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                );
            }

            _stateMachine = animator.GetBehaviour<StaminaStatusWidgetStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'StaminaStatusWidgetStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " +
                    "ステートをコントロールする 'StaminaStatusWidgetStateMachine' が登録されていません." +
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

            var originalStaminaValueText = staminaValue.text;
            _stateMachine.controller.gs2StaminaSetting.onGetStamina.AddListener(
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
            _stateMachine.controller.gs2StaminaSetting.onBuy.AddListener(
                () =>
                {
                    _stateMachine.Refresh();
                }
            );
            staminaValue.text = originalStaminaValueText
                .Replace("{current_stamina}", "--")
                .Replace("{max_stamina}", "--");
            nextRecoverCountDown.text = "--:--";

            // 画面の初期状態を設定
            InActiveAll();
        }

        /// <summary>
        /// メニューパネルをすべて非表示にする
        /// </summary>
        private void InActiveAll()
        {
            foreach (StaminaStatusWidgetStateMachine.State state in Enum.GetValues(
                typeof(StaminaStatusWidgetStateMachine.State)))
            {
                GetMenuGameObject(state).SetActive(false);
            }
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(StaminaStatusWidgetStateMachine.State state)
        {
            switch (state)
            {
                default:
                    return transform.Find("Panel").gameObject;
            }
        }

        private void Update()
        {
            if (_stateMachine.stamina != null)
            {
                if (_stateMachine.stamina.NextRecoverAt == 0)
                {
                    nextRecoverCountDown.text = "--:--";
                }
                else
                {
                    var timeSpan = UnixTime.FromUnixTime(_stateMachine.stamina.NextRecoverAt) - DateTime.UtcNow;
                    if (timeSpan.Ticks < 0)
                    {
                        if (_stateMachine.stamina.Value >= _stateMachine.stamina.MaxValue)
                        {
                            _stateMachine.stamina.Value = _stateMachine.stamina.MaxValue;
                            _stateMachine.stamina.NextRecoverAt = 0;
                        }
                        else
                        {
                            _stateMachine.stamina.Value += _stateMachine.stamina.RecoverValue;
                            _stateMachine.stamina.NextRecoverAt += _stateMachine.stamina.RecoverIntervalMinutes * 60 * 1000;
                            
                            timeSpan = UnixTime.FromUnixTime(_stateMachine.stamina.NextRecoverAt) - DateTime.UtcNow;
                        }
                        
                        _stateMachine.controller.gs2StaminaSetting.onGetStamina.Invoke(_stateMachine.stamina);
                    }
                    nextRecoverCountDown.text = $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
                }
            }
        }

        /// <summary>
        /// ストアを開く
        /// </summary>
        public void ClickToOpenStore()
        {
            GameObject.Find("Gs2StaminaInternalSetting").GetComponent<Gs2StaminaInternalSetting>().onOpenStore.Invoke();
        }
    }
}