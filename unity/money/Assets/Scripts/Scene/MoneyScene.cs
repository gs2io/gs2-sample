﻿using System;
using Gs2.Sample.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Money
{
    public class MoneyScene : MonoBehaviour
    {
        /// <summary>
        /// 発生したエラー
        /// </summary>
        [SerializeField]
        public Text errorMessage;

        /// <summary>
        /// ステートマシン
        /// </summary>
        private MoneySceneStateMachine _stateMachine;

        private void Start()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'MoneySceneStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " +
                    "ステートをコントロールする 'MoneySceneStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                );
            }

            _stateMachine = animator.GetBehaviour<MoneySceneStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'MoneySceneStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " +
                    "ステートをコントロールする 'MoneySceneStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                );
            }

            _stateMachine.controller.Initialize();
            _stateMachine.controller.gs2MoneySetting.onError.AddListener(
                e =>
                {
                    if (errorMessage != null)
                    {
                        errorMessage.text = e.Message;
                    }
                }
            );
            
            _stateMachine.onChangeState.AddListener(
                (_, state) =>
                {
                    InActiveAll();
                    GetMenuGameObject(state).SetActive(true);
                }
            );

            InActiveAll();
        }

        /// <summary>
        /// メニューパネルをすべて非表示にする
        /// </summary>
        private void InActiveAll()
        {
            foreach (MoneySceneStateMachine.State state in Enum.GetValues(typeof(MoneySceneStateMachine.State)))
            {
                GetMenuGameObject(state).SetActive(false);
            }
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(MoneySceneStateMachine.State state)
        {
            switch (state)
            {
                case MoneySceneStateMachine.State.Idle:
                    return transform.Find("Main").gameObject;
                case MoneySceneStateMachine.State.Error:
                    return transform.Find("Error").gameObject;
                case MoneySceneStateMachine.State.Store:
                    return transform.Find("Sub").gameObject;
                default:
                    return transform.Find("Processing").gameObject;
            }
        }

        public void OnOpenStatusHandler()
        {
            if (_stateMachine.OpenStatus())
            {
                var widget = Gs2Util.LoadGlobalResource<MoneyStatusWidget>();
                var parent = GameObject.Find("MoneyStatusHolder");
                var rectTransform = (RectTransform)widget.transform;
                rectTransform.SetParent(parent.transform);
                rectTransform.position = parent.transform.position;
                rectTransform.sizeDelta = new Vector2();
                rectTransform.localScale = new Vector3(1, 1, 1);
                widget.gameObject.SetActive(true);
            }
        }

        public void OnCloseStatusHandler(MoneyStatusWidget widget)
        {
            if (_stateMachine.CloseStatus())
            {
                Destroy(widget.gameObject);
            }
        }

        public void OnOpenStoreHandler()
        {
            if (_stateMachine.OpenStore())
            {
                var widget = Gs2Util.LoadGlobalResource<MoneyStoreWidget>();
                var parent = GameObject.Find("MoneyStoreHolder");
                var rectTransform = (RectTransform)widget.transform;
                rectTransform.SetParent(parent.transform);
                rectTransform.position = parent.transform.position;
                rectTransform.sizeDelta = new Vector2();
                rectTransform.localScale = new Vector3(1, 1, 1);
                widget.gameObject.SetActive(true);
            }
        }

        public void OnCloseStoreHandler(MoneyStoreWidget widget)
        {
            if (_stateMachine.CloseStore())
            {
                Destroy(widget.gameObject);
            }
        }
    }
}