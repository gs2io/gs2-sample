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

using System;
using System.Collections;
using Cysharp.Threading.Tasks.Linq;
using Gs2.Core.Model;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Gs2Ranking.Model;
using Gs2.Unity.Util;
using UnityEngine;
using Random = System.Random;

namespace Gs2.Sample.Simple
{
    public class RankingFuture : MonoBehaviour
    {
        public string clientId;
        public string clientSecret;

        public string accountNamespaceName;
        public string rankingNamespaceName;
        public string rankingCategoryName;
        
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
            
            // define GS2-Account namespace
            var gs2Account = gs2.Account.Namespace(
                this.accountNamespaceName
            );
            
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

            // generate score
            var score = Math.Abs(new Random().Next());
            
            // define GS2-Account namespace
            var gs2Ranking = gs2.Ranking.Namespace(
                this.rankingNamespaceName
            );

            // put score
            {
                var future = gs2Ranking.Me(
                    gameSession
                ).Ranking(
                    this.rankingCategoryName
                ).PutScoreFuture(
                    score,
                    "metadata"
                );
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
            }

            // fetch ranking result
            EzRanking ranking;
            {
                var future = gs2Ranking.Me(
                    gameSession
                ).Ranking(
                    this.rankingCategoryName
                ).ModelFuture(
                    gameSession.AccessToken.UserId
                );
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
                ranking = future.Result;
            }
            
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
            var it = gs2Ranking.Me(
                gameSession
            ).Rankings(
                this.rankingCategoryName
            );

            for (var i=0; i<30 && it.HasNext(); i++) {
                yield return it.Next();
                if (it.Error != null) {
                    throw it.Error;
                }
                var rank = it.Current;
                Debug.Log("Index: " + rank.Index + ", Rank: " + rank.Rank + ", Score: " + rank.Score + ", UserId: " + rank.UserId);
            }
        }
        
        public void Start() {
            StartCoroutine(Process());
        }
    }
}