#!/bin/bash

mongo localhost/soa_collector --eval '

	db.Sensors.aggregate(
		[
			{ 
				$match: {sensorName:"sensor_0"}
			},
			{
				$project: {
					_id:0,
					sensor_name:1,
					"records.timestamp":1
				}
			}			
			
		]
	
	).forEach(printjson)

' | grep --color "timestamp"
