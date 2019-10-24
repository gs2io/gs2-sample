using System.Collections.Generic;
using Gs2.Core.Exception;
using Gs2.Gs2Quest.Model;
using Gs2.Unity.Gs2Quest.Model;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Quest
{
    [System.Serializable]
    public class ListCompletedQuestsEvent : UnityEvent<List<EzCompletedQuestList>>
    {
    }
    [System.Serializable]
    public class ListQuestGroupModelEvent : UnityEvent<List<EzQuestGroupModel>>
    {
    }

    [System.Serializable]
    public class ListQuestModelEvent : UnityEvent<List<EzQuestModel>>
    {
    }

    [System.Serializable]
    public class StartEvent : UnityEvent<EzProgress>
    {
    }

    [System.Serializable]
    public class GetProgressEvent : UnityEvent<EzProgress>
    {
    }

    [System.Serializable]
    public class EndEvent : UnityEvent<EzProgress, List<EzReward>, bool>
    {
    }

    [System.Serializable]
    public class ErrorEvent : UnityEvent<Gs2Exception>
    {
    }

    public class Gs2QuestSetting : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string questNamespaceName;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string questKeyId;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string distributorNamespaceName;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        public string jobQueueNamespaceName;

        /// <summary>
        /// クエストグループを取得したとき
        /// </summary>
        [SerializeField]
        public ListCompletedQuestsEvent onListCompletedQuestsModel = new ListCompletedQuestsEvent();

        /// <summary>
        /// クエストグループを取得したとき
        /// </summary>
        [SerializeField]
        public ListQuestGroupModelEvent onListGroupQuestModel = new ListQuestGroupModelEvent();

        /// <summary>
        /// クエストを取得したとき
        /// </summary>
        [SerializeField]
        public ListQuestModelEvent onListQuestModel = new ListQuestModelEvent();

        /// <summary>
        /// クエストを開始したとき
        /// </summary>
        [SerializeField]
        public StartEvent onStart = new StartEvent();

        /// <summary>
        /// 進行中のクエストを取得したとき
        /// </summary>
        [SerializeField]
        public GetProgressEvent onGetProgress = new GetProgressEvent();

        /// <summary>
        /// クエストを完了したとき
        /// </summary>
        [SerializeField]
        public EndEvent onEnd = new EndEvent();

        /// <summary>
        /// エラー発生時に発行されるイベント
        /// </summary>
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();

    }
}