using System;
using System.Collections;
using Gs2.Core;
using Gs2.Unity;
using Gs2.Unity.Gs2Gateway.Result;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Core
{
    public class Gs2Client : MonoBehaviour
    {
        /// <summary>
        /// GS2 の設定プロファイル
        /// </summary>
        public Gs2.Unity.Util.Profile profile { get; private set; }

        /// <summary>
        /// GS2 のクライアントID
        /// </summary>
        public Gs2.Unity.Client client { get; private set; }

        /// <summary>
        /// GS2 の設定値
        /// </summary>
        public Gs2Setting setting;

        public void Start()
        {
            DontDestroyOnLoad (this);
        }
        
        private void Validate()
        {
            if (string.IsNullOrEmpty(setting.clientId))
            {
                throw new InvalidProgramException(
                    "'Gs2Setting' has no credential client ID set," +
                    "The client ID can be created by uploading the 'initialize_credential_template.yaml' bundled with the core sample as a GS2-Deploy stack." +
                    "Please check README.md for details." +
                    " / " + 
                    "'Gs2Setting' にクレデンシャルのクライアントIDが設定されていません、" +
                    "クライアントID は core サンプルに同梱されている 'initialize_credential_template.yaml' を GS2-Deploy のスタックとしてアップロードすることで作成できます。" +
                    "詳しくは README.md をご確認ください。"
                );
            }
            if (string.IsNullOrEmpty(setting.clientSecret))
            {
                throw new InvalidProgramException(
                    "'Gs2Setting' has no credential client secret set," +
                    "The client secret can be created by uploading the 'initialize_credential_template.yaml' bundled with the core sample as a GS2-Deploy stack." +
                    "Please check README.md for details." +
                    " / " + 
                    "'Gs2Setting' にクレデンシャルのクライアントシークレットが設定されていません、" +
                    "クライアントシークレット は core サンプルに同梱されている 'initialize_credential_template.yaml' を GS2-Deploy のスタックとしてアップロードすることで作成できます。" +
                    "詳しくは README.md をご確認ください。"
                );
            }

            // GS2 のクライアントを初期化
            profile = new Profile(
                setting.clientId,
                setting.clientSecret,
                new Gs2BasicReopener()
            );
            client = new Client(
                profile
            );
        }
        
        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator Initialize(
            UnityAction<AsyncResult<object>> callback
        )
        {
            Debug.Log("Validate");
            Validate();
            Debug.Log("Done");
            
            AsyncResult<object> result = null;
            Debug.Log("Profile.Initialize");
            yield return profile.Initialize(
                callback
            );
            Debug.Log("Done Profile.Initialize");
        }
    }
}