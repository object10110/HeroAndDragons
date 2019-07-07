using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HeadWorks_task.Models
{
    public class Dragon
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int Health { get; private set; }
        public DateTime CreationTime { get; private set; }
        
        private static Random random = new Random();
        [NotMapped]
        public const int MAX_HEALTH = 100;
        [NotMapped]
        public const int MIN_HEALTH = 80;

        public Dragon(string name)
        {
            Name = name;
            CreationTime = DateTime.Now;
            Health = random.Next(MIN_HEALTH, MAX_HEALTH + 1);
        }
    }
}
