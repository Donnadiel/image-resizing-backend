using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Stark_induestries.Entity;
using Stark_induestries.Service;

namespace Stark_induestries.Controllers
{ 
    [ApiController]
    [Route("api/images")]
    public class ImageController : ControllerBase
    {
        
        private readonly CloudService _cloud;
        private readonly ImageQueueService _queue;


        public ImageController(
             CloudService cloud, ImageQueueService queue)
        {
            _cloud = cloud;
            _queue = queue;
        }
        
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("A valid image file is required.");
            }

            var basename = await _cloud.UploadImage(file);

            var entry = new ImageEntry();

            entry.Identifier = basename;
            
            await _queue.Enqueue(entry, _cloud);
            
            
            return Ok(new
            {
                entry.Index,
                entry.Identifier
            });
        }


        [HttpGet("{index:int}/{size}")]
        public async Task<IActionResult> GetByIndex(int index, string size)
        {
            
            ImageEntry? entry = _queue.GetByIndex(index);
            
            if (entry == null)
            {
                return NotFound($"No image found at index {index}.");
            }

            //Retrieve from cloud
            var url = await _cloud.GetImageUrl(entry.Identifier, size);
            
            
            var response = new { entry.Index, entry.Identifier, size, url };
            var json = JsonSerializer.Serialize(response); ;
            return Content(json, "application/json");
        }
        
    }
    
}