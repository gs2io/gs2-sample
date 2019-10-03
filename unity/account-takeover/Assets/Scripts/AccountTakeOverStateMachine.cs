﻿using System;
using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Unity.Gs2Account.Model;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.AccountTakeOver
{
    public class AccountTakeOverStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            Initialize,
            MainMenu,
            SelectDoTakeOverType,
            GetTakeOverSettingsProcessing,
            SelectSetTakeOverType,
            DoEmailTakeOver,
            DoPlatformTakeOver,
            SetEmailTakeOver,
            SetPlatformTakeOver,
            DoTakeOverProcessing,
            SetTakeOverProcessing,
            DeleteTakeOver,
            DeleteTakeOverProcessing,
            DoTakeOverCompleted,
            Error,
        }

        public enum Trigger
        {
            InitializeSucceed,
            InitializeFailed,
            SelectDoTakeOverType,
            SelectSetTakeOverType,
            GetTakeOverSettingsSucceed,
            GetTakeOverSettingsFailed,
            SelectDoEmailTakeOver,
            SelectDoPlatformTakeOver,
            SelectSetEmailTakeOver,
            SelectSetPlatformTakeOver,
            ReadyDoTakeOver,
            ReadySetTakeOver,
            NotPlatformLogin,
            DoTakeOverSucceed,
            DoTakeOverFailed,
            SetTakeOverSucceed,
            SetTakeOverFailed,
            SelectDeleteTakeOver,
            ReadyDeleteTakeOver,
            DeleteTakeOverSucceed,
            DeleteTakeOverFailed,
            Back,
            ConfirmError,
        }
        
        public enum TakeOverType
        {
            Email = 0,
            Platform = 1,
        }

        /// <summary>
        /// マッチメイキングコントローラー
        /// </summary>
        public AccountTakeOverController controller;

        /// <summary>
        /// 現在有効な引継ぎ設定一覧
        /// </summary>
        private List<EzTakeOver> _takeOverSettings = new List<EzTakeOver>();
        
        /// <summary>
        /// 引継ぎの種類
        /// </summary>
        public int type;
        
        /// <summary>
        /// ユーザ識別子
        /// </summary>
        public string userIdentifier;
        
        /// <summary>
        /// パスワード
        /// </summary>
        public string password;

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
                controller.gs2AccountTakeOverSetting.onError.Invoke(
                    result.Error
                );
                animator.SetTrigger(Trigger.InitializeFailed.ToString());
                yield break;
            }
            
            animator.SetTrigger(Trigger.InitializeSucceed.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EzTakeOver GetEmailTakeOverSetting()
        {
            return _takeOverSettings.Find(takeOver => takeOver.Type == (int) TakeOverType.Email);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EzTakeOver GetPlatformTakeOverSetting()
        {
            return _takeOverSettings.Find(takeOver => takeOver.Type == (int) TakeOverType.Platform);
        }

        /// <summary>
        /// 現在登録されている引継ぎ情報を取得
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator GetTakeOverSettings(
            Animator animator
        )
        {
            yield return controller.ListAccountTakeOverSettings(
                r =>
                {
                    if (r.Error == null)
                    {
                        _takeOverSettings = r.Result.Items;
                    }
                    animator.SetTrigger(r.Error == null
                        ? Trigger.GetTakeOverSettingsSucceed.ToString()
                        : Trigger.GetTakeOverSettingsFailed.ToString());
                }
            );
        }

        /// <summary>
        /// 引継ぎ設定の登録を実行
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator SetTakeOverSetting(
            Animator animator
        )
        {
            yield return controller.AddAccountTakeOverSetting(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.SetTakeOverSucceed.ToString()
                        : Trigger.SetTakeOverFailed.ToString());
                },
                type,
                userIdentifier,
                password
            );
        }

        /// <summary>
        /// 引継ぎを実行
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator DoTakeOver(
            Animator animator
        )
        {
            yield return controller.DoAccountTakeOver(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.DoTakeOverSucceed.ToString()
                        : Trigger.DoTakeOverFailed.ToString());
                },
                type,
                userIdentifier,
                password
            );
        }

        /// <summary>
        /// 引継ぎ設定の削除を実行
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator DeleteTakeOverSetting(
            Animator animator
        )
        {
            yield return controller.DeleteAccountTakeOverSetting(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.DeleteTakeOverSucceed.ToString()
                        : Trigger.DeleteTakeOverFailed.ToString());
                },
                type
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
            else if (stateInfo.IsName(State.GetTakeOverSettingsProcessing.ToString()))
            {
                controller.StartCoroutine(
                    GetTakeOverSettings(
                        animator
                    )
                );
            }
            else if (stateInfo.IsName(State.SetTakeOverProcessing.ToString()))
            {
                controller.StartCoroutine(
                    SetTakeOverSetting(
                        animator
                    )
                );
            }
            else if (stateInfo.IsName(State.DoTakeOverProcessing.ToString()))
            {
                controller.StartCoroutine(
                    DoTakeOver(
                        animator
                    )
                );
            }
            else if (stateInfo.IsName(State.DeleteTakeOverProcessing.ToString()))
            {
                controller.StartCoroutine(
                    DeleteTakeOverSetting(
                        animator
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
