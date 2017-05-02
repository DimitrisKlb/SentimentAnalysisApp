using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;

using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

using SentimentAnalysisApp.SharedModels;
using WSP.Models;
using WSP.MasterActor.Interfaces;
using WSP.DBHandlerService.Interfaces;
using Microsoft.ServiceFabric.Services.Client;

namespace WSP.WebAPI.Controllers {
    public class ServiceController: ApiController {
        private BESearchRequestsController SReqController = new BESearchRequestsController();
        private static IDBHandlerService dbHandlerService = ServiceProxy.Create<IDBHandlerService>(
                new Uri( "fabric:/WebServiceProvider/DBHandlerService" ),
                new ServicePartitionKey( 1 ) );

        // POST: api/Service
        [ResponseType( typeof( BaseSearchRequest ) )]
        public async Task<IHttpActionResult> PostSearchRequest(BaseSearchRequest baseSearchRequest) {
            if(!ModelState.IsValid) {
                return BadRequest( ModelState );
            }
            BESearchRequest createdSearchRequest;
            BESearchRequest newBESearchRequest = new BESearchRequest( baseSearchRequest );
          
            // Store the new Search Request to the Database            
            try {
                createdSearchRequest = await dbHandlerService.StoreBESearchRequest( newBESearchRequest );
            } catch {
                return InternalServerError();
            }

            IMasterActor theMasterActor = ActorProxy.Create<IMasterActor>( new ActorId( createdSearchRequest.ID ) );
            try {
                await theMasterActor.FulfillSearchRequestAsync( createdSearchRequest );                
            } catch(InvalidOperationException) {
                return InternalServerError();
            } catch(Exception ex) {
                return InternalServerError();
            }

            return Ok();
        }

    }
}