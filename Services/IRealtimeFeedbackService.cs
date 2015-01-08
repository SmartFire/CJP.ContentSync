using CJP.ContentSync.Models;
using CJP.ContentSync.Models.Enums;
using Orchard;
using Orchard.Localization;

namespace CJP.ContentSync.Services
{
    public interface IRealtimeFeedbackService : IDependency 
    {
        void SendMessage(FeedbackLevel level, LocalizedString message);
    }
}