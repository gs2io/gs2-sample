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
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks.Linq;
using Gs2.Core.Model;
using Gs2.Unity.Core;
using Gs2.Unity.Util;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEditor;

namespace Gs2.Sample.Simple
{
    public class CloudSaveAsync : MonoBehaviour
    {
        public string clientId;
        public string clientSecret;

        public string accountNamespaceName;
        public string datastoreNamespaceName;
        
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

            // define GS2-Datastore namespace
            var gs2Datastore = gs2.Datastore.Namespace(
                this.datastoreNamespaceName
            );

            // define save data
            var saveData = new byte[] {
                (byte) 'A', (byte) 'B', (byte) 'C'
            };
            
            // upload save data
            await gs2Datastore.Me(
                gameSession
            ).UploadAsync(
                "public",
                new List<string>(),
                saveData,
                "PublicData"
            );

            // download save data
            var downloadedSaveData = await gs2Datastore.Me(
                gameSession
            ).DataObject(
                "PublicData"
            ).DownloadAsync();

            Debug.Log(Encoding.ASCII.GetString(downloadedSaveData));
        }
        
        public void Start() {
            StartCoroutine(ProcessAsync().ToCoroutine());
        }
    }
}
#endif