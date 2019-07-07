using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadWorks_task.Models
{
    public class Hit
    {
        public int Id { get; private set; }
        public int Damage { get; private set; }
        public int DragonId { get; private set; }
        public int HeroId { get; private set; }

        private static Random random = new Random();
        private readonly int MAX_DAMAGE_BONUS = 3;
        private readonly int MIN_DAMAGE_BONUS = 1;
        public Hit(){}
        public Hit(Hero hero, Dragon dragon, int currentDragonHealth)
        {
            Damage = hero.Weapon + random.Next(MIN_DAMAGE_BONUS, MAX_DAMAGE_BONUS + 1);
            if(Damage> currentDragonHealth)//Если урон превышает жизни дракона
            {
                Damage = currentDragonHealth;//приравниваем урон к остаткам жизней дракона
            }
            HeroId = hero.Id;
            DragonId = dragon.Id;
        }
    }
}
