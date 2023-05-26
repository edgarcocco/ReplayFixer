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
        public int ID { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileDirectory { get; set; } = string.Empty;
        public string MapName { get; set; } = string.Empty;
        public string MapNameCleaned => MapName.Replace("?", string.Empty);
        public string WorkshopFileName { get; set; } = string.Empty;
        public string WorkshopFileFullPath { get; set; } = string.Empty;
        public string GameDate { get; set; } = string.Empty;
        public string FixerBytes { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Map Name: {MapNameCleaned}\n" +
                   $"Workshop File Name: {WorkshopFileName}";
        }
    }
}
