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
using System.Linq;
using Cysharp.Threading.Tasks;
using Gs2.Core.Model;
using Gs2.Unity.Core;
using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.Simple
{
    public class LoginRewardAsync : MonoBehaviour
    {
        public string clientId;
        public string clientSecret;

        public string accountNamespaceName;
        public string moneyNamespaceName;
        public string loginRewardNamespaceName;
        public string loginRewardBonusName;
        
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
            
            // fetch wallet
            var wallet = await gs2.Money.Namespace(
                this.moneyNamespaceName
            ).Me(
                gameSession
            ).Wallet(
                0
            ).ModelAsync();
            Debug.Log("Wallet: " + (wallet.Free + wallet.Paid));
            
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
            var bonus = await gs2.LoginReward.Namespace(
                this.loginRewardNamespaceName
            ).BonusModel(
                this.loginRewardBonusName
            ).ModelAsync(
            );

            // display rewards
            foreach (var reward in bonus.Rewards) {
                Debug.Log("Day: " + (bonus.Rewards.IndexOf(reward) + 1));
                Debug.Log("AcquireActions: " + string.Join(", ", reward.AcquireActions.Select(v => v.Action)));
            }
            
            // receive rewards
            var transaction = await gs2.LoginReward.Namespace(
                this.loginRewardNamespaceName
            ).Me(
                gameSession
            ).Bonus(
            ).ReceiveAsync(
                this.loginRewardBonusName
            );

            // wait receive process
            await transaction.WaitAsync(true);

            // fetch wallet
            wallet = await gs2.Money.Namespace(
                this.moneyNamespaceName
            ).Me(
                gameSession
            ).Wallet(
                0
            ).ModelAsync();
            Debug.Log("Wallet: " + (wallet.Free + wallet.Paid));
        }
        
        public void Start() {
            StartCoroutine(ProcessAsync().ToCoroutine());
        }
    }
}
#endif