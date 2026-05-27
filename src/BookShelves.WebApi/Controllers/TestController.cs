using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Identity.Web.Resource;

namespace BookShelves.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    public class TestController : ControllerBase
    {
        [HttpGet(Name = "GetTest")]
        //[RequiredScopeOrAppPermission()]
        public IEnumerable<string> Test()
        {
            return new string[] { "Test1", "Test2", "Test3" };
        }
    }
}
