# Webhook delivery platform — requirements

## Problem statement

Design a system that allows internal services to publish events, and your platform reliably delivers those events to third-party customer endpoints via HTTP webhooks.

### Example

| Concept | Example |
|--------|---------|
| Event | `order.created` |
| Customer subscription | Endpoint `https://customer.com/webhooks` receives the event payload |

Your system must deliver the event payload to that endpoint.

---

## Functional requirements

1. **Event publishing** — Internal systems can create webhook events.
2. **Endpoints** — Each customer can register one or more webhook endpoints.
3. **Subscriptions** — Customers can subscribe to event types.
4. **Asynchronous delivery** — Webhook deliveries must be asynchronous.
5. **Retries** — Failed deliveries should be retried.
6. **Idempotency** — Each delivery should include an idempotency key or event ID.
7. **Delivery status** — Users should be able to query delivery status.

---

## Non-functional requirements

| Area | Requirement |
|------|----------------|
| Reliability | High reliability |
| Delivery semantics | At-least-once delivery |
| Consumers | Idempotent processing support |
| Load | Scalability for burst traffic |
| Operations | Visibility into failures |
| Retries | Safe retry behavior |
| Performance | Reasonable latency |
