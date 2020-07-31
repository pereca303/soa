using System;
using System.Text;
using CommunicationModel.BrokerModels;
using Newtonsoft.Json;
using RabbitMQ.Client;
using SensorService.Configuration;

namespace SensorService.Broker
{
	public class RabbitMqBroker : IMessageBroker
	{

		private ServiceConfiguration config;

		public RabbitMqBroker()
		{
			this.config = ServiceConfiguration.Instance;
		}

		public void PublishLog(ServiceLog log)
		{
			this.Publish(log,
					this.config.serviceLogTopic,
					this.config.sensorLogFilter);
		}

		public void PublishSensorEvent(SensorReaderEvent sensorEvent, string filter)
		{
			this.Publish(sensorEvent,
					this.config.sensorReaderEventTopic,
					filter);
		}

		private void Publish(ServiceEvent serviceEvent, string topic, string filter)
		{
			ServiceConfiguration config = ServiceConfiguration.Instance;

			ConnectionFactory factory = new ConnectionFactory()
			{
				HostName = config.brokerAddress,
				Port = config.brokerPort
			};

			using (IConnection connection = factory.CreateConnection())
			using (IModel channel = connection.CreateModel())
			{
				string txtContent = JsonConvert.SerializeObject(serviceEvent);
				byte[] content = Encoding.UTF8.GetBytes(txtContent);

				channel.ExchangeDeclare(topic,
										"topic",
										true,
										true,
										null);

				channel.BasicPublish(topic,
									filter,
									false,
									null,
									content);
			}
		}

	}
}