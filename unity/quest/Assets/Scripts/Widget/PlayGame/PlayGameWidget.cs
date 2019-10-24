﻿﻿using System;
  using System.Collections.Generic;
  using Gs2.Core.Exception;
  using Gs2.Sample.Quest.Internal;
using Gs2.Unity.Gs2Quest.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Quest
{
    public class PlayGameWidget : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public Text questName;

        /// <summary>
        /// 
        /// </summary>
        public Text randomSeed;

        /// <summary>
        /// ステートマシン
        /// </summary>
        private PlayGameWidgetStateMachine _stateMachine;

        private void OnStart(EzProgress progress)
        {
            var selectedQuest = GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().selectedQuest;
            questName.text = questName.text
                .Replace("{quest_name}", selectedQuest.Metadata);
            randomSeed.text = randomSeed.text
                .Replace("{random_seed}", progress.RandomSeed.ToString());
        }
            
        private void OnEnd(EzProgress progress, List<EzReward> rewards, bool isComplete)
        {
            GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().onEndQuest.Invoke(
                progress,
                rewards,
                isComplete
            );
            GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().onClosePlayGame.Invoke(
                this
            );
        }

        private void OnError(Gs2Exception exception)
        {
            GameObject.Find("Gs2QuestInternalSetting").GetComponent<Gs2QuestInternalSetting>().onClosePlayGame.Invoke(
                this
            );
        }

        private void Start()
        {
            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'PlayGameWidgetStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " +
                    "ステートをコントロールする 'PlayGameWidgetStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                );
            }

            _stateMachine = animator.GetBehaviour<PlayGameWidgetStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'PlayGameWidgetStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " +
                    "ステートをコントロールする 'PlayGameWidgetStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                );
            }

            _stateMachine.controller.Initialize();
            _stateMachine.controller.gs2QuestSetting.onStart.AddListener(
                OnStart
            );
            _stateMachine.controller.gs2QuestSetting.onEnd.AddListener(
                OnEnd
            );
            _stateMachine.controller.gs2QuestSetting.onError.AddListener(
                OnError
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
            _stateMachine.controller.gs2QuestSetting.onStart.RemoveListener(
                OnStart
            );
            _stateMachine.controller.gs2QuestSetting.onEnd.RemoveListener(
                OnEnd
            );
            _stateMachine.controller.gs2QuestSetting.onError.RemoveListener(
                OnError
            );
        }

        /// <summary>
        /// メニューパネルをすべて非表示にする
        /// </summary>
        private void InActiveAll()
        {
            foreach (PlayGameWidgetStateMachine.State state in Enum.GetValues(typeof(PlayGameWidgetStateMachine.State)))
            {
                GetMenuGameObject(state).SetActive(false);
            }
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(PlayGameWidgetStateMachine.State state)
        {
            switch (state)
            {
                case PlayGameWidgetStateMachine.State.PlayGame:
                    return transform.Find("PlayGame").gameObject;
                default:
                    return transform.Find("Processing").gameObject;
            }
        }

        /// <summary>
        /// 選択する
        /// </summary>
        public void ClickToComplete()
        {
            _stateMachine.Complete();
        }

        /// <summary>
        /// 選択する
        /// </summary>
        public void ClickToFailed()
        {
            _stateMachine.Failed();
        }
    }
}