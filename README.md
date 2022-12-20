# NServiceBus with OpenTelemetry

### Project structure

The solution contains five projects. The ClientUI, Sales, and Billing projects are endpoints that communicate with each other using NServiceBus messages. The ClientUI endpoint is implemented as a web application and is the entry point to our system. The Sales and Billing endpoints, implemented as console applications, contain business logic related to processing and fulfilling orders. Each endpoint references the Messages assembly, which contains the definitions of messages as simple class files. The Platform project will provide a demonstration of the Particular Service Platform, but initially, its code is commented out.

The ClientUI endpoint sends a PlaceOrder command to the Sales endpoint. As a result, the Sales endpoint will publish an OrderPlaced event using the publish/subscribe pattern, which will be received by the Billing endpoint.

The solution mimics a real-life retail system where the command to place an order is sent as a result of customer interaction, and the processing occurs in the background. Publishing an event allows us to further isolate the code to bill the credit card from the code to place the order, reducing coupling and making the system easier to maintain over the long term. Later in this tutorial, we'll see how to add a second subscriber to that event in a new Shipping endpoint which will begin the process of shipping the order.
### Setup
OpenTelemetry requires .NET 7.0+

This example is configured to send telemetry data to [Honeycomb](https://honeycomb.io), an team and API Key is required to view telemetry data. Set your API Key to `HONEYCOMB_API_KEY` in each service environment.

### Run App
Start each service from their directory using build and run.

```bash
cd ClientUI
dotnet build
dotnet run
```

Navigate to [localhost:5000](http://localhost:5000) to place orders via the ClientUI. Orders will be cached locally until the Sales service starts before processing orders (likewise with Billing). Telemetry data will be exported to Honeycomb.