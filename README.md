🔍 Transaction Search & Filter API
Overview

The Transaction Search & Filter API is a robust and high-performance querying system designed to retrieve transaction data using complex filter combinations, strict business rules, and optimized database access.
It enables users to search, filter, sort, and paginate transactions while enforcing access control, user-tier limitations, and performance constraints.

The solution is built with flexibility, scalability, and correctness in mind, supporting dynamic query construction, efficient filtering on large datasets, and asynchronous execution for long-running searches.

✨ Key Capabilities

Advanced transaction search with multiple filter combinations

Dynamic query construction using a fluent Query Builder

Strict validation of filter rules based on user tier

Optimized database querying with indexing and caching

Support for full-text search with performance safeguards

Asynchronous execution for heavy queries

Clean separation between validation, query building, and execution

📌 Supported Filters

Users can search transactions by:

Date range (custom or predefined presets)

Amount range or exact amount

Transaction types (Deposit, Withdrawal, Transfer, Refund)

Transaction status (Pending, Completed, Failed, Cancelled)

Recipient name or email (partial, case-insensitive)

Description (full-text search with stemming)

Payment method (Card, Bank Transfer, Wallet, Cash)

User-defined tags or categories

⚠️ Business Rules & Constraints

Date range limits enforced per user tier:

Regular users: up to 90 days

Premium users: up to 365 days

Admins: no limit

Default pagination (20 items), max page size (100)

Queries without date filters default to last 30 days

Full-text search limited to recent data only

Long-running queries automatically executed asynchronously

Strict access control ensures users only see authorized data
