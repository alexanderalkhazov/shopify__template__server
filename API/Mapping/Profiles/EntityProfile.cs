using AutoMapper;
using API.Models;
using API.Models.DTOs;

namespace API.Mapping.Profiles;

/// <summary>
/// AutoMapper profile for Entity mappings
/// </summary>
public class EntityProfile : Profile
{
    public EntityProfile()
    {
        // Entity to EntityDto (full mapping)
        CreateMap<Entity, EntityDto>();

        // Entity to EntitySummaryDto (simplified mapping)
        CreateMap<Entity, EntitySummaryDto>();

        // CreateEntityDto to Entity
        CreateMap<CreateEntityDto, Entity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // ID is auto-generated
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore()); // Will be set manually if needed

        // UpdateEntityDto to Entity (for updates)
        CreateMap<UpdateEntityDto, Entity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // ID should not be updated
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Created date should not change
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore()) // Created by should not change
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null)); // Only map non-null values

        // EntityDto to Entity (reverse mapping if needed)
        CreateMap<EntityDto, Entity>();
    }
}