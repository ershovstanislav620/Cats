using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Net;
using Microsoft.Extensions.Caching.Memory;

namespace Cats1.Controllers;

public class CatsController : Controller
{
    // Пример запроса: http://localhost:5116/Cats?url=https://www.youtube.com
    // Выполнил Ершов С.В


    private readonly IMemoryCache _cache;
    public CatsController(IMemoryCache memoryCache)
    {
        _cache = memoryCache;
    }



    public async Task<IActionResult> Index(string url)
    {
        HttpStatusCode statusCode;
        int intStatusCode;

        try
        {
            try
            {

                var urlString = HtmlEncoder.Default.Encode(url);
                string contentType = "image/png";

                if (_cache.TryGetValue(urlString, out byte[] image))
                {
                    
                    return File(image, contentType);
                }

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlString);
                request.Method = WebRequestMethods.Http.Head;
                request.AllowAutoRedirect = false;
                request.Accept = @"*/*";
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    statusCode = response.StatusCode;
                    response.Close();
                }
                catch (WebException we)
                {
                    statusCode = ((HttpWebResponse)we.Response).StatusCode;
                }


                WebClient wc = new WebClient();
                intStatusCode = (int)statusCode;
                try
                {
                    var urlCat = "https://http.cat/" + intStatusCode.ToString() + ".jpg";
                    Console.WriteLine(urlCat);
                    var data = await new HttpClient().GetAsync(urlCat);

                    image = await data.Content.ReadAsByteArrayAsync();
                }
                catch (WebException we)
                {
                    image = System.IO.File.ReadAllBytes(@"images\statuscodenotfound.png");
                    contentType = "image/jpeg";
                    return File(image, contentType);
                }




                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30));
                _cache.Set(urlString, image, cacheEntryOptions);


                return File(image, contentType);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine(ex.ToString());
                byte[] imageData = System.IO.File.ReadAllBytes(@"images\error.jpg");
                string contentType = "image/jpeg";
                return File(imageData, contentType);

            }
        }
        catch (UriFormatException ex)
        {
            byte[] imageData = System.IO.File.ReadAllBytes(@"images\error.jpg");
            string contentType = "image/jpeg";
            return File(imageData, contentType);
        }
    }
}