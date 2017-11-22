using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.DocAsCode.Plugins;
using System.Collections.Generic;
using MarkdigEngine;
using MarkdigEngine.Extensions;

namespace MarkdownPreview
{
    public static class MarkdigForDocfx
    {
        private static readonly string MARKIG_ENGINE_VERSION = typeof(MarkdigMarkdownService).Assembly.GetName().Version.ToString();
        private static readonly string MarkdownEngineName = "MarkdigEngine";
        private static readonly MarkdigMarkdownService _service;

        static MarkdigForDocfx()
        {
            var parameter = new MarkdownServiceParameters
            {
                BasePath = ".",
                Extensions = new Dictionary<string, object>
                {
                    { LineNumberExtension.EnableSourceInfo, false }
                }
            };
            _service = new MarkdigMarkdownService(parameter);
        }

        [FunctionName("markdig")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("MarkdigEngine processed a request.");

            // parse query parameter
            var text = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "text", true) == 0)
                .Value ?? string.Empty;

            if (text.Length > 1000)
            {
                text = text.Substring(0, 1000);
            }

            try
            {
                var result = _service.Markup(text, string.Empty);
                return req.CreateResponse(HttpStatusCode.OK, new
                {
                    name = MarkdownEngineName,
                    html = result.Html,
                    version = MARKIG_ENGINE_VERSION
                });
            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.OK, new
                {
                    name = MarkdownEngineName,
                    html = "exception: " + ex.Message,
                    version = MARKIG_ENGINE_VERSION
                });
            }
        }
    }
}
