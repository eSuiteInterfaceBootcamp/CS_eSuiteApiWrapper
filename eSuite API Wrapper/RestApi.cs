using System;
using System.IO;
using System.Net;
using System.Text;

namespace eSuiteApiWrapper
{
    public class RestApi
    {
        private string _baseURL;
        private string _userAuthToken;

        public RestApi(string baseURL, string userAuthToken)
        {
            if (baseURL.EndsWith("/") == false)
            {
                baseURL += "/";
            }

            if (baseURL.Contains("ws/rest/ecourt/") == false)
            {
                baseURL = baseURL + "ws/rest/ecourt/";
            }

            _baseURL = baseURL;
            _userAuthToken = userAuthToken;
        }
        public RestApi(string baseURL, string username, string password)
        {
            _baseURL = baseURL;
            _userAuthToken = GetUserAuthToken(username, password);
        }

        public static string GetUserAuthToken(string username, string password)
        {
            return 
                "Basic " +
                Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
        }


        public string GetApiVersion()
        {
            return getData("apiVersion");
        }
        public string GetEntitiesList()
        {
            return getData("entities");
        }

        public string GetData(string path)
        {
            return getData(path);
        }
        public string InsertData(string path, string jsonData)
        {
            return postData(path, jsonData);
        }
        public string UpdateData(string path, string jsonData)
        {
            return putData(path, jsonData);
        }
        public void DeleteData(string path)
        {
            deleteData(path);
        }


        private WebRequest getWebRequest(string url, string method, string contentType)
        {
            WebRequest request = WebRequest.Create(url);

            if (request is HttpWebRequest == false)
            {
                throw new Exception("Incorrect WebRequest type returned.");
            }

            request.Method = method;
            request.ContentType = contentType;
            request.Headers.Add("Authorization", _userAuthToken);
            request.Timeout = 10000;

            return request;
        }
        private string getApiResponse(WebRequest request, string jsonData = null)
        {
            if (jsonData != null)
            {
                byte[] jsonBytes = Encoding.GetEncoding("iso-8859-1").GetBytes(jsonData);
                request.ContentLength = jsonBytes.Length;

                using (var WriteStream = request.GetRequestStream())
                {
                    WriteStream.Write(jsonBytes, 0, jsonBytes.Length);
                }
            }

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("API call failed with status code: " + response.StatusCode + ".");
                }

                using (var stream = response.GetResponseStream())
                {
                    if (stream != null)
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            string data = reader.ReadToEnd();
                            return data;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private string getData(string subUrl)
        {
            return getApiResponse(getWebRequest(_baseURL + subUrl, "GET", "text/xml"));
        }
        private string postData(string subUrl, string jsonData)
        {
            return getApiResponse(getWebRequest(_baseURL + subUrl, "POST", "application/json"), jsonData);
        }
        private string putData(string subUrl, string jsonData)
        {
            return getApiResponse(getWebRequest(_baseURL + subUrl, "PUT", "application/json"), jsonData);
        }
        private void deleteData(string subUrl)
        {
            getApiResponse(getWebRequest(_baseURL + subUrl, "DELETE", "application/json"));
        }
    }
}
