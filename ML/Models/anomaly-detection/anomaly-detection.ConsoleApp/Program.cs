//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using Anomaly_detection.Model;

namespace Anomaly_detection.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create single instance of sample data from first line of dataset for model input
            ModelInput sampleData = new ModelInput()
            {
                Timestamp = @"2025-11-19 17:35:45",
                HeartRate = 84F,
                BloodPressureSystolic = 128F,
                BloodPressureDiastolic = 71F,
                Temperature = 97.9F,
            };

            // Make a single prediction on the sample data and print results
            var predictionResult = ConsumeModel.Predict(sampleData);

            Console.WriteLine("Using model to make single prediction -- Comparing actual Anomaly with predicted Anomaly from sample data...\n\n");
            Console.WriteLine($"Timestamp: {sampleData.Timestamp}");
            Console.WriteLine($"HeartRate: {sampleData.HeartRate}");
            Console.WriteLine($"BloodPressureSystolic: {sampleData.BloodPressureSystolic}");
            Console.WriteLine($"BloodPressureDiastolic: {sampleData.BloodPressureDiastolic}");
            Console.WriteLine($"Temperature: {sampleData.Temperature}");
            Console.WriteLine($"\n\nPredicted Anomaly value {predictionResult.Prediction} \nPredicted Anomaly scores: [{String.Join(",", predictionResult.Score)}]\n\n");
            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }
    }
}
