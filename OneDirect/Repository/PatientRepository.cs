﻿using Microsoft.EntityFrameworkCore;
using OneDirect.Extensions;
using OneDirect.Models;
using OneDirect.Repository.Interface;
using OneDirect.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneDirect.Repository
{
    public class PatientRepository : IPatient
    {
        private OneDirectContext context;

        public PatientRepository(OneDirectContext context)
        {
            this.context = context;
        }
        public string ClaimPatient(string patientloginid, string SurgeryDate)
        {
            string result = string.Empty;
            try
            {
                var _patient = (from p in context.Patient.Where(p => p.PatientLoginId.ToString() == patientloginid && Convert.ToDateTime(p.SurgeryDate).ToString("dd/MM/yyyy") == Convert.ToDateTime(SurgeryDate).ToString("dd/MM/yyyy"))
                                select p).FirstOrDefault();
                if (_patient != null)
                {
                    result = "success";
                }
                else
                {
                    result = "fail";
                }
            }
            catch (Exception ex)
            {
                result = "error";
            }
            return result;
        }
        public string CreatePIN(string patientloginid, string SurgeryDate, string PIN)
        {
            string result = string.Empty;
            try
            {
                var _patient = (from p in context.Patient.Where(p => p.PatientLoginId == patientloginid && Convert.ToDateTime(p.SurgeryDate).ToString("dd/MM/yyyy") == Convert.ToDateTime(SurgeryDate).ToString("dd/MM/yyyy"))
                                select p).FirstOrDefault();
                if (_patient != null)
                {
                    _patient.Pin = !String.IsNullOrEmpty(PIN) ? int.Parse(PIN) : 0;
                    context.Entry(_patient).State = EntityState.Modified;
                    context.SaveChanges();


                    result = "success";
                }
                else
                {
                    result = "fail";
                }
            }
            catch (Exception ex)
            {
                result = "error";
            }
            return result;
        }

        public string PatientLogin(string patientloginid, string PIN)
        {
            string result = string.Empty;
            try
            {
                var _patient = (from p in context.Patient.Where(p => p.PatientLoginId == patientloginid && p.Pin == int.Parse(PIN))
                                select p).FirstOrDefault();
                if (_patient != null)
                {
                    _patient.LoginSessionId = Guid.NewGuid();
                    context.Entry(_patient).State = EntityState.Modified;
                    context.SaveChanges();
                    result = _patient.LoginSessionId + "/success";
                }
                else
                {
                    result = "/fail";
                }
            }
            catch (Exception ex)
            {
                result = "/error";
            }
            return result;
        }
        public string UpdatePatientSessionId(string patientloginid)
        {
            string result = string.Empty;
            try
            {
                var _patient = (from p in context.Patient.Where(p => p.PatientLoginId == patientloginid)
                                select p).FirstOrDefault();
                if (_patient != null)
                {
                    _patient.LoginSessionId = null;
                    context.Entry(_patient).State = EntityState.Modified;
                    context.SaveChanges();
                    result = "success";
                }
                else
                {
                    result = "";
                }
            }
            catch (Exception ex)
            {
                result = "";
            }
            return result;
        }
        public Patient PatientLogins(string patientloginid, string PIN)
        {
            Patient result = null;
            try
            {
                var _patient = (from p in context.Patient.Where(p => p.PatientLoginId == patientloginid && p.Pin == int.Parse(PIN))
                                select p).FirstOrDefault();
                if (_patient != null)
                {
                    _patient.LoginSessionId = Guid.NewGuid();
                    context.Entry(_patient).State = EntityState.Modified;
                    context.SaveChanges();
                    result = _patient;
                }

            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public List<Patient> GetPatientByTherapistId(string therapsitId)
        {
            List<PatientMessage> msgList = new List<PatientMessage>();
            List<Patient> result = null;
            try
            {
                result = (from p in context.Patient.Where(p => p.Therapistid == therapsitId)
                          select p).ToList();
            }
            catch (Exception ex)
            {

            }
            return result;
        }
        public List<PatientMessage> GetPatientWithStatusByTherapistId(string therapsitId)
        {
            List<PatientMessage> msgList = new List<PatientMessage>();
            List<Patient> result = null;
            try
            {
                result = (from p in context.Patient.Where(p => p.Therapistid == therapsitId)
                          select p).ToList();
                if (result != null && result.Count > 0)
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        PatientMessage lmessage = new PatientMessage();
                        lmessage.Patient = result[i];
                        Messages lmsg = (from m in context.Messages.Where(p => p.UserId == lmessage.Patient.Therapistid && p.PatientId == lmessage.Patient.PatientLoginId)
                                         select m).OrderByDescending(x => x.Datetime).FirstOrDefault();
                        if (lmsg != null)
                        {
                            lmessage.replyStatus = lmsg.SentReceivedFlag;
                        }
                        else
                        {
                            lmessage.replyStatus = 1;
                        }
                        msgList.Add(lmessage);
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return msgList;
        }
        public List<Patient> GetPatientByPatientAdmin(string patientadminid)
        {
            List<Patient> result = null;
            try
            {
                result = (from p in context.Patient.Where(p => p.Paid == patientadminid)
                          select p).ToList();

            }
            catch (Exception ex)
            {

            }
            return result;
        }
        public List<Patient> GetPatientByProviderId(string providerid)
        {
            List<Patient> result = null;
            try
            {
                result = (from p in context.Patient.Where(p => p.ProviderId == providerid)
                          select p).ToList();

            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public List<PatientMessage> GetPatientWithStatusByProviderId(string providerid)
        {
            List<PatientMessage> msgList = new List<PatientMessage>();
            List<Patient> result = null;
            try
            {
                result = (from p in context.Patient.Where(p => p.ProviderId == providerid)
                          select p).ToList();
                if (result != null && result.Count > 0)
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        PatientMessage lmessage = new PatientMessage();
                        lmessage.Patient = result[i];
                        Messages lmsg = (from m in context.Messages.Where(p => p.UserId == lmessage.Patient.ProviderId && p.PatientId == lmessage.Patient.PatientLoginId)
                                         select m).OrderByDescending(x => x.Datetime).FirstOrDefault();
                        if (lmsg != null)
                        {
                            lmessage.replyStatus = lmsg.SentReceivedFlag;
                        }
                        else
                        {
                            lmessage.replyStatus = 1;
                        }
                        msgList.Add(lmessage);
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return msgList;
        }


        public List<Patient> GetAllPatients()
        {
            List<Patient> result = null;
            try
            {
                result = (from p in context.Patient
                          select p).ToList();

            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public List<PatientMessage> GetAllPatientStatus(string userId)
        {
            List<PatientMessage> msgList = new List<PatientMessage>();
            List<Patient> result = null;
            try
            {
                result = (from p in context.Patient
                          select p).ToList();

                if (result != null && result.Count > 0)
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        PatientMessage lmessage = new PatientMessage();
                        lmessage.Patient = result[i];
                        Messages lmsg = (from m in context.Messages.Where(p => p.UserId == userId && p.PatientId == lmessage.Patient.PatientLoginId)
                                         select m).OrderByDescending(x => x.Datetime).FirstOrDefault();
                        if (lmsg != null)
                        {
                            lmessage.replyStatus = lmsg.SentReceivedFlag;
                        }
                        else
                        {
                            lmessage.replyStatus = 1;
                        }
                        msgList.Add(lmessage);
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return msgList;
        }



        public PatientView PatientLoginsReturnPatientView(string patientPhone, string PIN)
        {
            PatientView result = null;
            try
            {
                var _patient = (from p in context.Patient.Where(p => p.PhoneNumber == patientPhone && p.Pin == int.Parse(PIN))
                                select p).FirstOrDefault();
                if (_patient != null)
                {
                    _patient.LoginSessionId = Guid.NewGuid();
                    context.Entry(_patient).State = EntityState.Modified;
                    context.SaveChanges();

                    result = new PatientView();
                    result = PatientExtension.PatientToPatientView(_patient);

                    User pUser = (from p in context.User.Where(p => p.UserId == result.PhoneNumber) select p).FirstOrDefault();
                    if (pUser != null)
                    {
                        result.VSeeId = pUser.Vseeid;
                    }

                }

            }
            catch (Exception ex)
            {

            }
            return result;
        }


        public PatientLoginView PatientLoginsReturnPatientLoginView(string patientPhone, string PIN)
        {
            PatientLoginView result = null;
            try
            {
                var _patient = (from p in context.Patient.Where(p => p.PhoneNumber == patientPhone && p.Pin == int.Parse(PIN))
                                select p).FirstOrDefault();
                if (_patient != null)
                {
                    _patient.LoginSessionId = Guid.NewGuid();
                    context.Entry(_patient).State = EntityState.Modified;
                    context.SaveChanges();

                    result = (from p in context.Patient
                              join rx in context.PatientRx on p.PatientId equals rx.PatientId

                              join therapist in context.User on p.Therapistid equals therapist.UserId into the
                              from ltherapist in the.DefaultIfEmpty()

                              join provider in context.User on p.ProviderId equals provider.UserId into pro
                              from lprovider in pro.DefaultIfEmpty()

                              join patientconfig in context.PatientConfiguration on p.PatientId equals patientconfig.PatientId into pconfig
                              from patconfig in pconfig.DefaultIfEmpty()

                              join vsee in context.User on p.PatientLoginId equals vsee.UserId
                              where p.PatientId == _patient.PatientId
                              select new PatientLoginView
                              {
                                  PatientLoginId = p.PatientLoginId,
                                  PatientId = p.PatientId,
                                  PatientFirstName = p.PatientName,
                                  TherapistId = ltherapist != null ? ltherapist.UserId : "",
                                  TherapistName = ltherapist != null ? ltherapist.Name : "",
                                  TherapistContactNo = ltherapist != null ? ltherapist.Phone : "",
                                  ProviderId = lprovider != null ? lprovider.UserId : "",
                                  ProviderName = lprovider != null ? lprovider.Name : "",
                                  ProviderContactNo = lprovider != null ? lprovider.Phone : "",
                                  InstallerId = patconfig != null ? (from p in context.User where p.UserId == patconfig.InstallerId select p).FirstOrDefault().UserId : "",
                                  InstallerName = patconfig != null ? (from p in context.User where p.UserId == patconfig.InstallerId select p).FirstOrDefault().Name : "",
                                  InstallerContactNo = patconfig != null ? (from p in context.User where p.UserId == patconfig.InstallerId select p).FirstOrDefault().Phone : "",
                                  RxStartDate = rx.RxStartDate.HasValue ? Convert.ToDateTime(rx.RxStartDate).ToString("yyyy-MM-dd'T'HH:mm:ss") : "",
                                  RxEndDate = rx.RxEndDate.HasValue ? Convert.ToDateTime(rx.RxEndDate).ToString("yyyy-MM-dd'T'HH:mm:ss") : "",
                                  LoginSessionId = p.LoginSessionId,
                                  VSeeId = vsee.Vseeid
                              }).FirstOrDefault();
                }

            }
            catch (Exception ex)
            {

            }
            return result;
        }


        public PatientLoginView PatientLoginsReturnPatientLoginViewUsingPatientLoginId(string patientloinId, string PIN)
        {
            PatientLoginView result = null;
            try
            {
                var _patient = (from p in context.Patient.Where(p => p.PatientLoginId == patientloinId && p.Pin == int.Parse(PIN))
                                select p).FirstOrDefault();
                if (_patient != null)
                {
                    _patient.LoginSessionId = Guid.NewGuid();
                    context.Entry(_patient).State = EntityState.Modified;
                    var res = context.SaveChanges();
                    if (res > 0)
                    {
                        User luser = (from p in context.User
                                      where p.UserId == patientloinId && p.Password == PIN && p.Type == 5
                                      select p).FirstOrDefault();
                        if (luser != null)
                        {
                            luser.LoginSessionId = _patient.LoginSessionId.ToString();
                            context.Entry(luser).State = EntityState.Modified;
                            int response = context.SaveChanges();

                            if (response > 0)
                            {

                                result = (from p in context.Patient
                                          join rx in context.PatientRx on p.PatientId equals rx.PatientId

                                          join therapist in context.User on p.Therapistid equals therapist.UserId into the
                                          from ltherapist in the.DefaultIfEmpty()

                                          join provider in context.User on p.ProviderId equals provider.UserId into pro
                                          from lprovider in pro.DefaultIfEmpty()

                                          join patientconfig in context.PatientConfiguration on p.PatientId equals patientconfig.PatientId into pconfig
                                          from patconfig in pconfig.DefaultIfEmpty()

                                          join vsee in context.User on p.PatientLoginId equals vsee.UserId into vseeconfig
                                          from vseeinfo in vseeconfig.DefaultIfEmpty()

                                          where p.PatientId == _patient.PatientId
                                          select new PatientLoginView
                                          {
                                              PatientLoginId = p.PatientLoginId,
                                              PatientId = p.PatientId,
                                              PatientFirstName = p.PatientName,
                                              TherapistId = ltherapist != null ? ltherapist.UserId : "",
                                              TherapistName = ltherapist != null ? ltherapist.Name : "",
                                              TherapistContactNo = ltherapist != null ? ltherapist.Phone : "",
                                              ProviderId = lprovider != null ? lprovider.UserId : "",
                                              ProviderName = lprovider != null ? lprovider.Name : "",
                                              ProviderContactNo = lprovider != null ? lprovider.Phone : "",
                                              InstallerId = patconfig != null ? (from p in context.User where p.UserId == patconfig.InstallerId select p).FirstOrDefault().UserId : "",
                                              InstallerName = patconfig != null ? (from p in context.User where p.UserId == patconfig.InstallerId select p).FirstOrDefault().Name : "",
                                              InstallerContactNo = patconfig != null ? (from p in context.User where p.UserId == patconfig.InstallerId select p).FirstOrDefault().Phone : "",
                                              RxStartDate = rx.RxStartDate.HasValue ? Convert.ToDateTime(rx.RxStartDate).ToString("yyyy-MM-dd'T'HH:mm:ss") : "",
                                              RxEndDate = rx.RxEndDate.HasValue ? Convert.ToDateTime(rx.RxEndDate).ToString("yyyy-MM-dd'T'HH:mm:ss") : "",
                                              LoginSessionId = p.LoginSessionId,
                                              VSeeId = vseeinfo != null ? vseeinfo.Vseeid : ""
                                          }).FirstOrDefault();
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public Patient GetPatientByPhone(string patientPhone, string PIN)
        {
            Patient result = null;
            try
            {
                var _patient = (from p in context.Patient.Where(p => p.PhoneNumber == patientPhone && p.Pin == int.Parse(PIN))
                                select p).FirstOrDefault();
                result = _patient;

            }
            catch (Exception ex)
            {

            }
            return result;
        }
        public Patient GetPatientByPatientLoginId(string patientloginid, string PIN)
        {
            Patient result = null;
            try
            {
                var _patient = (from p in context.Patient.Where(p => p.PatientLoginId == patientloginid && p.Pin == int.Parse(PIN))
                                select p).FirstOrDefault();
                result = _patient;

            }
            catch (Exception ex)
            {

            }
            return result;
        }
        public Patient GetPaitentbyTherapistIDandPatientLoginId(string PatientLoginId, string therapistId)
        {
            Patient _patient = (from p in context.Patient
                                where p.PatientLoginId == PatientLoginId && p.Therapistid == therapistId
                                select p).FirstOrDefault();
            return _patient;

        }

        public Patient GetPatientBySessionID(string sessionId)
        {
            Patient result = null;
            try
            {
                var _patient = (from p in context.Patient.Where(p => p.LoginSessionId == new Guid(sessionId))
                                select p).FirstOrDefault();
                result = _patient;

            }
            catch (Exception ex)
            {

            }
            return result;
        }

        public Patient GetPatientByPatientID(int patientId)
        {
            Patient result = null;
            try
            {
                var _patient = (from p in context.Patient
                                where p.PatientId == patientId
                                select p).FirstOrDefault();
                result = _patient;

            }
            catch (Exception ex)
            {

            }
            return result;
        }
    }
}
