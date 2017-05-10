using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;

namespace SentimentAnalysisApp.SharedModels {

    [Flags]
    public enum SourceOption: short {
        Twitter = 1,
        Facebook = 2,
        Other = 4
    }

    [DataContract]
    public class MiningSource {

        public static List<SourceOption> AllSources() {
            return Enum.GetValues( typeof( SourceOption ) ).Cast<SourceOption>().ToList();
        }

        [Key]
        [DataMember]
        public int ID { get; set; }

        [Required]
        [DataMember]
        public SourceOption TheSelection { get; set; }

        public List<SourceOption> AsList() {
            List<SourceOption> selectionAsList = new List<SourceOption>();
            foreach(var option in AllSources()) {
                if(TheSelection.HasFlag( option )) {
                    selectionAsList.Add( option );
                }
            }
            return selectionAsList;
        }

        public void SetSelectionFromList(List<SourceOption> theList) {
            TheSelection = 0;
            foreach(var option in theList) {
                TheSelection = TheSelection | option;
            }
        }

        public MiningSource(SourceOption TheSelection = 0) {
            this.TheSelection = TheSelection;
        }
        public MiningSource(MiningSource miningSource) {
            this.TheSelection = miningSource.TheSelection;
        }

    }
}