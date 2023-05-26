using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf.Ui.Appearance;

namespace ReplayFixer
{
    public class AppConfig
    {
        public string? Theme { get; set; }
        public string? Language { get; set; }
        public string? ReplayDatabase { get; set; }
        public string? GithubPat { get; set; }
        public string? AutoUpdaterFile { get; set; }
    }
}
