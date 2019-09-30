using System.Collections.Generic;
using Gs2.Sample.Core;
using Gs2.Sample.Matchmaking;
using Gs2.Unity.Gs2Account.Model;
using Gs2.Unity.Gs2Matchmaking.Model;
using Gs2.Unity.Gs2Realtime.Model;
using Gs2.Unity.Util;
using Scenes.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Run : MonoBehaviour
{
    private GameSession _gameSession;
    
    void Start()
    {
        DontDestroyOnLoad (this);
        SceneManager.LoadScene("AccountRegistrationLogin");
    }

    public void OnLogin(EzAccount account, GameSession session)
    {
        var request = Gs2Util.LoadGlobalResource<MatchmakingRequest>("MatchmakingRequest");
        request.gameSession = _gameSession = session;
        SceneManager.LoadScene("Matchmaking");
    }

    public void OnCompleteMatchmaking(EzGathering gathering, List<string> joinPlayerIds)
    {
        var request = Gs2Util.LoadGlobalResource<RealtimeRequest>("RealtimeRequest");
        request.gameSession = _gameSession;
        request.gatheringId = gathering.Name;
        SceneManager.LoadScene("Realtime");
    }
}
