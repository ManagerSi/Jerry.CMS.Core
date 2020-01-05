using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jerry.CMS.IServices;
using Jerry.CMS.Quartz;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Jerry.CMS.Admin.Controllers
{
    public class TaskInfoController : Controller
    {
        private readonly ScheduleCenter _scheduleCenter;
        private readonly ITaskInfoService _taskInfoService;
        public TaskInfoController(ScheduleCenter schedule, ITaskInfoService taskInfoService)
        {
            _scheduleCenter = schedule;
            _taskInfoService = taskInfoService;
        }
        // GET: /<controller>/
        [Route("TaskInfo")]
        public async Task<IActionResult>  Index()
        {
            var data = _taskInfoService.GetAll();
            ViewData["data"] = data;
            var x = data[0];
            var res = await _scheduleCenter.AddJobAsync(x.Name, x.Group, x.ClassName, x.Assembly, x.Cron);
            var c = res.ToString();
            return View();
        }
    }
}
