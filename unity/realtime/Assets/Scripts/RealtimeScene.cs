using System;
using System.Collections.Generic;
using Gs2.Sample.Core;
using Gs2.Unity.Gs2Realtime.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Scenes.Realtime
{
    public class RealtimeScene : MonoBehaviour
    {
        /// <summary>
        /// リアルタイム対戦操作をするためのコントローラー
        /// </summary>
        public RealtimeController controller;

        /// <summary>
        /// 
        /// </summary>
        public Player myCharacter;
        
        /// <summary>
        /// 
        /// </summary>
        public InputField myCharacterValue;

        /// <summary>
        /// 参加者リストにプレイヤー名を表示するプレハブ
        /// </summary>
        public GameObject otherPlayerPrefab;

        /// <summary>
        /// 発生したエラー
        /// </summary>
        [SerializeField]
        public Text errorMessage;

        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<uint, OtherPlayer> players = new Dictionary<uint, OtherPlayer>();

        /// <summary>
        /// ステートマシン
        /// </summary>
        private RealtimeStateMachine _stateMachine;
        
        private void Start()
        {
            controller.Initialize();
            
            if (controller.gs2RealtimeSetting == null)
            {
                throw new InvalidProgramException("'Gs2RealtimeSetting' is not null.");
            }
            if (string.IsNullOrEmpty(controller.gs2RealtimeSetting.realtimeNamespaceName))
            {
                throw new InvalidProgramException(
                    "'realtimeNamespaceName' of script 'Gs2RealtimeSetting' of 'Canvas' is not set. "+
                    "The value to be set for 'realtimeNamespaceName' can be created by uploading the 'initialize_realtime_template.yaml' bundled with the sample as a GS2-Deploy stack." +
                    "Please check README.md for details." +
                    " / " +
                    "'Canvas' の持つスクリプト 'Gs2RealtimeSetting' の 'realtimeNamespaceName' が設定されていません。" +
                    "'realtimeNamespaceName' に設定するべき値はサンプルに同梱されている 'initialize_realtime_template.yaml' を GS2-Deploy のスタックとしてアップロードすることで作成できます。" +
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
                        "You need to set GS2 Client on 'RealtimeRegistrationLoginController' or place a GameObject named 'Gs2Client' in the scene." +
                        "Please check README.md for details." +
                        " / " +
                        "GS2 Client を見つけられません。" +
                        "'RealtimeRegistrationLoginController' に GS2 Client を設定するか、'Gs2Client' という名前の GameObject をシーン内に配置する必要があります。" +
                        "詳しくは README.md をご確認ください。"
                    );
                }
            }

            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'RealtimeStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'RealtimeStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }
            _stateMachine = animator.GetBehaviour<RealtimeStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'RealtimeStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'RealtimeStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }


            _stateMachine.controller = controller;
            _stateMachine.onChangeState.AddListener(
                (_, state) =>
                {
                    if (state == RealtimeStateMachine.State.Main)
                    {
                        var request = Gs2Util.LoadGlobalGameObject<RealtimeRequest>("RealtimeRequest");
                        myCharacter.Session = _stateMachine.session;
                        myCharacter.Messenger = new Messenger(request.encryptionKey);
                        StartCoroutine(myCharacter.SendPosition());
                    }

                    InActiveAll();
                    GetMenuGameObject(state).SetActive(true);
                }
            );

            void JoinPlayerHandler(Gs2.Gs2Realtime.Message.Player player)
            {
                if (myCharacter == null || myCharacter.Session == null) return;
                if (player.ConnectionId == myCharacter.Session.MyConnectionId) return;

                otherPlayerPrefab.SetActive(false);

                var otherPlayer = Instantiate<GameObject>(otherPlayerPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
                otherPlayer.SetActive(true);
                otherPlayer.transform.SetParent(transform);
                players[player.ConnectionId] = otherPlayer.GetComponent<OtherPlayer>();
            }

            controller.gs2RealtimeSetting.onJoinPlayer.AddListener(JoinPlayerHandler);

            controller.gs2RealtimeSetting.onLeavePlayer.AddListener(
                player =>
                {
                    if (players.ContainsKey(player.ConnectionId))
                    {
                        Destroy(players[player.ConnectionId].gameObject);
                        players.Remove(player.ConnectionId);
                    }

                }
            );
                    
            controller.gs2RealtimeSetting.onUpdateProfile.AddListener(
                player => 
                {
                    if (players.ContainsKey(player.ConnectionId))
                    {
                        players[player.ConnectionId].Deserialize(player.Profile.ToByteArray());
                    }
                    else
                    {
                        JoinPlayerHandler(player);
                    }
                }
            );

            controller.gs2RealtimeSetting.onError.AddListener(
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
            myCharacter.GetComponent<Text>().text = myCharacterValue.text;
        }
    }
}