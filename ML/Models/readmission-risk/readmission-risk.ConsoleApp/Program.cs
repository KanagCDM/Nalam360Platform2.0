//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using Readmission_risk.Model;

namespace Readmission_risk.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create single instance of sample data from first line of dataset for model input
            ModelInput sampleData = new ModelInput()
            {
                Age = 43F,
                Gender = @"M",
                DiagnosisCode = @"J44.0",
                PreviousAdmissions = 0F,
                LengthOfStay = 9F,
                HasComorbidities = true,
            };

            // Make a single prediction on the sample data and print results
            var predictionResult = ConsumeModel.Predict(sampleData);

            Console.WriteLine("Using model to make single prediction -- Comparing actual Readmitted with predicted Readmitted from sample data...\n\n");
            Console.WriteLine($"Age: {sampleData.Age}");
            Console.WriteLine($"Gender: {sampleData.Gender}");
            Console.WriteLine($"DiagnosisCode: {sampleData.DiagnosisCode}");
            Console.WriteLine($"PreviousAdmissions: {sampleData.PreviousAdmissions}");
            Console.WriteLine($"LengthOfStay: {sampleData.LengthOfStay}");
            Console.WriteLine($"HasComorbidities: {sampleData.HasComorbidities}");
            Console.WriteLine($"\n\nPredicted Readmitted value {predictionResult.Prediction} \nPredicted Readmitted scores: [{String.Join(",", predictionResult.Score)}]\n\n");
            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }
    }
}
