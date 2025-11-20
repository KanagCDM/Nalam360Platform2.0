//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using Microsoft.ML.Data;

namespace Mortality_risk.Model
{
    public class ModelInput
    {
        [ColumnName("PatientId"), LoadColumn(0)]
        public string PatientId { get; set; }


        [ColumnName("Age"), LoadColumn(1)]
        public float Age { get; set; }


        [ColumnName("VitalSignsScore"), LoadColumn(2)]
        public float VitalSignsScore { get; set; }


        [ColumnName("LabResultsAbnormal"), LoadColumn(3)]
        public bool LabResultsAbnormal { get; set; }


        [ColumnName("ICUAdmission"), LoadColumn(4)]
        public bool ICUAdmission { get; set; }


        [ColumnName("Deceased"), LoadColumn(5)]
        public string Deceased { get; set; }


    }
}
