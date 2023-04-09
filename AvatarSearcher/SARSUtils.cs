using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace AvatarSearcher
{
    public class SARSUtils
    {
       

        
        public static List<Avatar> Search(string search)
        {
            string newURL = "https://unlocked.shrektech.xyz/Avatar/GetPublicAvatar";
            WWWForm form = new WWWForm();
            form.AddField("avatarName", "test");
            form.AddField("amount", 1);
      
            UnityWebRequest unityWebRequest = UnityWebRequest.Post(newURL, form);

            unityWebRequest.SendWebRequest();
            while (!unityWebRequest.isDone)
            {
            }
            if (unityWebRequest.isHttpError || unityWebRequest.isNetworkError)
            {
                MelonLogger.Msg("Failed Requesting " + unityWebRequest.GetError().ToString());
                return null;
            }
            AvatarList avatarList = JsonConvert.DeserializeObject<AvatarList>(unityWebRequest.downloadHandler.text);
            if (avatarList == null)
            {
                return null;
            }
            return avatarList.records;
        }
    }
}
