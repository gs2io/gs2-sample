using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Core.Model;
using Gs2.Unity.Gs2JobQueue.Model;
using Gs2.Unity.Gs2JobQueue.Result;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Core
{
    public class JobQueueExecutor : MonoBehaviour
    {
        private readonly Queue<NotificationMessage> _notificationMessages = new Queue<NotificationMessage>();
        
        public string jobQueueNamespaceName;
        public bool kill;

        [System.Serializable]
        public class ErrorEvent : UnityEvent<Gs2Exception>
        {
            
        }

        [System.Serializable]
        public class ResultEvent : UnityEvent<EzJob, int, string>
        {
            
        }
        
        public ErrorEvent onError = new ErrorEvent();
        public ResultEvent onResult = new ResultEvent();

        private IEnumerator Wait()
        {
            while (!kill)
            {
                if (_notificationMessages.Count > 0)
                {
                    var notificationMessage = _notificationMessages.Dequeue();
                    if (notificationMessage.issuer == "Gs2JobQueue:Push")
                    {
                        yield break;
                    }
                }

                yield return null;
            }
        }

        public IEnumerator Exec(Profile profile, GameSession session)
        {
            var gs2Client = Gs2Util.LoadGlobalGameObject<Gs2Client>("Gs2Settings");
            
            profile.Gs2Session.OnNotificationMessage += msg => _notificationMessages.Enqueue(msg);

            while (!kill)
            {
                AsyncResult<EzRunResult> result = null;
                yield return gs2Client.client.JobQueue.Run(
                    r => result = r,
                    session,
                    jobQueueNamespaceName
                );

                if (result.Error != null)
                {
                    onError.Invoke(result.Error);
                    yield break;
                }
                else if (result.Result != null && result.Result.Item != null)
                {
                    onResult.Invoke(
                        result.Result.Item,
                        result.Result.Result.StatusCode,
                        result.Result.Result.Result
                    );
                }

                if (result.Result != null && result.Result.IsLastJob)
                {
                    yield return Wait();
                }
            }
        }
    }
}