using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace SentimentAnalysisApp.SharedModels {

    [Flags]
    public enum SourceOption: short {
        Twitter = 1,
        Facebook = 2
    }

    [DataContract]
    public class MiningSource {

        public static List<SourceOption> AllSources() {
            return Enum.GetValues( typeof( SourceOption ) ).Cast<SourceOption>().ToList();
        }

        [DataMember]
        public SourceOption TheSelection { get; set; }

        public MiningSource() {
            TheSelection = 0;
        }
        public MiningSource(SourceOption theSelection = 0) {
            TheSelection = theSelection;
        }
        public MiningSource(MiningSource miningSource) {
            TheSelection = miningSource.TheSelection;
        }

        public void CopyFrom(MiningSource source) {
            TheSelection = source.TheSelection;
        }

        public List<SourceOption> GetAsList() {
            List<SourceOption> selectionAsList = new List<SourceOption>();
            foreach(var option in AllSources()) {
                if(TheSelection.HasFlag( option )) {
                    selectionAsList.Add( option );
                }
            }
            return selectionAsList;
        }

        public void SetFromList(List<SourceOption> theList) {
            TheSelection = 0;
            foreach(var option in theList) {
                TheSelection |= option;
            }
        }

        public void RemoveSource(SourceOption option) {
            TheSelection &= ~option;
        }

        public bool IsEmpty() {
            return (TheSelection == 0) ? true : false;
        }
       

    }
}