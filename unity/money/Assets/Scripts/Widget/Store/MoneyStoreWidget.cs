﻿using System;
 using System.Collections.Generic;
 using Gs2.Sample.Core;
 using Gs2.Sample.Money.Internal;
 using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Money
{
    public class MoneyStoreWidget : MonoBehaviour
    {
        /// <summary>
        /// ストア表示する際に商品を並べるビューポート
        /// </summary>
        public GameObject productViewPort;

        /// <summary>
        /// ステートマシン
        /// </summary>
        private MoneyStoreWidgetStateMachine _stateMachine;

        private void OnGetProducts(List<Product> products)
        {
            if (productViewPort != null)
            {
                for (int i = 0; i < productViewPort.transform.childCount; i++)
                {
                    Destroy(productViewPort.transform.GetChild(i).gameObject);
                }

                foreach (var product in products)
                {
                    var productView = Gs2Util.LoadGlobalResource<ProductView>();
                    productView.transform.SetParent(productViewPort.transform);
                    productView.Initialize(product);
                    productView.transform.localScale = new Vector3(1, 1, 1);
                    if (!productView.Sold)
                    {
                        productView.transform.GetComponentInChildren<Button>().onClick.AddListener(
                            () =>
                            {
                                ClickToBuy(product);
                            }
                        );
                    }
                }
            }
        }
        
        private void Start()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'MoneyStoreWidgetStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'MoneyStoreWidgetStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }
            _stateMachine = animator.GetBehaviour<MoneyStoreWidgetStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'MoneyStoreWidgetStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'MoneyStoreWidgetStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }

            _stateMachine.controller.Initialize();
            _stateMachine.controller.gs2MoneySetting.onGetProducts.AddListener(
                OnGetProducts
            );
            
            _stateMachine.onChangeState.AddListener(
                (_, state) =>
                {
                    InActiveAll();
                    GetMenuGameObject(state).SetActive(true);
                }
            );

            // 画面の初期状態を設定
            InActiveAll();
        }

        private void OnDestroy()
        {
            _stateMachine.controller.gs2MoneySetting.onGetProducts.RemoveListener(
                OnGetProducts
            );
        }

        /// <summary>
        /// メニューパネルをすべて非表示にする
        /// </summary>
        private void InActiveAll()
        {
            foreach (MoneyStoreWidgetStateMachine.State state in Enum.GetValues(typeof(MoneyStoreWidgetStateMachine.State)))
            {
                GetMenuGameObject(state).SetActive(false);
            }
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(MoneyStoreWidgetStateMachine.State state)
        {
            switch (state)
            {
                case MoneyStoreWidgetStateMachine.State.SelectProduct:
                    return transform.Find("Products").gameObject;
                default:
                    return transform.Find("Processing").gameObject;
            }
        }

        /// <summary>
        /// 購入する
        /// </summary>
        public void ClickToBuy(Product product)
        {
            _stateMachine.Select(product);
        }

        /// <summary>
        /// 閉じる
        /// </summary>
        public void ClickToClose()
        {
            var gs2MoneyInternalSetting = GameObject.Find("Gs2MoneyInternalSetting").GetComponent<Gs2MoneyInternalSetting>();
            gs2MoneyInternalSetting.onCloseStore.Invoke(
                this
            );
        }
    }
}