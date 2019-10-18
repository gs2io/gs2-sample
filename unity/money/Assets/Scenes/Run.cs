using System.Collections;
using Gs2.Sample.AccountRegistrationLoginSample;
using Gs2.Sample.Core;
using Gs2.Sample.Money;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Run : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad (this);
        SceneManager.LoadScene("AccountRegistrationLogin");
    }

    public IEnumerator LoadScene()
    {
        SceneManager.LoadScene("Money");
        yield break;
    }

    public void OnLogin(EzAccount account, GameSession session)
    {
        var request = Gs2Util.LoadGlobalResource<MoneyRequest>("MoneyRequest");
        request.gameSession = session;
        StartCoroutine(LoadScene());
    }
}
