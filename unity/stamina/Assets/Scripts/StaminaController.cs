using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Sample.Core;
using Gs2.Sample.Money;
using Gs2.Unity.Gs2Exchange.Result;
using Gs2.Unity.Gs2Stamina.Result;
using Gs2.Unity.Util;
using Gs2.Util.LitJson;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Stamina
{
    public class StaminaController
    {
        /// <summary>
        /// GS2-Matchmaking の設定値
        /// </summary>
        public Gs2StaminaSetting gs2StaminaSetting;

        /// <summary>
        /// Gs2Client
        /// </summary>
        public Gs2Client gs2Client;

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            if (!gs2StaminaSetting)
            {
                gs2StaminaSetting = Gs2Util.LoadGlobalGameObject<Gs2StaminaSetting>("Gs2Settings");
            }

            if (!gs2Client)
            {
                gs2Client = Gs2Util.LoadGlobalGameObject<Gs2Client>("Gs2Settings");
            }
        }

        /// <summary>
        /// スタミナ値を取得
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator GetStamina(
            UnityAction<AsyncResult<EzGetStaminaResult>> callback
        )
        {
            var request = Gs2Util.LoadGlobalGameObject<StaminaRequest>("StaminaRequest");

            AsyncResult<EzGetStaminaResult> result = null;
            yield return gs2Client.client.Stamina.GetStamina(
                r => { result = r; },
                request.gameSession,
                gs2StaminaSetting.staminaNamespaceName,
                gs2StaminaSetting.staminaName
            );
            
            if (result.Error != null)
            {
                gs2StaminaSetting.onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            gs2StaminaSetting.onGetStamina.Invoke(result.Result.Item);
            
            callback.Invoke(result);
        }

        /// <summary>
        /// スタミナ値を消費(非推奨 サービス間の連携（スタンプシート）を経由してスタミナ値を操作するほうが望ましい)
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="consumeValue"></param>
        /// <returns></returns>
        public IEnumerator ConsumeStamina(
            UnityAction<AsyncResult<EzConsumeResult>> callback,
            int consumeValue
        )
        {
            var request = Gs2Util.LoadGlobalGameObject<StaminaRequest>("StaminaRequest");

            AsyncResult<EzConsumeResult> result = null;
            yield return gs2Client.client.Stamina.Consume(
                r => { result = r; },
                request.gameSession,
                gs2StaminaSetting.staminaNamespaceName,
                gs2StaminaSetting.staminaName,
                consumeValue
            );
            
            if (result.Error != null)
            {
                gs2StaminaSetting.onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            gs2StaminaSetting.onGetStamina.Invoke(result.Result.Item);
            
            callback.Invoke(result);
        }

        /// <summary>
        /// スタミナを購入する
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator Buy(
            UnityAction<AsyncResult<object>> callback
        )
        {
            var request = Gs2Util.LoadGlobalGameObject<StaminaRequest>("StaminaRequest");

            string stampSheet = null;
            {
                AsyncResult<EzExchangeResult> result = null;
                yield return gs2Client.client.Exchange.Exchange(
                    r => { result = r; },
                    request.gameSession,
                    gs2StaminaSetting.exchangeNamespaceName,
                    gs2StaminaSetting.exchangeRateName,
                    1,
                    new List<Gs2.Unity.Gs2Exchange.Model.EzConfig>
                    {
                        new Gs2.Unity.Gs2Exchange.Model.EzConfig
                        {
                            Key = "slot",
                            Value = MoneyController.Slot.ToString(),
                        }
                    }
                );

                if (result.Error != null)
                {
                    gs2StaminaSetting.onError.Invoke(
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
                    gs2Client.client,
                    gs2StaminaSetting.distributorNamespaceName,
                    gs2StaminaSetting.exchangeKeyId
                );

                Gs2Exception exception = null;
                void OnError(Gs2Exception e)
                {
                    exception = e;
                }
                
                gs2StaminaSetting.onError.AddListener(OnError);
                yield return machine.Execute(gs2StaminaSetting.onError);
                gs2StaminaSetting.onError.RemoveListener(OnError);
                
                if (exception != null)
                {
                    callback.Invoke(new AsyncResult<object>(null, exception));
                    yield break;
                }
            }
            
            gs2StaminaSetting.onBuy.Invoke();
            
            callback.Invoke(new AsyncResult<object>(null, null));
        }
    }
}