using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gs2.Core;
using Gs2.Core.Exception;
using Gs2.Gs2Quest.Result;
using Gs2.Sample.Core;
using Gs2.Sample.Money;
using Gs2.Unity.Gs2Quest.Model;
using Gs2.Unity.Gs2Quest.Result;
using Gs2.Unity.Util;
using LitJson;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Quest
{
    public class QuestController
    {
        /// <summary>
        /// GS2-Matchmaking の設定値
        /// </summary>
        public Gs2QuestSetting gs2QuestSetting;

        /// <summary>
        /// Gs2Client
        /// </summary>
        public Gs2Client gs2Client;

        /// <summary>
        /// 初期化処理
        /// </summary>
        /// <returns></returns>
        public void Initialize()
        {
            if (!gs2QuestSetting)
            {
                gs2QuestSetting = Gs2Util.LoadGlobalGameObject<Gs2QuestSetting>("Gs2Settings");
            }

            if (!gs2Client)
            {
                gs2Client = Gs2Util.LoadGlobalGameObject<Gs2Client>("Gs2Settings");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quest"></param>
        /// <param name="action"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetConsumeAction<T>(
            EzQuestModel quest,
            string action
        )
        {
            var item = quest.ConsumeActions.FirstOrDefault(consumeAction => consumeAction.Action == action);
            if (item == null)
            {
                return default;
            }
            return (T)typeof(T).GetMethod("FromDict")?.Invoke(null, new object[] { Gs2Util.RemovePlaceholder(JsonMapper.ToObject(item.Request)) });
        }

        /// <summary>
        /// <summary>
        /// 進行中のクエストを取得
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator GetCompleteQuests(
            UnityAction<AsyncResult<EzDescribeCompletedQuestListsResult>> callback
        )
        {
            var request = Gs2Util.LoadGlobalGameObject<QuestRequest>("QuestRequest");

            AsyncResult<EzDescribeCompletedQuestListsResult> result = null;
            yield return gs2Client.client.Quest.DescribeCompletedQuestLists(
                r => { result = r; },
                request.gameSession,
                gs2QuestSetting.questNamespaceName
            );
            
            if (result.Error != null)
            {
                gs2QuestSetting.onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            gs2QuestSetting.onListCompletedQuestsModel.Invoke(result.Result.Items);
            
            callback.Invoke(result);
        }

        /// <summary>
        /// クエストグループの一覧を取得
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator GetQuestGroups(
            UnityAction<AsyncResult<EzListQuestGroupsResult>> callback
        )
        {
            AsyncResult<EzListQuestGroupsResult> result = null;
            yield return gs2Client.client.Quest.ListQuestGroups(
                r => { result = r; },
                gs2QuestSetting.questNamespaceName
            );
            
            if (result.Error != null)
            {
                gs2QuestSetting.onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            gs2QuestSetting.onListGroupQuestModel.Invoke(result.Result.Items);
            
            callback.Invoke(result);
        }

        /// <summary>
        /// クエストの一覧を取得
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="questGroup"></param>
        /// <returns></returns>
        public IEnumerator GetQuests(
            UnityAction<AsyncResult<EzListQuestsResult>> callback,
            EzQuestGroupModel questGroup
        )
        {
            AsyncResult<EzListQuestsResult> result = null;
            yield return gs2Client.client.Quest.ListQuests(
                r => { result = r; },
                gs2QuestSetting.questNamespaceName,
                questGroup.Name
            );
            
            if (result.Error != null)
            {
                gs2QuestSetting.onError.Invoke(
                    result.Error
                );
                callback.Invoke(result);
                yield break;
            }
            
            gs2QuestSetting.onListQuestModel.Invoke(result.Result.Items);
            
            callback.Invoke(result);
        }

        /// <summary>
        /// 進行中のクエストを取得
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEnumerator GetProgress(
            UnityAction<AsyncResult<EzGetProgressResult>> callback
        )
        {
            var request = Gs2Util.LoadGlobalGameObject<QuestRequest>("QuestRequest");

            AsyncResult<EzGetProgressResult> result = null;
            yield return gs2Client.client.Quest.GetProgress(
                r => { result = r; },
                request.gameSession,
                gs2QuestSetting.questNamespaceName
            );
            
            if (result.Error != null)
            {
                if (!(result.Error is NotFoundException))
                {
                    gs2QuestSetting.onError.Invoke(
                        result.Error
                    );
                }

                callback.Invoke(result);
                yield break;
            }
            
            gs2QuestSetting.onGetProgress.Invoke(result.Result.Item);
            
            callback.Invoke(result);
        }

        /// <summary>
        /// クエストを開始する
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="questGroup"></param>
        /// <param name="quest"></param>
        /// <returns></returns>
        public IEnumerator QuestStart(
            UnityAction<AsyncResult<EzProgress>> callback,
            EzQuestGroupModel questGroup,
            EzQuestModel quest
        )
        {
            var request = Gs2Util.LoadGlobalGameObject<QuestRequest>("QuestRequest");

            string stampSheet;
            {
                AsyncResult<EzStartResult> result = null;
                yield return gs2Client.client.Quest.Start(
                    r => { result = r; },
                    request.gameSession,
                    gs2QuestSetting.questNamespaceName,
                    questGroup.Name,
                    quest.Name,
                    config: new List<EzConfig>
                    {
                        new EzConfig
                        {
                            Key = "slot",
                            Value = MoneyController.Slot.ToString(),
                        }
                    }
                );

                if (result.Error != null)
                {
                    gs2QuestSetting.onError.Invoke(
                        result.Error
                    );
                    callback.Invoke(new AsyncResult<EzProgress>(null, result.Error));
                    yield break;
                }

                stampSheet = result.Result.StampSheet;
            }
            EzProgress progress = null;
            {
                var machine = new StampSheetStateMachine(
                    stampSheet,
                    gs2Client.client,
                    gs2QuestSetting.distributorNamespaceName,
                    gs2QuestSetting.questKeyId
                );

                Gs2Exception exception = null;
                machine.OnError += e =>
                {
                    exception = e;
                };
                machine.OnCompleteStampSheet += (sheet, stampResult) =>
                {
                    var json = JsonMapper.ToObject(stampResult.Result);
                    var result = CreateProgressByStampSheetResult.FromDict(json);
                    progress = new EzProgress(result.item);
                };
                yield return machine.Execute();

                if (exception != null)
                {
                    gs2QuestSetting.onError.Invoke(
                        exception
                    );
                    callback.Invoke(new AsyncResult<EzProgress>(null, exception));
                    yield break;
                }
            }
            
            gs2QuestSetting.onStart.Invoke(progress);
            
            callback.Invoke(new AsyncResult<EzProgress>(progress, null));
        }

        /// <summary>
        /// クエストを完了する
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="progress"></param>
        /// <param name="rewards"></param>
        /// <param name="isComplete"></param>
        /// <returns></returns>
        public IEnumerator QuestEnd(
            UnityAction<AsyncResult<object>> callback,
            EzProgress progress,
            List<EzReward> rewards,
            bool isComplete
        )
        {
            var request = Gs2Util.LoadGlobalGameObject<QuestRequest>("QuestRequest");

            string stampSheet;
            {
                AsyncResult<EzEndResult> result = null;
                yield return gs2Client.client.Quest.End(
                    r => { result = r; },
                    request.gameSession,
                    gs2QuestSetting.questNamespaceName,
                    progress.TransactionId,
                    rewards,
                    isComplete,
                    new List<EzConfig>
                    {
                        new EzConfig
                        {
                            Key = "slot",
                            Value = MoneyController.Slot.ToString(),
                        }
                    }
                );

                if (result.Error != null)
                {
                    gs2QuestSetting.onError.Invoke(
                        result.Error
                    );
                    callback.Invoke(new AsyncResult<object>(null, result.Error));
                    yield break;
                }

                stampSheet = result.Result.StampSheet;
            }
            {
                var machine = new StampSheetStateMachine(
                    stampSheet,
                    gs2Client.client,
                    gs2QuestSetting.distributorNamespaceName,
                    gs2QuestSetting.questKeyId
                );

                Gs2Exception exception = null;
                machine.OnError += e =>
                {
                    exception = e;
                };
                yield return machine.Execute();

                if (exception != null)
                {
                    gs2QuestSetting.onError.Invoke(
                        exception
                    );
                    callback.Invoke(new AsyncResult<object>(null, exception));
                    yield break;
                }
            }
            
            gs2QuestSetting.onEnd.Invoke(progress, rewards, isComplete);
            
            callback.Invoke(new AsyncResult<object>(progress, null));
        }
    }
}