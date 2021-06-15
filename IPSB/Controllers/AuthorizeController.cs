﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.Controllers
{

    [ApiController]
    public abstract class AuthorizeController : ControllerBase
    {
        protected abstract bool IsAuthorize();
    }
}
