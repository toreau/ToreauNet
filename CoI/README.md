# CoI — Composition over Inheritance in .NET

This repository demonstrates a proposed **best practice** approach to applying **composition over inheritance** in .NET.

The example models payment processing by composing small, focused services instead of building a deep inheritance hierarchy. Rather than deriving specialized payment processors from a shared base class, the application separates responsibilities into contracts, resolution logic, concrete gateway implementations, and orchestration. The result is a design that is easier to extend, test, and reason about.

## Why this approach

Composition over inheritance is a long-standing design principle, but in day-to-day .NET codebases it is often implemented inconsistently. This sample shows a practical, idiomatic way to structure it using:

* interfaces for behavior contracts
* dedicated services for decision-making
* a factory for controlled object selection
* DI registration as the composition root
* keyed services for explicit implementation resolution

This keeps business rules out of concrete implementations and avoids brittle inheritance trees where behavior is coupled through base classes.

## What this sample demonstrates

The application processes payments by selecting a payment gateway based on the incoming request.

At a high level:

1. `PaymentService` accepts a `PaymentRequest`
2. `PaymentGatewayFactory` determines which gateway should be used
3. `PaymentGatewayResolver` maps the request to a `PaymentGatewayKey`
4. the factory resolves the keyed `IPaymentGateway` implementation from DI
5. the selected gateway processes the payment
6. logging captures the flow end-to-end

This is composition because the application behavior emerges from collaborating components, each with a narrow responsibility.

## Design overview

### Contracts

The interfaces define the behavioral boundaries of the system:

* `IPaymentGateway` defines how a gateway processes a payment
* `IPaymentGatewayFactory` defines how a gateway is selected for a request
* `IPaymentGatewayResolver` defines how business rules map a request to a gateway key

This keeps the codebase oriented around abstractions rather than implementation details.

### Concrete implementations

Each payment gateway is implemented as an independent class:

* `VippsPaymentGateway`
* `PayPalPaymentGateway`

These classes do not inherit from a common base class with embedded shared logic. They only implement the behavior contract required by `IPaymentGateway`.

That is a key part of the pattern: concrete implementations stay small and self-contained.

### Resolver

`PaymentGatewayResolver` owns the business rule that decides which gateway to use.

In this sample:

* `NOK` routes to `Vipps`
* `USD` and `EUR` route to `PayPal`
* unsupported currencies throw an exception

This decision logic is intentionally separated from the gateways themselves and from the application service. That separation keeps the orchestration layer clean and prevents conditionals from spreading across the codebase.

### Factory

`PaymentGatewayFactory` bridges business decision-making and dependency resolution.

It:

* asks the resolver for the correct `PaymentGatewayKey`
* resolves the matching keyed `IPaymentGateway` from the DI container
* logs which implementation was selected

This gives the application a single, explicit place for implementation selection without leaking service provider usage into the rest of the application.

### Application service

`PaymentService` coordinates the use case:

* validate the request argument
* log the start of processing
* get the correct gateway from the factory
* execute the payment
* log the result

It does not know how the gateway is selected and does not depend on concrete gateway classes. That keeps it stable as new payment providers are introduced.

### Composition root

`Program.cs` is the composition root.

It wires together:

* logging
* keyed gateway registrations
* resolver registration
* factory registration
* application service registration

This is where implementations are bound to abstractions. The rest of the application operates on contracts.

## Why this is a strong composition-over-inheritance example

This sample avoids common inheritance-driven designs such as:

* a `PaymentGatewayBase` class with optional overridable hooks
* subclasses inheriting validation, logging, and selection behavior
* conditional branching inside abstract base classes
* hard-to-follow template method patterns for simple workflows

Instead, the design favors:

* explicit dependencies
* single-purpose classes
* replaceable implementations
* centralized composition
* business rules isolated in dedicated services

That produces code that is easier to evolve without creating tight coupling between unrelated behaviors.

## Dependency injection setup

The sample uses .NET keyed services to register each payment gateway against a distinct `PaymentGatewayKey`.

```csharp
services.AddKeyedTransient<IPaymentGateway, VippsPaymentGateway>(PaymentGatewayKey.Vipps);
services.AddKeyedTransient<IPaymentGateway, PayPalPaymentGateway>(PaymentGatewayKey.PayPal);

services.AddTransient<IPaymentGatewayResolver, PaymentGatewayResolver>();
services.AddTransient<IPaymentGatewayFactory, PaymentGatewayFactory>();
services.AddTransient<PaymentService>();
```

This is an effective pattern when:

* multiple implementations share the same interface
* runtime selection is required
* selection should be explicit and type-safe
* the application wants to avoid injecting collections and manually filtering implementations

## Request flow

A payment request flows through the system like this:

```text
PaymentRequest
    -> PaymentService
        -> IPaymentGatewayFactory
            -> IPaymentGatewayResolver
                -> PaymentGatewayKey
            -> keyed IPaymentGateway resolution
        -> selected gateway
    -> PaymentResult
```

This flow makes each step observable and testable in isolation.

## Example behavior

Given the resolver rules in this sample:

* a `NOK` payment is processed by `VippsPaymentGateway`
* a `USD` payment is processed by `PayPalPaymentGateway`
* a `JPY` payment results in an `InvalidOperationException`

That behavior is intentionally driven by composition rather than by polymorphism through inheritance chains.

## Benefits of this structure

### 1. Clear separation of concerns

Each class has one job:

* gateways process payments
* resolver chooses the key
* factory resolves the implementation
* service orchestrates the use case

That clarity reduces incidental complexity.

### 2. Easier extensibility

Adding a new payment gateway does not require modifying a base class hierarchy. The typical extension path is:

1. create a new `IPaymentGateway` implementation
2. add a new `PaymentGatewayKey`
3. update the resolver rules
4. register the implementation in DI

That is a straightforward, localized change.

### 3. Better testability

Because behavior is split into narrow units, each part can be tested independently:

* resolver tests for routing logic
* factory tests for resolution behavior
* service tests for orchestration
* gateway tests for processing behavior

This is much simpler than testing behavior spread across base and derived classes.

### 4. Reduced coupling

No gateway depends on another gateway.
No application service depends on concrete gateway types.
No base class dictates lifecycle or extension hooks.

That keeps the dependency graph intentional and maintainable.

### 5. DI-friendly by design

The pattern aligns naturally with the built-in .NET dependency injection model. Contracts are registered once, implementations are selected through composition, and runtime behavior remains explicit.

## Running the sample

This is a standard .NET console application.

```bash
dotnet run
```

When executed, the application will:

* process a `NOK` payment through `Vipps`
* process a `USD` payment through `PayPal`
* attempt a `JPY` payment and log the unsupported-currency error

## Extension example

To add a new gateway such as Stripe, the composition model stays the same:

1. add `Stripe` to `PaymentGatewayKey`
2. implement `StripePaymentGateway : IPaymentGateway`
3. register it with `AddKeyedTransient`
4. update `PaymentGatewayResolver` to route the relevant requests to `Stripe`

The surrounding application service does not need to change because it already depends on composed abstractions.

## When this pattern works especially well

This approach is especially effective when:

* multiple implementations satisfy the same contract
* the implementation choice depends on runtime business rules
* you want to avoid switch statements in the application service
* you want new implementations to be added with minimal friction
* you want to keep orchestration separate from execution details

Examples beyond payments include:

* notification providers
* shipping carriers
* tax calculators
* pricing strategies
* export format handlers
* third-party integrations
