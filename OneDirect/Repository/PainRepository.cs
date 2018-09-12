using OneDirect.Helper;
using OneDirect.Models;
using OneDirect.Repository.Interface;
using OneDirect.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneDirect.Repository
{
    public class PainRepository : IPainInterface
    {
        private OneDirectContext context;

        public PainRepository(OneDirectContext context)
        {
            this.context = context;
        }


        public Pain getPain(string PainId)
        {
            return (from p in context.Pain
                    where p.PainId == PainId
                    select p).FirstOrDefault();
        }

        public List<Pain> getPainbyPatientIdAndRxId(int patientId, string RxId)
        {
            return (from p in context.Pain
                    where p.PatientId == patientId && p.RxId == RxId
                    select p).ToList();
        }
        public string DeletePain(string painId)
        {
            try
            {
                var pain = (from p in context.Pain where p.PainId == painId select p).ToList();
                context.Pain.RemoveRange(pain);
                context.SaveChanges();

            }
            catch (Exception ex)
            {
                return "fail";
            }
            return "success";
        }
        public List<SessionPain> getPainBySessionId(string lSessionID)
        {
            return (from p in context.Pain
                    join _protocol in context.Protocol on p.ProtocolId equals _protocol.ProtocolId
                    where p.SessionId == lSessionID
                    select new SessionPain
                    {
                        PatientId = p.PatientId,
                        RxId = p.RxId,
                        ProtocolId = p.ProtocolId,
                        SessionId = p.SessionId,
                        PainId = p.PainId,
                        Angle = p.Angle,
                        RepeatNumber = p.RepeatNumber,
                        PainLevel = p.PainLevel,
                        FlexionRepNumber = p.FlexionRepNumber,
                        ExtensionRepNumber = p.ExtensionRepNumber,
                        Protocoltype = getProtocolType(_protocol.EquipmentType, _protocol.ProtocolEnum, _protocol.DeviceConfiguration)
                    }).ToList();
        }
        private string getProtocolType(string limb, int? proenum, string exenum)
        {
            List<ExcerciseProtocol> ExcerciseProtocollist = Utilities.GetExcerciseProtocol();
            ExcerciseProtocol _EquipmentExcercise = ExcerciseProtocollist.Where(p => p.Limb.ToLower() == limb.ToLower() && p.ProtocolEnum == Convert.ToInt32(proenum) && p.ExcerciseEnum == exenum).FirstOrDefault();
            return _EquipmentExcercise.ProtocolName;
        }
        private string getExcercise(string limb, string exenum)
        {
            List<EquipmentExcercise> EquipmentExcercise = Utilities.GetEquipmentExcercise();
            EquipmentExcercise _EquipmentExcercise = EquipmentExcercise.Where(p => p.Limb.ToLower() == limb.ToLower() && p.ExcerciseEnum == exenum).FirstOrDefault();
            return _EquipmentExcercise.ExcerciseName;
        }
        public void InsertPain(Pain pPain)
        {
            context.Pain.Add(pPain);
            context.SaveChanges();
        }

        public void UpdatePain(Pain pPain)
        {
            var _pain = (from p in context.Pain
                         where p.PainId == pPain.PainId
                         select p).FirstOrDefault();
            if (_pain != null)
            {
                context.Entry(_pain).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                context.SaveChanges();
            }
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
