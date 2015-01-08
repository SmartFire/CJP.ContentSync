using CJP.ContentSync.Models;
using CJP.ContentSync.Models.Enums;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.UI.Notify;

namespace CJP.ContentSync.Services {
    public class DefaultRealtimeFeedbackService : IRealtimeFeedbackService {
        public ILogger Logger { get; set; }

        public DefaultRealtimeFeedbackService() {
            Logger = NullLogger.Instance;
        }

        public void SendMessage(FeedbackLevel level, LocalizedString message)
        {
            switch (level)
            {
                case FeedbackLevel.Info:
                    Logger.Information(message.ToString());
                    return;
                case FeedbackLevel.Warn:
                    Logger.Information(message.ToString());
                    return;
                case FeedbackLevel.Error:
                    Logger.Information(message.ToString());
                    return;
            }
        }
    }
}