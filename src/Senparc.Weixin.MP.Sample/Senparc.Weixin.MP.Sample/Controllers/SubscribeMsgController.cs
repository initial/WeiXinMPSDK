﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.HttpUtility;
using Senparc.Weixin.MP.AdvancedAPIs;

namespace Senparc.Weixin.MP.Sample.Controllers
{
    public class SubscribeMsgController : BaseController
    {
        public ActionResult Index()
        {
            var reserved = DateTime.Now.Ticks.ToString();
            Session["WeixinSubscribeMsgReserved"] = reserved;
            string templateId = "63l8YSI2uYqlZwb8dkMSy2Lp8caHcaWc2Id0b_XYvtM";//订阅消息模板ID，登录公众平台后台，在接口权限列表处可查看订阅模板ID
            var returnUrl = "https://sdk.weixin.senparc.com/SubscribeMsg/Result";
            var url = TemplateApi.GetSubscribeMsgUrl(base.AppId, 1, templateId, returnUrl.UrlEncode(), reserved);
            return Redirect(url);
        }

        public ActionResult Result(string openId, string template_id, int scene, string reserved)
        {
            //template_id就是微信后台可以看到的template_id

            if (reserved != Session["WeixinSubscribeMsgReserved"] as string)
            {
                //reserved用于保持请求和回调的状态，授权请后原样带回给第三方。该参数可用于防止csrf攻击（跨站请求伪造攻击），建议第三方带上该参数，可设置为简单的随机数加session进行校验，开发者可以填写a-zA-Z0-9的参数值，最多128字节
                return Content("请求错误！");
            }

            WeixinTrace.SendCustomLog("一次性订阅消息-参数", string.Format("openId：{0}，templateId：{1}，scene：{2}", openId, template_id, scene));

            var action = Request.QueryString["action"];//MVC直接通过Action获取到的action参数为ActionName

            if (action == "confirm")
            {
                //发送提示
                var data = new
                {
                    content = new
                    {
                        value = "Value",
                        color = "#00ff00"
                    }
                };

                //var data1 = new
                //{
                //    content = new[]
                //    {
                //        new{
                //        value = "Value",
                //        color = "#00ff00"
                //        }
                //    }
                //};
                try
                {
                    TemplateApi.Subscribe(base.AppId, openId, template_id, scene, "这是一条“一次性订阅消息”", data);
                    return Content("发送成功！");
                }
                catch (ErrorJsonResultException e)
                {
                    if (e.JsonResult.errcode == ReturnCode.api功能未授权)
                    {
                        return Content("功能正常，由于微信官方（程序或文档）问题，返回错误：" + e.JsonResult.errcode + "。请等微信待官方更新！");
                    }
                    else
                    {
                        return Content("发生错误：" + e.Message);
                    }
                }

            }
            else
            {
                return Content("您已取消授权！");
            }
        }
    }
}