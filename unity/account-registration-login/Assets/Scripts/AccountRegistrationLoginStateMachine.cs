﻿using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Core.Model;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Gs2Account.Result;
using Gs2.Unity.Gs2Gateway.Result;
using Gs2.Unity.Util;
using UnityEngine;
 using UnityEngine.Events;
 using UnityEngine.Serialization;

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

        public class ChangeStateEvent : UnityEvent<State>
        {
        }
        
        public class CreateAccountEvent : UnityEvent<EzAccount>
        {
        }
        
        public class LoginEvent : UnityEvent<EzAccount, GameSession>
        {
        }
        
        public class ErrorEvent : UnityEvent<Gs2Exception>
        {
        }

        /// <summary>
        /// GS2のゲームプレイヤーアカウント情報
        /// </summary>
        public EzAccount account;

        /// <summary>
        /// GS2クライアント
        /// </summary>
        private Gs2.Unity.Client _client;

        /// <summary>
        /// GS2プロファイル
        /// </summary>
        private Gs2.Unity.Util.Profile _profile;

        /// <summary>
        /// GS2のゲームプレイヤーアカウントの永続化処理
        /// </summary>
        private IAccountRepository _repository;

        /// <summary>
        /// GS2の設定値
        /// </summary>
        private Gs2AccountSetting _setting;

        /// <summary>
        /// 
        /// </summary>
        private MonoBehaviour _monoBehaviour;

        /// <summary>
        /// アカウント作成メニューを表示するべきときに呼び出されるイベント
        /// </summary>
        public ChangeStateEvent onChangeState;

        /// <summary>
        /// アカウント作成時に発行されるイベント
        /// </summary>
        public CreateAccountEvent onCreateAccount;

        /// <summary>
        /// ログイン時に発行されるイベント
        /// </summary>
        public LoginEvent onLogin;

        /// <summary>
        /// エラー発生時に発行されるイベント
        /// </summary>
        public ErrorEvent onError;

        /// <summary>
        /// 
        /// </summary>
        public bool initialized;

        public void Initialize(
            Gs2.Unity.Client client,
            Gs2.Unity.Util.Profile profile,
            Gs2AccountSetting setting,
            IAccountRepository repository,
            MonoBehaviour monoBehaviour
        )
        {
            _client = client;
            _profile = profile;
            _setting = setting;
            _repository = repository;
            _monoBehaviour = monoBehaviour;
            
            onChangeState = new ChangeStateEvent();
            onCreateAccount = new CreateAccountEvent();
            onLogin = new LoginEvent();
            onError = new ErrorEvent();

            initialized = true;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="monoBehaviour"></param>
        /// <returns></returns>
        private IEnumerator Initialize(
            Animator animator,
            MonoBehaviour monoBehaviour
        )
        {
            AsyncResult<object> result = null;
            yield return _profile.Initialize(
                r =>
                {
                    result = r;
                }
            );
            
            if (result.Error != null)
            {
                if (onError != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                }

                animator.SetTrigger(Trigger.InitializeFailed.ToString());
                yield break;
            }

            animator.SetTrigger(Trigger.InitializeSucceed.ToString());
        }

        /// <summary>
        /// アカウントの永続化処理
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator LoadAccount(
            Animator animator
        )
        {
            if (_repository.IsRegistered())
            {
                account = _repository.LoadAccount();
                animator.SetTrigger(Trigger.LoadAccountSucceed.ToString());
            }
            else
            {
                if (onError != null)
                {
                    onError.Invoke(
                        new NotFoundException(new List<RequestError>())
                    );
                }
                animator.SetTrigger(Trigger.LoadAccountFailed.ToString());
            }
            yield break;
        }

        /// <summary>
        /// アカウントの作成処理
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator CreateAccount(
            Animator animator
        )
        {
            AsyncResult<EzCreateResult> result = null;
            yield return _client.Account.Create(
                r =>
                {
                    result = r;
                },
                _setting.accountNamespaceName
            );
            
            if (result.Error != null)
            {
                if (onError != null)
                {
                    onError.Invoke(
                        result.Error
                    );
                }

                animator.SetTrigger(Trigger.CreateAccountFailed.ToString());
                yield break;
            }

            account = result.Result.Item;

            if (onCreateAccount != null)
            {
                onCreateAccount.Invoke(account);
            }

            animator.SetTrigger(Trigger.CreateAccountSucceed.ToString());
        }

        /// <summary>
        /// アカウントの永続化処理
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator SaveAccount(
            Animator animator
        )
        {
            _repository.SaveAccount(account);
            animator.SetTrigger(Trigger.SaveAccountSucceed.ToString());
            yield break;
        }

        /// <summary>
        /// ログイン処理
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator Login(
            Animator animator
        )
        {
            AsyncResult<GameSession> result1 = null;
            yield return _profile.Login(
                new Gs2AccountAuthenticator(
                    _profile.Gs2Session,
                    _setting.accountNamespaceName,
                    _setting.accountEncryptionKeyId,
                    account.UserId,
                    account.Password
                ),
                r =>
                {
                    result1 = r;
                }
            );

            if (result1.Error != null)
            {
                if (onError != null)
                {
                    onError.Invoke(
                        result1.Error
                    );
                }

                animator.SetTrigger(Trigger.LoginFailed.ToString());
                yield break;
            }

            var session = result1.Result;
            
            AsyncResult<EzSetUserIdResult> result2 = null;
            yield return _client.Gateway.SetUserId(
                r => { result2 = r; },
                session,
                _setting.gatewayNamespaceName,
                true
            );
            
            if (result2.Error != null)
            {
                if (onError != null)
                {
                    onError.Invoke(
                        result2.Error
                    );
                }

                animator.SetTrigger(Trigger.LoginFailed.ToString());
                yield break;
            }

            animator.SetTrigger(Trigger.LoginSucceed.ToString());
            
            if (onLogin != null)
            {
                onLogin.Invoke(account, session);
            }
        }

        /// <summary>
        /// アカウント削除処理
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        private IEnumerator RemoveAccount(
            Animator animator
        )
        {
            _repository.DeleteAccount();
            animator.SetTrigger(Trigger.RemoveAccountSucceed.ToString());
            yield break;
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
                newState = State.Initialize;
            }
            else if (stateInfo.IsName(State.LoadAccount.ToString()))
            {
                // アカウント情報のロード
                newState = State.LoadAccount;
                _monoBehaviour.StartCoroutine(
                    LoadAccount(animator)
                );
            }
            else if (stateInfo.IsName(State.CreateAccountMenu.ToString()))
            {
                // アカウント作成メニュー
                newState = State.CreateAccountMenu;
            }
            else if (stateInfo.IsName(State.CreateAccountProcessing.ToString()))
            {
                // アカウントの作成処理
                newState = State.CreateAccountProcessing;
                _monoBehaviour.StartCoroutine(
                    CreateAccount(animator)
                );
            }
            else if (stateInfo.IsName(State.SaveAccount.ToString()))
            {
                // アカウントの永続化処理
                newState = State.SaveAccount;
                _monoBehaviour.StartCoroutine(
                    SaveAccount(animator)
                );
            }
            else if (stateInfo.IsName(State.LoginMenu.ToString()))
            {
                // ログインメニュー
                newState = State.LoginMenu;
            }
            else if (stateInfo.IsName(State.LoginProcessing.ToString()))
            {
                // ログイン処理
                newState = State.LoginProcessing;
                _monoBehaviour.StartCoroutine(
                    Login(animator)
                );
            }
            else if (stateInfo.IsName(State.LoginComplete.ToString()))
            {
                // ログイン完了
                newState = State.LoginComplete;
                animator.SetTrigger(Trigger.ResultCallback.ToString());
            }
            else if (stateInfo.IsName(State.RemoveAccountProcessing.ToString()))
            {
                // アカウント削除処理
                newState = State.RemoveAccountProcessing;
                _monoBehaviour.StartCoroutine(
                    RemoveAccount(animator)
                );
            }
            else if (stateInfo.IsName(State.Error.ToString()))
            {
                // エラー描画
                newState = State.Error;
            }

            if (!newState.HasValue)
            {
                if (onError != null)
                {
                    onError.Invoke(
                        new UnknownException("unknown state")
                    );
                }
            }

            if (onChangeState != null)
            {
                // ステート変化を通知
                onChangeState.Invoke(newState.Value);
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            if (animatorStateInfo.IsName(State.Initialize.ToString()))
            {
                if (initialized)
                {
                    _monoBehaviour.StartCoroutine(
                        Initialize(animator, _monoBehaviour)
                    );
                }
            }
        }
    }
}
