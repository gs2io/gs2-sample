﻿using System;
using Gs2.Sample.Core;
using UnityEngine;
 using UnityEngine.UI;

 namespace Gs2.Sample.AccountRegistrationLoginSample
{
    public class AccountRegistrationLogin : MonoBehaviour
    {
        /// <summary>
        /// GS2 の設定値
        /// </summary>
        [SerializeField]
        public Gs2AccountSetting gs2AccountSetting;

        /// <summary>
        /// ログイン画面に表示するユーザID
        /// </summary>
        [SerializeField]
        public Text userId;

        /// <summary>
        /// 発生したエラー
        /// </summary>
        [SerializeField]
        public Text errorMessage;

        /// <summary>
        /// 
        /// </summary>
        private Gs2Client _client;
        
        /// <summary>
        /// ステートマシン
        /// </summary>
        private AccountRegistrationLoginStateMachine _stateMachine;

        private void Start()
        {
            if (gs2AccountSetting == null)
            {
                throw new InvalidProgramException("'Gs2AccountSetting' is not null.");
            }
            if (string.IsNullOrEmpty(gs2AccountSetting.accountNamespaceName))
            {
                throw new InvalidProgramException(
                    "'accountNamespaceName' of script 'Gs2AccountSetting' of 'Canvas' is not set. "+
                    "The value to be set for 'accountNamespaceName' can be created by uploading the 'initialize_account_template.yaml' bundled with the sample as a GS2-Deploy stack." +
                    "Please check README.md for details." +
                    " / " +
                    "'Canvas' の持つスクリプト 'Gs2AccountSetting' の 'accountNamespaceName' が設定されていません。" +
                    "'accountNamespaceName' に設定するべき値はサンプルに同梱されている 'initialize_account_template.yaml' を GS2-Deploy のスタックとしてアップロードすることで作成できます。" +
                    "詳しくは README.md をご確認ください。"
                    );
            }
            if (string.IsNullOrEmpty(gs2AccountSetting.gatewayNamespaceName))
            {
                throw new InvalidProgramException(
                    "'gatewayNamespaceName' of script 'Gs2AccountSetting' of 'Canvas' is not set. "+
                    "The value to be set for 'gatewayNamespaceName' can be created by uploading the 'initialize_account_template.yaml' bundled with the sample as a GS2-Deploy stack." +
                    "Please check README.md for details." +
                    " / " +
                    "'Canvas' の持つスクリプト 'Gs2AccountSetting' の 'gatewayNamespaceName' が設定されていません。" +
                    "'gatewayNamespaceName' に設定するべき値はサンプルに同梱されている 'initialize_account_template.yaml' を GS2-Deploy のスタックとしてアップロードすることで作成できます。" +
                    "詳しくは README.md をご確認ください。"
                );
            }
            if (string.IsNullOrEmpty(gs2AccountSetting.accountEncryptionKeyId))
            {
                throw new InvalidProgramException(
                    "'accountEncryptionKeyId' of script 'Gs2AccountSetting' of 'Canvas' is not set. "+
                    "The value to be set for 'accountEncryptionKeyId' can be created by uploading the 'initialize_account_template.yaml' bundled with the sample as a GS2-Deploy stack." +
                    "Please check README.md for details." +
                    " / " +
                    "'Canvas' の持つスクリプト 'Gs2AccountSetting' の 'accountEncryptionKeyId' が設定されていません。" +
                    "'accountEncryptionKeyId' に設定するべき値はサンプルに同梱されている 'initialize_account_template.yaml' を GS2-Deploy のスタックとしてアップロードすることで作成できます。" +
                    "詳しくは README.md をご確認ください。"
                );
            }
        
            var gs2Client = GameObject.Find("Gs2Client");
            if (gs2Client == null)
            {
                throw new InvalidProgramException(
                    "No GameObject named 'Gs2Client' found" +
                    "It is necessary to place a GameObject registered with the settings for accessing GS2 in the scene." +
                    "Please check README.md for details." + 
                    " / " +
                    "'Gs2Client' という名前の GameObject が見つかりません。" +
                    "シーンに GS2 にアクセスするための設定を登録した GameObject を配置する必要があります。" +
                    "詳しくは README.md をご確認ください。"
                    );
            }
            _client = gs2Client.GetComponent<Gs2Client>();
            if (_client == null)
            {
                throw new InvalidProgramException(
                    "No GameObject named 'Gs2Client' found" +
                    "It is necessary to place a GameObject registered with the settings for accessing GS2 in the scene." +
                    "Please check README.md for details." + 
                    " / " +
                    "'Gs2Client' という名前の GameObject が見つかりません。" +
                    "シーンに GS2 にアクセスするための設定を登録した GameObject を配置する必要があります。" +
                    "詳しくは README.md をご確認ください。"
                );
            }
            if (!_client.initialized)
            {
                throw new InvalidProgramException(
                    "'Gs2Client' is not initialized. Change the initialization priority from 'Project Setting'." + 
                    " / " +
                    "'Gs2Client' が初期化されていません。 'Project Setting' から初期化優先度を変更してください。"
                );
            }
            
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'AccountRegistrationLoginStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'AccountRegistrationLoginStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }
            _stateMachine = animator.GetBehaviour<AccountRegistrationLoginStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'AccountRegistrationLoginStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'AccountRegistrationLoginStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }

