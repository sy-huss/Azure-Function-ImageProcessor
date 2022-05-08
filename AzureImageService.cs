using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ComputerVision
{
    public class Process
    {
        // Subscription Key & Endpoint
        private static readonly string _subscriptionKey = "{SUB_KEY}";

        private static readonly string _endpoint = "{Endpoint}";

        public static ImageAnalysisResults _imageResults;

        // Run Process
        public static void Run(string filename)
        {
            string imageUrl = $"https://eaxstore01.blob.core.windows.net/fileupload/{filename}";

            // Create a client
            ComputerVisionClient client = Authenticate(_endpoint, _subscriptionKey);

            // Analyze an image to get features and other properties.
            AnalyzeImageUrl(client, imageUrl).Wait();
        }

        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client =
              new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
              { Endpoint = endpoint };
            return client;
        }

        public static async Task<ImageAnalysisViewModel> AnalyzeImageUrl(ComputerVisionClient client, string imageUrl)
        {
            try
            {
                // Download image locally
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(imageUrl, "image.png");
                }

                // Create the Vision Taxonomy
                List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>() {
                    VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                    VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                    VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                    VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                    VisualFeatureTypes.Objects
                };

                ImageAnalysis results;

                // Process the downloaded file
                using (Stream imageStream = File.OpenRead("image.png"))
                {
                    results = await client.AnalyzeImageInStreamAsync(imageStream, visualFeatures: features);
                    imageStream.Close();
                }

                // Create a new Image Analysis Model
                ImageAnalysisViewModel imageAnalysis = new ImageAnalysisViewModel
                {
                    ImageAnalysisResult = results
                };

                // Search and apply appropriate tags
                ImageAnalysisResults m = new ImageAnalysisResults();

                foreach (var result in imageAnalysis.ImageAnalysisResult.Tags)
                {
                    m.Tag = result.Name;
                    m.Hint = result.Hint ??= "null";
                    m.Score = result.Confidence;
                }

                // Assign results
                _imageResults = m;

                return imageAnalysis;
            }
            catch (Exception e)
            {
                Log.Logger.Error($"{e}");
            }

            return null;
        }

        public class ImageAnalysisViewModel
        {
            public ImageAnalysis ImageAnalysisResult { get; set; }
        }

        public class ImageAnalysisResults
        {
            public string Tag { get; set; }

            public string Hint { get; set; }

            public double Score { get; set; }
        }
    }
}