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
using System.Text;
using Gs2.Core.Model;
using Gs2.Unity.Core;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.Simple
{
    public class CloudSaveFuture : MonoBehaviour
    {
        public string clientId;
        public string clientSecret;

        public string accountNamespaceName;
        public string datastoreNamespaceName;
        
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
                var future = gs2Account.CreateFuture();
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

            // define GS2-Datastore namespace
            var gs2Datastore = gs2.Datastore.Namespace(
                this.datastoreNamespaceName
            );

            // define save data
            var saveData = new byte[] {
                (byte) 'A', (byte) 'B', (byte) 'C'
            };
            
            // upload save data
            {
                var future = gs2Datastore.Me(
                    gameSession
                ).UploadFuture(
                    "public",
                    new List<string>(),
                    saveData,
                    "PublicData"
                );
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
            }

            // download save data
            byte[] downloadedSaveData;
            {
                var future = gs2Datastore.Me(
                    gameSession
                ).DataObject(
                    "PublicData"
                ).DownloadFuture();
                yield return future;
                if (future.Error != null) {
                    throw future.Error;
                }
                downloadedSaveData = future.Result;
            }
            
            Debug.Log(Encoding.ASCII.GetString(downloadedSaveData));
        }
        
        public void Start() {
            StartCoroutine(Process());
        }
    }
}