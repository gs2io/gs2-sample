using System.Collections;
using Gs2.Core;
using Gs2.Unity;
using Gs2.Unity.Gs2Gateway.Result;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Scenes
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

        /// <summary>
        /// 
        /// </summary>
        public bool initialized;

        private void Start()
        {
            DontDestroyOnLoad (this);
            
            // GS2 のクライアントを初期化
            profile = new Profile(
                setting.clientId,
                setting.clientSecret,
                new Gs2BasicReopener()
            );
            client = new Client(
                profile
            );

            initialized = true;
        }
    }
}