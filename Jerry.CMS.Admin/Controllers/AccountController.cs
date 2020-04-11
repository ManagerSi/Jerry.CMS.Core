using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Jerry.CMS.Admin.Models.ResultModel;
using Jerry.CMS.Core.Helper;
using Jerry.CMS.IServices;
using Jerry.CMS.ViewModels.Manager;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jerry.CMS.Admin.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly IManagerService _managerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string CaptchaCodeSessionName = "CaptchaCode";
        public AccountController(IHttpContextAccessor httpContext, IManagerService service)
        {
            _httpContextAccessor = httpContext;
            _managerService = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<string> LoginAsync(LoginModel model)
        {
            BaseResult result = new BaseResult();
            #region 判断验证码
            if (!ValidateCaptchaCode(model.CaptchaCode))
            {
                result.ResultCode = ResultCodeAddMsgKeys.SignInCaptchaCodeErrorCode;
                result.ResultMsg = ResultCodeAddMsgKeys.SignInCaptchaCodeErrorMsg;
                return result.ObjectToJson();
            }
            #endregion
            if (model==null || string.IsNullOrEmpty(model?.UserName))
            {
                result.ResultCode= ResultCodeAddMsgKeys.CommonModelStateInvalidCode;
                result.ResultMsg = "wrong username";
                return (result.ObjectToJson());
            }

            try
            {
                var manager = await _managerService.LoginAsync(model);
                return manager.ObjectToJson();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public IActionResult GetCaptchaImage()
        {
            var captchaCode = CaptchaHelper.GenerateCaptchaCode();
            var result = CaptchaHelper.GetImage(116, 36, captchaCode);
            HttpContext.Session.SetString(CaptchaCodeSessionName, captchaCode);
            return new FileStreamResult(new MemoryStream(result.CaptchaByteData), "image/png");
        }
        
        private bool ValidateCaptchaCode(string userInputCaptcha)
        {
            var isValid = userInputCaptcha.Equals(HttpContext.Session.GetString(CaptchaCodeSessionName), StringComparison.OrdinalIgnoreCase);
            HttpContext.Session.Remove(CaptchaCodeSessionName);
            return isValid;
        }
    }
}