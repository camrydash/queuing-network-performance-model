# Queuing Network Performance Model
>Jobs arrive in the system according to an exponential service time distribution, queue up for a processor, are scheduled to an available server on FCFS basis, obtain a service time quantum (based on an exponential service time distribution), perform I/O on one of the devices (chosen randomly), and then depart the system.


The interarrival times are specified by an exponential distribution
```
F(t) = 1- e**(-t/50)

* The execution time distribution for each task is also exponential and is specified as
```
F(t) = 1- e**(-t/5)

The execution time distribution for each device is also exponential and is specified as
```
F(t) = 1- e**(-t/10)

Notice that you can sample an exponential distribution function of the form
```
F(t) = 1- e**kt
