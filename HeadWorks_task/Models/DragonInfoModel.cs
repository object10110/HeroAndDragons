using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadWorks_task.Models
{
    public class DragonInfoModel
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public int AllHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public int AmountHeroDamageForThisDragon { get; private set; }
        public DragonInfoModel(Dragon dragon, int allDamage, int heroDamage)
        {
            Id = dragon.Id;
            Name = dragon.Name;
            AllHealth = dragon.Health;
            CurrentHealth = AllHealth - allDamage;
            AmountHeroDamageForThisDragon = heroDamage;
        }
    }
}
