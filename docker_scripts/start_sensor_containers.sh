#!/bin/bash

# default values
num_of_instances="2"
proc_per_instance="3"
data_path="/home/nemanja/soa_data"

sensor_name_prefix="soa-sensor-"

if [ "$#" -eq "0" ]
then 

	echo "Taking default values for arguments ... "

elif  [ "$#" -eq "3" ]
then 

	num_of_instances=$1
	proc_per_instance=$2
	data_path=$3

else
	echo "Please provide adequate arguments:"
	echo "arg1: number of containers"
	echo "arg2: processes per instance"
	echo "arg3: absolute path to the data files"
	echo "e.g.: ./start_sensor_containers.sh 10 6 /home/nemanja/soa_data"
	echo
	exit
fi

echo "Number of containers:  $num_of_instances "
echo "Processes per container: $proc_per_instance ... "
echo "Data should be on: $data_path "
echo # new line

data_index="0"
for (( container_ind=0; container_ind<$num_of_instances; container_ind++ ))
do 

	last_data_ind=$(($data_index+$proc_per_instance-1))
	echo "Starting container $sensor_name_prefix$container_ind with data: $data_index - $last_data_ind" 

	docker run -d \
				--rm \
				--name "$sensor_name_prefix$container_ind" \
				--volume "$data_path":/data \
				--network soa_default \
				 soa/sensor-service "$proc_per_instance" "$data_index"

	data_index=$(($data_index+$proc_per_instance))

done