//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using Microsoft.ML.Data;

namespace Readmission_risk.Model
{
    public class ModelInput
    {
        [ColumnName("PatientId"), LoadColumn(0)]
        public string PatientId { get; set; }


        [ColumnName("Age"), LoadColumn(1)]
        public float Age { get; set; }


        [ColumnName("Gender"), LoadColumn(2)]
        public string Gender { get; set; }


        [ColumnName("DiagnosisCode"), LoadColumn(3)]
        public string DiagnosisCode { get; set; }


        [ColumnName("PreviousAdmissions"), LoadColumn(4)]
        public float PreviousAdmissions { get; set; }


        [ColumnName("LengthOfStay"), LoadColumn(5)]
        public float LengthOfStay { get; set; }


        [ColumnName("HasComorbidities"), LoadColumn(6)]
        public bool HasComorbidities { get; set; }


        [ColumnName("Readmitted"), LoadColumn(7)]
        public string Readmitted { get; set; }


    }
}
