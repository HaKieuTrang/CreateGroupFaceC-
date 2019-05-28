using System;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System.IO;
using Newtonsoft.Json;
using System.Collections;
using System.Linq;
namespace Tao_Group
{
    class Program
    {
        static string _faceAPIKey = "19a4f858125e4f46bf02fd8c8146cf57";
        const string faceEndpoint = "https://eastasia.api.cognitive.microsoft.com";
        const string personGroupId = "myfriends";
        const string friendImageDir = @"C:\Apink\ApiFace\Rong";
        static void Main(string[] args)
        {
            var faceClient = new FaceClient(new ApiKeyServiceClientCredentials(_faceAPIKey),
                                            new System.Net.Http.DelegatingHandler[] { });
            faceClient.Endpoint = faceEndpoint;
           
            //Create a new PersonGroup

            var faceId = faceClient.PersonGroup.CreateAsync("myfriends", "My Friends").GetAwaiter();


            //Add a person to person group 

            var friendAnna = faceClient.PersonGroupPerson.CreateAsync(personGroupId, "anna").GetAwaiter().GetResult();


            //Register Faces

            string[] array = Directory.GetFiles(friendImageDir, "*.jpg");
            for (int i = 0; i < array.Length; i++)
            {
                string image = array[i];
                using (Stream imageStream = File.OpenRead(image))
                {
                    faceClient.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId, friendAnna.PersonId, imageStream).GetAwaiter();
                }
            }
          

            //Train faces 

            faceClient.PersonGroup.TrainAsync(personGroupId).GetAwaiter().GetResult();

            //Identify the face

            string testImage = @"C:\Apink\ApiFace\Rong\rong.jpg";

            using (Stream imageStream = File.OpenRead(testImage))
            {
                var faces = faceClient.Face.DetectWithStreamAsync(imageStream, true).GetAwaiter().GetResult();
                var faceids = faces.Select(e => (Guid)e.FaceId).ToList();
                var identifyResults = faceClient.Face.IdentifyAsync(faceids, personGroupId).GetAwaiter().GetResult();

                foreach (var result in identifyResults)
                {
                    if (result.Candidates.Count == 0)
                        Console.WriteLine("No one identified");
                    else
                    {
                        var candidateId = result.Candidates[0].PersonId;
                        var person = faceClient.PersonGroupPerson.GetAsync(personGroupId, candidateId)
                        .GetAwaiter().GetResult();
                        Console.WriteLine("Identified as {0}", person.Name);
                    }
                }
            }
        }
    }
}