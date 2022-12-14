using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplayFixer.Models.Data
{
    [DelimitedRecord("|")]
    [IgnoreEmptyLines]
    public class Replay
    {
        [FieldHidden]
        public int ID { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileDirectory { get; set; } = string.Empty;
        public string MapName { get; set; } = string.Empty;
        public string WorkshopFileName { get; set; } = string.Empty;
        public string WorkshopFileFullPath { get; set; } = string.Empty;

        [FieldConverter(ConverterKind.Date, "MMddyyyy")]
        public DateTime GameDate{ get; set; }
        public string FixerBytes { get; set; } = string.Empty;
    }
}
