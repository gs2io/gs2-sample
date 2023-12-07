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

#if GS2_ENABLE_UNITASK
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Gs2.Core.Model;
using Gs2.Unity.Core;
using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.Simple
{
    public class LotteryAsync : MonoBehaviour
    {
        public string clientId;
        public string clientSecret;

        public string accountNamespaceName;
        public string lotteryNamespaceName;
        public string inventoryNamespaceName;
        public string inventoryName;
        public string exchangeNamespaceName;
        public string exchangeRateName;
        
        private async UniTask ProcessAsync() {
            // Initialize GS2 SDK
            var gs2 = await Gs2Client.CreateAsync(
                new BasicGs2Credential(
                    clientId,
                    clientSecret
                ),
                Region.ApNortheast1
            );
            
            // Create an anonymous account
            var account = await (
                await gs2.Account.Namespace(
                    this.accountNamespaceName
                ).CreateAsync()
            ).ModelAsync();
            
            // login
            var gameSession = await gs2.LoginAsync(
                new Gs2AccountAuthenticator(
                    accountSetting: new AccountSetting {
                        accountNamespaceName = this.accountNamespaceName,
                    }
                ),
                account.UserId,
                account.Password
            );
            
            // clear gacha result buffer
            gs2.Lottery.ClearDrawnResult(this.lotteryNamespaceName);

            // draw gacha
            var transaction = await gs2.Exchange.Namespace(
                this.exchangeNamespaceName
            ).Me(
                gameSession
            ).Exchange(
            ).ExchangeAsync(
                this.exchangeRateName,
                1
            );
            
            // wait lottery process
            // This wait processing is not necessary. The purchase process can operate asynchronously without invoking this wait process.
            // Also, even if the purchase process is not yet complete, GS2-SDK will cause the acquisition type API to respond with the anticipated value ahead of time // even if the purchase process is not yet complete if the resource change after the purchase is predictable.
            // The preemptively reflected contents of the local cache from this speculative execution will be maintained for 10 seconds, and if the process is not completed within that time, the latest value will be retrieved from the server again.
            await transaction.WaitAsync(true);

            // fetch gacha results
            var drawnPrizes = await gs2.Lottery.Namespace(
                this.lotteryNamespaceName
            ).Me(
                gameSession
            ).Lottery(
            ).DrawnPrizesAsync().ToListAsync();

            foreach (var drawnPrize in drawnPrizes) {
                Debug.Log("DrawnPrize: " + drawnPrize.PrizeId);
            }
            
            // fetch inventory
            var itemSets = await gs2.Inventory.Namespace(
                this.inventoryNamespaceName
            ).Me(
                gameSession
            ).Inventory(
                this.inventoryName
            ).ItemSetsAsync().ToListAsync();

            foreach (var itemSet in itemSets) {
                Debug.Log("HeldItem: " + itemSet.ItemName + ", " + itemSet.Count);
            }
        }
        
        public void Start() {
            StartCoroutine(ProcessAsync().ToCoroutine());
        }
    }
}
#endif