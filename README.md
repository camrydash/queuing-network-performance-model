## Queuing Network Performance Model
Jobs arrive in the system according to an exponential service time distribution, queue up for a processor, are scheduled to an available server on FCFS basis, obtain a service time quantum (based on an exponential service time distribution), perform I/O on one of the devices (chosen randomly), and then depart the system.

##Description
This simulation model helps to trace a real world problem into an analytical model that helps to measure the performance of queuing. It calculates the efficiency of the simulated model based on its response time and throughput.

The given simulation model has three processors and a single job queue. Once the jobs are processed in one of the three processors, they are sent to one of the three I/O device queues, from where they are sent to one of the three I/O devices to perform I/O operations.

`Average arrival rate λ` of a simulation model determines rate at which jobs are arriving per second. These jobs maintain an inter arrival time to enter the queue. The jobs once arrive may have to wait in the job queue. The average time they wait in the queue is their waiting time. Each job has its own task execution time also known as service time. Similarly device execution time is the time each job is processed in the I/O device. Once both processor and device execution is complete, the job exits the system.

## Calculated Statistics
* Length of simulated time
* Server[*i*] was {busy|idle} at termination.
* Tasks completed:
* Tasks currently in the queue:
* Throughout rate (tasks/time):
* Average turnaround time:
* Average queue length:
* Maximum queue length:
* Average processor service time:
* Average device service time:
* Percent of time the server[*i*] was busy:
* Percent of time the device[*i*] was busy:
