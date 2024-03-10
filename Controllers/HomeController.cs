using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Youtube.MVC.Models;

namespace Youtube.MVC.Controllers
{
    [EnableCors("Cors")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(string playlistId)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string apiKey = "AIzaSyA4HGKpLTz76bL-xaXKCzbu9DGI1YtSJVA";

            if (string.IsNullOrWhiteSpace(playlistId))
            {
                ViewBag.ErrorMessage = "Please provide a valid playlistId.";
                return View();
            }

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = "YouTubePlaylistReader"
            });

            var playlistRequest = youtubeService.Playlists.List("snippet");
            playlistRequest.Id = playlistId;
            var playlistResponse = playlistRequest.Execute();

            if (playlistResponse.Items.Count > 0)
            {
                ViewBag.PlaylistTitle = playlistResponse.Items[0].Snippet.Title;
            }
            else
            {
                ViewBag.PlaylistTitle = "Unknown Playlist";
            }

            var request = youtubeService.PlaylistItems.List("snippet");
            request.PlaylistId = playlistId;
            request.MaxResults = 50;

            List<Tuple<string, string>> videoInfoList = new List<Tuple<string, string>>();

            do
            {
                var response = request.Execute();

                foreach (var playlistItem in response.Items)
                {
                    string videoId = playlistItem.Snippet.ResourceId.VideoId;
                    string videoTitle = playlistItem.Snippet.Title;
                    string embedLink = $"<iframe width=\"560\" height=\"315\" src=\"https://www.youtube.com/embed/{videoId}\" title=\"YouTube video player\" frameborder=\"0\" allow=\"accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share\" allowfullscreen></iframe>";
                    videoInfoList.Add(new Tuple<string, string>(videoTitle, embedLink));
                }

                request.PageToken = response.NextPageToken;

            } while (request.PageToken != null);

            return View(videoInfoList);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
