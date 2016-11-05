# NetMQ.High

NetMQ high is a sample project on how to write NetMQ components with the Actor pattern and ZProto.

Currently the project contains 4 components:

* Client - Client component that connect to Server or Load Balancer.
* AsyncServer - Server component that can reply to request asynchronously
* Load Balancer - Server component that load balancing request to different Workers according to Service.
* Worker - Worker component that reply to service request synchronously.

