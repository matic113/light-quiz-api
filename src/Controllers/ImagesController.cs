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
        /// <summary>
        /// Generates a secure upload URI for image files.
        /// </summary>
        /// <remarks>
        /// Creates a secure SAS (Shared Access Signature) URI that allows direct upload
        /// of image files to Azure Blob Storage. Returns a unique upload endpoint.
        /// </remarks>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("upload")]
        public IActionResult UploadImage()
        {
            var fileName = Guid.NewGuid().ToString() + ".jpg";
            var uploadUri = _blobService.GeneratePictureUploadSasUri(fileName);

            return Ok(new { uploadUri = uploadUri.ToString() });
        }
    }
}
