using Microsoft.AspNetCore.Mvc;

namespace IPSB.Controllers
{

    [ApiController]
    public abstract class AuthorizeController : ControllerBase
    {
        protected abstract bool IsAuthorize();
    }
}
