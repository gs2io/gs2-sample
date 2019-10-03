using System.Collections.Generic;
using Gs2.Core.Exception;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Gs2Matchmaking.Model;
using UnityEngine;
using UnityEngine.Events;

namespace Gs2.Sample.AccountTakeOver
{
    [System.Serializable]
    public class SetTakeOverEvent : UnityEvent<EzTakeOver>
    {
    }

    [System.Serializable]
    public class DeleteTakeOverEvent : UnityEvent<EzTakeOver>
    {
    }

    [System.Serializable]
    public class DoTakeOverEvent : UnityEvent<EzAccount>
    {
    }

    [System.Serializable]
    public class ErrorEvent : UnityEvent<Gs2Exception>
    {
    }

    public class Gs2AccountTakeOverSetting : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public SetTakeOverEvent onSetTakeOver = new SetTakeOverEvent();
        
        /// <summary>
        /// プレイヤーがギャザリングから離脱したとき
        /// </summary>
        [SerializeField]
        public DeleteTakeOverEvent onDeleteTakeOver = new DeleteTakeOverEvent();
        
        /// <summary>
        /// 参加中のプレイヤー一覧が更新されたとき
        /// </summary>
        [SerializeField]
        public DoTakeOverEvent onDoTakeOver = new DoTakeOverEvent();

        /// <summary>
        /// エラー発生時に発行されるイベント
        /// </summary>
        [SerializeField]
        public ErrorEvent onError = new ErrorEvent();

    }
}