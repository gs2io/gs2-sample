 using Gs2.Unity.Gs2Quest.Model;
 using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Quest
{
    public class QuestGroupView : MonoBehaviour
    {
        private QuestGroupInformation _questGroup;

        public Text questTypeName;

        public void Initialize(QuestGroupInformation questGroup)
        {
            _questGroup = questGroup;
            questTypeName.text = questTypeName.text
                .Replace("{quest_group_name}", questGroup.ScreenName) ;
        }
    }
}