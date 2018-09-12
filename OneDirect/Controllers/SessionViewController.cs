using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OneDirect.Repository.Interface;
using Microsoft.Extensions.Logging;
using OneDirect.Models;
using OneDirect.Repository;
using Microsoft.AspNetCore.Http;
using OneDirect.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace OneDirect.Controllers
{
    [TypeFilter(typeof(LoginAuthorizeAttribute))]
    public class SessionViewController : Controller
    {
        private readonly IRomChangeLogInterface IRomChangeLog;
        private readonly IPatientRxInterface IPatientRx;
        private readonly IUserInterface lIUserRepository;
        private readonly ISessionInterface lISessionInterface;
        private readonly INewPatient INewPatient;
        private readonly ILogger logger;
        private OneDirectContext context;

        public SessionViewController(OneDirectContext context, ILogger<SessionViewController> plogger)
        {
            logger = plogger;
            this.context = context;
            IRomChangeLog = new RomChangeLogRepository(context);
            IPatientRx = new PatientRxRepository(context);
            lIUserRepository = new UserRepository(context);
            lISessionInterface = new SessionRepository(context);
            INewPatient = new NewPatientRepository(context);
        }

        public IActionResult Index(string id, string Username = "", string protocolid = "", string protocolName = "", string Etype = "", string actuator = "")
        {
            var response = new Dictionary<string, object>();
            try
            {
               
                if (!String.IsNullOrEmpty(id))
                {
                    ViewBag.actuator = actuator;
                    ViewBag.EquipmentType = Etype;
                    ViewBag.patientId = id;
                    ViewBag.PatientName = Username;
                    HttpContext.Session.SetString("PatientID", id);
                }
                if (string.IsNullOrEmpty(actuator) && !string.IsNullOrEmpty(Etype))
                {
                    if (Etype.ToLower() == "shoulder")
                    {
                        actuator = "Forward Flexion";
                    }
                    else
                    {
                        actuator = "Flexion-Extension";
                    }
                    ViewBag.actuator = actuator;
                }
                logger.LogDebug("Pain Post Start");
                if (!String.IsNullOrEmpty(Username))
                    HttpContext.Session.SetString("PatientName", Username);
                if (HttpContext.Session.GetString("PatientName") != null && HttpContext.Session.GetString("PatientName").ToString() != "")
                {
                    if (!String.IsNullOrEmpty(Username))
                        ViewBag.PatientName = HttpContext.Session.GetString("PatientName").ToString();
                }
                List<UserSession> pSession = new List<UserSession>();
                if (String.IsNullOrEmpty(protocolid))
                {
                    HttpContext.Session.SetString("ProtocolName", "");
                    HttpContext.Session.SetString("ProtocoloId", "");
                    if (!String.IsNullOrEmpty(Username) && !String.IsNullOrEmpty(id))
                        pSession = lISessionInterface.getSessionList(id, actuator);
                    else
                    {
                        if (String.IsNullOrEmpty(Username) && String.IsNullOrEmpty(id) && HttpContext.Session.GetString("UserId") != null && HttpContext.Session.GetString("UserType") != null && HttpContext.Session.GetString("UserType").ToString() == "2")
                        {
                            pSession = lISessionInterface.getSessionListByTherapistId(HttpContext.Session.GetString("UserId"));
                        }
                        else
                        {
                            pSession = lISessionInterface.getSessionList();
                        }
                    }
                }
                else
                {
                    HttpContext.Session.SetString("PatientName", "");
                    HttpContext.Session.SetString("PatientID", "");
                    HttpContext.Session.SetString("ProtocolName", protocolName);
                    HttpContext.Session.SetString("ProtocoloId", protocolid);
                    if (!String.IsNullOrEmpty(protocolName))
                    {
                        ViewBag.PatientName = protocolName;
                    }
                    pSession = lISessionInterface.getSessionListByProtocoloId(protocolid);
                }
                if (pSession != null && pSession.Count > 0)
                    ViewBag.SessionList = pSession;

                return View();
            }
            catch (Exception ex)
            {
                logger.LogDebug("User Post Error: " + ex);
                return null;
            }
        }
        public IActionResult Delete(string sessionId, int patId = 0, string patName = "", string etype = "", string returnView = "")
        {
            string deviceConfiguration = string.Empty;
            try
            {
                Session lsession = lISessionInterface.getSession(sessionId);
                if (lsession != null)
                {
                    Protocol lprotocol = context.Protocol.FirstOrDefault(x => x.ProtocolId == lsession.ProtocolId);
                    if (lprotocol != null)
                    {
                        deviceConfiguration = lprotocol.DeviceConfiguration;
                    }
                    string result = lISessionInterface.DeleteSessionRecordsWithCasecade(sessionId);
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug("Error: " + ex);
            }
            if (!string.IsNullOrEmpty(returnView))
            {
                return RedirectToAction("Index", "Review", new { id = patId, Username = patName, EquipmentType = etype, actuator = deviceConfiguration, tab = "Sessions" });
            }
            else
            {
                return RedirectToAction("Index", "SessionView", new { id = patId, Username = patName, Etype = etype });
            }
        }
        public IActionResult Add(int id, string Username = "", string Etype = "", string actuator = "")
        {
            try
            {
                List<NewProtocol> ptoList = INewPatient.GetProtocolListBypatId(id.ToString());
                if (Etype == "Shoulder")
                {
                    ptoList = ptoList.Where(p => p.ExcerciseEnum == actuator).ToList();
                }
                else
                {
                    ptoList = ptoList.Where(p => p.ExcerciseEnum == "Flexion-Extension").ToList();
                }
                List<SelectListItem> list = new List<SelectListItem>();
                foreach (NewProtocol ex in ptoList)
                {

                    list.Add(new SelectListItem { Text = ex.ProtocolName.ToString(), Value = ex.ProtocolId.ToString() });
                }
                ViewBag.Protocol = list;
                SessionView sv = new SessionView();
                sv.PatientId = id;
                sv.Patname = Username;
                sv.EType = Etype;
                sv.EEnum = actuator;
                return View(sv);
            }
            catch (Exception ex)
            {
                logger.LogDebug("User Post Error: " + ex);
                return null;
            }
        }
        [HttpPost]
        public IActionResult Add(SessionView session)
        {
            try
            {
                NewProtocol ptoList = INewPatient.GetProtocolByproId(session.ProtocolId);
                if (ptoList != null)
                {
                    PatientRx lrx = IPatientRx.getPatientRx(ptoList.RxId);
                    if (lrx != null)
                    {
                        Session _session = new Session();

                        _session.SessionId = Guid.NewGuid().ToString();
                        _session.PatientId = ptoList.PatientId;
                        _session.RxId = ptoList.RxId;
                        _session.ProtocolId = ptoList.ProtocolId;
                        _session.SessionDate = session.SessionDate;
                        _session.Duration = session.Duration;
                        _session.Reps = session.Reps;
                        _session.MaxExtension = session.MaxExtension;
                        _session.MaxFlexion = session.MaxFlexion;
                        _session.MaxPain = session.MaxPain;
                        _session.PainCount = session.PainCount;

                        if (session.MaxFlexion > lrx.CurrentFlexion && session.MaxExtension > lrx.CurrentExtension)
                        {
                            int res = INewPatient.ChangeRxCurrent(lrx.RxId, session.MaxFlexion, session.MaxExtension, "Patient");
                        }
                        else if (session.MaxFlexion > lrx.CurrentFlexion)
                        {
                            int res = INewPatient.ChangeRxCurrentFlexion(lrx.RxId, session.MaxFlexion, "Patient");
                        }
                        else if (session.MaxExtension > lrx.CurrentExtension)
                        {
                            int res = INewPatient.ChangeRxCurrentExtension(lrx.RxId, session.MaxExtension, "Patient");
                        }
                        else
                        {
                            RomchangeLog plog = new RomchangeLog();
                            plog.RxId = lrx.RxId;
                            plog.PreviousFlexion = lrx.CurrentFlexion.HasValue ? Convert.ToInt32(lrx.CurrentFlexion) : 0;
                            plog.PreviousExtension = lrx.CurrentExtension.HasValue ? Convert.ToInt32(lrx.CurrentExtension) : 0;
                            plog.CreatedDate = DateTime.UtcNow;
                            plog.ChangedBy = "Patient";
                            IRomChangeLog.InsertRomChangeLog(plog);
                        }

                        lISessionInterface.InsertSession(_session);
                    }
                }

                return RedirectToAction("Index", "SessionView", new { id = session.PatientId, Username = session.Patname, Etype = session.EType, actuator = session.EEnum });
            }


            catch (Exception ex)
            {
                logger.LogDebug("User Post Error: " + ex);
                return null;
            }
        }
    }
}
