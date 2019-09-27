using UnityEngine;

namespace Gs2.Sample.AccountRegistrationLoginSample
{
    public class Gs2AccountSetting : MonoBehaviour
    {
        /// <summary>
        /// GS2-Account のネームスペース名
        /// </summary>
        [SerializeField]
        public string accountNamespaceName;

        /// <summary>
        /// GS2-Account でアカウント情報の暗号化に使用する GS2-Key の暗号鍵GRN
        /// </summary>
        [SerializeField]
        public string accountEncryptionKeyId;
        
        /// <summary>
        /// GS2-Gateway のネームスペース名
        /// </summary>
        [SerializeField]
        public string gatewayNamespaceName;
    }
}