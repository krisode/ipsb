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

        // GET: api/<TestUploadFileController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<TestUploadFileController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<TestUploadFileController>
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

        // PUT api/<TestUploadFileController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<TestUploadFileController>/5
        [HttpDelete]
        public void Delete([FromBody] UploadFileDM model)
        {

        }
    }
}
