using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneDirect.ViewModels
{
    public class SessionPain
    {
        public int PatientId { get; set; }
        public string RxId { get; set; }
        public string ProtocolId { get; set; }
        public string SessionId { get; set; }
        public string PainId { get; set; }
        public int? Angle { get; set; }
        public int? RepeatNumber { get; set; }
        public int? PainLevel { get; set; }
        public int? FlexionRepNumber { get; set; }
        public int? ExtensionRepNumber { get; set; }
        public string Protocoltype { get; set; }
      
}
}
