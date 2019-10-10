using System.Collections.Generic;
using Gs2.Core.Exception;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Gs2Matchmaking.Model;
using Gs2.Unity.Gs2Money.Model;
using Gs2.Unity.Gs2Showcase.Model;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Money
{
    [System.Serializable]
    public class GetWalletEvent : UnityEvent<EzWalletDetail>
    {
    }

    [System.Serializable]
    public class GetProductsEvent : UnityEvent<List<Product>>
    {
    }

    [System.Serializable]
    public class BuyEvent : UnityEvent<Product>
    {
    }

    [System.Serializable]
    public class ErrorEvent : UnityEvent<Gs2Exception>
    {
    }

    public class Gs2MoneySetting : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string moneyNamespaceName;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string showcaseNamespaceName;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string showcaseName;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string showcaseKeyId;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string limitNamespaceName;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string distributorNamespaceName;

        /// <summary>
        /// ウォレットを取得したとき
        /// </summary>
        [SerializeField]
        public GetWalletEvent onGetWallet = new GetWalletEvent();
        
        /// <summary>
        /// 販売中の課金通貨一覧を取得したとき
        /// </summary>
        [SerializeField]
        public GetProductsEvent onGetProducts = new GetProductsEvent();
        
        /// <summary>
        /// 課金通貨を購入したとき
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