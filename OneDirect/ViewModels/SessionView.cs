using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OneDirect.ViewModels
{
    public class SessionView
    {
        public int PatientId { get; set; }
        public string RxId { get; set; }

        [Required]
        public string ProtocolId { get; set; }
        public string SessionId { get; set; }
        [Required]
        public int? Reps { get; set; }
        [Required]
        public int? Duration { get; set; }
        public int? FlexionReps { get; set; }
        public int? ExtensionReps { get; set; }
        [Required]
        public DateTime? SessionDate { get; set; }
        [Required]
        public int? PainCount { get; set; }
        [Required]
        public int MaxFlexion { get; set; }
        [Required]
        public int MaxExtension { get; set; }
        [Required]
        public int MaxPain { get; set; }
        public string Patname { get; set; }
        public string EType { get; set; }
        public string EEnum { get; set; }

        public int? Boom1Position { get; set; }
        public int? Boom2Position { get; set; }
        public int? RangeDuration { get; set; }

    }
}