            if (!_stateMachine.initialized)
            {
                _stateMachine.Initialize(
                    _client.client,
                    _client.profile,
                    gs2AccountSetting,
                    new AccountRepository(),
                    _client
                );

                _stateMachine.onChangeState += (state) =>
                {
                    InActiveAll();
                    GetMenuGameObject(state).SetActive(true);
                };

                // 画面の初期状態を設定
                InActiveAll();
            }
        }

        /// <summary>
        /// メニューパネルをすべて非表示にする
        /// </summary>
        private void InActiveAll()
        {
            GetMenuGameObject(AccountRegistrationLoginStateMachine.State.Initialize).SetActive(false);
            GetMenuGameObject(AccountRegistrationLoginStateMachine.State.LoadAccount).SetActive(false);
            GetMenuGameObject(AccountRegistrationLoginStateMachine.State.CreateAccountMenu).SetActive(false);
            GetMenuGameObject(AccountRegistrationLoginStateMachine.State.CreateAccountProcessing).SetActive(false);
            GetMenuGameObject(AccountRegistrationLoginStateMachine.State.SaveAccount).SetActive(false);
            GetMenuGameObject(AccountRegistrationLoginStateMachine.State.LoginMenu).SetActive(false);
            GetMenuGameObject(AccountRegistrationLoginStateMachine.State.LoginProcessing).SetActive(false);
            GetMenuGameObject(AccountRegistrationLoginStateMachine.State.LoginComplete).SetActive(false);
            GetMenuGameObject(AccountRegistrationLoginStateMachine.State.RemoveAccountProcessing).SetActive(false);
            GetMenuGameObject(AccountRegistrationLoginStateMachine.State.Error).SetActive(false);
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(AccountRegistrationLoginStateMachine.State state)
        {
            switch (state)
            {
                case AccountRegistrationLoginStateMachine.State.CreateAccountMenu:
                    return transform.Find("CreateAccount").gameObject;
                case AccountRegistrationLoginStateMachine.State.LoginMenu:
                    if (_stateMachine.account != null)
                    {
                        userId.text = "UserId: " + _stateMachine.account.UserId;
                    }
                    return transform.Find("Login").gameObject;
                case AccountRegistrationLoginStateMachine.State.Error:
                    return transform.Find("Error").gameObject;
                default:
                    return transform.Find("Processing").gameObject;
            }
        }

        /// <summary>
        /// アカウントの作成ボタンをクリック
        /// </summary>
        public void ClickToCreateAccount()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(AccountRegistrationLoginStateMachine.Trigger.SelectCreateAccount.ToString());
        }

        /// <summary>
        /// ログインボタンをクリック
        /// </summary>
        public void ClickToLogin()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(AccountRegistrationLoginStateMachine.Trigger.SelectLogin.ToString());
        }

        /// <summary>
        /// アカウントの削除ボタンをクリック
        /// </summary>
        public void ClickToRemoveAccount()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(AccountRegistrationLoginStateMachine.Trigger.SelectRemoveAccount.ToString());
        }

        /// <summary>
        /// エラー内容の確認ボタンをクリック
        /// </summary>
        public void ClickToConfirmError()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(AccountRegistrationLoginStateMachine.Trigger.ConfirmError.ToString());
        }
    }
}