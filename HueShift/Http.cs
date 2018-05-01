using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;

namespace HueShift
{
    public static class Http
    {
        public static Uri AppendPath(Uri uri, string path)
        {
            UriBuilder builder = new UriBuilder(uri);
            builder.Path = builder.Path + '/' + path;

            return builder.Uri;
        }

        public static T GetJson<T>(Uri baseUri, string path)
        {
            var uri = AppendPath(baseUri, path);
            string result = Get(uri);
            T json = JsonConvert.DeserializeObject<T>(result);
            return json;
        }

        public static T PostJson<T>(Uri baseUri, string path, dynamic content)
        {
            var uri = AppendPath(baseUri, path);
            string json = JsonConvert.SerializeObject(content);

            var postResult = Post(uri, json);
            return JsonConvert.DeserializeObject<T>(postResult, new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore });
        }


        public static T PutJson<T>(Uri baseUri, string path, dynamic content)
        {
            var uri = AppendPath(baseUri, path);
            string json = JsonConvert.SerializeObject(content);

            var postResult = Put(uri, json);
            return JsonConvert.DeserializeObject<T>(postResult, new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Ignore });
        }

        public static string Get(Uri uri)
        {
            HttpClient httpClient = new HttpClient();
            var response = httpClient.GetAsync(uri).Result;
            var result =response.Content.ReadAsStringAsync().Result;
            return result;
        }

        public static string Post(Uri uri, string postData)
        {
            HttpClient httpClient = new HttpClient();
            var response = httpClient.PostAsync(uri, new StringContent(postData)).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            return result;
        }

        public static string Put(Uri uri, string postData)
        {
            HttpClient httpClient = new HttpClient();
            var response = httpClient.PutAsync(uri, new StringContent(postData)).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            return result;
        }
    }

}
