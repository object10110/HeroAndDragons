using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadWorks_task.Models
{
    public class SortModel
    {
        public SortState NameSort { get; private set; } // значение для сортировки по имени
        public SortState DamageSort { get; private set; } // значение для сортировки по урону
        public SortState Current { get; private set; }  // текущее значение сортировки

        public SortModel(SortState sortOrder)
        {
            NameSort = sortOrder == SortState.NameAsc ? SortState.NameDesc : SortState.NameAsc;
            DamageSort = sortOrder == SortState.DamageAsc ? SortState.DamageDesc : SortState.DamageAsc;
            Current = sortOrder;
        }
    }
}
