using System;
using System.Collections.Generic;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Gs2Realtime;
using Gs2.Unity.Gs2Realtime.Util;
using Gs2.Unity.Util;
using Scenes.Menu;
using Scenes.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes.Realtime
{
    public class Realtime : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        private RealtimeRequest _request;

        /// <summary>
        /// 
        /// </summary>
        private Messenger _messenger;
        
        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<uint, OtherPlayer> players = new Dictionary<uint, OtherPlayer>();

        /// <summary>
        /// GS2 の設定値
        /// </summary>
        [SerializeField]
        public Gs2RealtimeSetting gs2RealtimeSetting;

        /// <summary>
        /// 
        /// </summary>
        public Player myCharacter;
        
        /// <summary>
        /// 
        /// </summary>
        public TMP_InputField myCharacterValue;
        
        /// <summary>
        /// 参加者リストにプレイヤー名を表示するプレハブ
        /// </summary>
        public GameObject otherPlayerPrefab;

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
        private RealtimeStateMachine _stateMachine;
        
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
                _request = Gs2Util.LoadGlobalResource<RealtimeRequest>();
                
                var animator = GetComponent<Animator>();
                if (animator == null)
                {
                    throw new InvalidProgramException("'RealtimeStateMachine' is not found.");
                }
                _stateMachine = animator.GetBehaviour<RealtimeStateMachine>();
                if (_stateMachine == null)
                {
                    throw new InvalidProgramException("'RealtimeStateMachine' is not found.");
                }

                if (!_stateMachine.initialized)
                {
                    _stateMachine.Initialize(
                        _client.client,
                        _client.profile,
                        gs2RealtimeSetting,
                        _request.gameSession,
                        _request.gathering,
                        this,
                        _request.room
                    );

                    _stateMachine.OnChangeState += (state) =>
                    {
                        if (state == RealtimeStateMachine.State.Main)
                        {
                            myCharacter.Session = _stateMachine.session;
                            myCharacter.Messenger = _messenger;
                            StartCoroutine(myCharacter.SendPosition());
                        }
                        
                        InActiveAll();
                        GetMenuGameObject(state).SetActive(true);
                    };
                    
                    OnJoinPlayerHandler joinPlayerHandler = player => 
                    {
                        if(myCharacter == null || myCharacter.Session == null) return;
                        if(player.ConnectionId == myCharacter.Session.MyConnectionId) return;
                        
                        otherPlayerPrefab.SetActive(false);
                        
                        var otherPlayer = Instantiate<GameObject>(otherPlayerPrefab, new Vector3(0.0f,0.0f,0.0f), Quaternion.identity);
                        otherPlayer.SetActive(true);
                        otherPlayer.transform.SetParent(transform);
                        players[player.ConnectionId] = otherPlayer.GetComponent<OtherPlayer>();
                    };

                    _stateMachine.OnJoinPlayer += joinPlayerHandler;

                    _stateMachine.OnLeavePlayer += player =>
                    {
                        if (players.ContainsKey(player.ConnectionId))
                        {
                            Destroy(players[player.ConnectionId].gameObject);
                            players.Remove(player.ConnectionId);
                        }

                    };
                    
                    _stateMachine.OnUpdateProfile += player => 
                    {
                        if (players.ContainsKey(player.ConnectionId))
                        {
                            players[player.ConnectionId].Deserialize(player.Profile.ToByteArray());
                        }
                        else
                        {
                            joinPlayerHandler(player);
                        }
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
            GetMenuGameObject(RealtimeStateMachine.State.Initialize).SetActive(false);
            GetMenuGameObject(RealtimeStateMachine.State.GetRoom).SetActive(false);
            GetMenuGameObject(RealtimeStateMachine.State.ConnectRoom).SetActive(false);
            GetMenuGameObject(RealtimeStateMachine.State.SyncPlayerProfiles).SetActive(false);
            GetMenuGameObject(RealtimeStateMachine.State.Main).SetActive(false);
            GetMenuGameObject(RealtimeStateMachine.State.Disconnected).SetActive(false);
            GetMenuGameObject(RealtimeStateMachine.State.Error).SetActive(false);
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(RealtimeStateMachine.State state)
        {
            switch (state)
            {
                case RealtimeStateMachine.State.Main:
                    return transform.Find("BattleMain").gameObject;
                case RealtimeStateMachine.State.Disconnected:
                    return transform.Find("Disconnect").gameObject;
                case RealtimeStateMachine.State.Error:
                    return transform.Find("Error").gameObject;
                default:
                    return transform.Find("Processing").gameObject;
            }
        }

        /// <summary>
        /// エラー内容の確認ボタンをクリック
        /// </summary>
        public void ClickToConfirmDisconnect()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(RealtimeStateMachine.Trigger.ConfirmDisconnect.ToString());
        }
        
        /// <summary>
        /// エラー内容の確認ボタンをクリック
        /// </summary>
        public void ClickToConfirmError()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(RealtimeStateMachine.Trigger.ConfirmError.ToString());
        }

        public void ChangeCharacter()
        {
            myCharacter.GetComponent<TextMeshProUGUI>().SetText(myCharacterValue.text);
        }
    }
}