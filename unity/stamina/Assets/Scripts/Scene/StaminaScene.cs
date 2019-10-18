﻿using System;
 using Gs2.Sample.Core;
 using Gs2.Sample.Money;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Stamina
{
    public class StaminaScene : MonoBehaviour
    {
        /// <summary>
        /// 発生したエラー
        /// </summary>
        [SerializeField]
        public Text errorMessage;

        /// <summary>
        /// ステートマシン
        /// </summary>
        private StaminaSceneStateMachine _stateMachine;

        private void Start()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'StaminaSceneStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " +
                    "ステートをコントロールする 'StaminaSceneStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                );
            }

            _stateMachine = animator.GetBehaviour<StaminaSceneStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'StaminaSceneStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " +
                    "ステートをコントロールする 'StaminaSceneStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                );
            }

            _stateMachine.moneyController.Initialize();
            _stateMachine.moneyController.gs2MoneySetting.onError.AddListener(
                e =>
                {
                    if (errorMessage != null)
                    {
                        errorMessage.text = e.Message;
                    }
                }
            );
            
            _stateMachine.staminaController.Initialize();
            _stateMachine.staminaController.gs2StaminaSetting.onError.AddListener(
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
            foreach (StaminaSceneStateMachine.State state in Enum.GetValues(typeof(StaminaSceneStateMachine.State)))
            {
                GetMenuGameObject(state).SetActive(false);
            }
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(StaminaSceneStateMachine.State state)
        {
            switch (state)
            {
                case StaminaSceneStateMachine.State.Idle:
                    return transform.Find("Main").gameObject;
                case StaminaSceneStateMachine.State.Error:
                    return transform.Find("Error").gameObject;
                case StaminaSceneStateMachine.State.StaminaStore:
                case StaminaSceneStateMachine.State.MoneyStore:
                    return transform.Find("Sub").gameObject;
                default:
                    return transform.Find("Processing").gameObject;
            }
        }

        public void OnOpenMoneyStatusHandler()
        {
            if (_stateMachine.OpenMoneyStatus())
            {
                var widget = Gs2Util.LoadGlobalResource<MoneyStatusWidget>();
                var parent = GameObject.Find("MoneyStatusHolder");
                var rectTransform = (RectTransform) widget.transform;
                rectTransform.SetParent(parent.transform);
                rectTransform.position = parent.transform.position;
                rectTransform.sizeDelta = new Vector2();
                rectTransform.localScale = new Vector3(1, 1, 1);
                widget.gameObject.SetActive(true);
            }
        }

        public void OnOpenStaminaStatusHandler()
        {
            if (_stateMachine.OpenStaminaStatus())
            {
                var widget = Gs2Util.LoadGlobalResource<StaminaStatusWidget>();
                var parent = GameObject.Find("StaminaStatusHolder");
                var rectTransform = (RectTransform) widget.transform;
                rectTransform.SetParent(parent.transform);
                rectTransform.position = parent.transform.position;
                rectTransform.sizeDelta = new Vector2();
                rectTransform.localScale = new Vector3(1, 1, 1);
                widget.gameObject.SetActive(true);
            }
        }

        public void OnCloseMoneyStatusHandler(MoneyStatusWidget widget)
        {
            if (_stateMachine.CloseMoneyStatus())
            {
                Destroy(widget.gameObject);
            }
        }

        public void OnCloseStaminaStatusHandler(StaminaStatusWidget widget)
        {
            if (_stateMachine.CloseStaminaStatus())
            {
                Destroy(widget.gameObject);
            }
        }

        public void OnOpenStaminaStoreHandler()
        {
            if (_stateMachine.OpenStaminaStore())
            {
                var widget = Gs2Util.LoadGlobalResource<StaminaStoreWidget>();
                var parent = GameObject.Find("StaminaStoreHolder");
                var rectTransform = (RectTransform)widget.transform;
                rectTransform.SetParent(parent.transform);
                rectTransform.position = parent.transform.position;
                rectTransform.sizeDelta = new Vector2();
                rectTransform.localScale = new Vector3(1, 1, 1);
                widget.gameObject.SetActive(true);
            }
        }

        public void OnCloseStaminaStoreHandler(StaminaStoreWidget widget)
        {
            if (_stateMachine.CloseStaminaStore())
            {
                Destroy(widget.gameObject);
            }
        }

        public void OnOpenMoneyStoreHandler()
        {
            if (_stateMachine.OpenMoneyStore())
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

        public void OnCloseMoneyStoreHandler(MoneyStoreWidget widget)
        {
            if (_stateMachine.CloseMoneyStore())
            {
                Destroy(widget.gameObject);
            }
        }
        
        /// <summary>
        /// ストアを開く
        /// </summary>
        public void ClickToConsumeStamina()
        {
            _stateMachine.ConsumeStamina();
        }

        /// <summary>
        /// 戻る
        /// </summary>
        public void ClickToBack()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(StaminaSceneStateMachine.Trigger.Back.ToString());
        }
    }
}