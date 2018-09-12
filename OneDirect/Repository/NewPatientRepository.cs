﻿using OneDirect.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OneDirect.ViewModels;
using OneDirect.Models;
using OneDirect.Helper;
using Microsoft.EntityFrameworkCore;

namespace OneDirect.Repository
{
    public class NewPatientRepository : INewPatient
    {
        private OneDirectContext context;

        public NewPatientRepository(OneDirectContext context)
        {
            this.context = context;
        }
        public List<PatientRx> CreateNewPatientByProvider(NewPatientWithProtocol NewPatient)
        {
            List<PatientRx> lrxList = new List<PatientRx>();
            DateTime? RXSD = null;
            DateTime? RXED = null;
            try
            {
                Patient patient = new Patient();
                patient.PatientName = NewPatient.NewPatient.PatientName;
                patient.ProviderId = NewPatient.ProviderId;
                patient.Dob = NewPatient.NewPatient.Dob;
                patient.AddressLine = NewPatient.NewPatient.AddressLine;
                patient.EquipmentType = NewPatient.NewPatient.EquipmentType;
                patient.State = NewPatient.NewPatient.State;
                patient.City = NewPatient.NewPatient.City;
                patient.Zip = NewPatient.NewPatient.Zip;
                patient.PhoneNumber = NewPatient.NewPatient.PhoneNumber;
                patient.SurgeryDate = NewPatient.NewPatient.SurgeryDate;
                patient.Ssn = NewPatient.NewPatient.Ssn;
                patient.Side = NewPatient.NewPatient.Side;
                patient.Therapistid = NewPatient.NewPatient.TherapistId;
                patient.Paid = NewPatient.NewPatient.PatientAdminId;
                patient.DateCreated = DateTime.UtcNow;
               

                string patName = NewPatient.NewPatient.Ssn + NewPatient.NewPatient.PhoneNumber.Substring(NewPatient.NewPatient.PhoneNumber.Length - 4);
                patient.PatientLoginId = patName;

                context.Patient.Add(patient);
                context.SaveChanges();
                int count = 0;
                foreach (NewPatientRx _PatientRx in NewPatient.NewPatientRXList)
                {
                    if (count == 0)
                    {
                        RXSD = _PatientRx.RxStartDate;
                        RXED = _PatientRx.RxEndDate;
                        count++;
                    }
                    PatientRx patRx = new PatientRx();
                    patRx.RxId = Guid.NewGuid().ToString();
                    patRx.EquipmentType = NewPatient.NewPatient.EquipmentType;
                    patRx.DeviceConfiguration = _PatientRx.DeviceConfiguration;
                    patRx.RxStartDate = RXSD;
                    patRx.RxEndDate = RXED;
                    patRx.ProviderId = NewPatient.ProviderId;
                    patRx.PatientId = patient.PatientId;
                    patRx.CurrentFlexion = _PatientRx.CurrentFlexion;
                    patRx.CurrentExtension = _PatientRx.CurrentExtension;
                    patRx.GoalFlexion = _PatientRx.GoalFlexion;
                    patRx.GoalExtension = _PatientRx.GoalExtension;
                    patRx.RxDaysPerweek = 2;
                    patRx.RxSessionsPerWeek = 3;
                    patRx.Active = true;
                    patRx.DateCreated = DateTime.UtcNow;
                    patRx.PatientSide = patient.Side;
                    patRx.PainThreshold = NewPatient.PainThreshold;
                    patRx.RateOfChange = NewPatient.RateOfChange;
                    context.PatientRx.Add(patRx);
                    int response = context.SaveChanges();
                    if (response > 0)
                    {
                        patRx.Session = null;
                       
                        lrxList.Add(patRx);
                    }
                   
                }
                if (patient.PatientId > 0)
                {
                   
                    return lrxList;
                }
                else
                    return lrxList;
            }
            catch (Exception ex)
            {
                return lrxList;
            }
        }

