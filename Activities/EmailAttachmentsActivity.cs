using System.Collections.Generic;
using Orchard.Email.Services;
using Orchard.Events;
using Orchard.Localization;
using Orchard.Messaging.Services;
using Orchard.Workflows.Models;
using Orchard.Workflows.Services;
using MainBit.Workflows.Services;
using Orchard;
using System.Linq;

namespace MainBit.Workflows.Activities {

    public class EmailAttachmentsActivity : Task {
        private readonly IMessageService _messageService;
        private readonly IJobsQueueService _jobsQueueService;
        private readonly IOrchardServices _orchardServices;

        public EmailAttachmentsActivity(
            IMessageService messageService,
            IJobsQueueService jobsQueueService,
            IOrchardServices orchardServices
            ) {
            _messageService = messageService;
            _jobsQueueService = jobsQueueService;
            _orchardServices = orchardServices;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public override IEnumerable<LocalizedString> GetPossibleOutcomes(WorkflowContext workflowContext, ActivityContext activityContext) {
            return new[] { T("Done") };
        }

        public override string Form {
            get {
                return "EmailAttachmentsActivity";
            }
        }

        public override LocalizedString Category {
            get { return T("Messaging"); }
        }

        public override string Name {
            get { return "SendEmailAttachments"; }
        }

        public override LocalizedString Description {
            get { return T("Sends an email to a specific user."); }
        }

        public override IEnumerable<LocalizedString> Execute(WorkflowContext workflowContext, ActivityContext activityContext) {
            var body = activityContext.GetState<string>("Body");
            var subject = activityContext.GetState<string>("Subject");
            var recipients = activityContext.GetState<string>("Recipients");
            var attachments = activityContext.GetState<string>("Attachments");

            if(!string.IsNullOrWhiteSpace(attachments)) {
                attachments = string.Join(";", attachments.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => _orchardServices.WorkContext.HttpContext.Server.MapPath(p)).ToArray());
            }

            var parameters = new Dictionary<string, object> {
                {"Subject", subject},
                {"Body", body},
                {"Recipients", recipients},
                {"Attachments", attachments},
            };

            var queued = activityContext.GetState<bool>("Queued");

            if (!queued) {
                _messageService.Send(SmtpMessageChannel.MessageType, parameters);
            }
            else {
                var priority = activityContext.GetState<int>("Priority");
                _jobsQueueService.Enqueue("IMessageService.Send", new { type = SmtpMessageWithAttachmentsChannel.MessageType, parameters = parameters }, priority);
            }

            yield return T("Done");
        }
    }
}