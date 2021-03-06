using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore;

namespace Middleware
{
    //https://github.com/aspnet/Entropy/blob/dev/samples/Builder.Filtering.Web/Startup.cs
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            //We use it the type so it is clear. We will use the shortened version from now on.
            //app.Use(next => context => FilterAsync(context, next));
            app.Use((RequestDelegate next) =>
            {
                return (HttpContext context) => BufferAsync(context, next);
            });

            app.Run(async context =>
            {
                context.Response.Headers.Add("content-type", "text/html");
                await context.Response.WriteAsync("<h1>This sample uses buffering</h1>");
                await context.Response.WriteAsync("<p>This allows all your Middlewares to write to Response.</p>");
            });
        }

        public async Task BufferAsync(HttpContext context, RequestDelegate next)
        {
            var body = context.Response.Body;
            var buffer = new MemoryStream();
            context.Response.Body = buffer;

            try
            {
                await context.Response.WriteAsync("<html><body>");
                await next(context);
                await context.Response.WriteAsync("</body></html>");

                buffer.Position = 0;
                await buffer.CopyToAsync(body);
            }
            finally
            {
                context.Response.Body = body;
            }
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseEnvironment("Development");
    }
}