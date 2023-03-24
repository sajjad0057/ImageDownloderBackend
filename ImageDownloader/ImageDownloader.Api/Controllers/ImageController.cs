using AutoMapper;
using ImageDownloader.Api.Models;
using ImageDownloder.Infrastructure.BusinessObjects;
using ImageDownloder.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ImageDownloader.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly ILogger<ImageController> _logger;
        private readonly IImageDownloaderService _imageService;
        private readonly IMapper _mapper;

        public ImageController(ILogger<ImageController> logger,
            IImageDownloaderService imageService,IMapper mapper)
        {
            _logger = logger;
            _imageService = imageService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] RequestDownloadModel request)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    var downloadRequest = _mapper.Map<RequestDownload>(request);
                    _imageService.DownloadImageAsync(downloadRequest);

                    return Ok();

                }

                return BadRequest();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "There have a problem occured in download image!");

                return BadRequest();
            }
        }
    }
}
