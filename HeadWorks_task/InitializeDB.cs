using HeadWorks_task.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadWorks_task
{
    public static class InitializeDB
    {
        public static void Initialize(HeroesAndDragonsContext context)
        {
            if (!context.Heroes.Any())
            {
                context.Heroes.AddRange(
                    new Hero("Hero_1"),
                    new Hero("Hero_2")
                );
                context.Dragons.AddRange(
                    new Dragon("Drago"),
                    new Dragon("Fiero")
                );
                context.SaveChanges();
            }
            if (!context.Dragons.Any())
            {
                context.Dragons.AddRange(
                    new Dragon("Drago"),
                    new Dragon("Fiero"),
                    new Dragon("Drako")
                );
                context.SaveChanges();
            }
        }
    }
}
