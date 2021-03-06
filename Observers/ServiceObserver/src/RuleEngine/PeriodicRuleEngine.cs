using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CommunicationModel.BrokerModels;
using Microsoft.Extensions.Hosting;
using NRules;
using NRules.Fluent;
using ServiceObserver.Configuration;
using ServiceObserver.Data;

namespace ServiceObserver.RuleEngine
{
	public class PeriodicRuleEngine : IHostedService, IReloadable
	{

		private System.Timers.Timer timer;

		private ISession engineSession;

		private IEventsCache eventsCache;

		private IServiceProvider serviceProvider;

		private ConfigFields config;

		public PeriodicRuleEngine(IEventsCache eventsCache,
			IServiceProvider provider)
		{
			this.eventsCache = eventsCache;

			// used for rule engine dependencyResolver
			this.serviceProvider = provider;

			this.config = ServiceConfiguration.Instance;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			ServiceConfiguration.subscribeForReload((IReloadable)this);

			setupRuleEngine();

			timer = new System.Timers.Timer();
			timer.Interval = config.ruleEngineTriggerInterval;
			timer.Elapsed += this.timerEvent;
			timer.AutoReset = false;

			timer.Start();

			return Task.CompletedTask;
		}

		private void setupRuleEngine()
		{
			var ruleRepo = new RuleRepository();
			ruleRepo.Load(x => x.From(typeof(PeriodicRuleEngine).Assembly));
			// rules are (should be ?) in the same assembly as PeriodicRuleEngine

			ISessionFactory factory = ruleRepo.Compile();

			engineSession = factory.CreateSession();
			engineSession.DependencyResolver =
				new AspNetCoreDepResolver(serviceProvider);

		}

		private void timerEvent(Object source, ElapsedEventArgs arg)
		{
			timer.Stop();

			List<ServiceEvent> cacheContent = eventsCache.GetEvents();

			foreach (var singleEvent in cacheContent)
			{
				engineSession.Insert(singleEvent);
			}

			if (cacheContent.Count > 0)
			{
				Console.WriteLine($"Injecting {cacheContent.Count} new facts ... ");
			}
			// else // is handy in development 
			// {
			// 	Console.WriteLine(". ");
			// }

			engineSession.Fire();

			timer.Start();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			if (timer != null)
			{
				timer.Stop();
			}

			return Task.CompletedTask;
		}

		public Task reload(ConfigFields newConfig)
		{
			bool timerWasEnabled = timer.Enabled;
			timer.Stop();

			// currently there is no point in recreating ruleEngine
			// it doesn't depend on current configuration
			// rules can't be changed (each rule is a single class)
			// setupRuleEngine();

			this.config = newConfig;

			timer.Interval = config.ruleEngineTriggerInterval;

			if (timerWasEnabled)
			{
				timer.Start();
			}

			Console.WriteLine("PeriodicRuleEngine reloaded using new config ... ");

			return Task.CompletedTask;
		}
	}
}