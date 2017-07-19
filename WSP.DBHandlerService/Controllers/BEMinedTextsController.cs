using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SentimentAnalysisApp.SharedModels;
using WSP.Models;
using System.Data.Entity.Migrations;

namespace WSP.DBHandlerService {
    public class BEMinedTextsController {

        public async Task PostBEMinedTexts(IEnumerable<BEMinedText> newBEMinedTexts) {
            using(var db = new BEMainDBContext()) {
                db.BEMinedTexts.AddRange( newBEMinedTexts );
                await db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<BEMinedText>> GetBEMinedTexts(int executionID, SourceOption source, TextStatus status, int windowSize) {
            IEnumerable<BEMinedText> theTexts = null;
            using(var db = new BEMainDBContext()) {
                theTexts = db.BEMinedTexts
                            .Where( mt => (mt.ExecutionID == executionID) &&
                                            (mt.TheSource == source) &&
                                            (mt.TheStatus == status) )
                            .ToList()
                            .Take( windowSize );
            }
            return theTexts;
        }

        public async Task<int> GetBEMinedTextsCount(int executionID, SourceOption source, TextStatus status) {
            int textsCount = 0;
            using(var db = new BEMainDBContext()) {
                textsCount = db.BEMinedTexts
                            .Where( mt => (mt.ExecutionID == executionID) &&
                                            (mt.TheSource == source) &&
                                            (mt.TheStatus == status) )
                            .Count();
            }
            return textsCount;
        }

        public async Task UpdateBEMinedTexts(IEnumerable<BEMinedText> updatedTexts) {
            using(var db = new BEMainDBContext()) {
                foreach(BEMinedText text in updatedTexts) {
                    db.BEMinedTexts.AddOrUpdate( text );
                }
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateBEMinedTextsStatus(IEnumerable<int> IDs, TextStatus newStatus) {
            using(var db = new BEMainDBContext()) {
                db.BEMinedTexts
                .Where( mt => IDs.Contains( mt.ID ) )
                .ToList()
                .ForEach( mt => mt.TheStatus = newStatus );

                await db.SaveChangesAsync();
            }
        }

    }
}