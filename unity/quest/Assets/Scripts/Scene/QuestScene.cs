using System;
using Gs2.Core.Util;
using Gs2.Sample.Core;
using Gs2.Sample.Money;
using Gs2.Sample.Money.Internal;
using Gs2.Sample.Quest.Internal;
using Gs2.Sample.Stamina;
using Gs2.Sample.Stamina.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Quest
{
    public class QuestScene : MonoBehaviour
    {
        /// <summary>
        /// 発生したエラー
        /// </summary>
        [SerializeField]
        public Text errorMessage;

        /// <summary>
        /// ステートマシン
        /// </summary>
        private QuestSceneStateMachine _stateMachine;

        private void Start()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'QuestSceneStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " +
                    "ステートをコントロールする 'QuestSceneStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                );
            }

            _stateMachine = animator.GetBehaviour<QuestSceneStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'QuestSceneStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " +
                    "ステートをコントロールする 'QuestSceneStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                );
            }

            _stateMachine.moneyController.Initialize();
            _stateMachine.moneyController.gs2MoneySetting.onBuy.AddListener(
                product =>
                {
                    GameObject.Find("Gs2StaminaInternalSetting").GetComponent<Gs2StaminaInternalSetting>().onRefreshStatus.Invoke();
                }
            );
            _stateMachine.moneyController.gs2MoneySetting.onError.AddListener(
                e =>
                {
                    if (errorMessage != null)
                    {
                        errorMessage.text = e.Message;
                        
                        var stateMachine = GetComponent<Animator>();
                        stateMachine.SetTrigger(QuestSceneStateMachine.Trigger.Error.ToString());
                    }
                }
            );
            
            _stateMachine.staminaController.Initialize();
            _stateMachine.staminaController.gs2StaminaSetting.onBuy.AddListener(
                () =>
                {
                    GameObject.Find("Gs2MoneyInternalSetting").GetComponent<Gs2MoneyInternalSetting>().onRefreshStatus.Invoke();
                }
            );
            _stateMachine.staminaController.gs2StaminaSetting.onError.AddListener(
                e =>
                {
                    if (errorMessage != null)
                    {
                        errorMessage.text = e.Message;
                        
                        var stateMachine = GetComponent<Animator>();
                        stateMachine.SetTrigger(QuestSceneStateMachine.Trigger.Error.ToString());
                    }
                }
            );

            _stateMachine.questController.Initialize();
            _stateMachine.questController.gs2QuestSetting.onStart.AddListener(
                progress =>
                {
                    GameObject.Find("Gs2MoneyInternalSetting").GetComponent<Gs2MoneyInternalSetting>().onRefreshStatus.Invoke();
                    GameObject.Find("Gs2StaminaInternalSetting").GetComponent<Gs2StaminaInternalSetting>().onRefreshStatus.Invoke();
                }
            );
            _stateMachine.questController.gs2QuestSetting.onEnd.AddListener(
                (progress, rewards, isComplete) =>
                {
                    GameObject.Find("Gs2MoneyInternalSetting").GetComponent<Gs2MoneyInternalSetting>().onRefreshStatus.Invoke();
                    GameObject.Find("Gs2StaminaInternalSetting").GetComponent<Gs2StaminaInternalSetting>().onRefreshStatus.Invoke();
                }
            );
            _stateMachine.questController.gs2QuestSetting.onError.AddListener(
                e =>
                {
                    if (errorMessage != null)
                    {
                        errorMessage.text = e.Message;
                        
                        var stateMachine = GetComponent<Animator>();
                        stateMachine.SetTrigger(QuestSceneStateMachine.Trigger.Error.ToString());
                    }
                }
            );
            
            GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().onFewStamina.AddListener(
                () =>
                {
                    GameObject.Find("Gs2StaminaInternalSetting").GetComponent<Gs2StaminaInternalSetting>().onOpenStore.Invoke();
                }
            );

            _stateMachine.onChangeState.AddListener(
                (_, state) =>
                {
                    InActiveAll();
                    GetMenuGameObject(state).SetActive(true);
                }
            );

            var gs2Client = Gs2Util.LoadGlobalGameObject<Gs2Client>("Gs2Settings");
            var request = Gs2Util.LoadGlobalGameObject<QuestRequest>("QuestRequest");
            if (request != null)
            {
                var executor = GameObject.Find("Gs2QuestInternalSetting").GetComponent<JobQueueExecutor>();
                if (string.IsNullOrEmpty(executor.jobQueueNamespaceName))
                {
                    executor.jobQueueNamespaceName = _stateMachine.questController.gs2QuestSetting.jobQueueNamespaceName;
                }
                executor.onResult.AddListener(
                    (job, statusCode, result) =>
                    {
                        GameObject.Find("Gs2MoneyInternalSetting").GetComponent<Gs2MoneyInternalSetting>().onRefreshStatus.Invoke();
                        GameObject.Find("Gs2StaminaInternalSetting").GetComponent<Gs2StaminaInternalSetting>().onRefreshStatus.Invoke();
                    }
                );
                executor.onError.AddListener(
                    e =>
                    {
                        if (errorMessage != null)
                        {
                            errorMessage.text = e.Message;
                        
                            var stateMachine = GetComponent<Animator>();
                            stateMachine.SetTrigger(QuestSceneStateMachine.Trigger.Error.ToString());
                        }
                    }
                );
                StartCoroutine(
                    executor.Exec(
                        gs2Client.profile,
                        request.gameSession
                    )
                );
            }
            
            InActiveAll();
        }

        /// <summary>
        /// メニューパネルをすべて非表示にする
        /// </summary>
        private void InActiveAll()
        {
            foreach (QuestSceneStateMachine.State state in Enum.GetValues(typeof(QuestSceneStateMachine.State)))
            {
                GetMenuGameObject(state).SetActive(false);
            }
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(QuestSceneStateMachine.State state)
        {
            switch (state)
            {
                case QuestSceneStateMachine.State.CheckCurrentProgress:
                case QuestSceneStateMachine.State.SelectQuest:
                case QuestSceneStateMachine.State.PlayQuest:
                    return transform.Find("Main").gameObject;
                case QuestSceneStateMachine.State.Error:
                    return transform.Find("Error").gameObject;
                case QuestSceneStateMachine.State.StaminaStore:
                case QuestSceneStateMachine.State.MoneyStore:
                    return transform.Find("Sub").gameObject;
                default:
                    return transform.Find("Processing").gameObject;
            }
        }

        public void OnOpenSelectQuestHandler()
        {
            if (_stateMachine.OpenSelectQuest())
            {
                var widget = Gs2Util.LoadGlobalResource<SelectQuestWidget>();
                var parent = GameObject.Find("QuestSelectQuestHolder");
                var rectTransform = (RectTransform) widget.transform;
                rectTransform.SetParent(parent.transform);
                rectTransform.position = parent.transform.position;
                rectTransform.sizeDelta = new Vector2();
                rectTransform.localScale = new Vector3(1, 1, 1);
                widget.gameObject.SetActive(true);
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

        public void OnCloseSelectQuestHandler(SelectQuestWidget widget)
        {
            if (_stateMachine.CloseSelectQuest())
            {
                Destroy(widget.gameObject);
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
        
        public void OnPlayGameHandler()
        {
            if (_stateMachine.PlayGame())
            {
                var widget = Gs2Util.LoadGlobalResource<PlayGameWidget>();
                var parent = GameObject.Find("PlayGameHolder");
                var rectTransform = (RectTransform)widget.transform;
                rectTransform.SetParent(parent.transform);
                rectTransform.position = parent.transform.position;
                rectTransform.sizeDelta = new Vector2();
                rectTransform.localScale = new Vector3(1, 1, 1);
                widget.gameObject.SetActive(true);
            }
        }

        public void OnClosePlayGameHandler(PlayGameWidget widget)
        {
            if (_stateMachine.EndGame())
            {
                Destroy(widget.gameObject);
            }
        }

        /// <summary>
        /// 戻る
        /// </summary>
        public void ClickToBack()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(QuestSceneStateMachine.Trigger.Back.ToString());
        }
    }
}