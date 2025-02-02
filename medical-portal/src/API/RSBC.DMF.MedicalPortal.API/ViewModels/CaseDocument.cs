﻿using System;

namespace RSBC.DMF.MedicalPortal.API.ViewModels
{
    [Obsolete("This class is obsolete, use PatientCase instead.")]
    public class CaseDocument
    {
        public int DmerType { get; set; }
        public int DmerStatus { get; set; }
        public string CaseNumber { get; set; }
        public string FullName { get; set; }
        public DateTime? Birthday { get; set; }
        public DateTime? ComplianceDate { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public string Description { get; set; }
        public string DocumentUrl { get; set; }
        public string SubmittalStatus { get; set; }
        public string DocumentType { get; set; }
        public string DocumentSubType { get; set; }
        public string DocumentId { get; set; }
        public string LoginId { get; set; }

        // This field is Uploaded Date on the document
        public DateTimeOffset? FaxReceivedDate { get; set; }

        public bool showOnPortals { get; set; }
    }
}
