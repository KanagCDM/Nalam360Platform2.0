//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using Microsoft.ML.Data;

namespace Length_of_stay.Model
{
    public class ModelInput
    {
        [ColumnName("PatientId"), LoadColumn(0)]
        public string PatientId { get; set; }


        [ColumnName("Age"), LoadColumn(1)]
        public float Age { get; set; }


        [ColumnName("AdmissionType"), LoadColumn(2)]
        public string AdmissionType { get; set; }


        [ColumnName("DiagnosisCode"), LoadColumn(3)]
        public string DiagnosisCode { get; set; }


        [ColumnName("SeverityScore"), LoadColumn(4)]
        public float SeverityScore { get; set; }


        [ColumnName("ActualLOS"), LoadColumn(5)]
        public float ActualLOS { get; set; }


    }
}
