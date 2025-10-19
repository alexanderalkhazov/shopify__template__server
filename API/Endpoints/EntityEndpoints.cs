using API.Repositories.Interfaces;
using API.Services.Interfaces;
using API.Models.DTOs;
using API.Models;
using AutoMapper;

namespace API.Endpoints;

public static class EntityEndpoints
{
    public static RouteGroupBuilder MapEntityEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper) =>
        {
            const string cacheKey = "entities:all";

            var cachedEntities = await cacheService.GetAsync<List<EntitySummaryDto>>(cacheKey);
            if (cachedEntities != null)
            {
                return Results.Ok(new { data = cachedEntities, fromCache = true });
            }

            var entities = await unitOfWork.Entities.GetAllAsync();
            var entityDtos = mapper.Map<List<EntitySummaryDto>>(entities);
            await cacheService.SetAsync(cacheKey, entityDtos, TimeSpan.FromMinutes(5));

            return Results.Ok(new { data = entityDtos, fromCache = false });
        })
        .WithName("GetAllEntities")
        .WithSummary("Get all entities (with caching)")
        .WithOpenApi();

        group.MapGet("/{id:int}", async (int id, IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper) =>
        {
            var cacheKey = $"entity:id:{id}";

            var cachedEntity = await cacheService.GetAsync<EntityDto>(cacheKey);
            if (cachedEntity != null)
            {
                return Results.Ok(new { data = cachedEntity, fromCache = true });
            }

            var entity = await unitOfWork.Entities.GetByIdAsync(id);
            if (entity is not null)
            {
                var entityDto = mapper.Map<EntityDto>(entity);
                await cacheService.SetAsync(cacheKey, entityDto, TimeSpan.FromMinutes(10));
                return Results.Ok(new { data = entityDto, fromCache = false });
            }

            return Results.NotFound();
        })
        .WithName("GetEntityById")
        .WithSummary("Get entity by ID (with caching)")
        .WithOpenApi();

        group.MapGet("/code/{code}", async (string code, IUnitOfWork unitOfWork, ICacheService cacheService) =>
        {
            var cacheKey = $"entity:code:{code}";

            var cachedEntity = await cacheService.GetAsync<API.Models.Entity>(cacheKey);
            if (cachedEntity != null)
            {
                return Results.Ok(new { data = cachedEntity, fromCache = true });
            }

            var entity = await unitOfWork.Entities.GetByCodeAsync(code);
            if (entity is not null)
            {
                await cacheService.SetAsync(cacheKey, entity, TimeSpan.FromMinutes(10));
                return Results.Ok(new { data = entity, fromCache = false });
            }

            return Results.NotFound();
        })
        .WithName("GetEntityByCode")
        .WithSummary("Get entity by code (with caching)")
        .WithOpenApi();

        group.MapGet("/active", async (IUnitOfWork unitOfWork, ICacheService cacheService) =>
        {
            const string cacheKey = "entities:active";

            var cachedEntities = await cacheService.GetAsync<List<API.Models.Entity>>(cacheKey);
            if (cachedEntities != null)
            {
                return Results.Ok(new { data = cachedEntities, fromCache = true });
            }

            var activeEntities = await unitOfWork.Entities.GetActiveEntitiesAsync();
            await cacheService.SetAsync(cacheKey, activeEntities, TimeSpan.FromMinutes(3));

            return Results.Ok(new { data = activeEntities, fromCache = false });
        })
        .WithName("GetActiveEntities")
        .WithSummary("Get all active entities (with caching)")
        .WithOpenApi();

        group.MapGet("/featured", async (IUnitOfWork unitOfWork, ICacheService cacheService) =>
        {
            const string cacheKey = "entities:featured";

            var cachedEntities = await cacheService.GetAsync<List<API.Models.Entity>>(cacheKey);
            if (cachedEntities != null)
            {
                return Results.Ok(new { data = cachedEntities, fromCache = true });
            }

            var featuredEntities = await unitOfWork.Entities.GetFeaturedEntitiesAsync();
            await cacheService.SetAsync(cacheKey, featuredEntities, TimeSpan.FromMinutes(15));

            return Results.Ok(new { data = featuredEntities, fromCache = false });
        })
        .WithName("GetFeaturedEntities")
        .WithSummary("Get all featured entities (with caching)")
        .WithOpenApi();

        group.MapGet("/category/{category}", async (string category, IUnitOfWork unitOfWork, ICacheService cacheService) =>
        {
            var cacheKey = $"entities:category:{category}";

            var cachedEntities = await cacheService.GetAsync<List<API.Models.Entity>>(cacheKey);
            if (cachedEntities != null)
            {
                return Results.Ok(new { data = cachedEntities, fromCache = true });
            }

            var entities = await unitOfWork.Entities.GetByCategoryAsync(category);
            await cacheService.SetAsync(cacheKey, entities, TimeSpan.FromMinutes(8));

            return Results.Ok(new { data = entities, fromCache = false });
        })
        .WithName("GetEntitiesByCategory")
        .WithSummary("Get entities by category (with caching)")
        .WithOpenApi();

        group.MapGet("/status/{status}", async (string status, IUnitOfWork unitOfWork, ICacheService cacheService) =>
        {
            var cacheKey = $"entities:status:{status}";

            var cachedEntities = await cacheService.GetAsync<List<API.Models.Entity>>(cacheKey);
            if (cachedEntities != null)
            {
                return Results.Ok(new { data = cachedEntities, fromCache = true });
            }

            var entities = await unitOfWork.Entities.GetByStatusAsync(status);
            await cacheService.SetAsync(cacheKey, entities, TimeSpan.FromMinutes(5));

            return Results.Ok(new { data = entities, fromCache = false });
        })
        .WithName("GetEntitiesByStatus")
        .WithSummary("Get entities by status (with caching)")
        .WithOpenApi();

        group.MapGet("/search", async (string term, IUnitOfWork unitOfWork, ICacheService cacheService) =>
        {
            var cacheKey = $"entities:search:{term.ToLowerInvariant()}";

            var cachedEntities = await cacheService.GetAsync<List<API.Models.Entity>>(cacheKey);
            if (cachedEntities != null)
            {
                return Results.Ok(new { data = cachedEntities, fromCache = true });
            }

            var entities = await unitOfWork.Entities.SearchByNameAsync(term);
            await cacheService.SetAsync(cacheKey, entities, TimeSpan.FromMinutes(2));

            return Results.Ok(new { data = entities, fromCache = false });
        })
        .WithName("SearchEntities")
        .WithSummary("Search entities by name (with caching)")
        .WithOpenApi();

        group.MapGet("/priority", async (int min, int max, IUnitOfWork unitOfWork, ICacheService cacheService) =>
        {
            var cacheKey = $"entities:priority:{min}-{max}";

            var cachedEntities = await cacheService.GetAsync<List<API.Models.Entity>>(cacheKey);
            if (cachedEntities != null)
            {
                return Results.Ok(new { data = cachedEntities, fromCache = true });
            }

            var entities = await unitOfWork.Entities.GetByPriorityRangeAsync(min, max);
            await cacheService.SetAsync(cacheKey, entities, TimeSpan.FromMinutes(5));

            return Results.Ok(new { data = entities, fromCache = false });
        })
        .WithName("GetEntitiesByPriorityRange")
        .WithSummary("Get entities by priority range (with caching)")
        .WithOpenApi();

        // POST endpoint for creating entities
        group.MapPost("/", async (CreateEntityDto createDto, IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper) =>
        {
            try
            {
                var entity = mapper.Map<Entity>(createDto);
                entity.CreatedAt = DateTime.UtcNow;
                entity.UpdatedAt = DateTime.UtcNow;

                await unitOfWork.Entities.AddAsync(entity);
                await unitOfWork.SaveChangesAsync();

                // Clear related cache
                await cacheService.RemoveByPatternAsync("entities:*");

                var entityDto = mapper.Map<EntityDto>(entity);
                return Results.Created($"/entities/{entity.Id}", new { data = entityDto });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to create entity: {ex.Message}");
            }
        })
        .WithName("CreateEntity")
        .WithSummary("Create a new entity")
        .WithOpenApi();

        // PUT endpoint for updating entities
        group.MapPut("/{id:int}", async (int id, UpdateEntityDto updateDto, IUnitOfWork unitOfWork, ICacheService cacheService, IMapper mapper) =>
        {
            try
            {
                var existingEntity = await unitOfWork.Entities.GetByIdAsync(id);
                if (existingEntity == null)
                {
                    return Results.NotFound();
                }

                // Map non-null values from updateDto to existing entity
                mapper.Map(updateDto, existingEntity);
                existingEntity.UpdatedAt = DateTime.UtcNow;

                unitOfWork.Entities.Update(existingEntity);
                await unitOfWork.SaveChangesAsync();

                // Clear related cache
                await cacheService.RemoveByPatternAsync("entities:*");
                await cacheService.RemoveByPatternAsync($"entity:*");

                var entityDto = mapper.Map<EntityDto>(existingEntity);
                return Results.Ok(new { data = entityDto });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to update entity: {ex.Message}");
            }
        })
        .WithName("UpdateEntity")
        .WithSummary("Update an existing entity")
        .WithOpenApi();

        // DELETE endpoint for deleting entities
        group.MapDelete("/{id:int}", async (int id, IUnitOfWork unitOfWork, ICacheService cacheService) =>
        {
            try
            {
                var entity = await unitOfWork.Entities.GetByIdAsync(id);
                if (entity == null)
                {
                    return Results.NotFound();
                }

                unitOfWork.Entities.Remove(entity);
                await unitOfWork.SaveChangesAsync();

                // Clear related cache
                await cacheService.RemoveByPatternAsync("entities:*");
                await cacheService.RemoveByPatternAsync($"entity:*");

                return Results.Ok(new { message = "Entity deleted successfully" });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to delete entity: {ex.Message}");
            }
        })
        .WithName("DeleteEntity")
        .WithSummary("Delete an entity")
        .WithOpenApi();

        group.MapDelete("/cache", async (ICacheService cacheService) =>
        {
            try
            {
                await cacheService.RemoveByPatternAsync("entities:*");
                await cacheService.RemoveByPatternAsync("entity:*");
                return Results.Ok(new { message = "Entity cache cleared successfully" });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Failed to clear cache: {ex.Message}");
            }
        })
        .WithName("ClearEntityCache")
        .WithSummary("Clear all entity-related cache")
        .WithOpenApi();

        return group;
    }
}