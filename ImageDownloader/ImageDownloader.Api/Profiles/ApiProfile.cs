using AutoMapper;
using ImageDownloader.Api.Models;
using ImageDownloder.Infrastructure.BusinessObjects;

namespace ImageDownloader.Api.Profiles
{
    public class ApiProfile : Profile
    {
        public ApiProfile() 
        {
            CreateMap<RequestDownloadModel, RequestDownload>()
                .ReverseMap();
        }
    }
}
