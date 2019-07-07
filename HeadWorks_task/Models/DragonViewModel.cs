using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadWorks_task.Models
{
    public class DragonViewModel
    {
        public IEnumerable<DragonInfoModel> Dragons { get; set; }
        public PageViewModel PageViewModel { get; set; }
        public DragonFilterModel FilterViewModel { get; set; }
        public SortModel SortViewModel { get; set; }
    }
}
