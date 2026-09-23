using AgenticSoftwareEngineering.Models;
using AgenticSoftwareEngineering.Orchestration;
using AgenticSoftwareEngineering.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSwagger", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register URL Shortener service
builder.Services.AddSingleton<IUrlService, UrlService>();

// Register Agentic Orchestrator service
builder.Services.AddSingleton<TaskOrchestrator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ==========================================
// 1. URL SHORTENER API ENDPOINTS
// ==========================================

app.MapPost("/api/shorten", async (ShortenRequest request, HttpContext context, IUrlService service) =>
{
    try
    {
        var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
        var result = await service.ShortenUrlAsync(request, baseUrl);
        return Results.Created($"/{result.ShortCode}", result);
    }
    catch (ArgumentException ex) { return Results.BadRequest(new { error = ex.Message }); }
    catch (InvalidOperationException ex) { return Results.Conflict(new { error = ex.Message }); }
})
.WithTags("URL Shortener");

app.MapGet("/{code}", async (string code, HttpContext context, IUrlService service) =>
{
    // 1. Input Sanitization
    if (string.IsNullOrWhiteSpace(code) || code.Length > 30 || !System.Text.RegularExpressions.Regex.IsMatch(code, "^[a-zA-Z0-9_-]+$"))
    {
        return Results.BadRequest(new { error = "Invalid short code format." });
    }

    // 2. Lookup URL and increment analytics
    var longUrl = await service.GetLongUrlAsync(code);
    if (longUrl is null)
    {
        return Results.NotFound(new { error = "Short code not found or has expired." });
    }

    // 3. Security Headers
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");

    // 4. Check if request originated from Swagger UI
    var referer = context.Request.Headers.Referer.ToString();
    var isSwaggerRequest = referer.Contains("/swagger", StringComparison.OrdinalIgnoreCase);

    // If executed inside Swagger UI, return 200 OK JSON to prevent browser fetch failure
    if (isSwaggerRequest)
    {
        return Results.Ok(new
        {
            status = "Success",
            shortCode = code,
            targetUrl = longUrl,
            message = "Click count incremented. Direct browser redirect bypassed for Swagger UI."
        });
    }

    // Standard HTTP 302 Redirect for actual browser navigation
    return Results.Redirect(longUrl);
})
.WithTags("URL Shortener");

app.MapGet("/api/analytics/{code}", async (string code, IUrlService service) =>
{
    var analytics = await service.GetAnalyticsAsync(code);
    return analytics is not null ? Results.Ok(analytics) : Results.NotFound();
})
.WithTags("URL Shortener");


// ==========================================
// 2. AGENTIC ORCHESTRATION ENDPOINTS
// ==========================================

// Trigger Greenfield/Brownfield SDLC Workflow
app.MapPost("/api/agentic/run-workflow", async (TaskOrchestrator orchestrator) =>
{
    // Initialize standard SDLC execution graph
    orchestrator.AddTask(new AgentTask { Id = "T1", Title = "Parse & Normalize Requirements" });
    orchestrator.AddTask(new AgentTask { Id = "T2", Title = "Generate API & Domain Code", Dependencies = new() { "T1" } });
    orchestrator.AddTask(new AgentTask { Id = "T3", Title = "Run Safety & Test Suite", Dependencies = new() { "T1" } });
    orchestrator.AddTask(new AgentTask { Id = "T4", Title = "Production Deployment Gate", Dependencies = new() { "T2", "T3" }, RequiresApproval = true });

    await orchestrator.RunWorkflowAsync();
    return Results.Ok(new { message = "Workflow completed successfully", metrics = orchestrator.GetMetrics() });
})
.WithTags("Agentic Orchestrator");

// Get Workflow Tasks State & Audit Graph
app.MapGet("/api/agentic/tasks", (TaskOrchestrator orchestrator) =>
{
    return Results.Ok(orchestrator.GetTasks());
})
.WithTags("Agentic Orchestrator");

app.Run();