using System.Threading;
using System.Threading.Tasks;
using DotnetSpider.DataFlow;
using DotnetSpider.DataFlow.Parser;
using DotnetSpider.DataFlow.Storage;
using DotnetSpider.Downloader;
using DotnetSpider.Http;
using DotnetSpider.Infrastructure;
using DotnetSpider.Scheduler.Component;
using DotnetSpider.Selector;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace DotnetSpider.Sample.samples
{
	public class BaseUsageSpider : Spider
	{
		public static async Task RunAsync()
		{
			var builder = Builder.CreateDefaultBuilder<BaseUsageSpider>(x =>
			{
				x.Speed = 5;
			});
			builder.UseSerilog();
			builder.UseDownloader<HttpClientDownloader>();
			builder.UseQueueDistinctBfsScheduler<HashSetDuplicateRemover>();
			await builder.Build().RunAsync();
		}

		class MyDataParser : DataParser
		{
			public MyDataParser()
			{
				AddRequiredValidator("cnblogs\\.com");
				AddFollowRequestQuerier(Selectors.XPath("."));
			}

			protected override Task ParseAsync(DataFlowContext context)
			{
				context.AddData("URL", context.Request.RequestUri);
				context.AddData("Title", context.Selectable.XPath(".//title")?.Value);
				return Task.CompletedTask;
			}
		}

		public BaseUsageSpider(IOptions<SpiderOptions> options, DependenceServices services,
			ILogger<Spider> logger) : base(
			options, services, logger)
		{
		}

		protected override async Task InitializeAsync(CancellationToken stoppingToken)
		{
			await AddRequestsAsync(new Request("http://www.cnblogs.com/"));
			AddDataFlow(new MyDataParser());
			AddDataFlow(new ConsoleStorage());
		}

		protected override (string Id, string Name) GetIdAndName()
		{
			return (ObjectId.NewId().ToString(), "Cnblogs");
		}
	}
}
