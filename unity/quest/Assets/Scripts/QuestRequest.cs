using Gs2.Unity.Util;
using UnityEngine;

namespace Gs2.Sample.Quest
{
    [System.Serializable]
    public class QuestRequest : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public GameSession gameSession;

        private void Start()
        {
            DontDestroyOnLoad (this);
        }
    }
}