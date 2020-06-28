using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Net.Mime;
using System.Collections.Generic;
using Swan;

namespace PhonieCore.Mopidy
{
    public class Client : IDisposable
    {
        private const string MopidyUrl = "http://localhost:6680/mopidy/rpc";
        private HttpClient client;
        private bool disposedValue;

        public Client()
        {
            client = new HttpClient();            
        }

        private void Call(string method, Dictionary<string, object[]> parameters)
        {
            var request = new MultiParamRequest(method, parameters);
            Fire(request);
        }

        private void Call(string method, Dictionary<string, object> parameters)
        {
            var request = new SingleParamRequest(method, parameters);
            Fire(request);
        }

        private void Call(string method)
        {
            var request = new SingleParamRequest(method, null);
            Fire(request);
        }

        private void Fire(Request request)
        {
            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;

            string json = JsonConvert.SerializeObject(request, setting);
            Console.WriteLine(json);
            var httpContent = new StringContent(json, null, "application/json");
            httpContent.Headers.ContentType.CharSet = "";
            var result = client.PostAsync(MopidyUrl, httpContent).Result;
            Console.WriteLine(result.StatusCode + ", " + result.ReasonPhrase, ", ", result.Content);
        }

        public void Stop()
        {
            Call("playback.stop");
        }

        public void Pause()
        {
            Call("playback.pause");
        }

        public void Play()
        {
            Call("playback.play");
        }

        public void SetVolume(int volume)
        {
            Call("mixer.set_volume", new Dictionary<string, object> { { "volume",  volume } });
        }

        public void AddTrack(string uri)
        {
            Call("tracklist.add", new Dictionary<string, object[]> { { "uris", new string[] { uri } } });
        }

        public void ClearTracks()
        {
            Call("tracklist.clear");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    client.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
