﻿using System;
using Gs2.Sample.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Matchmaking
{
    public class MatchmakingScene : MonoBehaviour
    {
        /// <summary>
        /// アカウント操作をするためのコントローラー
        /// </summary>
        public MatchmakingController controller;

        /// <summary>
        /// 表示するユーザID
        /// </summary>
        [SerializeField]
        public Text userId;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public InputField capacityInputField;

        /// <summary>
        /// 参加者リストを表示する GameObject
        /// </summary>
        public GameObject joinedPlayersContent;

        /// <summary>
        /// 参加者リストにプレイヤー名を表示するプレハブ
        /// </summary>
        public GameObject displayPlayerNamePrefab;

        /// <summary>
        /// 発生したエラー
        /// </summary>
        [SerializeField]
        public Text errorMessage;

        /// <summary>
        /// ステートマシン
        /// </summary>
        private MatchmakingStateMachine _stateMachine;

        private void Start()
        {
            controller.Initialize();
            
            if (controller.gs2MatchmakingSetting == null)
            {
                throw new InvalidProgramException("'Gs2MatchmakingSetting' is not null.");
            }
            if (string.IsNullOrEmpty(controller.gs2MatchmakingSetting.matchmakingNamespaceName))
            {
                throw new InvalidProgramException(
                    "'matchmakingNamespaceName' of script 'Gs2MatchmakingSetting' of 'Canvas' is not set. "+
                    "The value to be set for 'matchmakingNamespaceName' can be created by uploading the 'initialize_matchmaking_template.yaml' bundled with the sample as a GS2-Deploy stack." +
                    "Please check README.md for details." +
                    " / " +
                    "'Canvas' の持つスクリプト 'Gs2MatchmakingSetting' の 'matchmakingNamespaceName' が設定されていません。" +
                    "'matchmakingNamespaceName' に設定するべき値はサンプルに同梱されている 'initialize_matchmaking_template.yaml' を GS2-Deploy のスタックとしてアップロードすることで作成できます。" +
                    "詳しくは README.md をご確認ください。"
                    );
            }
        
            if (controller.gs2Client == null)
            {
                controller.gs2Client = Gs2Util.LoadGlobalGameObject<Gs2Client>("Gs2Client");
                if (controller.gs2Client == null)
                {
                    throw new InvalidProgramException(
                        "Unable to find GS2 Client" +
                        "You need to set GS2 Client on 'MatchmakingRegistrationLoginController' or place a GameObject named 'Gs2Client' in the scene." +
                        "Please check README.md for details." +
                        " / " +
                        "GS2 Client を見つけられません。" +
                        "'MatchmakingRegistrationLoginController' に GS2 Client を設定するか、'Gs2Client' という名前の GameObject をシーン内に配置する必要があります。" +
                        "詳しくは README.md をご確認ください。"
                    );
                }
            }

            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'MatchmakingRegistrationLoginStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'MatchmakingRegistrationLoginStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }
            _stateMachine = animator.GetBehaviour<MatchmakingStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'MatchmakingRegistrationLoginStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'MatchmakingRegistrationLoginStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }

            _stateMachine.controller = controller;
            _stateMachine.onChangeState.AddListener(
                (_, state) =>
                {
                    InActiveAll();
                    GetMenuGameObject(state).SetActive(true);
                }
            );
            
            controller.gs2MatchmakingSetting.onUpdateJoinedPlayerIds.AddListener(
                (gathering, joinedPlayerIds) =>
                {
                    displayPlayerNamePrefab.SetActive(false);

                    if (joinedPlayersContent != null)
                    {
                        foreach (Transform child in joinedPlayersContent.transform)
                        {
                            if (child.gameObject != displayPlayerNamePrefab)
                            {
                                Destroy(child.gameObject);
                            }
                        }

                        foreach (var joinedPlayerId in joinedPlayerIds)
                        {
                            var gamePlayerName = Instantiate<GameObject>(displayPlayerNamePrefab,
                                new Vector3(0.0f, 0.0f, 0.0f),
                                Quaternion.identity);
                            gamePlayerName.transform.SetParent(joinedPlayersContent.transform);
                            var nameLabel = gamePlayerName.GetComponent<Text>();
                            nameLabel.text = joinedPlayerId;
                            nameLabel.enabled = true;
                            gamePlayerName.SetActive(true);
                        }
                    }
                }
            );

            controller.gs2MatchmakingSetting.onError.AddListener(
                e => { errorMessage.text = e.Message; }
            );

            // 画面の初期状態を設定
            InActiveAll();
        }

        /// <summary>
        /// メニューパネルをすべて非表示にする
        /// </summary>
        private void InActiveAll()
        {
            GetMenuGameObject(MatchmakingStateMachine.State.Initialize).SetActive(false);
            GetMenuGameObject(MatchmakingStateMachine.State.MainMenu).SetActive(false);
            GetMenuGameObject(MatchmakingStateMachine.State.CreateGatheringMenu).SetActive(false);
            GetMenuGameObject(MatchmakingStateMachine.State.CreateGathering).SetActive(false);
            GetMenuGameObject(MatchmakingStateMachine.State.JoinGathering).SetActive(false);
            GetMenuGameObject(MatchmakingStateMachine.State.Matchmaking).SetActive(false);
            GetMenuGameObject(MatchmakingStateMachine.State.CancelMatchmaking).SetActive(false);
            GetMenuGameObject(MatchmakingStateMachine.State.MatchmakingComplete).SetActive(false);
            GetMenuGameObject(MatchmakingStateMachine.State.GatheringNotFound).SetActive(false);
            GetMenuGameObject(MatchmakingStateMachine.State.Error).SetActive(false);
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(MatchmakingStateMachine.State state)
        {
            switch (state)
            {
                case MatchmakingStateMachine.State.MainMenu:
                    var request = Gs2Util.LoadGlobalGameObject<MatchmakingRequest>("MatchmakingRequest");
                    userId.text = "UserId: " + request.gameSession.AccessToken.userId;
                    return transform.Find("MainMenu").gameObject;
                case MatchmakingStateMachine.State.CreateGatheringMenu:
                    return transform.Find("CreateGatheringMenu").gameObject;
                case MatchmakingStateMachine.State.Matchmaking:
                    return transform.Find("Matchmaking").gameObject;
                case MatchmakingStateMachine.State.GatheringNotFound:
                    return transform.Find("GatheringNotFound").gameObject;
                case MatchmakingStateMachine.State.MatchmakingComplete:
                    return transform.Find("Complete").gameObject;
                case MatchmakingStateMachine.State.Error:
                    return transform.Find("Error").gameObject;
                default:
                    return transform.Find("Processing").gameObject;
            }
        }

        /// <summary>
        /// ギャザリングの新規作成ボタンをクリック
        /// </summary>
        public void ClickToCreateGathering()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(MatchmakingStateMachine.Trigger.SelectCreateGathering.ToString());
        }

        /// <summary>
        /// 募集人数を確定してマッチメイキングを開始するボタンをクリック
        /// </summary>
        public void ClickToSubmitCapacity()
        {
            var stateMachine = GetComponent<Animator>();
            var behaviour = stateMachine.GetBehaviour<MatchmakingStateMachine>();
            behaviour.capacity = int.Parse(capacityInputField.text);
            stateMachine.SetTrigger(MatchmakingStateMachine.Trigger.SubmitCapacity.ToString());
        }

        /// <summary>
        /// 既存のギャザリングへの参加ボタンをクリック
        /// </summary>
        public void ClickToJoinGathering()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(MatchmakingStateMachine.Trigger.SelectJoinGathering.ToString());
        }

        /// <summary>
        /// キャンセルボタンをクリック
        /// </summary>
        public void ClickToCancelMatchmaking()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(MatchmakingStateMachine.Trigger.SelectCancelMatchmaking.ToString());
        }

        /// <summary>
        /// エラー内容の確認ボタンをクリック
        /// </summary>
        public void ClickToConfirmError()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(MatchmakingStateMachine.Trigger.ConfirmError.ToString());
        }
    }
}