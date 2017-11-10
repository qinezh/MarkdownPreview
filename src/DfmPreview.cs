using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.DocAsCode.Dfm;

namespace MarkdownPreview
{
    public static class DfmPreview
    {
        private static readonly string DFM_VERSION = typeof(DocfxFlavoredMarked).Assembly.GetName().Version.ToString();
        private static readonly string MarkdownEngineName = "DFM";
        private static readonly DfmEngine _engine;

        static DfmPreview()
        {
            var option = DocfxFlavoredMarked.CreateDefaultOptions();
            option.LegacyMode = true;
            var builder = new DfmEngineBuilder(option);
            _engine = builder.CreateDfmEngine(new DfmRenderer());
        }

        [FunctionName("dfm")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("DfmPreview processed a request.");

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
