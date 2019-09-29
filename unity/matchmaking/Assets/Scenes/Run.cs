using Gs2.Sample.Core;
using Gs2.Sample.Matchmaking;
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

    public void OnLogin(EzAccount account, GameSession session)
    {
        var request = Gs2Util.LoadGlobalResource<MatchmakingRequest>("MatchmakingRequest");
        request.gameSession = session;
        SceneManager.LoadScene("Matchmaking");
    }
}
