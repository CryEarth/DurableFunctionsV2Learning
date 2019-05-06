using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace DurableFunctionsV2Learning
{
    public static class DurableFunctionsLearming
    {
        [FunctionName("DurableFunctionsLearming")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] DurableOrchestrationContext context)
        {
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("DurableFunctionsLearming_Hello", "Tokyo"));
            outputs.Add(await context.CallActivityAsync<string>("DurableFunctionsLearming_Hello", "Seattle"));
            outputs.Add(await context.CallActivityAsync<string>("DurableFunctionsLearming_Hello", "London"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("DurableFunctionsLearming_Hello")]
        public static string SayHello([ActivityTrigger] DurableActivityContext req, ILogger log)
        {
            var name = req.GetInput<string>();

            log.LogInformation($"Saying hello to {name}.");
            return $"Hello {name}!";
        }

        [FunctionName("DurableFunctionsLearming_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "{subPath}/{functionName}")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter,
            string functionName,
            string subPath,
            ILogger log)
        {
            log.LogInformation($"API Call Started： {subPath}/{functionName}/{req.Method.ToString()}");

            try
            {
                // 呼び出し方法で分岐する
                switch ((subPath + "/" + functionName + "/" + req.Method.ToString()).ToLower())
                {
                    case "sublearm/function1/post":
                        return await StarterFunction1(req, starter, log);

                    case "sublearm/function2/post":
                    case "sublearm/function2/get":
                        break;

                    case "affairs/function1/get":
                        break;

                    default:
                        return req.CreateErrorResponse(System.Net.HttpStatusCode.BadRequest, $"APIの呼び出しが不正です。：{subPath}/{functionName}/{req.Method.ToString()}");
                }

                return req.CreateErrorResponse(System.Net.HttpStatusCode.BadGateway, $"プログラムエラー：{subPath}/{functionName}/{req.Method.ToString()}");
            }
            catch (System.Exception ex)
            {
                return req.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, $"ErrorMsg:{ex.Message} / InnerErrorMsg{ex.InnerException?.Message}");
            }
            finally
            {
                log.LogInformation($"API Call Finished： {subPath}/{functionName}/{req.Method.ToString()}");
            }
        }

        /// <summary>
        /// Starterの業務ロジック
        /// </summary>
        /// <param name="req"></param>
        /// <param name="starter"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> StarterFunction1(
            HttpRequestMessage req,
            DurableOrchestrationClientBase starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("DurableFunctionsLearming", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }


    }
}
