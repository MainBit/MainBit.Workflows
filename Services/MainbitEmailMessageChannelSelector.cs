using Orchard;
using Orchard.Email.Services;
using Orchard.Messaging.Services;
using System;
using System.Collections.Generic;

namespace MainBit.Workflows.Services {
    public class MainbitEmailMessageChannelSelector : Component, IMessageChannelSelector {
        private readonly IWorkContextAccessor _workContextAccessor;
        public const string ChannelName = "EmailFrom";

        public MainbitEmailMessageChannelSelector(IWorkContextAccessor workContextAccessor)
        {
            _workContextAccessor = workContextAccessor;
        }

        public MessageChannelSelectorResult GetChannel(string messageType, object payload) {
            if (messageType == "EmailFrom")
            {
                var workContext = _workContextAccessor.GetContext();
                return new MessageChannelSelectorResult {
                    Priority = 50,
                    MessageChannel = () => workContext.Resolve<IMainbitSmtpChannel>()
                };
            }

            var parameters = payload as IDictionary<string, object>;
            if (messageType == "Email" && parameters != null && parameters.ContainsKey("Attachments")
                && !String.IsNullOrWhiteSpace(parameters["Attachments"] as string))
            {
                var workContext = _workContextAccessor.GetContext();
                return new MessageChannelSelectorResult {
                    Priority = 51,
                    MessageChannel = () => workContext.Resolve<IAttachmentsSmtpChannel>()
                }; 
            }

            return null;
        }
    }
}
