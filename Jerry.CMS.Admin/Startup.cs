using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexinea.Autofac.Extensions.DependencyInjection;
using Autofac;
using AutoMapper;
using FluentValidation.AspNetCore;
using Jerry.CMS.Admin.Filter;
using Jerry.CMS.Admin.Validation;
using Jerry.CMS.Core.Models;
using Jerry.CMS.IRepository;
using Jerry.CMS.IServices;
using Jerry.CMS.Quartz;
using Jerry.CMS.Repository.SqlServer;
using Jerry.CMS.Services;
using Jerry.CMS.ViewModels.TaskInfo;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using Quartz;

namespace Jerry.CMS.Admin
{
    public class Startup
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            env.ConfigureNLog("Nlog.config");//添加nlog
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //配置认证
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Index";
                    options.LogoutPath = "/Account/Logout";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                });

            //session
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(15);
                options.Cookie.HttpOnly = true;
            });
            services.AddAntiforgery(options =>
            {
                options.FormFieldName = "AntiforgeryKey_jerry";
                options.HeaderName = "X-CSRF-TOKEN-jerry";
                options.SuppressXFrameOptionsHeader = false;
            });

            services.AddMvc(options => { options.Filters.Add(new GlobalExceptionFilter()); })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddControllersAsServices()
                .AddFluentValidation(fv =>
                {
                    //程序集方式引入
                    fv.RegisterValidatorsFromAssemblyContaining<ManagerRoleValidation>();
                    //去掉其他的验证，只使用FluentValidation的验证规则
                    fv.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                });

            //DI了AutoMapper中需要用到的服务，其中包括AutoMapper的配置类 Profile
            services.AddAutoMapper();

            services.AddLogging();

            //添加依赖
            //添加数据连接字符串，给repository使用
            services.Configure<DbOption>("JerryCms", Configuration.GetSection("DbOption"));
            
            //定时任务调度器 单例
            services.AddSingleton<ScheduleCenter>();

            //autofac 
            var builder = new ContainerBuilder();//Autofac
            builder.Populate(services);     //Alexinea.Autofac.Extensions.DependencyInjection
            builder.RegisterAssemblyTypes(typeof(ManagerRoleRepository).Assembly)
                .Where(i => i.Name.EndsWith("Repository"))
                .AsImplementedInterfaces();
            builder.RegisterAssemblyTypes(typeof(ManagerRoleService).Assembly)
                .Where(i => i.Name.EndsWith("Service"))
                .AsImplementedInterfaces();
            //services.AddTransient<ITaskInfoRepository, TaskInfoRepository>();
            //services.AddTransient<ITaskInfoService, TaskInfoService>();

            return new AutofacServiceProvider(builder.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory
            , IApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseAuthentication();

            //add NLog to ASP.NET Core
            loggerFactory.AddNLog();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}");
            });

            try
            {
                var jobInfoAppService = app.ApplicationServices.GetService<ITaskInfoService>();
                var scheduleCenter = app.ApplicationServices.GetService<ScheduleCenter>();
                applicationLifetime.ApplicationStarted.Register(async () =>
                {
                    var jobs = await jobInfoAppService.GetListByJobStatuAsync((int) TaskInfoStatus.SystemStopped);
                    if (jobs?.Count > 0)
                    {
                        jobs.ForEach(async job =>
                        {
                            await scheduleCenter.AddJobAsync(job.Name, job.Group, job.ClassName, job.Assembly,
                                job.Cron);
                            await jobInfoAppService.ResumeSystemStoppedAsync();
                        });
                    }
                });
                applicationLifetime.ApplicationStopping.Register(async () =>
                {
                    var list = await jobInfoAppService.GetListByJobStatuAsync((int) TaskInfoStatus.Running);
                    if (list?.Count > 0)
                    {
                        list.ForEach(async job =>
                        {
                            await jobInfoAppService.SystemStoppedAsync();
                            await scheduleCenter.DeleteJobAsync(job.Name, job.Group);
                        });
                    }
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                logger.Error(e, nameof(Startup));
            }

        }
    }
}
