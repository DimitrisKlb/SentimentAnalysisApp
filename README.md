# A Sentiment Analysis cloud-based platform
My Thesis for the Diploma in [Computer Engineering & Informatics Department][ceid], at the University of Patras.

Sentiment analysis, or opinion mining, is the process according to which a system has
to identify the author’s feeling - opinion expressed in his text, by deciding whether the
aforementioned text is positive, negative or neutral. A common use of this technology
is to discover how people feel about a particular topic.

The purpose of this project was to implement an *extensible*, *scalable* and *reliable* computer system which performs sentiment analysis on user textual data
mined from sources around the internet like social media, forums, etc. It is comprised of two discrete subsystems.

## FrontEnd Server - SentiMeter Application
This is a web app that enables users to graphically create and manage requests for sentiment analysis. It was created using the .NET Framework libraries (EntityFramework, MCV, etc).

## BackEnd Server - WebServiceProvider
This is the main subsystem that performs all of the project's core funcionality, by acting as a BackEnd Server in order to fulfil the requests received by the
SentiMeter app:

- Firstly, it mines texts from the selected sources. 
- Secondly, it analyses each one of them and classifies it as positive, negative or neutral.
- Lastly, it calculates results and sends them back to the app.

In order to achieve the desired aforementioned principles, this system was designed as a cloud system application using the [Microsoft Azure Service Fabric Platform][asf] (PaaS),
as well as other important and basic programming patterns, techniques and concepts such us:

- Cloud Programming
- Microservices architectures
- Actor Model
- RESTful Services
- Dependency injection
- Modular Hierarchies

Therefore, the system can easily be extended to support :

- additional mining sources like social media (Facebook, Instagram) and forums
- the implementation of other sentiment analysis algorithms, for each different source, combination etc, making easy to observe their results and test their efficiency
- additional features like results caching, more specific search criteria, live observation of the progress of users' sentimental trends and opinions, etc


[ceid]: <https://www.ceid.upatras.gr/en>
[asf]: <https://azure.microsoft.com/en-us/services/service-fabric/>