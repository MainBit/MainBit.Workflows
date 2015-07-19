using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Email.Models;
using Orchard.Localization;
using MainBit.Workflows.Models;

namespace MainBit.Workflows.Drivers {

    // We define a specific driver instead of using a TemplateFilterForRecord, because we need the model to be the part and not the record.
    // Thus the encryption/decryption will be done when accessing the part's property

    public class MainbitSmtpSettingsPartDriver : ContentPartDriver<MainbitSmtpSettingsPart> {
        private const string TemplateName = "Parts/MainbitSmtpSettings";

        public MainbitSmtpSettingsPartDriver()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "MainbitSmtpSettings"; } }

        protected override DriverResult Editor(MainbitSmtpSettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_MainbitSmtpSettings_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix))
                    .OnGroup("Email MainBit");
        }

        protected override DriverResult Editor(MainbitSmtpSettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return ContentShape("Parts_MainbitSmtpSettings_Edit", () => {
                    var previousPassword = part.Password;
                    updater.TryUpdateModel(part, Prefix, null, null);

                    // restore password if the input is empty, meaning it has not been reseted
                    if (string.IsNullOrEmpty(part.Password)) {
                        part.Password = previousPassword;
                    }
                    return shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix);
                })
                .OnGroup("Email MainBit");
        }
    }
}