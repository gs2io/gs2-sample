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
using System.Linq;
using Gs2.Core.Model;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Gs2LoginReward.Model;
using Gs2.Unity.Gs2Money.Model;
using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.Simple
{
    public class LoginRewardFuture : MonoBehaviour
    {
        public string clientId;
        public string clientSecret;

        public string accountNamespaceName;
        public string moneyNamespaceName;
        public string loginRewardNamespaceName;
        public string loginRewardBonusName;
        
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
            
            // fetch rewards
            EzBonusModel bonus;
            {
                var future = gs2.LoginReward.Namespace(
                    this.loginRewardNamespaceName
                ).BonusModel(
                    this.loginRewardBonusName
                ).ModelFuture();
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
                bonus = future.Result;
            }

            // display rewards
            foreach (var reward in bonus.Rewards) {
                Debug.Log("Day: " + (bonus.Rewards.IndexOf(reward) + 1));
                Debug.Log("AcquireActions: " + string.Join(", ", reward.AcquireActions.Select(v => v.Action)));
            }
            
            // receive rewards
            {
                var future = gs2.LoginReward.Namespace(
                    this.loginRewardNamespaceName
                ).Me(
                    gameSession
                ).Bonus(
                ).ReceiveFuture(
                    this.loginRewardBonusName
                );
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
                var transaction = future.Result;

                // wait receive process
                var future2 = transaction.WaitFuture(true);
                yield return future2;
                if (future2.Error != null) {
                    throw future2.Error;
                }
            }

            // fetch wallet
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
                Debug.Log("Wallet: " + (wallet.Free + wallet.Paid));
            }
        }
        
        public void Start() {
            StartCoroutine(Process());
        }
    }
}