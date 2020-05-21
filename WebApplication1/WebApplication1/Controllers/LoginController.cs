﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Http;
using WebApplication1.Dto;
using WebApplication1.Dto.LoginDto;
using WebApplication1.Models;
using WebApplication1.Utility;

namespace WebApplication1.Controllers
{
    public class LoginController : ApiController
    {
        [HttpPost]
        [Route("api/Login/user")]
        public string UserLogin([FromBody]UserLoginDto userLogin)
        {
            try
            {

                using (var context = new ServiceContext())
                {
                    if (userLogin == null)
                    {
                        LogHelper.Error("[UserLogin]:userLogin == null");
                        return "登录失败";
                    }

                    //find user
                    var user = context.User.Where(u => u.UserName.Equals(userLogin.UserName)).FirstOrDefault();

                    if (user == null)
                    {
                        LogHelper.Error("[UserLogin]:user == null");
                        return "找不到用户名";
                    }

                    if (!user.PassWord.Equals(MD5Password.Encryption(userLogin.PassWord)))
                    {
                        LogHelper.Error("[UserLogin]:PassWord is wrong");
                        return "密码错误";
                    }

                    HttpContext.Current.Session["userLogin"] = userLogin.UserName;

                    return "登录成功";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("[UserLogin]: " + ex.ToString());
                return "登录失败";
            }
        }

        [HttpPost]
        [Route("api/Login/visitor")]
        public string VisitorLogin([FromBody]VisitorLoginDto visitorLogin)
        {
            try
            {
                using (var context = new ServiceContext())
                {
                    if (visitorLogin == null)
                    {
                        LogHelper.Error("[VisitorLogin]:userLogin == null");
                        return "登录失败";
                    }

                    //find user
                    var user = context.User.Where(u => u.UserName.Equals(visitorLogin.UserName)).FirstOrDefault();

                    if (user == null)
                    {
                        LogHelper.Error("[VisitorLogin]:userLogin == null");
                        return "找不到该用户";
                    }

                    if (user.License == null)
                    {
                        LogHelper.Error("[VisitorLogin]:License == null");
                        return "该用户未授权";
                    }

                    if (visitorLogin.License.Equals(user.License) == false)
                    {
                        LogHelper.Error("[VisitorLogin]：License is wrong");
                        return "授权码不正确";
                    }

                    HttpContext.Current.Session["visitorLogin"] = visitorLogin.UserName;
                    return "登录成功";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("[VisitorLogin]: " + ex.ToString());
                return "登录失败";
            }
        }

        [HttpPost]
        [Route("api/Login/register")]
        public string Register([FromBody]RegisterDto registerDto)
        {
            try
            {
                using (var context = new ServiceContext())
                {
                    if (registerDto == null)
                    {
                        LogHelper.Error("[Register]:registerDto == null");
                        return "注册失败";
                    }

                    if (string.IsNullOrEmpty(registerDto.PassWord) || string.IsNullOrEmpty(registerDto.UserName))
                    {
                        LogHelper.Error("[Register]:IsNullOrEmpty");
                        return "注册失败";
                    }

                    var userEntity = context.User.Where(u => u.UserName.Equals(registerDto.UserName)).FirstOrDefault();

                    if (userEntity != null)
                    {
                        LogHelper.Error("[Register]:IsNullOrEmpty");
                        return "该用户名已被使用,请重新输入";
                    }

                    var user = new User();
                    user.UserName = registerDto.UserName;
                    user.PassWord = MD5Password.Encryption(registerDto.PassWord);
                    user.Updatetime = DateTime.Now;
                    context.User.Add(user);
                    context.SaveChanges();
                    return "注册成功";
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("[Register]: " + ex.ToString());
                return "注册失败";
            }
        }


        [HttpGet]
        [Route("api/Login/session")]
        public bool SessionValidate()
        {
            try
            {
                if(HttpContext.Current.Session["userLogin"] == null)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Error("[SessionValidate]: " + ex.ToString());
                return false;
            }
        }
    }
}
