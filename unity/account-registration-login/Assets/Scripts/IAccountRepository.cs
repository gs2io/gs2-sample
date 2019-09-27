﻿using Gs2.Unity.Gs2Account.Model;

namespace Gs2.Sample.AccountRegistrationLoginSample
{
    public interface IAccountRepository
    {
        /// <summary>
        /// アカウント情報が存在するか確認する
        /// </summary>
        /// <returns></returns>
        bool IsRegistered();

        /// <summary>
        /// アカウント情報を永続化する
        /// </summary>
        /// <param name="account"></param>
        void SaveAccount(EzAccount account);

        /// <summary>
        /// アカウント情報を読み込む
        /// </summary>
        EzAccount LoadAccount();

        /// <summary>
        /// アカウント情報を削除
        /// </summary>
        void DeleteAccount();
    }
}