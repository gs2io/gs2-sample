﻿using System;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Util;
using Scenes.MatchmakingMenu.Model;
using Scenes.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes.MatchmakingMenu
{
    public class MatchmakingMenu : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        private MatchmakingMenuRequest _request;
        
        /// <summary>
        /// GS2 の設定値
        /// </summary>
        [SerializeField]
        public Gs2MatchmakingSetting gs2MatchmakingSetting;

        /// <summary>
        /// 表示するユーザID
        /// </summary>
        [SerializeField]
        public TextMeshProUGUI userId;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public TMP_InputField capacityInputField;

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
        public TextMeshProUGUI errorMessage;

        /// <summary>
        /// 
        /// </summary>
        private Gs2Client _client;

        /// <summary>
        /// ステートマシン
        /// </summary>
        private MatchmakingMenuStateMachine _stateMachine;
        
        void Update()
        {
            if (_client == null)
            {
                _client = GameObject.Find("Gs2Client").GetComponent<Gs2Client>();
                if (_client == null)
                {
                    throw new InvalidProgramException("'Gs2Client' is not found.");
                }
            }

            if (_client.initialized)
            {
                _request = Gs2Util.LoadGlobalResource<MatchmakingMenuRequest>();
                
                var animator = GetComponent<Animator>();
                if (animator == null)
                {
                    throw new InvalidProgramException("'MatchmakingMenuStateMachine' is not found.");
                }
                _stateMachine = animator.GetBehaviour<MatchmakingMenuStateMachine>();
                if (_stateMachine == null)
                {
                    throw new InvalidProgramException("'MatchmakingMenuStateMachine' is not found.");
                }

                if (!_stateMachine.initialized)
                {
                    _stateMachine.Initialize(
                        _client.client,
                        _client.profile,
                        gs2MatchmakingSetting,
                        _request.gameSession,
                        this
                    );

                    _stateMachine.OnChangeState += (state) =>
                    {
                        InActiveAll();
                        GetMenuGameObject(state).SetActive(true);
                    };

                    _stateMachine.OnUpdateJoinedPlayerIds += (gathering, joinedPlayerIds) =>
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
                                var nameLabel = gamePlayerName.GetComponent<TextMeshProUGUI>();
                                nameLabel.SetText(joinedPlayerId);
                                nameLabel.enabled = true;
                                gamePlayerName.SetActive(true);
                            }
                        }
                    };

                    _stateMachine.OnMatchmakingComplete += (gathering, joinedPlayerIds) =>
                    {
                        var request = Gs2Util.LoadGlobalResource<RealtimeRequest>();
                        request.gameSession = _request.gameSession;
                        request.gathering = gathering;
                        SceneManager.LoadScene("Realtime");
                        
                        animator.SetTrigger(MatchmakingMenuStateMachine.Trigger.MatchmakingSucceed.ToString());
                    };

                    _stateMachine.OnError += exception => { errorMessage.SetText(exception.ToString()); };

                    // 画面の初期状態を設定
                    InActiveAll();
                }
            }
        }

        /// <summary>
        /// メニューパネルをすべて非表示にする
        /// </summary>
        private void InActiveAll()
        {
            GetMenuGameObject(MatchmakingMenuStateMachine.State.Initialize).SetActive(false);
            GetMenuGameObject(MatchmakingMenuStateMachine.State.MainMenu).SetActive(false);
            GetMenuGameObject(MatchmakingMenuStateMachine.State.CreateGatheringMenu).SetActive(false);
            GetMenuGameObject(MatchmakingMenuStateMachine.State.CreateGathering).SetActive(false);
            GetMenuGameObject(MatchmakingMenuStateMachine.State.JoinGathering).SetActive(false);
            GetMenuGameObject(MatchmakingMenuStateMachine.State.Matchmaking).SetActive(false);
            GetMenuGameObject(MatchmakingMenuStateMachine.State.CancelMatchmaking).SetActive(false);
            GetMenuGameObject(MatchmakingMenuStateMachine.State.MatchmakingComplete).SetActive(false);
            GetMenuGameObject(MatchmakingMenuStateMachine.State.GatheringNotFound).SetActive(false);
            GetMenuGameObject(MatchmakingMenuStateMachine.State.Error).SetActive(false);
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(MatchmakingMenuStateMachine.State state)
        {
            switch (state)
            {
                case MatchmakingMenuStateMachine.State.MainMenu:
                    userId.text = "UserId: " + _request.gameSession.AccessToken.userId;
                    return transform.Find("MainMenu").gameObject;
                case MatchmakingMenuStateMachine.State.CreateGatheringMenu:
                    return transform.Find("CreateGatheringMenu").gameObject;
                case MatchmakingMenuStateMachine.State.Matchmaking:
                    return transform.Find("Matchmaking").gameObject;
                case MatchmakingMenuStateMachine.State.GatheringNotFound:
                    return transform.Find("GatheringNotFound").gameObject;
                case MatchmakingMenuStateMachine.State.Error:
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
            stateMachine.SetTrigger(MatchmakingMenuStateMachine.Trigger.SelectCreateGathering.ToString());
        }

        /// <summary>
        /// 募集人数を確定してマッチメイキングを開始するボタンをクリック
        /// </summary>
        public void ClickToSubmitCapacity()
        {
            var stateMachine = GetComponent<Animator>();
            var behaviour = stateMachine.GetBehaviour<MatchmakingMenuStateMachine>();
            behaviour.capacity = int.Parse(capacityInputField.text);
            stateMachine.SetTrigger(MatchmakingMenuStateMachine.Trigger.SubmitCapacity.ToString());
        }

        /// <summary>
        /// 既存のギャザリングへの参加ボタンをクリック
        /// </summary>
        public void ClickToJoinGathering()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(MatchmakingMenuStateMachine.Trigger.SelectJoinGathering.ToString());
        }

        /// <summary>
        /// キャンセルボタンをクリック
        /// </summary>
        public void ClickToCancelMatchmaking()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(MatchmakingMenuStateMachine.Trigger.SelectCancelMatchmaking.ToString());
        }

        /// <summary>
        /// エラー内容の確認ボタンをクリック
        /// </summary>
        public void ClickToConfirmError()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(MatchmakingMenuStateMachine.Trigger.ConfirmError.ToString());
        }
    }
}