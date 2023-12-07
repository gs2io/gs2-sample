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
using Cysharp.Threading.Tasks.Linq;
using Gs2.Core.Model;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Gs2Inventory.Model;
using Gs2.Unity.Gs2Lottery.Model;
using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.Simple
{
    public class LotteryFuture : MonoBehaviour
    {
        public string clientId;
        public string clientSecret;

        public string accountNamespaceName;
        public string lotteryNamespaceName;
        public string inventoryNamespaceName;
        public string inventoryName;
        public string exchangeNamespaceName;
        public string exchangeRateName;
        
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
            
            // clear gacha result buffer
            gs2.Lottery.ClearDrawnResult(this.lotteryNamespaceName);

            // draw gacha
            {
                var future = gs2.Exchange.Namespace(
                    this.exchangeNamespaceName
                ).Me(
                    gameSession
                ).Exchange(
                ).ExchangeFuture(
                    this.exchangeRateName,
                    1
                );
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
                var transaction = future.Result;
                
                // wait lottery process
                // This wait processing is not necessary. The purchase process can operate asynchronously without invoking this wait process.
                // Also, even if the purchase process is not yet complete, GS2-SDK will cause the acquisition type API to respond with the anticipated value ahead of time // even if the purchase process is not yet complete if the resource change after the purchase is predictable.
                // The preemptively reflected contents of the local cache from this speculative execution will be maintained for 10 seconds, and if the process is not completed within that time, the latest value will be retrieved from the server again.
                var future2 = transaction.WaitFuture(true);
                yield return future2;
                if (future2.Error != null) {
                    throw future2.Error;
                }
            }

            // fetch gacha results
            var drawnPrizes = new List<EzDrawnPrize>();
            {
                var it = gs2.Lottery.Namespace(
                    this.lotteryNamespaceName
                ).Me(
                    gameSession
                ).Lottery(
                ).DrawnPrizes();
                while (it.HasNext()) {
                    yield return it.Next();
                    if (it.Error != null) {
                        throw it.Error;
                    }
                    drawnPrizes.Add(it.Current);
                }
            }

            foreach (var drawnPrize in drawnPrizes) {
                Debug.Log("DrawnPrize: " + drawnPrize.PrizeId);
            }
            
            // fetch inventory
            var itemSets = new List<EzItemSet>();
            {
                var it = gs2.Inventory.Namespace(
                    this.inventoryNamespaceName
                ).Me(
                    gameSession
                ).Inventory(
                    this.inventoryName
                ).ItemSets();
                while (it.HasNext()) {
                    yield return it.Next();
                    if (it.Error != null) {
                        throw it.Error;
                    }
                    itemSets.Add(it.Current);
                }
            }

            foreach (var itemSet in itemSets) {
                Debug.Log("HeldItem: " + itemSet.ItemName + ", " + itemSet.Count);
            }
        }
        
        public void Start() {
            StartCoroutine(Process());
        }
    }
}