using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace HeadWorks_task.Models
{
    public class HeroFilterModel
    {
        public HeroFilterModel(string name, DateTime startTime, DateTime finishTime)
        {
            SelectedStartTime = startTime;
            SelectedFinishTime = finishTime;
            SelectedName = name;
        }
        public string SelectedName { get; private set; }    // введенное имя
        public DateTime SelectedStartTime { get; private set; } //начало временного отрезка
        public DateTime SelectedFinishTime { get; private set; }//конец временного отрезка
    }
}
