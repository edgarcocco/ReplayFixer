using ReplayFixer.Models.Data;
using ReplayFixer.Models.Deserializers;
using Xunit;

namespace ReplayFixer.Tests
{
    public class ReplayFileUnitTest
    {
        [Fact]
        public void Replay_RawHeaderSection_Convert64()
        {
            
        }

        [Fact]
        public void Compare_Replay_RawHeaderSection_Length()
        {
            string cohPlaybacks = @"C:\Users\edgar\Documents\My Games\Company of Heroes Relaunch\playback";
            Replay replay1 = new Replay();
            Replay replay2 = new Replay();

            using (FileStream stream = File.Open(cohPlaybacks + @"\OBSERVER2_STURZDORF.2022-10-21.20-22-56-first-method-no-vscode-testing.rec", FileMode.Open))
            {
                replay1 = ReplayDeserializer.FromStream(stream);
            }
            using (FileStream stream = File.Open(cohPlaybacks + @"\OBSERVER2_STURZDORF.2022-02-07.21-50-33.rec", FileMode.Open))
            {
                replay2 = ReplayDeserializer.FromStream(stream);
            }

            Assert.Equal(replay1.FixerBytes.Length, replay2.FixerBytes.Length);
        }

    }
}