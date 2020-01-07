using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jerry.CMS.IServices;
using Jerry.CMS.Quartz;
using Jerry.CMS.ViewModels.TaskInfo;
using Microsoft.AspNetCore.Mvc;
using NLog;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Jerry.CMS.Admin.Controllers
{
    public class TaskInfoController : Controller
    {
        public static Logger _logger = LogManager.GetCurrentClassLogger();

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
            _logger.Info("TaskInfoController.Index");
            var data = await _taskInfoService.GetListByJobStatuAsync((int)TaskInfoStatus.Stopped);
            ViewData["data"] = data;
            var x = data[0];
            var res = await _scheduleCenter.AddJobAsync(x.Name, x.Group, x.ClassName, x.Assembly, x.Cron);
            var c = res.ToString();
            return View();
        }
    }
}
