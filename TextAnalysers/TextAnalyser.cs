using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextAnalysers {

    public enum SentiClass: short {
        Negative,
        Neutral,
        Positive
    }

    public abstract class TextAnalyser {
        public abstract SentiClass classifyText(string text);
    }

}
