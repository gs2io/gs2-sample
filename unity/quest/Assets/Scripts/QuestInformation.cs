using System.Collections.Generic;
using Gs2.Gs2Stamina.Request;
using Gs2.Unity.Gs2Quest.Model;

namespace Gs2.Sample.Quest
{
    public class QuestInformation
    {
        public string Id;
        public string Name;
        public string ScreenName;
        public int? consumeStamina;
        public bool open;
        public bool completed;

        public QuestInformation(EzQuestModel model, EzCompletedQuestList currentCompletedQuestList)
        {
            Id = model.QuestModelId;
            Name = model.Name;
            ScreenName = model.Metadata;
            var action = QuestController.GetConsumeAction<ConsumeStaminaByUserIdRequest>(
                model,
                "Gs2Stamina:ConsumeStaminaByUserId"
            );
            if (action != null)
            {
                consumeStamina = action.ConsumeValue;
            }

            if (currentCompletedQuestList == null)
            {
                currentCompletedQuestList = new EzCompletedQuestList
                {
                    CompleteQuestNames = new List<string>()
                };
            }
            
            var premiseQuestNames = new HashSet<string>(model.PremiseQuestNames);
            premiseQuestNames.ExceptWith(currentCompletedQuestList.CompleteQuestNames);
            open = premiseQuestNames.Count == 0;
            completed = currentCompletedQuestList.CompleteQuestNames.Contains(model.Name);
        }
    }
}