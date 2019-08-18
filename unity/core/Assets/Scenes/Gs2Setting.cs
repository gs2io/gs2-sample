using UnityEngine;

namespace Scenes
{
    public class Gs2Setting : MonoBehaviour
    {
        /// <summary>
        /// GS2 のクライアントID
        /// </summary>
        [SerializeField]
        public string clientId;
    
        /// <summary>
        /// GS2 のクライアントシークレット
        /// </summary>
        [SerializeField]
        public string clientSecret;

        private void Start()
        {
            DontDestroyOnLoad (this);
        }
    }
}