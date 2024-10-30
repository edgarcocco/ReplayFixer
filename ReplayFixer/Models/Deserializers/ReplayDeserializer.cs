using ReplayFixer.Extensions;
using ReplayFixer.Models.Data;
using ReplayFixer.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReplayFixer.Models.Deserializers
{
    public static class ReplayDeserializer
    {
        public static Replay FromStream(Stream stream)
        {
            return FromStream(stream, 0x24);
        }

        public static Replay FromStream(Stream stream, byte byteDelimiter)
        {
            // start by trying to get the file name trying to cast the stream to filestream

            string fileName = string.Empty;
            string fileDirectory = string.Empty;
            if (stream is FileStream fileStream)
            {
                var fileNameSplitted = fileStream.Name.Split('\\');
                fileName = fileNameSplitted.Last();
                fileDirectory = String.Join('\\', fileNameSplitted, 0, fileNameSplitted.Length - 1);
            }

            using (BinaryReader reader = new BinaryReader(stream))
            {
                byte[] buffer = reader.ReadBytes((int)stream.Length);
                // delimiter $ is used to separate chunks of data
                //IEnumerable<byte> delimiter = new byte[] { 0x24 };
                // delimiter * is used to separate chunks of data
                // changed to this delimiter
                IEnumerable<byte> delimiter = new byte[] { byteDelimiter };

                //IEnumerable<byte> bufferCleaned = buffer.Select(x => x).Where(x => x != 0);

                // we copy the buffer of the replay file to initialize a queue collection
                // this way as we read data from the list we remove them from it at the same time.
                // note: we could also remove the nulls from the buffer but we would lose data and positioning.
                //       remove nulls: buffer.Select(x => x).Where(x => x != 0));
                // also note: if we remove nulls the length of a chunk to read will be the amount of bytes to read with nulls divided by 2
                //            for example to get the date you might require to read 36 bytes with nulls included without them 36/2
                Queue<byte> bufferQueued = new Queue<byte>(buffer);
                //IEnumerable<byte> bufferQueuedCleaned = bufferQueued.Select(x => x).Where(x => x != 0);
                // first 4 bytes are for file version
                byte[] fileVersion = bufferQueued.DequeueChunk(4).ToArray();

                // verify we are dealing with a COH 1 replay by dequeing 8 bytes pertaining to the file type.
                byte[] replayHandshake = bufferQueued.DequeueChunk(8).ToArray();

                // let's get that handshake!
                if (replayHandshake.SequenceEqual(COH_IDS.COH__REC))
                {
                    // getting the date is a bit tricky, because there time may be of 24 hour format or 12 hours
                    // first try to get it with meridian
                    //var gameDate = new DateTime();
                    var gameDate = string.Empty;
                    var ci = new CultureInfo("en-US");
                    var formats = new[] { "dd/MM/yyyy HH:mm", "M-d-yyyy", "dd-MM-yyyy", "MM-dd-yyyy", "M.d.yyyy", "dd.MM.yyyy", "MM.dd.yyyy", "d/M/yyyy HH:MM", "d/M/yyyy H:m" }
                            .Union(ci.DateTimeFormat.GetAllDateTimePatterns()).ToArray();

                    var postMeridiemIndex = HelperMethods.SearchFirst(bufferQueued.ToArray(), COH_IDS.PM);
                    var anteMeridiemIndex = HelperMethods.SearchFirst(bufferQueued.ToArray(), COH_IDS.AM);
                    // what if date is not in a meridian format?

                    var validMeridiemPosition = 0;
                    // check if post meridiem and ante meridiem are less than 64 bytes
                    // the problem is that there could be another date stored in the file
                    // and surely the date we want to get is not after 64 bytes, it has to be closer that's how relic chunky files works
                    if (postMeridiemIndex is > 0 and < 64)
                        validMeridiemPosition = postMeridiemIndex;
                    else if (anteMeridiemIndex is > 0 and < 64)
                        validMeridiemPosition = anteMeridiemIndex;
                    if (validMeridiemPosition > 0)
                    {
                        // get position until meridiem then grab the 3 bytes pertaining to post meridiem or ante meridiem with null in between the bytes
                        /*gameDate = DateTime.Parse(Encoding.ASCII.GetString(bufferQueued
                                .DequeueChunk(validMeridiemPosition + 3)
                                .RemoveByteNulls()
                                .ToArray()));*/
                        gameDate = Encoding.ASCII.GetString(bufferQueued
                                                  .DequeueChunk(validMeridiemPosition + 3)
                                                  .RemoveByteNulls()
                                                  .ToArray());
                    }
                    else
                    {
                        // if this fails lets just get the next 32 bytes corresponding to the date
                        /*gameDate = DateTime.ParseExact(Encoding.ASCII.GetString(bufferQueued.DequeueChunk(32)
                            .RemoveByteNulls()
                            .ToArray()), formats, ci);*/

                        gameDate = Encoding.ASCII.GetString(bufferQueued.DequeueChunk(32)
                            .RemoveByteNulls()
                            .ToArray());

                    }

                    // whenever there is a _ (discard) it will store unnecessary bytes


                    // next we need relic chunky and 20 bytes not human readable behind it, included
                    // so we store the position of the first relic chunky plus those 20 extra bytes behind relic chunky.
                    int relicChunkyAndExtraPosition = HelperMethods.SearchFirst(bufferQueued.ToArray(), COH_IDS.RELIC_CHUNKY);

                    // there are a couple bytes after we take the date that we don't need so we erase those based off the relic chunky
                    // we don't need to store trash
                    _ = bufferQueued.DequeueChunk(relicChunkyAndExtraPosition - 20).ToArray();

                    // we got the first chunk to be able to fix other replays now lets grab the second one
                    byte[] firstRelicChunky = bufferQueued.DequeueChunk(32)
                                                          .Concat(delimiter)
                                                          .ToArray();


                    //Helpers.SearchFirst(bufferQueued.ToArray(), COH_IDS.INT_MAX_VALUE);

                    // search the first 0xffffffff and deque the previous data until we are positioned there
                    _ = bufferQueued.DequeueChunk(HelperMethods.SearchFirst(bufferQueued.ToArray(), COH_IDS.INT_MAX_VALUE)).ToArray();
                    // for this one we just need the two bytes with values behind the second relic chunky without the nulls
                    byte[] secondaryRelicChunky = bufferQueued.DequeueChunk(12)
                                                              .Concat(delimiter)
                                                              .ToArray();

                    // next up is the DATASDSC
                    _ = bufferQueued.DequeueChunk(HelperMethods.SearchFirst(bufferQueued.ToArray(), COH_IDS.DATASDSC)).ToArray();
                    byte[] firstDataSDSC = bufferQueued.DequeueChunk(14)
                                                       .Concat(delimiter)
                                                       .ToArray();

                    // erase all the bytes before the first RelicCoH%
                    // test if it's RelicCOH or RelicCoH for some reason some replays have a lower case o instead of capital O
                    int relicCoHIndex = HelperMethods.SearchFirst(bufferQueued.ToArray(), COH_IDS.RELICCOH);
                    if (relicCoHIndex <= 0) relicCoHIndex = HelperMethods.SearchFirst(bufferQueued.ToArray(), COH_IDS.RELICCoH);
                    _ = bufferQueued.DequeueChunk(relicCoHIndex).ToArray();
                    int firstFoldInfoPosition = HelperMethods.SearchFirst(bufferQueued.ToArray(), COH_IDS.FOLDINFO);

                    // last chunk of data to extract we may have to create a queue out of this one because we need to extract data from it
                    // for that we get the index of the first foldinfo, after RelicCoH which is the chunk data that we want next
                    // and then we remove exactly 53 bytes of nulls to the index and we dequeue that length.
                    /*byte[] firstRelicCOH = bufferQueued.DequeueChunk(firstFoldInfoPosition - 53)
                                                       .Concat(delimiter)
                                                       .ToArray();*/
                    Queue<byte> firstRelicCOHQueued = new Queue<byte>(bufferQueued
                                                    .DequeueChunk(firstFoldInfoPosition - 53)
                                                    .Concat(delimiter));


                    // we have to do some inner scrape into the firstRelicCOH bytes array
                    // to get the MapName and WorkshopFileName and WorkshopFullPath
                    string firstRelicCOHDecoded = Encoding.ASCII.GetString(firstRelicCOHQueued.RemoveByteNulls().ToArray());

                    string mapName = string.Empty;
                    string workshopFileName = string.Empty;
                    string workshopFileFullPath = string.Empty;
                    // verify we are about to read RelicCOH chunk
                    if (firstRelicCOHDecoded.StartsWith(Encoding.ASCII.GetString(COH_IDS.RELICCOH)) ||
                        firstRelicCOHDecoded.StartsWith(Encoding.ASCII.GetString(COH_IDS.RELICCoH)))
                    {
                        // find the first DATA chunk
                        int firstDATAIndex = firstRelicCOHDecoded.IndexOf("DATA");

                        // we defined the index of the DATA chunk but we need a delimiter to stop reading the DATA chunk
                        var allowedASCIIChars = Enumerable.Range(48, 122).ToArray();
                        // we position the string at the start of the DATA
                        // because we need to extract the map name
                        var headerDATAChunk = firstRelicCOHDecoded.Substring(firstDATAIndex, firstRelicCOHDecoded.Length - firstDATAIndex);

                        // begin to extract the map name, make sure each character belongs to the allowed ascii chars range
                        int index = 0;
                        int readingChar = headerDATAChunk[index];
                        string headerDATAString = string.Empty;
                        while (allowedASCIIChars.Contains(readingChar))
                        {
                            //splittedDATAChunk += (char)readingChar;
                            headerDATAString += (char)(readingChar);
                            //splittedDATAChunk[index] = readingChar;
                            readingChar = headerDATAChunk[++index];
                        }
                        //char[] firstDataChunk = firstRelicCOHDecoded.TakeWhile(x => x);


                        // from here we can explode the string
                        string[] explodedHeaderDATA = headerDATAString.Split('\\');

                        // the map name is usually in the last index so lets get it from the last index
                        mapName = explodedHeaderDATA.Last();
                    }

                    // now we have to get the workshop file name and workshop file full path
                    // we know that the directory will always start
                    // to get the workshop file and full path we assume the user has his workshops map in a folder under \Users\
                    // so we look up for the index of that folder in the RelicCoH chunk
                    var usersIndex = HelperMethods.SearchFirst(firstRelicCOHQueued.ToArray(), COH_IDS.USERS);

                    // now we need to store 4 bytes behind C:\Users these 4 bytes will help fix the replay as well
                    var bytesBehindDiskDriveIndex = (usersIndex - 4) - 4;
                    // remove all the bytes behind the data that we want to extract next
                    _ = firstRelicCOHQueued.DequeueChunk(bytesBehindDiskDriveIndex).ToArray();
                    byte[] bytesBehindDiskDrive = firstRelicCOHQueued.DequeueChunk(4).Concat(delimiter).ToArray();
                    // now that we have all the bytes for fixing other replays
                    // we can now get the workshop file name and also the workshop full path directory
                    byte[] workshopFileFullPathBuffer = firstRelicCOHQueued.ToArray();
                    workshopFileFullPath = Encoding.ASCII.GetString(firstRelicCOHQueued.RemoveByteNulls().ToArray());
                    // we can extract the workshop file name by using a simple digit regex that ends in a .sga extension
                    Match match = Regex.Match(workshopFileFullPath, HelperMethods.ValidSGAExpression);//"(([0-9]+)|([a-zA-Z ]+)).sga");
                    if (match.Success)
                        workshopFileName = match.Value;
                    Replay replay = new Replay()
                    {
                        FileName = fileName,
                        FileDirectory = fileDirectory,
                        GameDate = gameDate,
                        MapName = mapName,
                        WorkshopFileName = workshopFileName,
                        WorkshopFileFullPath = workshopFileFullPath,
                        FixerBytes = Convert.ToBase64String(firstRelicChunky
                                                                  .Concat(secondaryRelicChunky)
                                                                  .Concat(firstDataSDSC)
                                                                  .Concat(bytesBehindDiskDrive)
                                                                  .Concat(workshopFileFullPathBuffer)
                                                                  .ToArray())
                    };
                    return replay;
                }
                else
                {
                    //return new Replay();
                    throw new Exception("PARSER: Handshake failed most obvious reason is when trying to load a corrupted replay file.");
                }
                //string str = Convert.ToBase64String(buffer);

            }
        }
    }
}
