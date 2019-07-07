using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadWorks_task.Models
{
    public class Hero
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public DateTime CreationTime { get; private set; }
        public int Weapon { get; private set; }

        private static Random random = new Random();
        private readonly int MAX_WEAPON = 6;
        private readonly int MIN_WEAPON = 1;

        public Hero(string name)
        {
            Name = name;
            CreationTime = DateTime.Now;
            Weapon = random.Next(MIN_WEAPON, MAX_WEAPON+1);
        }
    }
}