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
using System;
using Cysharp.Threading.Tasks.Linq;
using Gs2.Core.Model;
using Gs2.Unity.Core;
using Gs2.Unity.Util;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEditor;
using Random = System.Random;

namespace Gs2.Sample.Simple
{
    public class RankingAsync : MonoBehaviour
    {
        public string clientId;
        public string clientSecret;

        public string accountNamespaceName;
        public string rankingNamespaceName;
        public string rankingCategoryName;
        
        private async UniTask ProcessAsync() {
            // Initialize GS2 SDK
            var gs2 = await Gs2Client.CreateAsync(
                new BasicGs2Credential(
                    clientId,
                    clientSecret
                ),
                Region.ApNortheast1
            );
            
            // define GS2-Account namespace
            var gs2Account = gs2.Account.Namespace(
                this.accountNamespaceName
            );
            
            // Create an anonymous account
            var account = await (
                await gs2Account.CreateAsync()
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

            // generate score
            var score = Math.Abs(new Random().Next());
            
            // define GS2-Account namespace
            var gs2Ranking = gs2.Ranking.Namespace(
                this.rankingNamespaceName
            );

            // put score
            await gs2Ranking.Me(
                gameSession
            ).Ranking(
                this.rankingCategoryName
            ).PutScoreAsync(
                score,
                "metadata"
            );

            // fetch ranking result
            var ranking = await gs2Ranking.Me(
                gameSession
            ).Ranking(
                this.rankingCategoryName
            ).ModelAsync(
                gameSession.AccessToken.UserId
            );
            
            // Rankings are compiled at intervals determined by the master data, and rankings immediately after scores are updated are estimated approximately based on the most recent compiled results.
            // If you need to calculate an exact ranking, please use the results that can be obtained after the score registration has been terminated and the timing of the tally has been reached.
            
            // The ranking model has two fields with very similar properties: index and rank.
            // The index stores the order from the beginning with no duplicates, and the rank is numbered in such a way that if others have the same score, the same value is used.
            // 
            // A simple example is shown below.
            // Index: 1, 2, 3
            // Rank: 1, 1, 3
            Debug.Log("Index: " + ranking.Index);
            Debug.Log("Rank: " + ranking.Rank);
            Debug.Log("Score: " + ranking.Score);
            
            // fetch list ranking
            var rankings = await gs2Ranking.Me(
                gameSession
            ).RankingsAsync(
                this.rankingCategoryName
            ).Take(30).ToListAsync();

            foreach (var rank in rankings) {
                Debug.Log("Index: " + rank.Index + ", Rank: " + rank.Rank + ", Score: " + rank.Score + ", UserId: " + rank.UserId);
            }
        }
        
        public void Start() {
            StartCoroutine(ProcessAsync().ToCoroutine());
        }
    }
}
#endif