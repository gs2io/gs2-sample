using Gs2.Unity.Util;
using UnityEngine;

namespace Scenes.AccountMenu
{
    public class AccountMenuRequest : MonoBehaviour
    {
        private void Start()
        {
            DontDestroyOnLoad (this);
        }
    }
}