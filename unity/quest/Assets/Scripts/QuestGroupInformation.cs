using System.Collections.Generic;
using Gs2.Unity.Gs2Quest.Model;
using LitJson;

namespace Gs2.Sample.Quest
{
    public class QuestGroupInformation
    {
        public string Name;
        public string ScreenName;

        public QuestGroupInformation(EzQuestGroupModel model)
        {
            Name = model.Name;
            ScreenName = model.Metadata;
        }
    }
}