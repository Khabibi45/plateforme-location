# EF Core & CQRS Best Practices : Performance and Pitfalls

This guide details best practices for using Entity Framework Core with CQRS in a Vertical Slice Architecture (VSA), focusing on performance, scalability, and avoiding common pitfalls.

## 1. CQRS Separation (Queries vs Commands)

In CQRS, we distinguish between **Commands** (writes) and **Queries** (reads).

### Queries (Reads)
- **Always use `AsNoTracking()`**: This avoids the overhead of tracking changes, which is unnecessary for read operations.
- **Use Projections (`Select`)**: Only fetch the columns you need. Avoid fetching whole entities if only a subset of properties is required for the DTO/BFF response.
- **Data Shaping at DB level**: Projection allows the database to perform the data shaping, reducing the amount of data transferred and memory used by the application.
- **Avoid Heavy Entities**: Don't load entity trees with multiple collections unless they are explicitly needed for the response.

### Commands (Writes)
- **Keep Entities for business logic**: Unlike queries, commands usually need the full entity to enforce business invariants and save changes.
- **Batching**: EF Core handles batching automatically, but be mindful of the number of entities in a single `SaveChanges()` call.
- **Wolverine Transactions**: Use `opts.UseEntityFrameworkCoreTransactions()` to automatically manage transaction boundaries around your handlers.

## 2. Avoiding N+1 Select Pitfalls

The N+1 problem occurs when the application makes one query to fetch a list of items and then N additional queries to fetch related data for each item.

### Solutions:
- **Eager Loading (`Include`)**: Use `.Include(x => x.Related)` to fetch related data in a single (or split) query.
- **Projection**: Projecting into a DTO automatically includes the required fields if mapped correctly, often more efficiently than `Include`.
- **Split Queries**: For entities with multiple collections, use `.AsSplitQuery()` to avoid cartesian products that can explode the result set size.

## 3. Filtering and Pagination

### Database-Side Execution
- **Always filter in the database**: Use `Where()` before `ToListAsync()`, `FirstOrDefaultAsync()`, etc.
- **Avoid In-Memory Filtering**: Never use `ToListAsync()` and then `.Where(...)` in C#. This pulls the entire table into the application's memory.
- **Pagination**: Always use `.Skip()` and `.Take()` at the end of your query before execution.

### Ordering
- **Always provide a stable sort**: When using pagination, ensure you have a deterministic sort order (e.g., `.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)`).

## 4. EF Core Performance Features

### Compiled Queries
For very high-frequency queries, consider using `EF.CompileQuery` to avoid the overhead of query translation on every call.

### Tagging Queries
Use `.TagWith("MyQueryDescription")` to make it easier to identify queries in database logs or profilers.

### Global Query Filters
Use them for cross-cutting concerns like "Soft Delete" (`IsDeleted == false`), but be aware of how to disable them (`IgnoreQueryFilters()`) when necessary.

## 5. Specifics for Modular Monolith / VSA

### DbContext Isolation
- Each module MUST use its own `DbContext`.
- Never perform cross-module joins in a single LINQ query.
- If data from another module is needed, fetch it via its ID (correlation) or through an asynchronous event/message.

### In-Memory Database for Testing vs Production
- **Warning**: `InMemoryDatabase` behaves differently than relational databases (PostgreSQL/SQL Server). It does not support many relational features (like `jsonb` or complex `Where` clauses).
- **Practice**: Always test your complex queries against a real database (e.g., using Testcontainers) to ensure they translate correctly.

## 6. Common Pitfalls Checklist

1. [ ] **N+1 Query**: Check if any loop in your code or projection is triggering extra database calls.
2. [ ] **Large Result Sets**: Are you missing `Take()` on a query that could return thousands of rows?
3. [ ] **Tracking for Reads**: Did you forget `AsNoTracking()` on a query?
4. [ ] **Heavy `Include`**: Are you loading large blobs or unused collections?
5. [ ] **Client-side evaluation**: Check logs for warnings about queries that couldn't be translated to SQL and are being evaluated in-memory.
