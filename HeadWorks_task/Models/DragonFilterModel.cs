using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadWorks_task.Models
{
    public class DragonFilterModel
    {
        public DragonFilterModel(string name, int minCurHealth = 0, int maxCurHealth = Dragon.MAX_HEALTH, 
            int minHealth = Dragon.MIN_HEALTH, int maxHealth = Dragon.MAX_HEALTH)
        {
            SelectedName = name;
            SelectedMinCurrentHealth = minCurHealth;
            SelectedMaxCurrentHealth = maxCurHealth;
            SelectedMinHealth = minHealth;
            SelectedMaxHealth = maxHealth;
        }
        public string SelectedName { get; private set; }    // введенное имя
        public int SelectedMinCurrentHealth { get; private set; }//
        public int SelectedMaxCurrentHealth { get; private set; }//
        public int SelectedMinHealth { get; private set; }//
        public int SelectedMaxHealth { get; private set; }//
    }
}
