﻿using System;
 using System.Collections;
 using System.Collections.Generic;
 using Gs2.Core;
 using Gs2.Core.Exception;
 using Gs2.Core.Model;
 using Gs2.Sample.Core;
 using Gs2.Unity.Gs2Account.Model;
 using Gs2.Unity.Gs2Account.Result;
 using Gs2.Unity.Gs2Gateway.Result;
 using Gs2.Unity.Util;
 using UnityEngine;
 using UnityEngine.Events;
 using UnityEngine.UI;

 namespace Gs2.Sample.AccountRegistrationLoginSample
{
    public class AccountRegistrationLoginController : MonoBehaviour
    {
        /// <summary>
        /// GS2-Account の設定値
        /// </summary>
        [SerializeField]
        public Gs2AccountSetting gs2AccountSetting;

        /// <summary>
        /// Gs2Client
        /// </summary>
        [SerializeField]
        public Gs2Client gs2Client;

        /// <summary>
        /// GS2のゲームプレイヤーアカウントの永続化処理
        /// </summary>
        private readonly AccountRepository _repository = new AccountRepository();

        public void Initialize()
        {
            if (!gs2AccountSetting)
            {
                gs2AccountSetting = Gs2Util.LoadGlobalGameObject<Gs2AccountSetting>("Gs2Settings");
            }
            if (!gs2Client)
            {
                gs2Client = Gs2Util.LoadGlobalGameObject<Gs2Client>("Gs2Settings");
            }

            Validate();
        }
        
        private void Validate()
        {
            if (!gs2AccountSetting)
            {
                throw new InvalidProgramException("'Gs2AccountSetting' が設定されていません");
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
        
            if (!gs2Client)
            {
                throw new InvalidProgramException("'Gs2Client' が設定されていません");
            }
        }

        /// <summary>
        /// アカウントを新規作成する
        /// </summary>
        /// <returns></returns>
        public IEnumerator Registration(
            UnityAction<AsyncResult<EzCreateResult>> callback
        )
        {
            Initialize();
            
            AsyncResult<EzCreateResult> result = null;
            yield return gs2Client.client.Account.Create(
                r =>
                {
                    result = r;
                },
                gs2AccountSetting.accountNamespaceName
            );
            
            if (result.Error != null)
            {
                gs2AccountSetting.onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }

            var account = result.Result.Item;

            gs2AccountSetting.onCreateAccount.Invoke(account);
            callback.Invoke(result);
        }

        /// <summary>
        /// アカウントの永続化処理
        /// </summary>
        /// <returns></returns>
        public IEnumerator LoadAccount(
            UnityAction<AsyncResult<EzAccount>> callback
        )
        {
            Initialize();

            if (_repository.IsRegistered())
            {
                var account = _repository.LoadAccount();
                gs2AccountSetting.onLoadAccount.Invoke(account);
                callback.Invoke(new AsyncResult<EzAccount>(account, null));
            }
            else
            {
                var result = new NotFoundException(new List<RequestError>());
                gs2AccountSetting.onError.Invoke(
                    result
                );
                callback.Invoke(new AsyncResult<EzAccount>(null, result));
            }
            yield break;
        }

        /// <summary>
        /// アカウントの永続化処理
        /// </summary>
        /// <returns></returns>
        public IEnumerator SaveAccount(
            UnityAction<AsyncResult<object>> callback,
            EzAccount account
        )
        {
            Initialize();

            _repository.SaveAccount(account);
            gs2AccountSetting.onSaveAccount.Invoke(account);
            
            callback.Invoke(new AsyncResult<object>(null, null));
            yield break;
        }

        /// <summary>
        /// アカウント削除処理
        /// </summary>
        /// <returns></returns>
        public IEnumerator RemoveAccount(
            UnityAction<AsyncResult<object>> callback
        )
        {
            Initialize();

            _repository.DeleteAccount();
            
            callback.Invoke(new AsyncResult<object>(null, null));
            yield break;
        }

        /// <summary>
        /// ログインする
        /// </summary>
        /// <returns></returns>
        public IEnumerator Login(
            UnityAction<AsyncResult<GameSession>> callback,
            EzAccount account
        )
        {
            Initialize();

            AsyncResult<GameSession> result1 = null;
            yield return gs2Client.profile.Login(
                new Gs2AccountAuthenticator(
                    gs2Client.profile.Gs2Session,
                    gs2AccountSetting.accountNamespaceName,
                    gs2AccountSetting.accountEncryptionKeyId,
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
                gs2AccountSetting.onError.Invoke(
                    result1.Error
                );
                yield break;
            }

            var session = result1.Result;
            
            AsyncResult<EzSetUserIdResult> result2 = null;
            yield return gs2Client.client.Gateway.SetUserId(
                r => { result2 = r; },
                session,
                gs2AccountSetting.gatewayNamespaceName,
                true
            );
            
            if (result2.Error != null)
            {
                gs2AccountSetting.onError.Invoke(
                    result2.Error
                );
                yield break;
            }

            gs2AccountSetting.onLogin.Invoke(account, session);
            callback.Invoke(result1);
        }
    }
}