        public Protocol CreateProtocol(NewProtocol protocol)
        {
            try
            {
                Protocol pro = new Protocol();
                if (String.IsNullOrEmpty(protocol.ProtocolId))
                {
                    pro.ProtocolId = Guid.NewGuid().ToString();
                    pro.ProtocolEnum = Convert.ToInt32(protocol.ExcerciseName);
                }
                else
                {
                    pro = (from p in context.Protocol
                           where p.ProtocolId == protocol.ProtocolId
                           select p).FirstOrDefault();
                   
                }
                pro.ProtocolName = protocol.ProtocolName;
                pro.PatientId = protocol.PatientId;
                pro.RxId = protocol.RxId;
                pro.FlexUpLimit = protocol.FlexUpLimit;
                pro.StretchUpLimit = protocol.StretchUpLimit;
                pro.FlexDownLimit = protocol.FlexDownLimit;
                pro.StretchDownLimit = protocol.StretchDownLimit;
                pro.FlexDownHoldtime = protocol.FlexDownHoldtime;
                pro.FlexUpHoldtime = protocol.FlexUpHoldtime;
                pro.EquipmentType = protocol.EquipmentType;
                pro.DeviceConfiguration = protocol.ExcerciseEnum;
                pro.StretchUpHoldtime = protocol.StretchUpHoldtime;
                pro.StretchDownHoldtime = protocol.StretchDownHoldtime;
                pro.RestPosition = protocol.RestPosition;

                pro.StartDate = protocol.StartDate;
                pro.EndDate = protocol.EndDate;
                pro.RestAt = protocol.RestAt;
                pro.RepsAt = protocol.RepsAt;
                pro.Speed = protocol.Speed;
                pro.Time = protocol.Time;
                pro.RestTime = protocol.RestTime;
                pro.Reps = protocol.Reps;
                if (String.IsNullOrEmpty(protocol.ProtocolId))
                    context.Protocol.Add(pro);
                else
                    context.Entry(pro).State = EntityState.Modified;
                int res = context.SaveChanges();
                if (res > 0)
                    return pro;
                else
                    return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public NewProtocol GetProtocolByproId(string proId)
        {
            return (from p in context.Protocol
                    join pat in context.Patient on p.PatientId equals pat.PatientId
                    where p.ProtocolId == proId
                    select new NewProtocol
                    {
                        ProtocolId = p.ProtocolId,
                        PatientId = p.PatientId,
                        RxId = p.RxId,
                        ProtocolName = p.ProtocolName,
                        RestPosition = p.RestPosition,
                        MaxUpLimit = p.MaxUpLimit,
                        StretchUpLimit = p.StretchUpLimit,
                        MaxDownLimit = p.MaxDownLimit,
                        StretchDownLimit = p.StretchDownLimit,
                        RestAt = p.RestAt,
                        RepsAt = p.RepsAt,
                        Speed = p.Speed,
                        Reps = p.Reps,
                        EquipmentType = p.EquipmentType,
                        FlexUpLimit = p.FlexUpLimit,
                       
                        StretchUpHoldtime = p.StretchUpHoldtime,
                        FlexDownLimit = p.FlexDownLimit,
                       
                        StretchDownHoldtime = p.StretchDownHoldtime,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        
                        RestTime = p.RestTime,
                        Time = p.Time,
                        SurgeryDate = p.Rx.RxStartDate,
                        PatientName = pat.PatientName,
                        ExcerciseEnum = p.DeviceConfiguration,
                        ProtocolEnum = p.ProtocolEnum,
                        ExcerciseName = getProtocolType(p.EquipmentType, p.ProtocolEnum, p.DeviceConfiguration),
                        Rx = p.Rx,
                        RxEndDate = p.Rx.RxEndDate
                    }).FirstOrDefault();
        }


        public List<NewProtocol> GetProtocolByRxId(string rxId)
        {
            return (from p in context.Protocol
                    join pat in context.Patient on p.PatientId equals pat.PatientId
                    where p.RxId == rxId && p.ProtocolName.StartsWith("Initial")
                    select new NewProtocol
                    {
                        ProtocolId = p.ProtocolId,
                        PatientId = p.PatientId,
                        RxId = p.RxId,
                        ProtocolName = p.ProtocolName,
                        RestPosition = p.RestPosition,
                        MaxUpLimit = p.MaxUpLimit,
                        StretchUpLimit = p.StretchUpLimit,
                        MaxDownLimit = p.MaxDownLimit,
                        StretchDownLimit = p.StretchDownLimit,
                        RestAt = p.RestAt,
                        RepsAt = p.RepsAt,
                        Speed = p.Speed,
                        Reps = p.Reps,
                        EquipmentType = p.EquipmentType,
                        FlexUpLimit = p.FlexUpLimit,
                       
                        StretchUpHoldtime = p.StretchUpHoldtime,
                        FlexDownLimit = p.FlexDownLimit,
                        
                        StretchDownHoldtime = p.StretchDownHoldtime,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        
                        RestTime = p.RestTime,
                        Time = p.Time,
                        SurgeryDate = p.Rx.RxStartDate,
                        PatientName = pat.PatientName,
                        ExcerciseEnum = p.DeviceConfiguration,
                        ProtocolEnum = p.ProtocolEnum,
                        ExcerciseName = getProtocolType(p.EquipmentType, p.ProtocolEnum, p.DeviceConfiguration),
                        Rx = p.Rx,
                        RxEndDate = p.Rx.RxEndDate
                    }).ToList();
        }

        public PatientRx GetNewPatientRxByRxId(string Rxid)
        {
            return (from p in context.PatientRx.Include(x => x.Patient)
                    where p.RxId == Rxid
                    select p).FirstOrDefault();
        }

        public List<Protocol> GetProtocolListByPatientId(string patId)
        {
            return (from p in context.Protocol
                    where p.PatientId == Convert.ToInt32(patId)
                    select p).ToList();
        }

        public List<NewProtocol> GetProtocolListBypatId(string patId)
        {
            return (from p in context.Protocol
                    let cCount = (from c in context.Session where p.ProtocolId == c.ProtocolId select c).Count()
                    where p.PatientId == Convert.ToInt32(patId)
                    select new NewProtocol
                    {
                        ProtocolId = p.ProtocolId,
                        PatientId = p.PatientId,
                        RxId = p.RxId,
                        ProtocolName = p.ProtocolName,
                        RestPosition = p.RestPosition,
                        MaxUpLimit = p.MaxUpLimit,
                        StretchUpLimit = p.StretchUpLimit,
                        MaxDownLimit = p.MaxDownLimit,
                        StretchDownLimit = p.StretchDownLimit,
                        RestAt = p.RestAt,
                        RepsAt = p.RepsAt,
                        Speed = p.Speed,
                        Reps = p.Reps,
                        EquipmentType = p.EquipmentType,
                        FlexUpLimit = p.FlexUpLimit,
                        FlexUpHoldtime = p.FlexUpHoldtime,
                        StretchUpHoldtime = p.StretchUpHoldtime,
                        FlexDownLimit = p.FlexDownLimit,
                        FlexDownHoldtime = p.FlexDownHoldtime,
                        StretchDownHoldtime = p.StretchDownHoldtime,
                        UpReps = p.UpReps,
                        DownReps = p.DownReps,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        Time = p.Time,
                        RestTime = p.RestTime,
                        ExcerciseEnum = p.DeviceConfiguration,
                        ProtocolEnum = p.ProtocolEnum,
                        ExcerciseName = getProtocolType(p.EquipmentType, p.ProtocolEnum, p.DeviceConfiguration),
                        Rx = p.Rx,
                        SessionCount = cCount,
                        PatientName = ""
                    }).ToList();
        }
        private string getProtocolType(string limb, int? proenum, string exenum)
        {
            List<ExcerciseProtocol> ExcerciseProtocollist = Utilities.GetExcerciseProtocol();
            ExcerciseProtocol _EquipmentExcercise = ExcerciseProtocollist.Where(p => p.Limb.ToLower() == limb.ToLower() && p.ProtocolEnum == Convert.ToInt32(proenum) && p.ExcerciseEnum == exenum).FirstOrDefault();
            return _EquipmentExcercise != null ? _EquipmentExcercise.ProtocolName : "";
        }
        private string getExcercise(string limb, string exenum)
        {
            List<EquipmentExcercise> EquipmentExcercise = Utilities.GetEquipmentExcercise();
            EquipmentExcercise _EquipmentExcercise = EquipmentExcercise.Where(p => p.Limb.ToLower() == limb.ToLower() && p.ExcerciseEnum == exenum).FirstOrDefault();
            return _EquipmentExcercise.ExcerciseName;
        }

        public Patient UpdatePatient(NewPatient NewPatient)
        {
            try
            {
                Patient patient = (from p in context.Patient
                                   where p.PatientId == NewPatient.PatientId
                                   select p).FirstOrDefault();
                if (patient != null)
                {
                    patient.PatientName = NewPatient.PatientName;
                    patient.PatientId = NewPatient.PatientId;
                    patient.ProviderId = NewPatient.ProviderId;
                    patient.Therapistid = NewPatient.TherapistId;
                    patient.Paid = NewPatient.PatientAdminId;
                    patient.Dob = NewPatient.Dob;
                    patient.AddressLine = NewPatient.AddressLine;
                    patient.EquipmentType = NewPatient.EquipmentType;
                    patient.State = NewPatient.State;
                    patient.City = NewPatient.City;
                    patient.Zip = NewPatient.Zip;
                    patient.Side = NewPatient.Side;
                    patient.PhoneNumber = NewPatient.PhoneNumber;
                    patient.SurgeryDate = NewPatient.SurgeryDate;
                    

                    patient.DateModified = DateTime.UtcNow;
                    context.Entry(patient).State = EntityState.Modified;
                    int res = context.SaveChanges();
                    if (res > 0)
                    {
                        return patient;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<PatientRx> UpdatePatientRx(List<NewPatientRx> NewPatientRxs, int PainThreshold = 0, int RateOfChange = 0, string usertype = "")
        {
            try
            {
                List<PatientRx> lrxlist = new List<PatientRx>();
                DateTime? RXSD = null;
                DateTime? RXED = null;
                int count = 0;
                foreach (NewPatientRx NewPatientRx in NewPatientRxs)
                {
                    if (count == 0)
                    {
                        RXSD = NewPatientRx.RxStartDate;
                        RXED = NewPatientRx.RxEndDate;
                        count++;
                    }
                    PatientRx patRx = (from p in context.PatientRx
                                       where p.RxId == NewPatientRx.RxId
                                       select p).FirstOrDefault();
                    if (patRx != null)
                    {
                        //Prabhu - insert RomChangeLog
                        RomchangeLog plog = new RomchangeLog();
                        plog.RxId = patRx.RxId;
                        plog.PreviousFlexion = patRx.CurrentFlexion.HasValue ? Convert.ToInt32(patRx.CurrentFlexion) : 0;
                        plog.PreviousExtension = patRx.CurrentExtension.HasValue ? Convert.ToInt32(patRx.CurrentExtension) : 0;
                        plog.CreatedDate = DateTime.UtcNow;

                        if (usertype == ConstantsVar.Admin.ToString())
                        {
                            plog.ChangedBy = "Admin";
                        }
                        else if (usertype == ConstantsVar.Support.ToString())
                        {
                            plog.ChangedBy = "Support";
                        }
                        else if (usertype == ConstantsVar.Therapist.ToString())
                        {
                            plog.ChangedBy = "Therapist";
                        }
                        else if (usertype == ConstantsVar.PatientAdministrator.ToString())
                        {
                            plog.ChangedBy = "PatientAdministrator";
                        }
                        else if (usertype == ConstantsVar.Provider.ToString())
                        {
                            plog.ChangedBy = "Provider";
                        }

                        context.RomchangeLog.Add(plog);
                        int res = context.SaveChanges();

                        patRx.RxId = NewPatientRx.RxId;
                        patRx.EquipmentType = NewPatientRx.EquipmentType;
                        patRx.DeviceConfiguration = NewPatientRx.DeviceConfiguration;
                        patRx.RxStartDate = RXSD;
                        patRx.RxEndDate = RXED;
                        patRx.ProviderId = NewPatientRx.ProviderId;
                        patRx.PatientId = NewPatientRx.PatientId;
                        patRx.CurrentFlexion = NewPatientRx.CurrentFlexion;
                        patRx.CurrentExtension = NewPatientRx.CurrentExtension;
                        patRx.GoalFlexion = NewPatientRx.GoalFlexion;
                        patRx.GoalExtension = NewPatientRx.GoalExtension;
                        patRx.PainThreshold = PainThreshold;
                        patRx.RateOfChange = RateOfChange;
                        patRx.Active = true;
                        patRx.DateModified = DateTime.UtcNow;
                        context.Entry(patRx).State = EntityState.Modified;
                        int response = context.SaveChanges();
                        if (response > 0)
                        {
                            lrxlist.Add(patRx);
                        }
                    }
                }
                return lrxlist;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public int ChangeRxCurrent(string RxID, int CurrentFlexion, int CurrentExtension, string Code)
        {
            int result = 0;
            PatientRx patRx = (from p in context.PatientRx
                               where p.RxId == RxID
                               select p).FirstOrDefault();
            if (patRx != null)
            {
                RomchangeLog plog = new RomchangeLog();
                plog.RxId = patRx.RxId;
                plog.PreviousFlexion = patRx.CurrentFlexion.HasValue ? Convert.ToInt32(patRx.CurrentFlexion) : 0;
                plog.PreviousExtension = patRx.CurrentExtension.HasValue ? Convert.ToInt32(patRx.CurrentExtension) : 0;
                plog.CreatedDate = DateTime.UtcNow;
                plog.ChangedBy = Code;
                context.RomchangeLog.Add(plog);
                int res = context.SaveChanges();
                if (res > 0)
                {

                    patRx.CurrentFlexion = CurrentFlexion;
                    patRx.CurrentExtension = CurrentExtension;
                    patRx.DateModified = DateTime.UtcNow;
                    context.Entry(patRx).State = EntityState.Modified;
                    result = context.SaveChanges();
                }
            }
            return result;
        }
        public int ChangeRxCurrentExtension(string RxID, int CurrentExtension, string Code)
        {
            int result = 0;
            PatientRx patRx = (from p in context.PatientRx
                               where p.RxId == RxID
                               select p).FirstOrDefault();
            if (patRx != null)
            {
                RomchangeLog plog = new RomchangeLog();
                plog.RxId = patRx.RxId;
                plog.PreviousFlexion = patRx.CurrentFlexion.HasValue ? Convert.ToInt32(patRx.CurrentFlexion) : 0;
                plog.PreviousExtension = patRx.CurrentExtension.HasValue ? Convert.ToInt32(patRx.CurrentExtension) : 0;
                plog.CreatedDate = DateTime.UtcNow;
                plog.ChangedBy = Code;
                context.RomchangeLog.Add(plog);
                int res = context.SaveChanges();
                if (res > 0)
                {
                    patRx.CurrentExtension = CurrentExtension;
                    patRx.DateModified = DateTime.UtcNow;
                    context.Entry(patRx).State = EntityState.Modified;
                    result = context.SaveChanges();
                }
            }
            return result;
        }
        public int ChangeRxCurrentFlexion(string RxID, int CurrentFlexion, string Code)
        {
            int result = 0;
            PatientRx patRx = (from p in context.PatientRx
                               where p.RxId == RxID
                               select p).FirstOrDefault();
            if (patRx != null)
            {
                RomchangeLog plog = new RomchangeLog();
                plog.RxId = patRx.RxId;
                plog.PreviousFlexion = patRx.CurrentFlexion.HasValue ? Convert.ToInt32(patRx.CurrentFlexion) : 0;
                plog.PreviousExtension = patRx.CurrentExtension.HasValue ? Convert.ToInt32(patRx.CurrentExtension) : 0;
                plog.CreatedDate = DateTime.UtcNow;
                plog.ChangedBy = Code;
                context.RomchangeLog.Add(plog);
                int res = context.SaveChanges();
                if (res > 0)
                {

                    patRx.CurrentFlexion = CurrentFlexion;
                    patRx.DateModified = DateTime.UtcNow;
                    context.Entry(patRx).State = EntityState.Modified;
                    result = context.SaveChanges();
                }
            }
            return result;
        }
        public PatientRx GetPatientRxByPatIdandDeviceConfig(int patId, string deviceConfig)
        {
            return (from p in context.PatientRx
                    where p.PatientId == patId && p.DeviceConfiguration.ToLower() == deviceConfig.ToLower()
                    select p).FirstOrDefault();
        }

        public NewPatient GetPatientByPatId(int PatId)
        {
            return (from p in context.Patient
                    where p.PatientId == PatId
                    select new NewPatient
                    {
                        PatientLoginId = p.PatientLoginId,
                        PatientId = p.PatientId,
                        PatientName = p.PatientName,
                        ProviderId = p.ProviderId,
                        TherapistId = p.Therapistid,
                        PatientAdminId = p.Paid,
                        Dob = p.Dob,
                        AddressLine = p.AddressLine,
                        EquipmentType = p.EquipmentType,
                        State = p.State,
                        City = p.City,
                        Zip = p.Zip,
                        PhoneNumber = p.PhoneNumber,
                        SurgeryDate = p.SurgeryDate,
                        Ssn = p.Ssn,
                        Side = p.Side,
                        Pin = p.Pin,
                        DateModified = p.DateModified,
                        DateCreated = p.DateCreated
                    }).FirstOrDefault();
        }

        public NewPatient GetPatientByPatitentLoginId(string patLoginId)
        {
            return (from p in context.Patient
                    where p.PatientLoginId == patLoginId
                    select new NewPatient
                    {
                        PatientLoginId = p.PatientLoginId,
                        PatientId = p.PatientId,
                        PatientName = p.PatientName,
                        ProviderId = p.ProviderId,
                        TherapistId = p.Therapistid,
                        PatientAdminId = p.Paid,
                        Dob = p.Dob,
                        AddressLine = p.AddressLine,
                        EquipmentType = p.EquipmentType,
                        State = p.State,
                        City = p.City,
                        Zip = p.Zip,
                        PhoneNumber = p.PhoneNumber,
                        SurgeryDate = p.SurgeryDate,
                        Ssn = p.Ssn,
                        Side = p.Side,
                        Pin = p.Pin,
                        DateModified = p.DateModified,
                        DateCreated = p.DateCreated
                    }).FirstOrDefault();
        }
        public List<PatientRx> GetNewPatientRxByPatId(string Patid)
        {
            return (from p in context.PatientRx.Include(x => x.Patient)
                    where p.PatientId == Convert.ToInt32(Patid)
                    orderby p.DeviceConfiguration descending
                    select p).ToList();

          
        }

        public string DeleteProtocolRecordsWithCasecade(string proId)
        {
            
            try
            {
                var pain = (from p in context.Pain where p.ProtocolId == proId select p).ToList();
                context.Pain.RemoveRange(pain);
                context.SaveChanges();
                var session = (from p in context.Session where p.ProtocolId == proId select p).ToList();
                context.Session.RemoveRange(session);
                context.SaveChanges();
                var protocol = (from p in context.Protocol where p.ProtocolId == proId select p).ToList();
                context.Protocol.RemoveRange(protocol);
                context.SaveChanges();

            }
            catch (Exception ex)
            {
                return "fail";
            }
            return "success";
        }
    }
}
