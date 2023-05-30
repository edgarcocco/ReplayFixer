using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplayFixer.Services
{
    public sealed class MessageService
    {
        private readonly IStringLocalizer<MessageService> _localizer = null!;

        public MessageService(IStringLocalizer<MessageService> localizer) => _localizer = localizer;

        public string HomeBadgeTitle => _localizer["Home_BadgeTitle"];
        public string HomeMainMessage => _localizer["Home_MainMessage"];
        public string HomeMethodOneBody => _localizer["Home_MethodOne_Body"];
        public string HomeMethodOneTitle => _localizer["Home_MethodOneTitle"];
        public string InfoDonate => _localizer["Info_Donate"];
        public string InfoMadeBy=> _localizer["Info_MadeBy"];
        public string MethodOneStep1Title => _localizer["MethodOne_Step1Title"];
        public string MethodOneStep2Title => _localizer["MethodOne_Step2Title"];
        public string MethodOneStep2Body=> _localizer["MethodOne_Step2Body"];
        public string MethodOneStep3Title => _localizer["MethodOne_Step3Title"];
        public string MethodOneStep3InputFileNameLabel => _localizer["MethodOne_Step3InputFileNameLabel"];
        public string MethodOneStep3InputPathLabel => _localizer["MethodOne_Step3InputPathLabel"];
        public string MethodOneStep3Options1 => _localizer["MethodOne_Step3Options1"];
        public string Sidebar_Home => _localizer["Sidebar_Home"];
        public string Sidebar_Settings => _localizer["Sidebar_Settings"];

        [return:NotNullIfNotNull(nameof(_localizer))]
        public string? GetHomeObjective()
        {
            LocalizedString localizedString = _localizer["Home_Objective"];
            return localizedString;
        }
    }
}
