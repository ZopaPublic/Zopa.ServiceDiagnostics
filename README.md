# Zopa.ServiceDiagnostics

A really tiny library to enable you to easily monitor service health & build info, and see what's going on in an environment.

If like us, you have many microservices in your production environment, sometimes things go wrong.  It could be a message queue misbehaving, a service failing to initialise or even an external resource going awry.  `Zopa.ServiceDiagnostics` provides a simple object model to help you quickly diagnose what is happening without having to trawl through logs.  It is in no way designed to replace a robust logging infrasturcture, and in fact is designed to play well with it, making use of correlation ids to help identify problems within log files.

Health Checks
-------------

At the heart of the system is the health check runner.  It executes all supplied instances of `IAmAHealthCheck` or `IAmADescriptiveHealthCheck` passed to it in a somewhat parallelised fashion, and spits out the results

Both interfaces are designed to perform essentially the same function - they test a specific dependency, but report their findings in slightly different ways.  The preferred impementation is `IAmAHealthCheck`

## `IAmAHealthCheck`
The simplest, and preferred method of performing a health check; though method `Task ExecuteAsync(Guid correlationId)`. To pass a health check, simply return a non-faulted task.  If a problem occurs, throw an exception.  This interface has the benefit that the result is automatically generated, including the execution time.

## `IAmADescriptiveHealthCheck`
This interface allows for slightly more descriptive results to be returned, but delegating responsibility for creating the result to the healtch check its self.  E.g. If an external system has a login phase and an execution phase, this interface could be used for returning descriptive diagnostics of where an error occured in the execution pipeline.

## Correlation Ids
These are purely optional parameters, and may be safely ignored if your estate doesn't make use of them.  Simply put, they are a unique & arbitrary identifier used for tying up combing through logs.  If a healthcheck run reposrts an error and you are logging all messages against a correlaiotn id, you can search for that id in your log viewor of choice to see excatly what lead to the issue.  A new correlation id will be generated for each run of the `HealthCheckRuner`, and passed to each health check.

Running the system
------------------
Running the system is super-simple.

```c#
var healthchecks = SomehowInstantiateYourStandardHealthchecks();
var descriptiveHealthchecks = SomehowInstantiateYourDescripticeHealthchecks();

var runner = new HealthCheckRunner()

var result = await runner.DoAsync();
```

That's it.  We typically run the above code in a controller and return the output as json, but the code here has no opinion on how it should be used.  Of course, we also prefer to hook everthing up via DI, but nothing is mandated)

Service Information
-------------------

Service information is a container that presents health check information with some static 'devopsy' details about the system's build.

To use it, you need to declare two attributes, possibly in your AssemblyInfo.cs, possibly patched by your CI server...

```c#
[assembly: BuildInformation("50ge34a", "2016-01-01 14:46:00", "build7.zopa.com", "https://build7.zopa.com/job/zopa-somesuch-ci/123")]
[assembly: ProjectInformation("https://www.github/com/zopa.sumsuch/commit/50ge34a", "myteam@zopa.com")]
```

To create, simply pass the assembly that contains the attributes, and the healthcheck results

```c#
var healthcheckResults = await GetHealthcheckResults();
var serviceInfo = new ServiceInformation(typeof(someType).Assembly, healthcheckResults)
```