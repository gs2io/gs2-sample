using Gs2.Core.Exception;
using Gs2.Unity.Gs2Stamina.Model;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Stamina
{
    [System.Serializable]
    public class GetStaminaEvent : UnityEvent<EzStamina>
    {
    }

    [System.Serializable]
    public class BuyEvent : UnityEvent
    {
    }

    [System.Serializable]
    public class ErrorEvent : UnityEvent<Gs2Exception>
    {
    }

    public class Gs2StaminaSetting : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string staminaNamespaceName;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string staminaName;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string exchangeNamespaceName;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string exchangeRateName;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string exchangeKeyId;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string distributorNamespaceName;

        /// <summary>
        /// スタミナを取得したとき
        /// </summary>
        [SerializeField]
        public GetStaminaEvent onGetStamina = new GetStaminaEvent();
        
        /// <summary>
        /// スタミナを購入したとき
        /// </summary>
        [SerializeField]
        public BuyEvent onBuy = new BuyEvent();

        /// <summary>
        /// エラー発生時に発行されるイベント
        /// </summary>
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();

    }
}