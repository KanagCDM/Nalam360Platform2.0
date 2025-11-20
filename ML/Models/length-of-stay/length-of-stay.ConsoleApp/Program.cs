//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using Length_of_stay.Model;

namespace Length_of_stay.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create single instance of sample data from first line of dataset for model input
            ModelInput sampleData = new ModelInput()
            {
                Age = 25F,
                AdmissionType = @"Urgent",
                DiagnosisCode = @"I21.0",
                SeverityScore = 48F,
            };

            // Make a single prediction on the sample data and print results
            var predictionResult = ConsumeModel.Predict(sampleData);

            Console.WriteLine("Using model to make single prediction -- Comparing actual ActualLOS with predicted ActualLOS from sample data...\n\n");
            Console.WriteLine($"Age: {sampleData.Age}");
            Console.WriteLine($"AdmissionType: {sampleData.AdmissionType}");
            Console.WriteLine($"DiagnosisCode: {sampleData.DiagnosisCode}");
            Console.WriteLine($"SeverityScore: {sampleData.SeverityScore}");
            Console.WriteLine($"\n\nPredicted ActualLOS: {predictionResult.Score}\n\n");
            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }
    }
}
