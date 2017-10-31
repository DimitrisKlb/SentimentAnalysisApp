using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace TextAnalysers {
    public class SWNDictionary {

        private class Score {
            public double ScorePositive { get; }
            public double ScoreNegative { get; }

            public Score(double scorePos = 0, double scoreNeg = 0) {
                ScorePositive = scorePos;
                ScoreNegative = scoreNeg;
            }
        }

        private Dictionary<string, Score> theDictionary;

        public SWNDictionary(string pathToSWNfile) {
            Dictionary<string, Dictionary<int, Score>> tempDictionary = new Dictionary<string, Dictionary<int, Score>>(); ;

            string line;
            int lineNumber = 0;
            StreamReader file = new StreamReader( pathToSWNfile );

            while((line = file.ReadLine()) != null) {
                if(line.Trim().StartsWith( "#" ) != true) {
                    string[] lineParts = line.Split( '\t' );

                    // Invalid Line Format
                    if(lineParts.Length != 6) {
                        throw new ArgumentException( $"Incorrect format in file, line {lineNumber}" );
                    }

                    // Get Scores
                    double scorePos = double.Parse( lineParts[2], CultureInfo.InvariantCulture );
                    double scoreNeg = double.Parse( lineParts[3], CultureInfo.InvariantCulture );

                    // For every term that belongs to this synset
                    string[] synTermsSplit = lineParts[4].Split( ' ' );
                    foreach(string synTermSplit in synTermsSplit) {
                        string[] synTermAndRank = synTermSplit.Split( '#' );
                        string synTerm = synTermAndRank[0] + "#" + lineParts[0];
                        int rank = int.Parse( synTermAndRank[1] );

                        // Add synTerm to the dictionary if doesn't already contain it
                        if(tempDictionary.ContainsKey( synTerm ) == false) {
                            tempDictionary.Add( synTerm, new Dictionary<int, Score>() );
                        }

                        Dictionary<int, Score> scoresDictionary;
                        tempDictionary.TryGetValue( synTerm, out scoresDictionary );
                        scoresDictionary.Add( rank, new Score( scorePos, scoreNeg ) );
                    }
                    //Console.WriteLine( $"{lineParts[0]} - {lineParts[1]}" );
                }

                lineNumber++;
            }
            file.Close();

            // Create the actual dictionary
            theDictionary = new Dictionary<string, Score>();

            // Iterate through every synTerm in the dictionary
            foreach(string synTerm in tempDictionary.Keys) {
                Dictionary<int, Score> scoresDictionary;
                tempDictionary.TryGetValue( synTerm, out scoresDictionary );

                double scorePosSum = 0.0;
                double scoreNegSum = 0.0;
                double sum = 0.0;
                foreach(int rank in scoresDictionary.Keys) {
                    Score theScore;
                    scoresDictionary.TryGetValue( rank, out theScore );
                    scorePosSum += theScore.ScorePositive / (double)rank;
                    scoreNegSum += theScore.ScoreNegative / (double)rank;
                    sum += 1.0 / rank;
                }
                scorePosSum /= sum;
                scoreNegSum /= sum;
                theDictionary.Add( synTerm, new Score( scorePosSum, scoreNegSum ) );
            }
        }

        public void GetScore(string synTerm, string synTermPOS, out float scorePositive, out float scoreNegative) {
            Score score;
            theDictionary.TryGetValue( synTerm + "#" + synTermPOS, out score );
            if(score != null) {
                scorePositive = (float)score.ScorePositive;
                scoreNegative = (float)score.ScoreNegative;
            } else {
                scorePositive = 0;
                scoreNegative = 0;
            }
        }

    }

}
