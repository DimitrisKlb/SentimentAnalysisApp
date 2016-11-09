using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using SentimentAnalysisApp.Models;

/*
namespace SentimentAnalysisApp.Controllers
{
    public class MinedTextsController : ApiController
    {
        private MinedDataContext db = new MinedDataContext();

        // GET: api/MinedTexts
        public IQueryable<MinedText> GetMinedTexts()
        {
            return db.MinedTexts;
        }

        // GET: api/MinedTexts/5
        [ResponseType(typeof(MinedText))]
        public async Task<IHttpActionResult> GetMinedText(int id)
        {
            MinedText minedText = await db.MinedTexts.FindAsync(id);
            if (minedText == null)
            {
                return NotFound();
            }

            return Ok(minedText);
        }

        // PUT: api/MinedTexts/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutMinedText(int id, MinedText minedText)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != minedText.ID)
            {
                return BadRequest();
            }

            db.Entry(minedText).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MinedTextExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/MinedTexts
        [ResponseType(typeof(MinedText))]
        public async Task<IHttpActionResult> PostMinedText(MinedText minedText)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.MinedTexts.Add(minedText);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = minedText.ID }, minedText);
        }

        // DELETE: api/MinedTexts/5
        [ResponseType(typeof(MinedText))]
        public async Task<IHttpActionResult> DeleteMinedText(int id)
        {
            MinedText minedText = await db.MinedTexts.FindAsync(id);
            if (minedText == null)
            {
                return NotFound();
            }

            db.MinedTexts.Remove(minedText);
            await db.SaveChangesAsync();

            return Ok(minedText);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MinedTextExists(int id)
        {
            return db.MinedTexts.Count(e => e.ID == id) > 0;
        }
    }
}
*/