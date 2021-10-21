using System.Collections;
using Gs2.Core;
using Gs2.Sample.Core;
using Gs2.Unity.Gs2Account.Result;
using Gs2.Sample.AccountRegistrationLoginSample;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.AccountTakeOver
{
    public class AccountTakeOverController : MonoBehaviour
    {
        /// <summary>
        /// GS2-Matchmaking の設定値
        /// </summary>
        [SerializeField]
        public Gs2AccountSetting gs2AccountSetting;

        /// <summary>
        /// GS2-Matchmaking の設定値
        /// </summary>
        [SerializeField]
        public Gs2AccountTakeOverSetting gs2AccountTakeOverSetting;

        /// <summary>
        /// Gs2Client
        /// </summary>
        [SerializeField]
        public Gs2Client gs2Client;

        private void Validate()
        {
            if (!gs2AccountSetting)
            {
                gs2AccountSetting = Gs2Util.LoadGlobalGameObject<Gs2AccountSetting>("Gs2Settings");
            }

            if (!gs2AccountTakeOverSetting)
            {
                gs2AccountTakeOverSetting = Gs2Util.LoadGlobalGameObject<Gs2AccountTakeOverSetting>("Gs2Settings");
            }

            if (!gs2Client)
            {
                gs2Client = Gs2Util.LoadGlobalGameObject<Gs2Client>("Gs2Settings");
            }
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            Validate();

            if (!Social.localUser.authenticated)
            {
                Social.localUser.Authenticate (success =>
                {
                    Debug.Log("signed-in");
                });
            }
        }

        /// <summary>
        /// 設定済みの引継ぎ設定一覧を取得
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator ListAccountTakeOverSettings(
            UnityAction<AsyncResult<EzListTakeOverSettingsResult>> callback
        )
        {
            Validate();

            var request = Gs2Util.LoadGlobalGameObject<AccountTakeOverRequest>("AccountTakeOverRequest");

            AsyncResult<EzListTakeOverSettingsResult> result = null;
            yield return gs2Client.client.Account.ListTakeOverSettings(
                r => { result = r; },
                request.gameSession,
                gs2AccountSetting.accountNamespaceName,
                30,
                null
            );
            
            if (result.Error != null)
            {
                gs2AccountTakeOverSetting.onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            callback.Invoke(result);
        }

        /// <summary>
        /// 既存のギャザリングに参加する
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="type"></param>
        /// <param name="userIdentifier"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public IEnumerator AddAccountTakeOverSetting(
            UnityAction<AsyncResult<EzAddTakeOverSettingResult>> callback,
            int type,
            string userIdentifier,
            string password
        )
        {
            Validate();

            var request = Gs2Util.LoadGlobalGameObject<AccountTakeOverRequest>("AccountTakeOverRequest");

            AsyncResult<EzAddTakeOverSettingResult> result = null;
            yield return gs2Client.client.Account.AddTakeOverSetting(
                r => { result = r; },
                request.gameSession,
                gs2AccountSetting.accountNamespaceName,
                type,
                userIdentifier,
                password
            );
            
            if (result.Error != null)
            {
                gs2AccountTakeOverSetting.onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            gs2AccountTakeOverSetting.onSetTakeOver.Invoke(
                result.Result.Item
            );
            callback.Invoke(result);
        }

        /// <summary>
        /// 既存のギャザリングに参加する
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="type"></param>
        /// <param name="userIdentifier"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public IEnumerator DoAccountTakeOver(
            UnityAction<AsyncResult<EzDoTakeOverResult>> callback,
            int type,
            string userIdentifier,
            string password
        )
        {
            Validate();

            AsyncResult<EzDoTakeOverResult> result = null;
            yield return gs2Client.client.Account.DoTakeOver(
                r => { result = r; },
                gs2AccountSetting.accountNamespaceName,
                type,
                userIdentifier,
                password
            );
            
            if (result.Error != null)
            {
                gs2AccountTakeOverSetting.onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            gs2AccountTakeOverSetting.onDoTakeOver.Invoke(
                result.Result.Item
            );
            callback.Invoke(result);
        }

        /// <summary>
        /// 既存のギャザリングに参加する
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerator DeleteAccountTakeOverSetting(
            UnityAction<AsyncResult<EzDeleteTakeOverSettingResult>> callback,
            int type
        )
        {
            Validate();

            var request = Gs2Util.LoadGlobalGameObject<AccountTakeOverRequest>("AccountTakeOverRequest");

            AsyncResult<EzDeleteTakeOverSettingResult> result = null;
            yield return gs2Client.client.Account.DeleteTakeOverSetting(
                r => { result = r; },
                request.gameSession,
                gs2AccountSetting.accountNamespaceName,
                type
            );
            
            if (result.Error != null)
            {
                gs2AccountTakeOverSetting.onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            callback.Invoke(result);
        }

    }
}