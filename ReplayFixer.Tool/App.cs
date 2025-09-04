using Microsoft.Extensions.Logging;
using ReplayFixer.Library.Serializers;
using ReplayFixer.Models.Deserializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplayFixer.Tool
{
    public class App
    {

        private readonly ILogger _logger;
        public App(ILogger<App> logger)
        {
            _logger = logger;
        }
        public void Run(string[] args)
        {
            string filePath = "";
            string outputFileName = "replay.json";

            if (args.Length < 1)
            {
                _logger.LogError("You must provide a file path to a replay file as the first argument");
                return;
            }

            filePath = args[0];
            if (args.Length > 1)
                outputFileName = args[1];


            /*for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--filePath="))
                {
                    filePath = args[i].Substring(11);
                }
                if (args[i].StartsWith("--delimter="))
                {
                    delimiter = Convert.ToByte(args[i].Substring(11));
                }
                if (args[i].StartsWith("--outputFileName="))
                {
                    outputFileName = args[i].Substring(17);
                }
            }*/

            _logger.LogInformation("Starting ReplayFixer.Tool");

            var replay = new Library.Models.Data.Replay();
            using (FileStream stream = File.Open(filePath, FileMode.Open))
            {
                replay = ReplayDeserializer.FromStream(stream);
            }
            JsonSerialization.WriteToJsonFile(outputFileName, replay);
            _logger.LogInformation(replay?.ToString());
            _logger.LogInformation("Output this information to {outputFileName} which is a JSON file", outputFileName);
        }
    }
}
