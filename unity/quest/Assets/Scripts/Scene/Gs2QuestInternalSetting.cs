using System.Collections.Generic;
using Gs2.Unity.Gs2Quest.Model;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Quest.Internal
{
    [System.Serializable]
    public class OpenSelectQuestEvent : UnityEvent
    {
    }

    [System.Serializable]
    public class CloseSelectQuestEvent : UnityEvent<SelectQuestWidget>
    {
    }
    
    [System.Serializable]
    public class StartQuest : UnityEvent<EzQuestGroupModel, EzQuestModel>
    {
    }

    [System.Serializable]
    public class EndQuest : UnityEvent<EzProgress, List<EzReward>, bool>
    {
    }

    [System.Serializable]
    public class ClosePlayGameEvent : UnityEvent<PlayGameWidget>
    {
    }

    [System.Serializable]
    public class FewStaminaEvent : UnityEvent
    {
    }

    public class Gs2QuestInternalSetting : MonoBehaviour
    {
        /// <summary>
        /// ウォレットを開いたとき
        /// </summary>
        [SerializeField]
        public OpenSelectQuestEvent onOpenSelectQuest = new OpenSelectQuestEvent();

        /// <summary>
        /// ウォレットを閉じたとき
        /// </summary>
        [SerializeField]
        public CloseSelectQuestEvent onCloseSelectQuest = new CloseSelectQuestEvent();

        /// <summary>
        /// ウォレットを閉じたとき
        /// </summary>
        [SerializeField]
        public StartQuest onStartQuest = new StartQuest();

        /// <summary>
        /// ウォレットを閉じたとき
        /// </summary>
        [SerializeField]
        public EndQuest onEndQuest = new EndQuest();

        /// <summary>
        /// ウォレットを閉じたとき
        /// </summary>
        [SerializeField]
        public ClosePlayGameEvent onClosePlayGame = new ClosePlayGameEvent();

        /// <summary>
        /// ウォレットを閉じたとき
        /// </summary>
        [SerializeField]
        public FewStaminaEvent onFewStamina = new FewStaminaEvent();

        /// <summary>
        /// 
        /// </summary>
        public EzQuestGroupModel selectedQuestGroup;
        
        /// <summary>
        /// 
        /// </summary>
        public EzQuestModel selectedQuest;
        
        /// <summary>
        /// 
        /// </summary>
        public EzProgress progress;
    }
}