using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.DocAsCode.Dfm;

namespace MarkdownPreview
{
    public static class DfmLatestPreview
    {
        private static readonly string DFM_VERSION = typeof(DocfxFlavoredMarked).Assembly.GetName().Version.ToString();
        private static readonly string MarkdownEngineName = "DFM-Latest";
        private static readonly DfmEngine _engine;

        static DfmLatestPreview()
        {
            var option = DocfxFlavoredMarked.CreateDefaultOptions();
            option.LegacyMode = false;
            var builder = new DfmEngineBuilder(option);
            _engine = builder.CreateDfmEngine(new DfmRenderer());
        }

        [FunctionName("dfmlatest")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("DfmLatestPreview processed a request.");

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
                var result = _engine.Markup(text, string.Empty);
                return req.CreateResponse(HttpStatusCode.OK, new
                {
                    name = MarkdownEngineName,
                    html = result,
                    version = DFM_VERSION
                });
            }
            catch (Exception ex)
            {
                return req.CreateResponse(HttpStatusCode.OK, new
                {
                    name = MarkdownEngineName,
                    html = "exception: " + ex.Message,
                    version = DFM_VERSION
                });
            }
        }
    }
}
