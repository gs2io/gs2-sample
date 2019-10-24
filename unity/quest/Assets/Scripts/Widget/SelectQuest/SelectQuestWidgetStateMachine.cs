﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using Gs2.Core;
  using Gs2.Gs2Stamina.Request;
  using Gs2.Sample.Core;
  using Gs2.Sample.Quest.Internal;
  using Gs2.Sample.Stamina;
  using Gs2.Unity.Gs2Quest.Model;
using Gs2.Unity.Gs2Quest.Result;
  using Gs2.Unity.Gs2Stamina.Result;
  using UnityEngine;
using UnityEngine.Events;

 namespace Gs2.Sample.Quest
{
    public class SelectQuestWidgetStateMachine : StateMachineBehaviour
    {
        public enum State
        {
            GetQuestGroupProcessing,
            SelectQuestGroup,
            GetQuestProcessing,
            SelectQuest,
            CheckStamina,
        }

        public enum Trigger
        {
            GetQuestGroupSucceed,
            GetQuestGroupFailed,
            SelectQuestGroup,
            GetQuestSucceed,
            GetQuestFailed,
            SelectQuest,
            GetStaminaFailed,
            CheckStaminaFailed,
            CheckStaminaSucceed,
            Back,
        }
        
        /// <summary>
        /// アニメーター
        /// </summary>
        private Animator _animator;

        /// <summary>
        /// 現在のステータス
        /// </summary>
        private State _state;

        /// <summary>
        /// 
        /// </summary>
        public List<EzQuestGroupModel> questGroups;

        /// <summary>
        /// 
        /// </summary>
        public List<EzCompletedQuestList> completedQuests;

        /// <summary>
        /// 
        /// </summary>
        public EzQuestGroupModel selectQuestGroup;
        
        /// <summary>
        /// 
        /// </summary>
        public List<EzQuestModel> quests;

        /// <summary>
        /// 
        /// </summary>
        public EzQuestModel selectQuest;

        /// <summary>
        /// スタミナコントローラー
        /// </summary>
        public StaminaController staminaController = new StaminaController();

        /// <summary>
        /// スタミナコントローラー
        /// </summary>
        public QuestController questController = new QuestController();

        [System.Serializable]
        public class ChangeStateEvent : UnityEvent<Animator, State>
        {
        }

        /// <summary>
        /// ステートが変化した時に呼び出されるイベント
        /// </summary>
        [SerializeField]
        public ChangeStateEvent onChangeState = new ChangeStateEvent();

        /// <summary>
        /// クエストグループとクリア状況を取得
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetQuestGroupsTask()
        {
            if (_animator)
            {
                {
                    AsyncResult<EzListQuestGroupsResult> result = null;
                    yield return questController.GetQuestGroups(
                        r => result = r
                    );

                    if (result.Error != null)
                    {
                        _animator.SetTrigger(Trigger.GetQuestGroupFailed.ToString());
                        yield break;
                    }

                    questGroups = result.Result.Items;
                }
                {
                    AsyncResult<EzDescribeCompletedQuestListsResult> result = null;
                    yield return questController.GetCompleteQuests(
                        r => result = r
                    );

                    if (result.Error != null)
                    {
                        _animator.SetTrigger(Trigger.GetQuestGroupFailed.ToString());
                        yield break;
                    }

                    completedQuests = result.Result.Items;
                }

                _animator.SetTrigger(Trigger.GetQuestGroupSucceed.ToString());
            }
        }

        /// <summary>
        /// クエストを取得
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetQuestsTask()
        {
            if (_animator)
            {
                {
                    AsyncResult<EzListQuestsResult> result = null;
                    yield return questController.GetQuests(
                        r => result = r,
                        selectQuestGroup
                    );

                    if (result.Error != null)
                    {
                        _animator.SetTrigger(Trigger.GetQuestFailed.ToString());
                        yield break;
                    }

                    quests = result.Result.Items;
                }

                _animator.SetTrigger(Trigger.GetQuestSucceed.ToString());
            }
        }

        /// <summary>
        /// クエストを取得
        /// </summary>
        /// <returns></returns>
        public IEnumerator CheckStaminaTask(EzQuestModel quest)
        {
            if (_animator)
            {
                {
                    AsyncResult<EzGetStaminaResult> result = null;
                    yield return staminaController.GetStamina(
                        r => result = r
                    );

                    if (result.Error != null)
                    {
                        _animator.SetTrigger(Trigger.GetStaminaFailed.ToString());
                        yield break;
                    }

                    var action = QuestController.GetConsumeAction<ConsumeStaminaByUserIdRequest>(
                        quest,
                        "Gs2Stamina:ConsumeStaminaByUserId"
                    );
                    if (action != null)
                    {
                        if (result.Result.Item.Value < action.consumeValue)
                        {
                            GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().onFewStamina.Invoke();
                            _animator.SetTrigger(Trigger.CheckStaminaFailed.ToString());
                            yield break;
                        }
                    }
                }

                GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().selectedQuestGroup = selectQuestGroup;
                GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().selectedQuest = selectQuest;
                GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().progress = null;
                GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().onStartQuest.Invoke(
                    selectQuestGroup,
                    selectQuest
                );
                
                _animator.SetTrigger(Trigger.CheckStaminaSucceed.ToString());
            }
        }

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _animator = animator;

            if (stateInfo.IsName(State.GetQuestGroupProcessing.ToString()))
            {
                staminaController.Initialize();
                questController.Initialize();
                questController.gs2QuestSetting.StartCoroutine(
                    GetQuestGroupsTask()
                );
            }
            if (stateInfo.IsName(State.GetQuestProcessing.ToString()))
            {
                questController.gs2QuestSetting.StartCoroutine(
                    GetQuestsTask()
                );
            }
            if (stateInfo.IsName(State.CheckStamina.ToString()))
            {
                questController.gs2QuestSetting.StartCoroutine(
                    CheckStaminaTask(
                        selectQuest
                    )
                );
            }

            foreach (State state in Enum.GetValues(typeof(State)))
            {
                if (stateInfo.IsName(state.ToString()))
                {
                    _state = state;
                    onChangeState.Invoke(animator, state);
                    break;
                }
            }
        }

        public bool SelectQuestGroup(EzQuestGroupModel questGroup)
        {
            if (_state == State.SelectQuestGroup)
            {
                selectQuestGroup = questGroup;
                _animator.SetTrigger(Trigger.SelectQuestGroup.ToString());
                return true;
            }

            return false;
        }

        public bool SelectQuest(EzQuestModel quest)
        {
            if (_state == State.SelectQuest)
            {
                selectQuest = quest;
                _animator.SetTrigger(Trigger.SelectQuest.ToString());
                return true;
            }

            return false;
        }
    }
}
