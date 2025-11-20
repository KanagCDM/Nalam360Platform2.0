//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using Mortality_risk.Model;

namespace Mortality_risk.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create single instance of sample data from first line of dataset for model input
            ModelInput sampleData = new ModelInput()
            {
                Age = 84F,
                VitalSignsScore = 51F,
                LabResultsAbnormal = false,
                ICUAdmission = false,
            };

            // Make a single prediction on the sample data and print results
            var predictionResult = ConsumeModel.Predict(sampleData);

            Console.WriteLine("Using model to make single prediction -- Comparing actual Deceased with predicted Deceased from sample data...\n\n");
            Console.WriteLine($"Age: {sampleData.Age}");
            Console.WriteLine($"VitalSignsScore: {sampleData.VitalSignsScore}");
            Console.WriteLine($"LabResultsAbnormal: {sampleData.LabResultsAbnormal}");
            Console.WriteLine($"ICUAdmission: {sampleData.ICUAdmission}");
            Console.WriteLine($"\n\nPredicted Deceased value {predictionResult.Prediction} \nPredicted Deceased scores: [{String.Join(",", predictionResult.Score)}]\n\n");
            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }
    }
}
