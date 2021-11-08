using Contentful.Core;
using EmployerFrontDoor.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Contentful.Core.Models;

namespace EmployerFrontDoor.Web.Controllers
{
    public class Scheme
    {
        public string Name { get; set; }
        public string SchemeUrl { get; set; }
        public Document Description { get; set; }
    }

        /// <summary>
    /// A renderer for a heading.
    /// </summary>
    public class GdsHeadingRenderer : IContentRenderer
    {
        private readonly ContentRendererCollection _rendererCollection;

        /// <summary>
        /// Initializes a new HeadingRenderer.
        /// </summary>
        /// <param name="rendererCollection">The collection of renderer to use for sub-content.</param>
        public GdsHeadingRenderer(ContentRendererCollection rendererCollection)
        {
            _rendererCollection = rendererCollection;
        }

        /// <summary>
        /// The order of this renderer in the collection.
        /// </summary>
        public int Order { get; set; } = 50;

        /// <summary>
        /// Whether or not this renderer supports the provided content.
        /// </summary>
        /// <param name="content">The content to evaluate.</param>
        /// <returns>Returns true if the content is a heading, otherwise false.</returns>
        public bool SupportsContent(IContent content)
        {
//            return content is Heading1 || content is Heading2 || content is Heading3 || content is Heading4 || content is Heading5 || content is Heading6;
            return content is Heading1 or Heading2 or Heading3 or Heading4;
        }

        /// <summary>
        /// Renders the content to an html h-tag.
        /// </summary>
        /// <param name="content">The content to render.</param>
        /// <returns>The p-tag as a string.</returns>
        public string Render(IContent content)
        {
            int headingSize;
            string gdsHeadingClassSize;
            
            switch (content)
            {
                case Heading1:
                    gdsHeadingClassSize = "xl";
                    headingSize = 1;
                    break;
                case Heading2:
                    gdsHeadingClassSize = "l";
                    headingSize = 2;
                    break;
                case Heading3:
                    gdsHeadingClassSize = "m";
                    headingSize = 3;
                    break;
                case Heading4:
                    gdsHeadingClassSize = "s";
                    headingSize = 4;
                    break;
                default:
                    throw new Exception("Unexpected content type.");
                // case Heading5:
                //     headingSize = 5;
                //     break;
                // case Heading6:
                //     headingSize = 6;
                //     break;
            }

            var heading = content as IHeading;

            var sb = new StringBuilder();
            sb.Append($"<h{headingSize} class=\"govuk-heading-{gdsHeadingClassSize}\">");

            //class="govuk-heading-l"
            
            foreach (var subContent in heading.Content)
            {
                var renderer = _rendererCollection.GetRendererForContent(subContent);
                sb.Append(renderer.Render(subContent));
            }

            sb.Append($"</h{headingSize}>");
            return sb.ToString();
        }

        /// <summary>
        /// Renders the content asynchronously.
        /// </summary>
        /// <param name="content">The content to render.</param>
        /// <returns>The rendered string.</returns>
        public Task<string> RenderAsync(IContent content)
        {
            return Task.FromResult(Render(content));
        }
    }
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var httpClient = new HttpClient();
            var client = new ContentfulClient(httpClient,
                "", 
                "",
                "0liyzri8haz6");

            var entry = await client.GetEntry<Scheme>("6YMOVJcUS66vdhyP4q9CAs");

            var htmlRenderer = new HtmlRenderer();
            htmlRenderer.AddRenderer(new GdsHeadingRenderer(htmlRenderer.Renderers) );
            var html = await htmlRenderer.ToHtml(entry.Description);
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
