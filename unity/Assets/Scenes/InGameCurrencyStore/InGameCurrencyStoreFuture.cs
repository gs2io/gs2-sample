/*
 * Copyright 2016 Game Server Services, Inc. or its affiliates. All Rights
 * Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core.Model;
using Gs2.Gs2Money.Request;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Gs2Money.Model;
using Gs2.Unity.Gs2Showcase.Model;
using Gs2.Unity.Util;
using Gs2.Util.LitJson;
using UnityEngine;

namespace Gs2.Sample.Simple
{
    public class InGameCurrencyStoreFuture : MonoBehaviour
    {
        public string clientId;
        public string clientSecret;

        public string accountNamespaceName;
        public string moneyNamespaceName;
        public string showcaseNamespaceName;
        public string showcaseName;
        
        private IEnumerator Process() {
            // Initialize GS2 SDK
            Gs2Domain gs2;
            {
                var future = Gs2Client.CreateFuture(
                    new BasicGs2Credential(
                        this.clientId,
                        this.clientSecret
                    ),
                    Region.ApNortheast1
                );
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
                gs2 = future.Result;
            }

            // Create an anonymous account
            EzAccount account;
            {
                var future = gs2.Account.Namespace(
                    this.accountNamespaceName
                ).CreateFuture();
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
                var future2 = future.Result.ModelFuture();
                yield return future2;
                if (future2.Error != null) {
                    throw future2.Error;
                }
                account = future2.Result;
            }

            // login
            GameSession gameSession;
            {
                var future = gs2.LoginFuture(
                    new Gs2AccountAuthenticator(
                        accountSetting: new AccountSetting {
                            accountNamespaceName = this.accountNamespaceName,
                        }
                    ),
                    account.UserId,
                    account.Password
                );
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
                gameSession = future.Result;
            }

            // fetch wallet
            EzWallet wallet;
            {
                var future = gs2.Money.Namespace(
                    this.moneyNamespaceName
                ).Me(
                    gameSession
                ).Wallet(
                    0
                ).ModelFuture();
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
                wallet = future.Result;
                Debug.Log("Wallet: " + (wallet.Free + wallet.Paid));
            }

            // subscribe wallet
            gs2.Money.Namespace(
                this.moneyNamespaceName
            ).Me(
                gameSession
            ).Wallet(
                0
            ).Subscribe(wallet =>
            {
                Debug.Log("Change Wallet: " + (wallet.Free + wallet.Paid));
            });

            // fetch showcase
            EzShowcase showcase;
            {
                var future = gs2.Showcase.Namespace(
                    this.showcaseNamespaceName
                ).Me(
                    gameSession
                ).Showcase(
                    this.showcaseName
                ).ModelFuture();
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
                showcase = future.Result;
            }

            // display sales items
            foreach (var displayItem in showcase.DisplayItems) {
                Debug.Log("Name: " + displayItem.SalesItem.Name);
                Debug.Log("ConsumeActions: " + string.Join(", ", displayItem.SalesItem.ConsumeActions.Select(v => v.Action)));
                Debug.Log("AcquireActions: " + string.Join(", ", displayItem.SalesItem.AcquireActions.Select(v => v.Action)));
            }

            var configs = new List<EzConfig>();
#if !GS2_ENABLE_PURCHASING
            Debug.LogError("Unity IAP must be installed for this sample to work.");
#else
            // purchase store platform
            PurchaseParameters purchaseParameters = null;
            var needReceipt = showcase.DisplayItems[0].SalesItem.ConsumeActions.FirstOrDefault(
                v => v.Action == "Gs2Money:RecordReceipt"
            );
            if (needReceipt != null) {
                var request = RecordReceiptRequest.FromJson(JsonMapper.ToObject(needReceipt.Request));
                var future = new IAPUtil().BuyFuture(request.ContentsId);
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
                purchaseParameters = future.Result;
                configs.Add(new EzConfig {
                    Key = "receipt",
                    Value = purchaseParameters.receipt,
                });
            }
#endif

            // buy 1st product
            {
                var future = gs2.Showcase.Namespace(
                    this.showcaseNamespaceName
                ).Me(
                    gameSession
                ).Showcase(
                    showcaseName
                ).DisplayItem(
                    showcase.DisplayItems[0].DisplayItemId
                ).BuyFuture(
                    1,
                    configs.ToArray()
                );
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
                var transaction = future.Result;

                // wait purchase process
                // This wait processing is not necessary. The purchase process can operate asynchronously without invoking this wait process.
                // Also, even if the purchase process is not yet complete, GS2-SDK will cause the acquisition type API to respond with the anticipated value ahead of time // even if the purchase process is not yet complete if the resource change after the purchase is predictable.
                // The preemptively reflected contents of the local cache from this speculative execution will be maintained for 10 seconds, and if the process is not completed within that time, the latest value will be retrieved from the server again.
                var future2 = transaction.WaitFuture(true);
                yield return future2;
                if (future2.Error != null) {
                    throw future2.Error;
                }
            }

#if GS2_ENABLE_PURCHASING
            if (purchaseParameters != null) {
                // confirm store platform
                purchaseParameters.controller.ConfirmPendingPurchase(purchaseParameters.product);
            }
#endif

            // fetch wallet again
            {
                var future = gs2.Money.Namespace(
                    this.moneyNamespaceName
                ).Me(
                    gameSession
                ).Wallet(
                    0
                ).ModelFuture();
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
                wallet = future.Result;
                Debug.Log("Wallet Free: " + wallet.Free);
                Debug.Log("Wallet Paid: " + wallet.Paid);
            }
        }
        
        public void Start() {
            StartCoroutine(Process());
        }
    }
}