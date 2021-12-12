using IPSB.ExternalServices;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IPSB.Controllers
{
    [Route("api/v1.0/upload-files")]
    [ApiController]
    public class UploadFileController : ControllerBase
    {
        private readonly IUploadFileService _uploadFileService;
        public UploadFileController(IUploadFileService uploadFileService)
        {
            _uploadFileService = uploadFileService;
        }

        // POST api/<UploadFileController>
        [HttpPost]
        public async Task<ActionResult> PostFile([FromForm] UploadFileCM model)
        {
            if (model.File == null)
            {
                return BadRequest();
            }
            string imageUrl = null;

            try
            {
                imageUrl = await _uploadFileService.UploadFile("123456798", model.File, "temp", "temp-files");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            if (imageUrl != null)
            {
                return Ok(imageUrl);
            }
            return NotFound(imageUrl);
        }
    }
}
