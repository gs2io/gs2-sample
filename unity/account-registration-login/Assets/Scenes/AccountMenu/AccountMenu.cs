using System;
using Scenes.AccountMenu.Model;
using Scenes.AccountMenu.Repository;
using Scenes.MatchmakingMenu;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes.AccountMenu
{
    public class AccountMenu : MonoBehaviour
    {
        /// <summary>
        /// GS2 の設定値
        /// </summary>
        [SerializeField]
        public Gs2AccountSetting gs2AccountSetting;

        /// <summary>
        /// ログイン画面に表示するユーザID
        /// </summary>
        [SerializeField]
        public TextMeshProUGUI userId;

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
        private AccountMenuStateMachine _stateMachine;
        
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
                var animator = GetComponent<Animator>();
                if (animator == null)
                {
                    throw new InvalidProgramException("'AccountMenuStateMachine' is not found.");
                }
                _stateMachine = animator.GetBehaviour<AccountMenuStateMachine>();
                if (_stateMachine == null)
                {
                    throw new InvalidProgramException("'AccountMenuStateMachine' is not found.");
                }

                if (!_stateMachine.initialized)
                {
                    _stateMachine.Initialize(
                        _client.client,
                        _client.profile,
                        gs2AccountSetting,
                        new AccountRepository(),
                        _client
                    );

                    _stateMachine.OnChangeState += (state) =>
                    {
                        InActiveAll();
                        GetMenuGameObject(state).SetActive(true);
                    };

                    _stateMachine.OnError += exception => { errorMessage.SetText(exception.ToString()); };

                    _stateMachine.OnLogin += (account, session) =>
                    {
                        var request = Gs2Util.LoadGlobalResource<MatchmakingMenuRequest>();
                        request.gameSession = session;
                        SceneManager.LoadScene("MatchmakingMenu");
                    };

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
            GetMenuGameObject(AccountMenuStateMachine.State.Initialize).SetActive(false);
            GetMenuGameObject(AccountMenuStateMachine.State.LoadAccount).SetActive(false);
            GetMenuGameObject(AccountMenuStateMachine.State.CreateAccountMenu).SetActive(false);
            GetMenuGameObject(AccountMenuStateMachine.State.CreateAccountProcessing).SetActive(false);
            GetMenuGameObject(AccountMenuStateMachine.State.SaveAccount).SetActive(false);
            GetMenuGameObject(AccountMenuStateMachine.State.LoginMenu).SetActive(false);
            GetMenuGameObject(AccountMenuStateMachine.State.LoginProcessing).SetActive(false);
            GetMenuGameObject(AccountMenuStateMachine.State.LoginComplete).SetActive(false);
            GetMenuGameObject(AccountMenuStateMachine.State.RemoveAccountProcessing).SetActive(false);
            GetMenuGameObject(AccountMenuStateMachine.State.Error).SetActive(false);
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(AccountMenuStateMachine.State state)
        {
            switch (state)
            {
                case AccountMenuStateMachine.State.CreateAccountMenu:
                    return transform.Find("CreateAccount").gameObject;
                case AccountMenuStateMachine.State.LoginMenu:
                    if (_stateMachine.account != null)
                    {
                        userId.text = "UserId: " + _stateMachine.account.UserId;
                    }
                    return transform.Find("Login").gameObject;
                case AccountMenuStateMachine.State.Error:
                    return transform.Find("Error").gameObject;
                default:
                    return transform.Find("Processing").gameObject;
            }
        }

        /// <summary>
        /// アカウントの作成ボタンをクリック
        /// </summary>
        public void ClickToCreateAccount()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(AccountMenuStateMachine.Trigger.SelectCreateAccount.ToString());
        }

        /// <summary>
        /// ログインボタンをクリック
        /// </summary>
        public void ClickToLogin()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(AccountMenuStateMachine.Trigger.SelectLogin.ToString());
        }

        /// <summary>
        /// アカウントの削除ボタンをクリック
        /// </summary>
        public void ClickToRemoveAccount()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(AccountMenuStateMachine.Trigger.SelectRemoveAccount.ToString());
        }

        /// <summary>
        /// エラー内容の確認ボタンをクリック
        /// </summary>
        public void ClickToConfirmError()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(AccountMenuStateMachine.Trigger.ConfirmError.ToString());
        }
    }
}