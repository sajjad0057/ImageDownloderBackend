using AutoMapper;
using ImageDownloader.Api.Commons;
using ImageDownloader.Api.Models;
using ImageDownloder.Infrastructure.BusinessObjects;
using ImageDownloder.Infrastructure.Exceptions;
using ImageDownloder.Infrastructure.Services;
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
        [Route("download")]
        public async Task<IActionResult> PostAsync([FromBody] RequestDownloadModel request)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    var downloadRequest = _mapper.Map<RequestDownload>(request);

                    var UrlAndNames = await _imageService.DownloadImageAsync(downloadRequest);

                    return base.Ok(ResponseDownload.SuccessResponse(UrlAndNames, "Images are downloaded successfully !"));
                }

                return base.BadRequest(ResponseDownload.FailedResponse("Validation Error Occured !"));
            }
            catch(DuplicateUrlException ex)
            {
                _logger.LogError(ex, "Duplicate download image url not acceptable !");

                return base.BadRequest(ResponseDownload.FailedResponse($"Duplicate download image url not acceptable ! : ${ex.Message}"));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "There have a problem occured when download image !");

                return base.BadRequest(ResponseDownload.FailedResponse($"Internal server Error ! : ${ex.Message}"));
            }
        }


        [HttpGet]
        [Route("get-image-by-name/{image_name}")]
        public async Task<IActionResult> GetAsync([FromRoute]string image_name)
        {
            try
            {
                var base64String = await _imageService.GetImageByNameAsync(image_name);

                if (!string.IsNullOrWhiteSpace(base64String))
                {
                    return Ok(ResponseGet<string>.SuccessResponse(base64String,"Successfully get base64String of image !"));
                }
                else
                {
                    return BadRequest(ResponseGet<string>.FailedResponse("Image file does not exists !"));
                }      
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There have a problem occured when get base64String of image !");

                return BadRequest(ResponseGet<string>.FailedResponse($"Internal server error occured ! {ex.Message}"));
            }
        }
    }
}
