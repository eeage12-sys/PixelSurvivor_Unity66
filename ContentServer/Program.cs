using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(server => server.ListenLocalhost(7188, listen => listen.UseHttps()));
var app = builder.Build();
var content = Path.Combine(app.Environment.ContentRootPath, "Content");
if (!Directory.Exists(content)) throw new DirectoryNotFoundException(content);
app.MapGet("/health", () => Results.Json(new { status = "ok", transport = "https" }));
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(content),
    RequestPath = "/content",
    OnPrepareResponse = context => context.Context.Response.Headers.CacheControl = "no-store"
});
app.Run();
