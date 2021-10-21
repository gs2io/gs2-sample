using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Gs2Limit.Request;
using Gs2.Gs2Money.Request;
using Gs2.Gs2Showcase.Model;
using Gs2.Sample.Core;
using Gs2.Unity.Gs2Limit.Result;
using Gs2.Unity.Gs2Money.Result;
using Gs2.Unity.Gs2Showcase.Model;
using Gs2.Unity.Gs2Showcase.Result;
using Gs2.Unity.Util;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Money
{
    public class MoneyController
    {
#if UNITY_IPHONE
        public const int Slot = 1;
#elif UNITY_ANDROID
        public const int Slot = 2;
#else
        public const int Slot = 0;
#endif

        /// <summary>
        /// Gs2Client
        /// </summary>
        private Gs2Client _gs2Client;

        /// <summary>
        /// GS2 の設定値
        /// </summary>
        public Gs2MoneySetting gs2MoneySetting;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            if (!_gs2Client)
            {
                _gs2Client = Gs2Util.LoadGlobalGameObject<Gs2Client>("Gs2Settings");
            }

            if (!gs2MoneySetting)
            {
                gs2MoneySetting = Gs2Util.LoadGlobalGameObject<Gs2MoneySetting>("Gs2Settings");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="salesItem"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T GetAcquireAction<T>(
            EzSalesItem salesItem,
            string action
        )
        {
            var item = salesItem.AcquireActions.FirstOrDefault(acquireAction => acquireAction.Action == action);
            if (item == null)
            {
                return default;
            }
            return (T)typeof(T).GetMethod("FromJson")?.Invoke(null, new object[] { Gs2Util.RemovePlaceholder(JsonMapper.ToObject(item.Request)) });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="salesItem"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private T GetConsumeAction<T>(
            EzSalesItem salesItem,
            string action
        )
        {
            var item = salesItem.ConsumeActions.FirstOrDefault(consumeAction => consumeAction.Action == action);
			if (item == null)
            {
                return default;
            }
            return (T)typeof(T).GetMethod("FromJson")?.Invoke(null, new object[] { Gs2Util.RemovePlaceholder(JsonMapper.ToObject(item.Request)) });
        }

        /// <summary>
        /// 課金通貨のウォレットを取得
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator GetWallet(
            UnityAction<AsyncResult<EzGetResult>> callback
        )
        {
            Initialize();
            
            var request = Gs2Util.LoadGlobalGameObject<MoneyRequest>("MoneyRequest");

            AsyncResult<EzGetResult> result = null;
            yield return _gs2Client.client.Money.Get(
                r => { result = r; },
                request.gameSession,
                gs2MoneySetting.moneyNamespaceName,
                Slot
            );
            
            if (result.Error != null)
            {
                gs2MoneySetting.onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            gs2MoneySetting.onGetWallet.Invoke(result.Result.Item);
            
            callback.Invoke(result);
        }

        /// <summary>
        /// 販売されている商品の一覧を取得
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator ListProducts(
            UnityAction<AsyncResult<List<Product>>> callback
        )
        {
            Initialize();

            var request = Gs2Util.LoadGlobalGameObject<MoneyRequest>("MoneyRequest");

            AsyncResult<EzGetShowcaseResult> result = null;
            yield return _gs2Client.client.Showcase.GetShowcase(
                r => { result = r; },
                request.gameSession,
                gs2MoneySetting.showcaseNamespaceName,
                gs2MoneySetting.showcaseName
            );
            
            if (result.Error != null)
            {
                gs2MoneySetting.onError.Invoke(
                    result.Error
                );
                callback.Invoke(new AsyncResult<List<Product>>(null, result.Error));
                yield break;
            }

            var products = new List<Product>();
            foreach (var displayItem in result.Result.Item.DisplayItems)
            {
                var depositRequest = GetAcquireAction<DepositByUserIdRequest>(
                    displayItem.SalesItem, 
                    "Gs2Money:DepositByUserId"
                );
                var recordReceiptRequest = GetConsumeAction<RecordReceiptRequest>(
                    displayItem.SalesItem, 
                    "Gs2Money:RecordReceipt"
                );
                var countUpRequest = GetConsumeAction<CountUpByUserIdRequest>(
                    displayItem.SalesItem, 
                    "Gs2Limit:CountUpByUserId"
                );
                var price = depositRequest.Price;
                var count = depositRequest.Count;

                int? boughtCount = null;
                if(countUpRequest != null) {
                    AsyncResult<EzGetCounterResult> result2 = null;
                    yield return _gs2Client.client.Limit.GetCounter(
                        r => { result2 = r; },
                        request.gameSession,
                        countUpRequest.NamespaceName,
                        countUpRequest.LimitName,
                        countUpRequest.CounterName
                    );
                    if (result2.Error == null)
                    {
                        boughtCount = result2.Result.Item.Count;
                    }
                    else if (result2.Error is NotFoundException)
                    {
                        boughtCount = 0;
                    }
                }
                products.Add(new Product
                {
                    Id = displayItem.DisplayItemId,
                    ContentsId = recordReceiptRequest.ContentsId,
                    Price = price,
                    CurrencyCount = count,
                    BoughtCount = boughtCount,
                    BoughtLimit = countUpRequest == null ? null : countUpRequest.MaxValue,
                });
            }
            
            gs2MoneySetting.onGetProducts.Invoke(products);
            
            callback.Invoke(new AsyncResult<List<Product>>(products, result.Error));
        }

        /// <summary>
        /// 通貨を購入する
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="product"></param>
        /// <returns></returns>
        public IEnumerator Buy(
            UnityAction<AsyncResult<object>> callback,
            Product product
        )
        {
            Initialize();

            var request = Gs2Util.LoadGlobalGameObject<MoneyRequest>("MoneyRequest");

            string receipt = null;
            {
#if UNITY_PURCHASING
                AsyncResult<PurchaseParameters> result = null;
                yield return new IAPUtil().Buy(
                    r => { result = r; },
                    product.ContentsId
                );
                if (result.Error != null)
                {
                    gs2MoneySetting.onError.Invoke(
                        result.Error
                    );
                    callback.Invoke(new AsyncResult<object>(null, result.Error));
                    yield break;
                }

                receipt = result.Result.receipt;
#endif
            }
            string stampSheet = null;
            {
                AsyncResult<EzBuyResult> result = null;
                yield return _gs2Client.client.Showcase.Buy(
                    r => { result = r; },
                    request.gameSession,
                    gs2MoneySetting.showcaseNamespaceName,
                    gs2MoneySetting.showcaseName,
                    product.Id,
                    new List<EzConfig>
                    {
                        new EzConfig
                        {
                            Key = "slot",
                            Value = Slot.ToString(),
                        },
                        new EzConfig
                        {
                            Key = "receipt",
                            Value = receipt,
                        },
                    }
                );

                if (result.Error != null)
                {
                    gs2MoneySetting.onError.Invoke(
                        result.Error
                    );
                    callback.Invoke(new AsyncResult<object>(null, result.Error));
                    yield break;
                }

                stampSheet = result.Result.StampSheet;
            }
            {
                var machine = new StampSheetStateMachine(
                    stampSheet,
                    _gs2Client.client,
                    gs2MoneySetting.distributorNamespaceName,
                    gs2MoneySetting.showcaseKeyId
                );

                Gs2Exception exception = null;
                void OnError(Gs2Exception e)
                {
                    exception = e;
                }
                
                gs2MoneySetting.onError.AddListener(OnError);
                yield return machine.Execute(gs2MoneySetting.onError);
                gs2MoneySetting.onError.RemoveListener(OnError);
                
                if (exception != null)
                {
                    callback.Invoke(new AsyncResult<object>(null, exception));
                    yield break;
                }
            }
            
            gs2MoneySetting.onBuy.Invoke(product);
            
            callback.Invoke(new AsyncResult<object>(null, null));
        }
    }
}