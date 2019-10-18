using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Stamina.Internal
{
    [System.Serializable]
    public class OpenStatusEvent : UnityEvent
    {
    }

    [System.Serializable]
    public class CloseStatusEvent : UnityEvent<StaminaStatusWidget>
    {
    }

    [System.Serializable]
    public class OpenStoreEvent : UnityEvent
    {
    }

    [System.Serializable]
    public class CloseStoreEvent : UnityEvent<StaminaStoreWidget>
    {
    }

    public class Gs2StaminaInternalSetting : MonoBehaviour
    {
        /// <summary>
        /// ウォレットを開いたとき
        /// </summary>
        [SerializeField]
        public OpenStatusEvent onOpenStatus = new OpenStatusEvent();

        /// <summary>
        /// ウォレットを閉じたとき
        /// </summary>
        [SerializeField]
        public CloseStatusEvent onCloseStatus = new CloseStatusEvent();

        /// <summary>
        /// ストアを開いたとき
        /// </summary>
        [SerializeField]
        public OpenStoreEvent onOpenStore = new OpenStoreEvent();

        /// <summary>
        /// ストアを閉じたとき
        /// </summary>
        [SerializeField]
        public CloseStoreEvent onCloseStore = new CloseStoreEvent();
    }
}