using System.Linq;
using AutoMapper;
using Packt.Ecommerce.Data.Models.Models;
using Packt.Ecommerce.DTO.Models;

namespace Packt.Ecommerce.Product
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            MapEntity();
        }

        private void MapEntity()
        {
            CreateMap<Data.Models.Models.Product, ProductDetailsViewModel>();

            CreateMap<Rating, RatingViewModel>();

            CreateMap<Data.Models.Models.Product, ProductListViewModel>()
                .ForMember(x => x.AverageRating, 
                           o =>
                               o.MapFrom(a => a.Rating != null ? a.Rating.Average(y => y.Stars) : 0));
        }
    }
}
