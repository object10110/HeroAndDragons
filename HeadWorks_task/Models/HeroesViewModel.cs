using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadWorks_task.Models
{
    public class HeroesViewModel
    {
        public IEnumerable<Hero> Heroes { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public HeroFilterModel FilterViewModel { get; set; }
        public SortModel SortViewModel { get; set; }
    }
}
