﻿using System;
using Gs2.Sample.Core;
 using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.AccountTakeOver
{
    public class AccountTakeOverScene : MonoBehaviour
    {
        /// <summary>
        /// マッチメイキング操作をするためのコントローラー
        /// </summary>
        public AccountTakeOverController controller;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public Text emailTakeOverButtonLabel;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public InputField setEmailTakeOverSettingUserIdentifier;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public InputField setEmailTakeOverSettingPassword;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public InputField doEmailTakeOverSettingUserIdentifier;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public InputField doEmailTakeOverSettingPassword;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        public Text platformTakeOverButtonLabel;

        /// <summary>
        /// 発生したエラー
        /// </summary>
        [SerializeField]
        public Text errorMessage;

        /// <summary>
        /// ステートマシン
        /// </summary>
        private AccountTakeOverStateMachine _stateMachine;

        private void Start()
        {
            controller.Initialize();
            
            if (controller.gs2AccountTakeOverSetting == null)
            {
                throw new InvalidProgramException("'Gs2AccountTakeOverSetting' is not null.");
            }
            if (controller.gs2Client == null)
            {
                controller.gs2Client = Gs2Util.LoadGlobalGameObject<Gs2Client>("Gs2Client");
                if (controller.gs2Client == null)
                {
                    throw new InvalidProgramException(
                        "Unable to find GS2 Client" +
                        "You need to set GS2 Client on 'AccountTakeOverController' or place a GameObject named 'Gs2Client' in the scene." +
                        "Please check README.md for details." +
                        " / " +
                        "GS2 Client を見つけられません。" +
                        "'AccountTakeOverController' に GS2 Client を設定するか、'Gs2Client' という名前の GameObject をシーン内に配置する必要があります。" +
                        "詳しくは README.md をご確認ください。"
                    );
                }
            }

            var animator = GetComponent<Animator>();
            if (animator == null)
            {
                throw new InvalidProgramException(
                    "'AccountTakeOverStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'AccountTakeOverStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }
            _stateMachine = animator.GetBehaviour<AccountTakeOverStateMachine>();
            if (_stateMachine == null)
            {
                throw new InvalidProgramException(
                    "'AccountTakeOverStateMachine' that controls the state is not registered." +
                    "Check if Animator is registered in 'Canvas', if the correct controller is set, or if the script is set in the animator's Behavior" +
                    " / " + 
                    "ステートをコントロールする 'AccountTakeOverStateMachine' が登録されていません." +
                    "'Canvas' に Animator が登録されているか、正しいコントローラーが設定されているか、アニメーターの Behaviour にスクリプトが設定されているかを確認してください"
                    );
            }

            _stateMachine.controller = controller;
            _stateMachine.onChangeState.AddListener(
                (_, state) =>
                {
                    if (state == AccountTakeOverStateMachine.State.SelectSetTakeOverType)
                    {
                        if (_stateMachine.GetEmailTakeOverSetting() != null)
                        {
                            emailTakeOverButtonLabel.text = "Email - Registered";
                        }
                        else
                        {
                            emailTakeOverButtonLabel.text = "Email";
                        }
                        if (_stateMachine.GetPlatformTakeOverSetting() != null)
                        {
                            platformTakeOverButtonLabel.text = "Game Center - Registered";
#if UNITY_ANDROID
                            platformTakeOverButtonLabel.text = "Google Play - Registered";
#endif
                        }
                        else
                        {
                            platformTakeOverButtonLabel.text = "Game Center";
#if UNITY_ANDROID
                            platformTakeOverButtonLabel.text = "Google Play";
#endif
                        }
                    }
                    if (state == AccountTakeOverStateMachine.State.SetPlatformTakeOver)
                    {
                        if (Social.localUser.authenticated)
                        {
                            _stateMachine.userIdentifier = Social.localUser.id;
                            _stateMachine.password = Social.localUser.id;
                            animator.SetTrigger(AccountTakeOverStateMachine.Trigger.ReadySetTakeOver.ToString());
                        }
                        else
                        {
                            errorMessage.text = "Not logged in to Game Center";
#if UNITY_ANDROID
                            errorMessage.text = "Not logged in to Google Play Game Services";
#endif
                            animator.SetTrigger(AccountTakeOverStateMachine.Trigger.NotPlatformLogin.ToString());
                        }
                    }

                    if (state == AccountTakeOverStateMachine.State.DoPlatformTakeOver)
                    {
                        if (Social.localUser.authenticated)
                        {
                            _stateMachine.userIdentifier = Social.localUser.id;
                            _stateMachine.password = Social.localUser.id;
                            animator.SetTrigger(AccountTakeOverStateMachine.Trigger.ReadyDoTakeOver.ToString());
                        }
                        else
                        {
                            errorMessage.text = "Not logged in to Game Center";
#if UNITY_ANDROID
                            errorMessage.text = "Not logged in to Google Play Game Services";
#endif
                            animator.SetTrigger(AccountTakeOverStateMachine.Trigger.NotPlatformLogin.ToString());
                        }
                    }
                    InActiveAll();
                    GetMenuGameObject(state).SetActive(true);
                }
            );
            
            controller.gs2AccountTakeOverSetting.onError.AddListener(
                e =>
                {
                    if (errorMessage != null)
                    {
                        errorMessage.text = e.Message;
                    }
                }
            );

            // 画面の初期状態を設定
            InActiveAll();
        }

        /// <summary>
        /// メニューパネルをすべて非表示にする
        /// </summary>
        private void InActiveAll()
        {
            GetMenuGameObject(AccountTakeOverStateMachine.State.Initialize).SetActive(false);
            GetMenuGameObject(AccountTakeOverStateMachine.State.MainMenu).SetActive(false);
            GetMenuGameObject(AccountTakeOverStateMachine.State.SelectDoTakeOverType).SetActive(false);
            GetMenuGameObject(AccountTakeOverStateMachine.State.GetTakeOverSettingsProcessing).SetActive(false);
            GetMenuGameObject(AccountTakeOverStateMachine.State.SelectSetTakeOverType).SetActive(false);
            GetMenuGameObject(AccountTakeOverStateMachine.State.DoEmailTakeOver).SetActive(false);
            GetMenuGameObject(AccountTakeOverStateMachine.State.DoPlatformTakeOver).SetActive(false);
            GetMenuGameObject(AccountTakeOverStateMachine.State.SetEmailTakeOver).SetActive(false);
            GetMenuGameObject(AccountTakeOverStateMachine.State.SetPlatformTakeOver).SetActive(false);
            GetMenuGameObject(AccountTakeOverStateMachine.State.DoTakeOverProcessing).SetActive(false);
            GetMenuGameObject(AccountTakeOverStateMachine.State.SetTakeOverProcessing).SetActive(false);
            GetMenuGameObject(AccountTakeOverStateMachine.State.DeleteTakeOver).SetActive(false);
            GetMenuGameObject(AccountTakeOverStateMachine.State.DeleteTakeOverProcessing).SetActive(false);
            GetMenuGameObject(AccountTakeOverStateMachine.State.DoTakeOverCompleted).SetActive(false);
            GetMenuGameObject(AccountTakeOverStateMachine.State.Error).SetActive(false);
        }

        /// <summary>
        /// ステートに対応したメニューパネルを取得
        /// </summary>
        /// <param name="state">ステート</param>
        /// <returns>メニューパネル</returns>
        private GameObject GetMenuGameObject(AccountTakeOverStateMachine.State state)
        {
            switch (state)
            {
                case AccountTakeOverStateMachine.State.MainMenu:
                    return transform.Find("MainMenu").gameObject;
                case AccountTakeOverStateMachine.State.SelectDoTakeOverType:
                    return transform.Find("SelectDoTakeOverType").gameObject;
                case AccountTakeOverStateMachine.State.SelectSetTakeOverType:
                    return transform.Find("SelectSetTakeOverType").gameObject;
                case AccountTakeOverStateMachine.State.SetEmailTakeOver:
                    return transform.Find("SetEmailTakeOver").gameObject;
                case AccountTakeOverStateMachine.State.DoEmailTakeOver:
                    return transform.Find("DoEmailTakeOver").gameObject;
                case AccountTakeOverStateMachine.State.DeleteTakeOver:
                    return transform.Find("DeleteConfirm").gameObject;
                case AccountTakeOverStateMachine.State.Error:
                    return transform.Find("Error").gameObject;
                case AccountTakeOverStateMachine.State.DoTakeOverCompleted:
                    return transform.Find("Complete").gameObject;
                default:
                    return transform.Find("Processing").gameObject;
            }
        }

        /// <summary>
        /// 引継ぎを実行
        /// </summary>
        public void ClickToDoTakeOver()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(AccountTakeOverStateMachine.Trigger.SelectDoTakeOverType.ToString());
        }

        /// <summary>
        /// 引継ぎ情報を登録
        /// </summary>
        public void ClickToSetTakeOver()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(AccountTakeOverStateMachine.Trigger.SelectSetTakeOverType.ToString());
        }

        /// <summary>
        /// メールアドレスによる引継ぎ実行
        /// </summary>
        public void ClickToDoEmailTakeOver()
        {
            var stateMachine = GetComponent<Animator>();
            _stateMachine.type = (int)AccountTakeOverStateMachine.TakeOverType.Email;
            stateMachine.SetTrigger(AccountTakeOverStateMachine.Trigger.SelectDoEmailTakeOver.ToString());
        }

        /// <summary>
        /// Platformによる引継ぎ実行
        /// </summary>
        public void ClickToDoPlatformTakeOver()
        {
            var stateMachine = GetComponent<Animator>();
            _stateMachine.type = (int)AccountTakeOverStateMachine.TakeOverType.Platform;
            stateMachine.SetTrigger(AccountTakeOverStateMachine.Trigger.SelectDoPlatformTakeOver.ToString());
        }

        /// <summary>
        /// メールアドレスによる引継ぎ設定
        /// </summary>
        public void ClickToSetEmailTakeOver()
        {
            var stateMachine = GetComponent<Animator>();
            _stateMachine.type = (int)AccountTakeOverStateMachine.TakeOverType.Email;
            if (_stateMachine.GetEmailTakeOverSetting() == null)
            {
                stateMachine.SetTrigger(AccountTakeOverStateMachine.Trigger.SelectSetEmailTakeOver.ToString());
            }
            else
            {
                stateMachine.SetTrigger(AccountTakeOverStateMachine.Trigger.SelectDeleteTakeOver.ToString());
            }
        }

        /// <summary>
        /// Platformによる引継ぎ設定
        /// </summary>
        public void ClickToSetPlatformTakeOver()
        {
            var stateMachine = GetComponent<Animator>();
            _stateMachine.type = (int)AccountTakeOverStateMachine.TakeOverType.Platform;
            if (_stateMachine.GetPlatformTakeOverSetting() == null)
            {
                stateMachine.SetTrigger(AccountTakeOverStateMachine.Trigger.SelectSetPlatformTakeOver.ToString());
            }
            else
            {
                stateMachine.SetTrigger(AccountTakeOverStateMachine.Trigger.SelectDeleteTakeOver.ToString());
            }
        }

        /// <summary>
        /// メールアドレスによる引継ぎ設定を実行
        /// </summary>
        public void ClickToSubmitSetEmailTakeOver()
        {
            var stateMachine = GetComponent<Animator>();
            _stateMachine.userIdentifier = setEmailTakeOverSettingUserIdentifier.text;
            _stateMachine.password = setEmailTakeOverSettingPassword.text;
            stateMachine.SetTrigger(AccountTakeOverStateMachine.Trigger.ReadySetTakeOver.ToString());
        }

        /// <summary>
        /// メールアドレスによる引継ぎを実行
        /// </summary>
        public void ClickToSubmitDoEmailTakeOver()
        {
            var stateMachine = GetComponent<Animator>();
            _stateMachine.userIdentifier = doEmailTakeOverSettingUserIdentifier.text;
            _stateMachine.password = doEmailTakeOverSettingPassword.text;
            stateMachine.SetTrigger(AccountTakeOverStateMachine.Trigger.ReadyDoTakeOver.ToString());
        }

        /// <summary>
        /// 引継ぎ設定を削除
        /// </summary>
        public void ClickToSubmitDeleteTakeOver()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(AccountTakeOverStateMachine.Trigger.ReadyDeleteTakeOver.ToString());
        }

        /// <summary>
        /// 戻る
        /// </summary>
        public void ClickToBack()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(AccountTakeOverStateMachine.Trigger.Back.ToString());
        }

        /// <summary>
        /// エラー内容の確認ボタンをクリック
        /// </summary>
        public void ClickToConfirmError()
        {
            var stateMachine = GetComponent<Animator>();
            stateMachine.SetTrigger(AccountTakeOverStateMachine.Trigger.ConfirmError.ToString());
        }
    }
}