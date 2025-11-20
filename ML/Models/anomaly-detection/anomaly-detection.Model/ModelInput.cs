//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using Microsoft.ML.Data;

namespace Anomaly_detection.Model
{
    public class ModelInput
    {
        [ColumnName("PatientId"), LoadColumn(0)]
        public string PatientId { get; set; }


        [ColumnName("Timestamp"), LoadColumn(1)]
        public string Timestamp { get; set; }


        [ColumnName("HeartRate"), LoadColumn(2)]
        public float HeartRate { get; set; }


        [ColumnName("BloodPressureSystolic"), LoadColumn(3)]
        public float BloodPressureSystolic { get; set; }


        [ColumnName("BloodPressureDiastolic"), LoadColumn(4)]
        public float BloodPressureDiastolic { get; set; }


        [ColumnName("Temperature"), LoadColumn(5)]
        public float Temperature { get; set; }


        [ColumnName("Anomaly"), LoadColumn(6)]
        public string Anomaly { get; set; }


    }
}
