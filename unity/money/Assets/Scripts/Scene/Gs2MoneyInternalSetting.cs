using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.Money.Internal
{
    [System.Serializable]
    public class OpenStatusEvent : UnityEvent
    {
    }

    [System.Serializable]
    public class CloseStatusEvent : UnityEvent<MoneyStatusWidget>
    {
    }

    [System.Serializable]
    public class OpenStoreEvent : UnityEvent
    {
    }

    [System.Serializable]
    public class CloseStoreEvent : UnityEvent<MoneyStoreWidget>
    {
    }

    public class Gs2MoneyInternalSetting : MonoBehaviour
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