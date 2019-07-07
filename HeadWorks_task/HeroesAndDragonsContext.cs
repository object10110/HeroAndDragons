using HeadWorks_task.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadWorks_task
{
    public class HeroesAndDragonsContext:DbContext
    {
        public HeroesAndDragonsContext(DbContextOptions<HeroesAndDragonsContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        public DbSet<Hero> Heroes { get; set; }
        public DbSet<Dragon> Dragons { get; set; }
        public DbSet<Hit> Hits { get; set; }
    }
}
