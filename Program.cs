using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculateMean {
   class Program {
      static void Main(string[] args) {
         // Parameters validation.
         string iFile, oFile;
         int timeInterval;

         if (args.Length != 3) {
            Console.WriteLine("Missing parameters");
            return;
         } else if (!Int32.TryParse(args[2], out timeInterval)) {
            Console.WriteLine("Not an integer.");
            return;
         }

         iFile = args[0];
         oFile = args[1];

         // Process the data.
         using (TextFieldParser csvParser = new TextFieldParser(iFile)) {
            using (StreamWriter csvWriter = new StreamWriter(oFile)) {
               // Create a csv parser.
               csvParser.TextFieldType = FieldType.Delimited;
               csvParser.SetDelimiters(",");
               csvParser.ReadLine(); // To get rid of the headers.

               // Create the header row for the output file.
               csvWriter.WriteLine("Start Stamp,End Stamp,Number of Values,Mean");

               int timeEnd = -1; // Initialize to -1 so that the first value is considered as setting the initial time range.
               double accumulator = 0; // Accumulates the values for a particular time range.
               int valuesInInterval = 0; // Keeps track of the amount of data points per time range.
               double aDouble;

               while (!csvParser.EndOfData) {
                  // First field is the time.
                  string[] fields = csvParser.ReadFields();
                  int timeStamp = timeStampFromString(fields[0]);

                  if (timeStamp >= 0) {

                     // If the current time is at or after the current time range...
                     if (timeStamp >= timeEnd) {
                        if (valuesInInterval > 0) {
                           aDouble = accumulator / (double) valuesInInterval;
                           csvWriter.WriteLine(
                              timeStampFromInt(timeEnd - timeInterval) + "," +
                              timeStampFromInt(timeEnd - 1) + "," +
                              valuesInInterval.ToString() + "," +
                              aDouble.ToString()
                           );
                           accumulator = 0;
                           valuesInInterval = 0;
                        }

                        // If the current time skips at least one time range forward or if this is
                        // the first run then move the time range forward to the new beginning.
                        if ((timeStamp - timeEnd) >= timeInterval || timeEnd < 0) {
                           timeEnd = timeStamp;
                        }
                        // Move forward the new end for the time range.
                        timeEnd += timeInterval;
                     }

                     if (Double.TryParse(fields[1], out aDouble)) {
                        accumulator += aDouble;
                        valuesInInterval++;
                     }
                  }
               }
               // Because a time range is calculated after we encounter a data point from a new
               // time range we need to do a final calculation for the last time range.
               if (valuesInInterval > 0) {
                  aDouble = accumulator / (double)valuesInInterval;
                  csvWriter.WriteLine(
                     timeStampFromInt(timeEnd - timeInterval) + "," +
                     timeStampFromInt(timeEnd - 1) + "," +
                     valuesInInterval.ToString() + "," +
                     aDouble.ToString()
                  );
               }
            }
         }
      }

      // Converts a time given as a string expressed in the format HH:mm:ss to an integer that represents the
      // amount of seconds since the start of the day.
      static private int timeStampFromString(string timeStamp) {
         int result = -1, timeHours, timeMinutes, timeSeconds;
         string[] timeSections = timeStamp.Split(':');

         if (timeSections.Length >= 3 && Int32.TryParse(timeSections[0], out timeHours)
             && Int32.TryParse(timeSections[1], out timeMinutes) && Int32.TryParse(timeSections[2], out timeSeconds)) {

            result = timeSeconds + (60 * timeMinutes) + (60 * 60 * timeHours);
         }
         return result;
      }

      // Converts a time given as an integer that represents the amount of seconds since the start of the day
      // to a string expressed in the format HH:mm:ss.
      static private string timeStampFromInt(int timeStamp) {
         int timeHours = timeStamp / (60 * 60),
             timeMinutes = (timeStamp % (60 * 60)) / 60,
             timeSeconds = (timeStamp % (60 * 60)) % 60;
         return (timeHours.ToString() + ":" + timeMinutes.ToString("D2") + ":" + timeSeconds.ToString("D2"));
      }
   }
}
