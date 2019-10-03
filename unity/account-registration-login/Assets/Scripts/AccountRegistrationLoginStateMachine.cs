﻿using System;
using System.Collections;
using Gs2.Core;
using Gs2.Unity.Gs2Account.Model;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.AccountRegistrationLoginSample
{
    public class AccountRegistrationLoginStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            Initialize,
            LoadAccount,
            CreateAccountMenu,
            CreateAccountProcessing,
            SaveAccount,
            LoginMenu,
            LoginProcessing,
            LoginComplete,
            RemoveAccountProcessing,
            Error,
        }

        public enum Trigger
        {
            InitializeSucceed,
            InitializeFailed,
            LoadAccountSucceed,
            LoadAccountFailed,
            SelectCreateAccount,
            CreateAccountSucceed,
            CreateAccountFailed,
            SaveAccountSucceed,
            SaveAccountFailed,
            SelectLogin,
            LoginSucceed,
            LoginFailed,
            SelectRemoveAccount,
            RemoveAccountSucceed,
            RemoveAccountFailed,
            ResultCallback,
            ConfirmError,
        }

        public class ChangeStateEvent : UnityEvent<Animator, State>
        {
        }

        /// <summary>
        /// アカウント作成・ログインコントローラー
        /// </summary>
        public AccountRegistrationLoginController controller;

        /// <summary>
        /// アカウント情報
        /// </summary>
        public EzAccount account;

        /// <summary>
        /// ステートが変化した時に呼び出されるイベント
        /// </summary>
        public ChangeStateEvent onChangeState = new ChangeStateEvent();

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator Initialize(
            Animator animator
        )
        {
            controller.gs2AccountSetting.onLoadAccount.AddListener(
                loadAccount => { this.account = loadAccount; }
            );
            controller.gs2AccountSetting.onCreateAccount.AddListener(
                saveAccount => { this.account = saveAccount; }
            );
            
            AsyncResult<object> result = null;
            yield return controller.gs2Client.Initialize(
                r =>
                {
                    result = r;
                }
            );
            
            if (result.Error != null)
            {
                controller.gs2AccountSetting.onError.Invoke(
                    result.Error
                );
                animator.SetTrigger(Trigger.InitializeFailed.ToString());
                yield break;
            }

            animator.SetTrigger(Trigger.InitializeSucceed.ToString());
        }

        /// <summary>
        /// アカウントの作成
        /// </summary>
        /// <returns></returns>
        private IEnumerator Registration(
            Animator animator
        )
        {
            yield return controller.Registration(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.CreateAccountSucceed.ToString()
                        : Trigger.CreateAccountFailed.ToString());
                }
            );
        }

        /// <summary>
        /// アカウントの永続化処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator LoadAccount(
            Animator animator
        )
        {
            yield return controller.LoadAccount(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.LoadAccountSucceed.ToString()
                        : Trigger.LoadAccountFailed.ToString());
                }
            );
        }

        /// <summary>
        /// アカウントの永続化処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator SaveAccount(
            Animator animator
        )
        {
            yield return controller.SaveAccount(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.SaveAccountSucceed.ToString()
                        : Trigger.SaveAccountFailed.ToString());
                },
                account
            );
        }

        /// <summary>
        /// ログイン処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator Login(
            Animator animator
        )
        {
            yield return controller.Login(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.LoginSucceed.ToString()
                        : Trigger.LoginFailed.ToString());
                },
                account
            );
        }

        /// <summary>
        /// アカウント削除処理
        /// </summary>
        /// <returns></returns>
        private IEnumerator RemoveAccount(
            Animator animator
        )
        {
            yield return controller.RemoveAccount(
                r =>
                {
                    animator.SetTrigger(r.Error == null
                        ? Trigger.RemoveAccountSucceed.ToString()
                        : Trigger.RemoveAccountFailed.ToString());
                }
            );
        }
        
        /// <summary>
        /// ステートマシンのステートが変化したときのイベントハンドラ
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="stateInfo"></param>
        /// <param name="layerIndex"></param>
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            State? newState = null;
            if (stateInfo.IsName(State.Initialize.ToString()))
            {
                // 初期化処理
                controller.StartCoroutine(
                    Initialize(animator)
                );
            }
            else if (stateInfo.IsName(State.LoadAccount.ToString()))
            {
                // アカウント情報のロード
                controller.StartCoroutine(
                    LoadAccount(animator)
                );
            }
            else if (stateInfo.IsName(State.CreateAccountProcessing.ToString()))
            {
                // アカウントの作成処理
                controller.StartCoroutine(
                    Registration(animator)
                );
            }
            else if (stateInfo.IsName(State.SaveAccount.ToString()))
            {
                // アカウントの永続化処理
                controller.StartCoroutine(
                    SaveAccount(animator)
                );
            }
            else if (stateInfo.IsName(State.LoginProcessing.ToString()))
            {
                // ログイン処理
                controller.StartCoroutine(
                    Login(animator)
                );
            }
            else if (stateInfo.IsName(State.LoginComplete.ToString()))
            {
                // ログイン完了
                animator.SetTrigger(Trigger.ResultCallback.ToString());
            }
            else if (stateInfo.IsName(State.RemoveAccountProcessing.ToString()))
            {
                // アカウント削除処理
                controller.StartCoroutine(
                    RemoveAccount(animator)
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
