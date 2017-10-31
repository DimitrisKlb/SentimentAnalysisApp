using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using java.util;
using java.io;

using edu.stanford.nlp.ling;
using edu.stanford.nlp.pipeline;

using TextAnalysers;

namespace TextAnalysers {
    public class SWNAnalyser: TextAnalyser {
        StanfordCoreNLP thePipeline; // The Stanford's NLP pipeline
        SWNDictionary theSWNDictionary; // The SentiWordNet dictionary
        float neutralityBreakpoint = 0.15F;

        public SWNAnalyser(string SNLP_Path, string SWN_Path) {
            var props = new Properties();
            props.setProperty( "annotators", "tokenize, ssplit, pos, lemma" );
            var curDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory( SNLP_Path );
            thePipeline = new StanfordCoreNLP( props );
            Directory.SetCurrentDirectory( curDir );

            theSWNDictionary = new SWNDictionary( Path.Combine( SWN_Path, "SentiWordNet_3.0.0_20130122.txt" ) );
        }

        public override SentiClass classifyText(string text) {
            float posScore, negScore, finalScore;
            analyseText( text, out posScore, out negScore );
            finalScore = posScore - negScore;
            if(finalScore > -neutralityBreakpoint && finalScore < neutralityBreakpoint) {
                return SentiClass.Neutral;
            }
            return (finalScore > 0) ? SentiClass.Positive : SentiClass.Negative;
        }

        public void analyseText(string text, out float posScoreSum, out float negScoreSum) {
            float posScore, negScore;
            var annotation = new Annotation( text );
            thePipeline.annotate( annotation );

            posScoreSum = 0;
            negScoreSum = 0;
            var sentences = annotation.get( typeof( CoreAnnotations.SentencesAnnotation ) );
            foreach(Annotation sentence in sentences as ArrayList) {

                var tokens = sentence.get( typeof( CoreAnnotations.TokensAnnotation ) );
                foreach(CoreLabel token in tokens as ArrayList) {
                    string word = token.get( typeof( CoreAnnotations.TextAnnotation ) ).ToString();
                    string pos = token.get( typeof( CoreAnnotations.PartOfSpeechAnnotation ) ).ToString();
                    string wordLemma = token.get( typeof( CoreAnnotations.LemmaAnnotation ) ).ToString();

                    var basicPOS = getBasicPOS( pos );
                    if(basicPOS != null) {
                        theSWNDictionary.GetScore( wordLemma, basicPOS, out posScore, out negScore );
                        posScoreSum += posScore;
                        negScoreSum += negScore;
                    }
                }
            }

        }


        private static string getBasicPOS(string pos) {
            string result = null;
            List<string> nounPOSes = new List<string> { "NN", "NNP", "NNPS", "NNS" };
            List<string> adjectivePOSes = new List<string> { "JJ", "JJR", "JJS" };
            List<string> verbPOSes = new List<string> { "VB", "VBD", "VBG", "VBN", "VBP", "VBZ" };
            List<string> adverbPOSes = new List<string> { "RB", "RBR", "RBS", "RP" };

            if(nounPOSes.Contains( pos )) {
                result = "n";
            }
            if(adjectivePOSes.Contains( pos )) {
                result = "a";
            }
            if(verbPOSes.Contains( pos )) {
                result = "v";
            }
            if(adverbPOSes.Contains( pos )) {
                result = "r";
            }
            return result;
        }

    }

}
