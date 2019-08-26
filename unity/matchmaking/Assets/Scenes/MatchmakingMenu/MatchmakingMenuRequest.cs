using Gs2.Unity.Util;
using UnityEngine;

namespace Scenes.MatchmakingMenu
{
    public class MatchmakingMenuRequest : MonoBehaviour
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