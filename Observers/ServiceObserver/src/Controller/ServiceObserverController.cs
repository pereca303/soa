using System;
using System.Collections.Generic;
using CollectorService.Data;
using CommunicationModel.RestModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ServiceObserver.Data;

namespace ServiceObserver.Controller
{
	// dev: 5004 prod: 3000
	[Route("observer/service")]
	[ApiController]
	public class ServiceObserverController : ControllerBase
	{

		private IDatabaseService db;

		public ServiceObserverController(IDatabaseService db)
		{
			this.db = db;
		}

		[HttpGet]
		public string getSomeString()
		{
			return "Hello there, I am observing, don't worry ... ";
		}

		[HttpGet]
		[Route("getAll")]
		public IActionResult getAllUnstableEvents()
		{
			List<UnstableServiceDbRecord> dbRecords = db.GetAllUnstableRecords();

			if (dbRecords != null)
			{
				List<UnstableServiceRecord> resList = new List<UnstableServiceRecord>();

				foreach (UnstableServiceDbRecord dbRecord in dbRecords)
				{
					resList.Add(new UnstableServiceRecord(dbRecord.serviceId,
													dbRecord.downCount,
													dbRecord.downEvents,
													dbRecord.recordedTime));
				}

				return new OkObjectResult(resList);
			}
			else
			{
				return StatusCode(500);
			}
		}

		[HttpGet]
		[Route("getForService")]
		public IActionResult getUnstableEventsForService([FromQuery] string serviceId)
		{
			List<UnstableServiceDbRecord> dbRecords = db.GetUnstableRecordsForService(serviceId);

			if (dbRecords != null)
			{
				List<UnstableServiceRecord> resList = new List<UnstableServiceRecord>();

				foreach (UnstableServiceDbRecord dbRecord in dbRecords)
				{
					resList.Add(new UnstableServiceRecord(dbRecord.serviceId,
													dbRecord.downCount,
													dbRecord.downEvents,
													dbRecord.recordedTime));
				}

				return new OkObjectResult(resList);
			}
			else
			{
				return StatusCode(500);
			}
		}

		[HttpGet]
		[Route("getLatest")]
		public IActionResult getLatestUnstableEvent()
		{

			UnstableServiceDbRecord dbRecord = db.GetLatestRecord();

			if (dbRecord != null)
			{

				UnstableServiceRecord resRecord = new UnstableServiceRecord(
													dbRecord.serviceId,
													dbRecord.downCount,
													dbRecord.downEvents,
													dbRecord.recordedTime);
				return new OkObjectResult(resRecord);
			}
			else
			{
				return StatusCode(500);
			}

		}

		[HttpGet]
		[Route("getOldConfigs")]
		public IActionResult getConfigs()
		{
			ConfigBackupRecord record = db.GetConfigs();

			if (record != null)
			{
				List<ConfigRecord> resultList = new List<ConfigRecord>();

				foreach (DatedConfigRecord singleConfRec in record.oldConfigs)
				{
					string txtConfig = JsonConvert.SerializeObject(singleConfRec);
					resultList.Add(new ConfigRecord(record.serviceId,
										txtConfig,
										singleConfRec.backupDate));
				}

				return new OkObjectResult(resultList);
			}
			else
			{
				return StatusCode(500);
			}
		}

	}
}