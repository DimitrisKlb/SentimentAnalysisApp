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

        //[Route( "Service/Ping" )]
        [HttpGet]
        public string Get() {
            return "OK";
        }

        //[Route( "Service/Submit" )]
        [HttpPost]
        [ResponseType( typeof( BaseSearchRequest ) )]
        public async Task<IHttpActionResult> PostSearchRequest(BaseSearchRequest baseSearchRequest) {
             if(!ModelState.IsValid) {
                return BadRequest( ModelState );
            }
            BESearchRequest theSearchRequest;
            BESearchRequest newBESearchRequest = new BESearchRequest( baseSearchRequest );
            try {
                theSearchRequest = await dbHandlerService.StoreOrUpdateSearchRequest( newBESearchRequest);
                IMasterActor theMasterActor = ActorProxy.Create<IMasterActor>( new ActorId( theSearchRequest.ID ) );
                await theMasterActor.FulfillSearchRequestAsync( theSearchRequest );
            } catch {
                return InternalServerError();
            }

            return Ok();
        }



    }
}