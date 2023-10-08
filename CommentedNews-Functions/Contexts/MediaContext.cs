using CommentedNews_Functions.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentedNews_Functions
{
    public class MediaContext : DbContext
    {
        public MediaContext(DbContextOptions<MediaContext> options) : base(options) { 
        
        }

        public DbSet<Media> Media { get; set; }
    }

    
}
