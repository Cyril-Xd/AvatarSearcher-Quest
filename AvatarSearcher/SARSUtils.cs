using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using MelonLoader;
using Newtonsoft.Json;
using Directory = Il2CppSystem.IO.Directory;

namespace AvatarSearcher
{
    public class SARSUtils
    {
        private static string baseUrl = "https://unlocked.ares-mod.com/records/";

        private static string apiUrl =
            $"{baseUrl}Avatars?include=Created,TimeDetected,AvatarID,AvatarName,AvatarDescription,AuthorID,AuthorName,PCAssetURL,QUESTAssetURL,ImageURL,ThumbnailURL,UnityVersion,Releasestatus,Tags,Pin,PinCode&order=Created,desc&";

        private static string apiKey = null;

        public static async void OnIntialize()
        {
            if (File.Exists($"{Environment.CurrentDirectory}\\AvatarSearch\\APIKey.cum"))
            {
                apiKey = File.ReadAllText($"{Environment.CurrentDirectory}\\AvatarSearch\\APIKey.cum");
            }

            if (apiKey == null)
            {
                MelonLogger.Msg("Enter SARS API key:");
                apiKey = Console.ReadLine();
            }

            HttpClient httpClient = new HttpClient();
            
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SARS V1.1.2.10");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}&size=1");
            
            request.Headers.Add("X-API-Key", apiKey);
            
            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                MelonLogger.Msg("API Key Valid!");
                if(!Directory.Exists($"{Environment.CurrentDirectory}\\AvatarSearch"))
                    Directory.CreateDirectory($"{Environment.CurrentDirectory}\\AvatarSearch");
                File.WriteAllText($"{Environment.CurrentDirectory}\\AvatarSearch\\APIKey.cum", apiKey);
            }
            else
            {
                if(response.StatusCode == HttpStatusCode.Forbidden)
                {
                    MelonLogger.Error("Invalid Key!");
                    MelonLogger.Msg("Enter new key:");
                    apiKey = Console.ReadLine();
                    if(!Directory.Exists($"{Environment.CurrentDirectory}\\AvatarSearch"))
                        Directory.CreateDirectory($"{Environment.CurrentDirectory}\\AvatarSearch");
                    File.WriteAllText($"{Environment.CurrentDirectory}\\AvatarSearch\\APIKey.cum", apiKey);
                    OnIntialize();
                }
            }
        }

        public static async Task<List<Avatar>> Search(string search)
        {
            string url = apiUrl + $"filter=AvatarName,cs,{search}";
            url += $"&filter=Releasestatus,eq,public";
            url += $"&filter=PCAssetURL,sw,https://";
            url += $"&size={1000}";

            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SARS V1.1.2.10");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add("X-API-Key", apiKey);

            HttpResponseMessage response = await httpClient.SendAsync(request);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            
            AvatarList avatarList = JsonConvert.DeserializeObject<AvatarList>(jsonResponse);
            
            return avatarList.records;
        }
        
        public static async Task<List<Avatar>> SearchAuthor(string search)
        {
            string url = apiUrl + $"filter=AuthorID,eq,{search}";
            url += $"&filter=Releasestatus,eq,public";
            url += $"&filter=PCAssetURL,sw,https://";
            url += $"&size={1000}";

            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SARS V1.1.2.10");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add("X-API-Key", apiKey);

            HttpResponseMessage response = await httpClient.SendAsync(request);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            
            AvatarList avatarList = JsonConvert.DeserializeObject<AvatarList>(jsonResponse);
            
            return avatarList.records;
        }
    }
}