using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

using SentimentAnalysisApp.SharedModels;
using WSP.Models;
using WSP.MasterActor.Interfaces;
using WSP.DBHandlerService.Interfaces;

namespace WSP.WebAPI.Controllers {
    public class ServiceController: ApiController {       
        private static IDBHandlerService dbHandlerService = ServiceProxy.Create<IDBHandlerService>(
                new Uri( "fabric:/WebServiceProvider/DBHandlerService" ),
                new ServicePartitionKey( 1 ) );

        [Route( "Service/Submit" )]
        [HttpPost]
        [ResponseType( typeof( BaseSearchRequest ) )]
        public async Task<IHttpActionResult> SubmitSearchRequest(BaseSearchRequest baseSearchRequest) {
            if(!ModelState.IsValid) {
                return BadRequest( ModelState );
            }
            BESearchRequest createdSearchRequest;
            BESearchRequest newBESearchRequest = new BESearchRequest( baseSearchRequest );
            newBESearchRequest.TheStatus = Status.Mining_Done;

            // Store the new Search Request to the Database            
            try {
                //createdSearchRequest = await dbHandlerService.StoreBESearchRequest( newBESearchRequest );
                createdSearchRequest = newBESearchRequest;
            } catch {
                return InternalServerError();
            }

            IMasterActor theMasterActor = ActorProxy.Create<IMasterActor>( new ActorId( createdSearchRequest.ID ) );
            try {
                await theMasterActor.FulfillSearchRequestAsync( createdSearchRequest );
            } catch(InvalidOperationException) {
                return InternalServerError();
            } catch {
                return InternalServerError();
            }

            return Ok();
        }

    }
}