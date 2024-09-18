using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace StreetViewImageRetrieve
{
    public class StreetView
    {
        public static List<PanoInfo> GetPanoIds(float latitude, float longitude)
        {
            string url = string.Format("https://maps.googleapis.com/maps/api/js/GeoPhotoService.SingleImageSearch?pb=!1m5!1sapiv3!5sUS!11m2!1m1!1b0!2m4!1m2!3d{0:}!4d{1:}!2d50!3m10!2m2!1sen!2sGB!9m1!1e2!11m4!1m3!1e2!2b1!3e2!4m10!1e1!1e2!1e3!1e4!1e8!1e6!5m1!1e2!6m1!1e2&callback=_xdc_._v2mub5", latitude, longitude);

            var panoIds = new List<string>();
            using (var client = new HttpClient(new HttpClientHandler()))
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                var s = response.Content.ReadAsStringAsync().Result;
                var x = Regex.Matches(s, "\\[[0-9]+,\"(.{22})\"\\].+?\\[\\[null,null,(-?[0-9]+.[0-9]+),(-?[0-9]+.[0-9]+)");
                //var d = Regex.Matches(s, "([0-9]?[0-9]?[0-9])?,?\\[(20[0-9][0-9]),([0-9]+)\\]");
                var p = Regex.Matches(s, "\\\"(.{22})\\\"");
                var items = p.Cast<Match>().Select(m => m.Value).ToList();
                panoIds.AddRange(items.Distinct());
                panoIds.RemoveAll(a => a.Contains("Google"));
                panoIds.RemoveAll(a => a.Contains("null"));
                panoIds.RemoveAll(a => a.Contains(","));
            }

            var list = new List<PanoInfo>();
            foreach(var item in panoIds)
            {
                list.Add(new PanoInfo(item.Replace("\"", string.Empty), latitude, longitude));
            }

            return list;
        }

        public static void GetImages(PanoInfo panoInfo)
        {

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    var u = string.Format("https://cbk0.google.com/cbk?output=tile&panoid={0}&zoom={1}&x={2}&y={3}", panoInfo.PanoId, 2, i, j);

                    using (var client = new HttpClient(new HttpClientHandler()))
                    {
                        HttpResponseMessage response = client.GetAsync(u).Result;
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var s = response.Content.ReadAsByteArrayAsync().Result;
                            File.WriteAllBytes("c:\\Images\\" + panoInfo.PanoId + "_" + i + "_" + j + ".jpg", s);
                        }
                    }
                }
            }
        }
    }
}
