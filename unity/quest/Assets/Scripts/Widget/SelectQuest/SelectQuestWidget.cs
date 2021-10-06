﻿﻿using System;
  using System.Collections.Generic;
  using System.Linq;
  using Gs2.Core.Util;
using Gs2.Sample.Core;
using Gs2.Sample.Money;
  using Gs2.Sample.Quest.Internal;
  using Gs2.Sample.Stamina;
  using Gs2.Unity.Gs2Quest.Model;
  using UnityEngine;
  using UnityEngine.Events;
  using UnityEngine.UI;

namespace Gs2.Sample.Quest
{
    public class SelectQuestWidget : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public GameObject questGroupsViewPort;

        /// <summary>
        /// 
        /// </summary>
        public GameObject questsViewPort;

        /// <summary>
        /// ステートマシン
        /// </summary>
        private SelectQuestWidgetStateMachine _stateMachine;

        /// <summary>
        /// 
        /// </summary>
        private List<EzCompletedQuestList> _completedQuestLists;
        
        /// <summary>
        /// 
        /// </summary>
        private EzCompletedQuestList _currentCompletedQuestList;
        
        private void OnListCompletedQuestsModelFunc(List<EzCompletedQuestList> list)
        {
            _completedQuestLists = list;
        }

        private void OnListGroupQuestModelFunc(List<EzQuestGroupModel> questGroups)
        {
            if (questGroupsViewPort != null)
            {
                for (int i = 0; i < questGroupsViewPort.transform.childCount; i++)
                {
                    Destroy(questGroupsViewPort.transform.GetChild(i).gameObject);
                }

                foreach (var questGroup in questGroups)
                {
                    var questType = Gs2Util.LoadGlobalResource<QuestGroupView>();
                    questType.transform.SetParent(questGroupsViewPort.transform);
                    questType.Initialize(new QuestGroupInformation(questGroup));
                    questType.transform.localScale = new Vector3(1, 1, 1);
                    questType.transform.GetComponentInChildren<Button>().onClick.AddListener(
                        () =>
                        {
                            _currentCompletedQuestList = _completedQuestLists.Find(completedQuestList =>
                                completedQuestList.QuestGroupName == questGroup.Name);

                            ClickToSelect(questGroup);
                        }
                    );
                    questType.gameObject.SetActive(true);
                }
            }
        }
        
        private void OnListQuestModel(List<EzQuestModel> quests)
        {
            if (questsViewPort != null)
            {
                for (int i = 0; i < questsViewPort.transform.childCount; i++)
                {
                    Destroy(questsViewPort.transform.GetChild(i).gameObject);
                }

                foreach (var quest in quests)
                {
                    var questType = Gs2Util.LoadGlobalResource<QuestView>();
                    questType.transform.SetParent(questsViewPort.transform);
                    questType.Initialize(new QuestInformation(quest, _currentCompletedQuestList));
                    questType.transform.localScale = new Vector3(1, 1, 1);
                    questType.transform.GetComponentInChildren<Button>().onClick.AddListener(
                        () =>
                        {
                            ClickToSelect(quest);
                        }
                    );
                }
            }
        }
        
        private void OnStartQuest(EzQuestGroupModel questGroup, EzQuestModel quest)
        {
            GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().onCloseSelectQuest.Invoke(
                this
            );
        }

        private void Start()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'SelectQuestWidgetStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct questController is set, or if the script is set in the animator's Behavior" +
                    " / " +
                    "ステートをコントロールする 'SelectQuestWidgetStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                );
            }

            _stateMachine = animator.GetBehaviour<SelectQuestWidgetStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'SelectQuestWidgetStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct questController is set, or if the script is set in the animator's Behavior" +
                    " / " +
                    "ステートをコントロールする 'SelectQuestWidgetStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                );
            }

            _stateMachine.questController.Initialize();

            _stateMachine.questController.gs2QuestSetting.onListCompletedQuestsModel.AddListener(
                OnListCompletedQuestsModelFunc
            );

            _stateMachine.questController.gs2QuestSetting.onListGroupQuestModel.AddListener(
                OnListGroupQuestModelFunc
            );
            
            _stateMachine.questController.gs2QuestSetting.onListQuestModel.AddListener(
                OnListQuestModel
            );

            GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().onStartQuest.AddListener(
                OnStartQuest
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

        private void OnDestroy()
        {
            _stateMachine.questController.gs2QuestSetting.onListCompletedQuestsModel.RemoveListener(
                OnListCompletedQuestsModelFunc
            );

            _stateMachine.questController.gs2QuestSetting.onListGroupQuestModel.RemoveListener(
                OnListGroupQuestModelFunc
            );
            
            _stateMachine.questController.gs2QuestSetting.onListQuestModel.RemoveListener(
                OnListQuestModel
            );

            GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().onStartQuest.RemoveListener(
                OnStartQuest
            );
        }

        /// <summary>
        /// メニューパネルをすべて非表示にする
        /// </summary>
        private void InActiveAll()
        {
            foreach (SelectQuestWidgetStateMachine.State state in Enum.GetValues(typeof(SelectQuestWidgetStateMachine.State)))
            {
                GetMenuGameObject(state).SetActive(false);
            }
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(SelectQuestWidgetStateMachine.State state)
        {
            switch (state)
            {
                case SelectQuestWidgetStateMachine.State.SelectQuestGroup:
                    return transform.Find("SelectQuestGroup").gameObject;
                case SelectQuestWidgetStateMachine.State.SelectQuest:
                    return transform.Find("SelectQuest").gameObject;
                default:
                    return transform.Find("Processing").gameObject;
            }
        }

        /// <summary>
        /// クエストグループを選択する
        /// </summary>
        public void ClickToSelect(EzQuestGroupModel questGroup)
        {
            _stateMachine.SelectQuestGroup(questGroup);
        }

        /// <summary>
        /// クエストを選択する
        /// </summary>
        public void ClickToSelect(EzQuestModel quest)
        {
            _stateMachine.SelectQuest(quest);
        }

        /// <summary>
        /// 戻る
        /// </summary>
        public void ClickToBack()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(SelectQuestWidgetStateMachine.Trigger.Back.ToString());
        }
    }
}