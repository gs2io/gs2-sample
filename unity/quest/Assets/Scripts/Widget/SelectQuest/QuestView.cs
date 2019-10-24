using UnityEngine;
using UnityEngine.UI;

namespace Gs2.Sample.Quest
{
    public class QuestView : MonoBehaviour
    {
        private QuestInformation _quest;

        public Text questName;

        public Text consumeStamina;

        public Text status;

        public void Initialize(QuestInformation quest)
        {
            _quest = quest;
            questName.text = questName.text
                .Replace("{quest_name}", quest.ScreenName);
            consumeStamina.text = consumeStamina.text
                .Replace("{consume_stamina}", quest.consumeStamina.ToString());
            status.text = status.text
                .Replace("{status}", quest.completed ? "Completed" : "Open");
            
            gameObject.SetActive(quest.open);
        }
    }
}