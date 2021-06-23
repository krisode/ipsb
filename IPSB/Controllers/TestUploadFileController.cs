﻿using IPSB.ExternalServices;
using IPSB.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IPSB.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestUploadFileController : ControllerBase
    {
        private readonly IUploadFileService _uploadFileService;
        public TestUploadFileController(IUploadFileService uploadFileService)
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
        public async Task<ActionResult> TestPost([FromForm] TestModel test)
        {
            string imageUrl = await _uploadFileService.UploadFile("123456798", test.File, "test", "test-detail");
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
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
