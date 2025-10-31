namespace API.Endpoints;

public static class EndpointExtensions
{
    public static WebApplication MapApiEndpoints(this WebApplication app)
    {
        app.MapGroup("/health")
            .WithTags("Health")
            .MapHealthEndpoints();

        var apiV1 = app.MapGroup("/api/v1")
            .WithTags("API v1")
            .WithOpenApi();

        apiV1.MapGroup("/entities")
            .WithTags("Entities")
            .MapEntityEndpoints();

        apiV1.MapGroup("/jobs")
            .WithTags("Background Jobs")
            .MapJobEndpoints();

        app.MapDiscordEndpoints();
        app.MapShopifyEndpoints();

        return app;
    }
}