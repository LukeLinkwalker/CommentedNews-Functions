using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(CommentedNews_Functions.Startup))]

namespace CommentedNews_Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddDbContext<ArticleContext>(
                options => options.UseSqlServer(Environment.GetEnvironmentVariable("SqlConnectionString")));

            builder.Services.AddDbContext<MediaContext>(
                options => options.UseSqlServer(Environment.GetEnvironmentVariable("SqlConnectionString")));
        }
    }
}
