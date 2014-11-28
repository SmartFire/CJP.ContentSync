using CJP.ContentSync.Models;
using CJP.ContentSync.Services;
using Orchard.Localization;

namespace CJP.ContentSync.ExtensionMethods 
{
    public static class RealtimeFeedbackServiceExtensions
    {
        public static void Info(this IRealtimeFeedbackService service, LocalizedString message)
        {
            service.SendMessage(FeedbackLevel.Info, message);
        }
        public static void Warn(this IRealtimeFeedbackService service, LocalizedString message)
        {
            service.SendMessage(FeedbackLevel.Warn, message);
        }
        public static void Error(this IRealtimeFeedbackService service, LocalizedString message)
        {
            service.SendMessage(FeedbackLevel.Error, message);
        }
    }
}