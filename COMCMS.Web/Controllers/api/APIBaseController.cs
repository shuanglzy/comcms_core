﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using COMCMS.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace COMCMS.Web.Controllers.api
{
    [Produces("application/json")]
    public class APIBaseController : Controller
    {
        #region 通用信息
        public ReJSON reJson = new ReJSON();
        #endregion

        #region 验证签名
        public bool CheckSignature(SortedDictionary<string, string> pars, string signature)
        {
            if (!MySign.CheckSign(pars, signature))
            {
                reJson.code = 40004;
                reJson.message = "signature 错误！";
                return false;
            }
            //判断是否超时
            string timeStamp = pars["timeStamp"];
            //判断时间有效性
            DateTime postTime = Utils.StampToDateTime(timeStamp);
            if (postTime < DateTime.Now.AddSeconds(-120))//30秒有效期
            {
                reJson.code = 40004;
                reJson.message = "数据请求超时！";
                reJson.isReload = 1;
                return false;
            }
            return true;
        }

        #endregion

        #region 自动验证QueryString 的签名
        /// <summary>
        /// 自动验证QueryString 的签名
        /// </summary>
        /// <returns></returns>
        public bool AutoCheckQueryStringSignature()
        {
            var queryStrings = Request.Query;
            List<string> keys = new List<string>();
            //string[] keys = HttpContext.Current.Request.QueryString.AllKeys;
            if (queryStrings == null)
            {
                return false;
            }
            foreach (var item in queryStrings.Keys)
            {
                keys.Add(item);
            }
            SortedDictionary<string, string> pars = new SortedDictionary<string, string>();
            string signature = "";
            foreach (var k in keys)
            {
                if (k != "signature")
                {
                    string v = Request.Query[k];
                    pars.Add(k, v);
                }
                else
                {
                    signature = Request.Query[k];
                }
            }
            //没有签名返回错误
            if (string.IsNullOrEmpty(signature))
                return false;
            return CheckSignature(pars, signature);
        }
        #endregion
    }

    #region 通用返回信息
    public class ReJSON
    {
        /// <summary>
        /// 返回代码 0 为正确
        /// </summary>
        public int code { get; set; } = 40000;

        /// <summary>
        /// 提示语
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 是否重新加载
        /// </summary>
        public int isReload { get; set; } = 0;

        /// <summary>
        /// 数据详情 object
        /// </summary>
        public object detail { get; set; }
    }
    #endregion
}