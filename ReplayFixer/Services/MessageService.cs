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

        [return:NotNullIfNotNull(nameof(_localizer))]
        public string? GetHomeObjective()
        {
            LocalizedString localizedString = _localizer["Home_Objective"];
            return localizedString;
        }
    }
}
