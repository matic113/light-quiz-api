using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace light_quiz_api.Controllers
{
    [Route("api/images")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly FileBlobService _blobService;
        public ImagesController(FileBlobService blobService)
        {
            _blobService = blobService;
        }
        [HttpPost("upload")]
        public IActionResult UploadImage()
        { 
            var fileName = Guid.NewGuid().ToString() + ".jpg";
            var uploadUri = _blobService.GeneratePictureUploadSasUri(fileName);

            return Ok(new { uploadUri = uploadUri.ToString() });
        }
    }
}
