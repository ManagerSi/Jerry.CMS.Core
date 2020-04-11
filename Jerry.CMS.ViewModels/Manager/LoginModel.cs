using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jerry.CMS.ViewModels.Manager
{
    public class LoginModel
    {
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string CaptchaCode { get; set; }
        
    }
}
