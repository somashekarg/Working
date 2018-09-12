﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OneDirect.Helper;
using OneDirect.Repository.Interface;
using OneDirect.Models;
using Microsoft.Extensions.Logging;
using OneDirect.Repository;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneDirect.ViewModels;
using OneDirect.Extensions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OneDirect.Vsee;
using OneDirect.VSee;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace OneDirect.Controllers
{
    [TypeFilter(typeof(LoginAuthorizeAttribute))]
    public class TherapistController : Controller
    {
        private readonly IUserActivityLogInterface lIUserActivityLogRepository;
        private readonly IUserInterface lIUserRepository;
        private readonly IProtocolInterface lIProtocolInterface;
        private readonly IPatientRxInterface lIPatientRxRepository;
        private readonly ILogger logger;
        private OneDirectContext context;

        public TherapistController(OneDirectContext context, ILogger<TherapistController> plogger)
        {
            logger = plogger;
            this.context = context;
            lIUserRepository = new UserRepository(context);
            lIProtocolInterface = new ProtocolRepository(context);
            lIPatientRxRepository = new PatientRxRepository(context);
            lIUserActivityLogRepository = new UserActivityLogRepository(context);
        }


        //get the list of therapists 
        // GET: /<controller>/
        public IActionResult Index()
        {
            var response = new Dictionary<string, object>();
            
            try
            {
                logger.LogDebug("Pain Post Start");
                if (HttpContext.Session.GetString("UserId") != null && HttpContext.Session.GetString("UserType") != null && HttpContext.Session.GetString("UserType").ToString() == ConstantsVar.Provider.ToString())
                {
                    List<User> pUser = lIUserRepository.getTherapistListByProviderId(HttpContext.Session.GetString("UserId"));
                    ViewBag.userlist = pUser;
                }
                else
                {
                    List<UserListView> pUser = lIUserRepository.getUserListByType1243(ConstantsVar.Therapist);
                    ViewBag.userlist = pUser;
                }
               
                return View();
            }
            catch (Exception ex)
            {
                logger.LogDebug("User Post Error: " + ex);
                return null;
            }

        }
        public IActionResult Add()
        {
            
            return View();
        }

        //add a new record of therapist with unique Npi and UserId
        [HttpPost]
        public IActionResult Add(UserViewModel pUser)
        {
            pUser.Type = ConstantsVar.Therapist;
            User _user = lIUserRepository.getUser(pUser.UserId);
            User _user2 = lIUserRepository.getUserNpi(pUser.Npi);
            User _userNew = UserExtension.UserViewModelToUser(pUser);
            if ((_user == null) && (_user2 == null))
            {
                _userNew.Phone = RemoveSpecialChars(_userNew.Phone);
                string _User = lIUserRepository.InsertUser(_userNew);
                if (_User == "Username already exists")
                {
                    TempData["Installer"] = JsonConvert.SerializeObject(_userNew);
                    TempData["msg"] = "<script>Helpers.ShowMessage('User with same Therapist Id is already registered, please use different one', 1);</script>";
                    pUser.UserId = null;
                    return View(pUser);
                }
            }
            else
            {
                if (_user2 != null)
                {
                    if (_user2.Npi == _userNew.Npi)
                    {
                        TempData["Therapist"] = JsonConvert.SerializeObject(_userNew);
                        TempData["msg"] = "<script>Helpers.ShowMessage('User with same Npi is already registered, please use different one', 1);</script>";
                        pUser.Npi = null;
                        return View(pUser);
                    }
                }
                if (_user.UserId == _userNew.UserId)
                {
                    TempData["Therapist"] = JsonConvert.SerializeObject(_userNew);
                    TempData["msg"] = "<script>Helpers.ShowMessage('User with same Therapist Id is already registered, please use different one', 1);</script>";
                    pUser.UserId = null;
                    return View(pUser);
                }
            }
            return RedirectToAction("Index");
        }

        //get and show the profile of the therapist
        public IActionResult Profile(string id)
        {
            User pUser = lIUserRepository.getUser(id);

          
            if (pUser != null)
            {
                UserViewModel _user = UserExtension.UserToUserViewModel(pUser);
                ViewBag.Name = _user.Name;
                return View(_user);
            }
            else
            {
                return View(null);
            }
        }

       //get the dashboard of therapist using userId
        public IActionResult Dashboard()
        {
            List<DashboardView> lDashboardView = null;
            string _uType = HttpContext.Session.GetString("UserType");
            if (HttpContext.Session.GetString("UserId") != null && HttpContext.Session.GetString("UserType") != null && HttpContext.Session.GetString("UserType").ToString() == ConstantsVar.Therapist.ToString())
            {
                try
                {
                    lDashboardView = lIPatientRxRepository.getDashboardForTherapist(HttpContext.Session.GetString("UserId"));
                }
                catch (Exception ex)
                {
                    logger.LogDebug("Error: " + ex);
                }
                return View(lDashboardView);
            }
            else
            {
                
            }
            return View(null);
        }

        //updating the changes in profile of the therapist
        [HttpPost]
        public IActionResult Profile(UserViewModel pUser)
        {
            pUser.Type = ConstantsVar.Therapist;
            User _user2 = lIUserRepository.getUserNpi(pUser.Npi);
            User _user = UserExtension.UserViewModelToUser(pUser);
            _user.Phone = RemoveSpecialChars(_user.Phone);

            if (_user2 == null)
            {
                string _result = lIUserRepository.UpdateUser(_user);
            }
            else
            {
                TempData["Therapist"] = JsonConvert.SerializeObject(_user);
                TempData["msg"] = "<script>Helpers.ShowMessage('User with same Npi is already registered, please use different one', 1);</script>";
                pUser.Npi = null;
                return View(pUser);
            }

            if (HttpContext.Session.GetString("UserType") != null && HttpContext.Session.GetString("UserType").ToString() == ConstantsVar.Therapist.ToString())
            {
                return RedirectToAction("Profile", new { id = pUser.UserId });
            }
            return RedirectToAction("Index");
        }


        //remove the all special characters from phone number before saving in database
        public string RemoveSpecialChars(string str)
        {
            string[] chars = new string[] { "-", " ", "/", "!", "@", "#", "$", "%", "^", "&", "*", "'", "\"", ";", "_", "(", ")", ":", "|", "[", "]" };
            for (var i = 0; i < chars.Length; i++)
            {
                if (str.Contains(chars[i]))
                {
                    str = str.Replace(chars[i], "");
                }
            }
            return str;
        }
        
        //delete the therapist record 
        public IActionResult DeleteTherapist(string therapist)
        {

            try
            {

                var _user = (from p in context.User where p.UserId == therapist select p).FirstOrDefault();
                context.User.Remove(_user);
               
                int res = context.SaveChanges();
                if (res > 0)
                {
                    VSeeHelper lhelper = new VSeeHelper();
                    DeleteUser luser = new DeleteUser();
                    luser.secretkey = ConfigVars.NewInstance.secretkey;
                    luser.username = _user.UserId;
                    var resUser = lhelper.DeleteUser(luser);
                    if (resUser != null && resUser["status"] == "success")
                    {
                        //Insert to User Activity Log
                        UserActivityLog llog = new UserActivityLog();
                        llog.SessionId = HttpContext.Session.GetString("SessionId");
                        llog.ActivityType = "Update";
                        llog.StartTimeStamp = !string.IsNullOrEmpty(HttpContext.Session.GetString("SessionTime")) ? Convert.ToDateTime(HttpContext.Session.GetString("SessionTime")) : DateTime.Now;
                        llog.Duration = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(HttpContext.Session.GetString("SessionTime"))).TotalSeconds);
                        llog.RecordChangeType = "Delete";
                        llog.RecordType = "Therapist";
                        llog.Comment = "Record deleted";
                        llog.RecordJson = JsonConvert.SerializeObject(_user);
                        llog.UserId = HttpContext.Session.GetString("UserId");
                        llog.UserName = HttpContext.Session.GetString("UserName");
                        llog.UserType = HttpContext.Session.GetString("UserType");
                        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("ReviewID")))
                        {
                            llog.ReviewId = HttpContext.Session.GetString("ReviewID");
                        }
                        lIUserActivityLogRepository.InsertUserActivityLog(llog);

                    }
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Therapist");
            }
            return RedirectToAction("Index", "Therapist");
        }
    }
}

