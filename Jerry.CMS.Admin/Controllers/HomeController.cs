using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Jerry.CMS.Admin.Models;
using Jerry.CMS.Core.Extensions;
using Jerry.CMS.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Jerry.CMS.Admin.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IManagerRoleService _managerRoleService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(IManagerRoleService managerRoleService, IHttpContextAccessor httpContextAccessor)
        {
            _managerRoleService = managerRoleService;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            var key = HttpContext.Session.GetString("NickName");
            ViewData["NickName"] = _httpContextAccessor.HttpContext.Session.GetString("NickName");
            ViewData["Avatar"] = _httpContextAccessor.HttpContext.Session.GetString("Avatar");
            return View();
        }
        
        /// <summary>
        /// 控制中心
        /// </summary>
        /// <returns></returns>
        public IActionResult Main()
        {
            ViewData["LoginCount"] = User.Claims.FirstOrDefault(x => x.Type == "LoginCount")?.Value;
            ViewData["LoginLastIp"] = User.Claims.FirstOrDefault(x => x.Type == "LoginLastIp")?.Value;
            ViewData["LoginLastTime"] = User.Claims.FirstOrDefault(x => x.Type == "LoginLastTime")?.Value;
            return View();
        }


        public string GetMenu()
        {
            var roleId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
            var navViewTree = _managerRoleService.GetMenusByRoleId(Int32.Parse(roleId)).GenerateTree(x => x.Id, x => x.ParentId);
            return navViewTree.ObjectToJson();
        }





        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
