# Test Refactoring Plan: Eliminating Duplication & Fixing Provider Registration
## Enhanced with XUnit Fixtures & Test Collections

## Executive Summary
Refactor test infrastructure to eliminate duplication in `builder.ConfigureServices` blocks while fixing the EF Core provider conflict. Leverage XUnit fixtures and test collections for shared setup, reduced boilerplate, and better test organization.

## Current State
- **Problem 1**: All test classes (BrowseReleasesTests, ImportMasterReleaseTests, QueryReleasesTests, CustomersFeaturesTests) contain identical duplicated code for configuring DbContext and services
- **Problem 2**: Tests fail with "Services for database providers 'Npgsql.EntityFrameworkCore.PostgreSQL', 'Microsoft.EntityFrameworkCore.InMemory' have been registered"
- **Current Approach**: Using test base classes (DiscogsTestBase, CustomersTestBase) with CreateHostAsync method

## Root Cause of Provider Conflict

### Why It Happens
1. Program.cs line 23 checks `UseInMemoryDatabase` config to decide provider
2. If config is false/missing → registers Npgsql provider
3. If config is true → registers InMemory provider
4. Test's DiscogsTestBase tries to add InMemory provider via `AddDbContext`
5. Result: Both providers registered → EF Core error

### Why Current Approach Doesn't Work
- `ConfigureAppConfiguration` callback runs AFTER Program.cs has already read config
- By the time we add config in the callback, Program.cs has already made its provider decision
- Service removal doesn't help because the provider is already registered internally by EF Core
- Timing is the fundamental issue

## Solution: Three-Part Approach with XUnit Fixtures & Collections

### Part 1: Modify Program.cs (REQUIRED)
**File**: `src/PlateformeLocationDisques.WebApi/Program.cs`  
**Line**: 23

**Current Code**:
```csharp
var useInMemoryDb = builder.Configuration.GetValue<bool>("UseInMemoryDatabase", false);
```

**New Code**:
```csharp
var useInMemoryDb = builder.Environment.IsEnvironment("Test") || 
                    builder.Configuration.GetValue<bool>("UseInMemoryDatabase", false);
```

**Rationale**: 
- Check environment FIRST before checking config
- When environment is "Test", Program.cs will use InMemory provider from the start
- This prevents Npgsql from ever being registered in test scenarios
- Timing: Environment is set before Program.cs runs, so this check happens at the right moment

### Part 2: Analyze Test Patterns for Intelligent Isolation

#### Categorize Tests by Mutation Pattern

**Read-Only Tests** (can share fixture + dataset):
- `BrowseReleasesTests`: All tests are read-only queries (GetAllGenres, GetAllArtists, SearchReleases)
  - Each test seeds same data (import master/1) then queries
  - No mutations after seeding
  - **Strategy**: Shared fixture with pre-seeded dataset, sequential execution
  
- `QueryReleasesTests` (partial):
  - Read-only: GetMasterReleaseById_Should_Return_DTO, SearchReleases_Should_Return_Paginated_Results, GetReleasesByGenre, GetReleasesByArtist
  - Error-case: GetMasterReleaseById_Should_Return_404, GetMasterReleaseById_Should_Return_400 (no seeding needed)
  - **Strategy**: Separate collections for read-only vs. error-case tests

**Mutating Tests** (require isolation):
- `ImportMasterReleaseTests`: Creates/modifies data via import operations
  - Each test must have clean database
  - **Strategy**: Isolated fixture, fresh database per test

**Error-Case Tests** (minimal setup):
- Tests that verify error handling without seeding
  - No database state needed
  - **Strategy**: Lightweight fixture, no pre-seeding

### Part 2a: Create Shared Read-Only Fixture with Pre-Seeded Data

#### Create `DiscogsReadOnlyFixture` (IAsyncLifetime)
**File**: `PlateformeLocationDisques.Tests/Helpers/DiscogsReadOnlyFixture.cs`

**Purpose**: Shared fixture for read-only tests with pre-seeded dataset
- Implements `IAsyncLifetime` for async initialization/cleanup
- Creates WebApplicationBuilder with "Test" environment
- Configures FakeDiscogsClient override
- **Seeds data once during initialization** (import master/1)
- Provides `Host` property for test access
- Handles disposal of host resources

**Key Features**:
- Single host instance shared across all read-only tests
- Pre-seeded dataset initialized once, reused by all tests
- Sequential execution prevents race conditions
- Tests must be read-only (no mutations allowed)

### Part 2b: Create Isolated Fixture for Mutating Tests

#### Create `DiscogsIsolatedFixture` (IAsyncLifetime)
**File**: `PlateformeLocationDisques.Tests/Helpers/DiscogsIsolatedFixture.cs`

**Purpose**: Isolated fixture for mutating tests with fresh database per test
- Implements `IAsyncLifetime` for async initialization/cleanup
- Creates WebApplicationBuilder with "Test" environment
- Configures FakeDiscogsClient override
- **No pre-seeding** (each test starts with clean database)
- Provides `Host` property for test access
- Handles disposal of host resources

**Key Features**:
- Fresh host instance per test (no sharing)
- Clean database state for each test
- Tests can mutate data without affecting others
- Slightly higher overhead but guarantees isolation

### Part 2c: Create Lightweight Fixture for Error-Case Tests

#### Create `DiscogsErrorCaseFixture` (IAsyncLifetime)
**File**: `PlateformeLocationDisques.Tests/Helpers/DiscogsErrorCaseFixture.cs`

**Purpose**: Lightweight fixture for error-case tests with minimal setup
- Implements `IAsyncLifetime` for async initialization/cleanup
- Creates WebApplicationBuilder with "Test" environment
- Configures FakeDiscogsClient override
- **No seeding** (tests verify error handling, not data)
- Provides `Host` property for test access
- Handles disposal of host resources

**Key Features**:
- Minimal setup overhead
- Can be shared across multiple error-case tests
- Sequential execution acceptable (no data mutations)

#### Create `CustomersFixture` (IAsyncLifetime)
**File**: `PlateformeLocationDisques.Tests/Helpers/CustomersFixture.cs`

**Purpose**: Encapsulate host creation and cleanup for Customers-related tests
- Implements `IAsyncLifetime` for async initialization/cleanup
- Creates WebApplicationBuilder with "Test" environment
- Provides `Host` property for test access
- Handles disposal of host resources

**Key Features**:
- Separate fixture allows independent configuration if needed
- Follows same pattern as Discogs fixtures for consistency
- Can be extended with Customers-specific overrides

### Part 3: Create XUnit Test Collections with Intelligent Isolation Strategy

#### Create `DiscogsReadOnlyCollection` (ICollectionFixture)
**File**: `PlateformeLocationDisques.Tests/Modules/DiscogsImportation/DiscogsReadOnlyCollection.cs`

**Purpose**: Define a named collection for read-only tests sharing pre-seeded dataset
- Declares `[CollectionDefinition("Discogs Read-Only Collection")]`
- Implements `ICollectionFixture<DiscogsReadOnlyFixture>`
- Marks read-only tests with `[Collection("Discogs Read-Only Collection")]`

**Test Classes in This Collection**:
- `BrowseReleasesTests` (all 5 tests are read-only)
- `QueryReleasesTests` (subset: GetMasterReleaseById_Should_Return_DTO, SearchReleases_Should_Return_Paginated_Results, GetReleasesByGenre, GetReleasesByArtist)

**Benefits**:
- Single host instance shared across all read-only tests
- Pre-seeded dataset initialized once, reused by all tests
- Tests run sequentially (prevents race conditions)
- Faster execution (no repeated seeding)
- Clear semantic grouping: "read-only tests"

#### Create `DiscogsIsolatedCollection` (ICollectionFixture)
**File**: `PlateformeLocationDisques.Tests/Modules/DiscogsImportation/DiscogsIsolatedCollection.cs`

**Purpose**: Define a named collection for mutating tests with isolated fixtures
- Declares `[CollectionDefinition("Discogs Isolated Collection")]`
- Implements `ICollectionFixture<DiscogsIsolatedFixture>`
- Marks mutating tests with `[Collection("Discogs Isolated Collection")]`

**Test Classes in This Collection**:
- `ImportMasterReleaseTests` (all tests mutate data via imports)

**Benefits**:
- Fresh host instance per test (no sharing)
- Clean database state for each test
- Tests can mutate data without affecting others
- Guarantees isolation and test independence
- Clear semantic grouping: "isolated tests"

#### Create `DiscogsErrorCaseCollection` (ICollectionFixture)
**File**: `PlateformeLocationDisques.Tests/Modules/DiscogsImportation/DiscogsErrorCaseCollection.cs`

**Purpose**: Define a named collection for error-case tests with minimal setup
- Declares `[CollectionDefinition("Discogs Error-Case Collection")]`
- Implements `ICollectionFixture<DiscogsErrorCaseFixture>`
- Marks error-case tests with `[Collection("Discogs Error-Case Collection")]`

**Test Classes in This Collection**:
- `QueryReleasesTests` (subset: GetMasterReleaseById_Should_Return_404, GetMasterReleaseById_Should_Return_400)

**Benefits**:
- Minimal setup overhead (no seeding)
- Can be shared across multiple error-case tests
- Sequential execution acceptable (no data mutations)
- Clear semantic grouping: "error-case tests"

#### Create `CustomersCollection` (ICollectionFixture)
**File**: `PlateformeLocationDisques.Tests/Modules/Customers/CustomersCollection.cs`

**Purpose**: Define a named collection for Customers tests
- Declares `[CollectionDefinition("Customers Collection")]`
- Implements `ICollectionFixture<CustomersFixture>`
- Marks all Customers tests with `[Collection("Customers Collection")]`

**Benefits**:
- Shared fixture initialization/cleanup
- Tests run sequentially (prevents concurrent DB access issues)
- Clear grouping of related tests

### Part 4: Refactor Test Classes to Use Appropriate Fixtures

#### Update BrowseReleasesTests
**Changes**:
- Remove inheritance from `DiscogsTestBase`
- Add `[Collection("Discogs Read-Only Collection")]` attribute
- Accept `DiscogsReadOnlyFixture` via constructor injection
- Replace `CreateHostAsync()` calls with `_fixture.Host`
- Remove all `builder.ConfigureServices` blocks (fixture handles it)
- Remove seeding code from each test (fixture pre-seeds once)

**Result**: Tests become pure read-only assertions, no setup duplication

#### Update QueryReleasesTests (Read-Only Methods)
**Methods**: GetMasterReleaseById_Should_Return_DTO, SearchReleases_Should_Return_Paginated_Results, GetReleasesByGenre, GetReleasesByArtist

**Changes**:
- Remove inheritance from `DiscogsTestBase`
- Add `[Collection("Discogs Read-Only Collection")]` attribute
- Accept `DiscogsReadOnlyFixture` via constructor injection
- Replace `CreateHostAsync()` calls with `_fixture.Host`
- Remove all `builder.ConfigureServices` blocks
- Remove seeding code (fixture pre-seeds once)

**Result**: Read-only tests share pre-seeded dataset

#### Update QueryReleasesTests (Error-Case Methods)
**Methods**: GetMasterReleaseById_Should_Return_404, GetMasterReleaseById_Should_Return_400

**Changes**:
- Remove inheritance from `DiscogsTestBase`
- Add `[Collection("Discogs Error-Case Collection")]` attribute
- Accept `DiscogsErrorCaseFixture` via constructor injection
- Replace `CreateHostAsync()` calls with `_fixture.Host`
- Remove all `builder.ConfigureServices` blocks
- Keep as-is (no seeding needed)

**Result**: Error-case tests have minimal setup overhead

**Note**: QueryReleasesTests is now split across two collections. This is acceptable in XUnit; a test class can have multiple test methods in different collections by using conditional logic or by splitting the class.

#### Update ImportMasterReleaseTests
**Changes**:
- Remove inheritance from `DiscogsTestBase`
- Add `[Collection("Discogs Isolated Collection")]` attribute
- Accept `DiscogsIsolatedFixture` via constructor injection
- Replace `CreateHostAsync()` calls with `_fixture.Host`
- Remove all `builder.ConfigureServices` blocks (fixture handles it)
- Keep seeding code (each test needs fresh database)

**Result**: Mutating tests have guaranteed isolation

#### Update CustomersFeaturesTests
**Changes**:
- Remove inheritance from `CustomersTestBase`
- Add `[Collection("Customers Collection")]` attribute
- Accept `CustomersFixture` via constructor injection
- Replace `CreateHostAsync()` calls with `_fixture.Host`
- Remove all `builder.ConfigureServices` blocks (fixture handles it)

**Result**: Test classes become focused on test logic, not setup

### Part 5: Remove Base Classes

- Delete `PlateformeLocationDisques.Tests/Helpers/DiscogsTestBase.cs`
- Delete `PlateformeLocationDisques.Tests/Helpers/CustomersTestBase.cs`
- Delete `PlateformeLocationDisques.Tests/Helpers/TestHostBuilder.cs` (if exists)
- All setup logic moved to fixtures

**Rationale**: Fixtures are more XUnit-idiomatic than base classes; composition over inheritance

## Implementation Sequence

### Step 1: Modify Program.cs
- Add environment check to line 23
- No other changes needed
- This is the critical fix that enables everything else
- **Verification**: No immediate test run needed yet; this enables fixture setup

### Step 2: Create DiscogsReadOnlyFixture
- Create `PlateformeLocationDisques.Tests/Helpers/DiscogsReadOnlyFixture.cs`
- Implement `IAsyncLifetime` interface
- Extract host creation logic from DiscogsTestBase
- Configure FakeDiscogsClient override
- **Seed data once in InitializeAsync()**: Call import master/1 endpoint
- Add `Host` property for test access
- Implement `InitializeAsync()` and `DisposeAsync()`

### Step 3: Create DiscogsIsolatedFixture
- Create `PlateformeLocationDisques.Tests/Helpers/DiscogsIsolatedFixture.cs`
- Implement `IAsyncLifetime` interface
- Extract host creation logic from DiscogsTestBase
- Configure FakeDiscogsClient override
- **No seeding** (each test starts with clean database)
- Add `Host` property for test access
- Implement `InitializeAsync()` and `DisposeAsync()`

### Step 4: Create DiscogsErrorCaseFixture
- Create `PlateformeLocationDisques.Tests/Helpers/DiscogsErrorCaseFixture.cs`
- Implement `IAsyncLifetime` interface
- Extract host creation logic from DiscogsTestBase
- Configure FakeDiscogsClient override
- **No seeding** (error-case tests don't need data)
- Add `Host` property for test access
- Implement `InitializeAsync()` and `DisposeAsync()`

### Step 5: Create CustomersFixture
- Create `PlateformeLocationDisques.Tests/Helpers/CustomersFixture.cs`
- Implement `IAsyncLifetime` interface
- Extract host creation logic from CustomersTestBase
- Add `Host` property for test access
- Implement `InitializeAsync()` and `DisposeAsync()`

### Step 6: Create DiscogsReadOnlyCollection Definition
- Create `PlateformeLocationDisques.Tests/Modules/DiscogsImportation/DiscogsReadOnlyCollection.cs`
- Declare `[CollectionDefinition("Discogs Read-Only Collection")]`
- Implement `ICollectionFixture<DiscogsReadOnlyFixture>`
- No test logic; definition only

### Step 7: Create DiscogsIsolatedCollection Definition
- Create `PlateformeLocationDisques.Tests/Modules/DiscogsImportation/DiscogsIsolatedCollection.cs`
- Declare `[CollectionDefinition("Discogs Isolated Collection")]`
- Implement `ICollectionFixture<DiscogsIsolatedFixture>`
- No test logic; definition only

### Step 8: Create DiscogsErrorCaseCollection Definition
- Create `PlateformeLocationDisques.Tests/Modules/DiscogsImportation/DiscogsErrorCaseCollection.cs`
- Declare `[CollectionDefinition("Discogs Error-Case Collection")]`
- Implement `ICollectionFixture<DiscogsErrorCaseFixture>`
- No test logic; definition only

### Step 9: Create CustomersCollection Definition
- Create `PlateformeLocationDisques.Tests/Modules/Customers/CustomersCollection.cs`
- Declare `[CollectionDefinition("Customers Collection")]`
- Implement `ICollectionFixture<CustomersFixture>`
- No test logic; definition only

### Step 10: Refactor BrowseReleasesTests
- Remove `DiscogsTestBase` inheritance
- Add `[Collection("Discogs Read-Only Collection")]` attribute
- Add constructor accepting `DiscogsReadOnlyFixture` parameter
- Replace all `CreateHostAsync()` calls with `_fixture.Host`
- Remove all `builder.ConfigureServices` blocks
- Remove seeding code from each test (fixture pre-seeds once)
- Keep assertion logic unchanged

### Step 11: Refactor QueryReleasesTests (Read-Only Methods)
- Remove `DiscogsTestBase` inheritance
- Add `[Collection("Discogs Read-Only Collection")]` attribute to read-only methods
- Add constructor accepting `DiscogsReadOnlyFixture` parameter
- For methods: GetMasterReleaseById_Should_Return_DTO, SearchReleases_Should_Return_Paginated_Results, GetReleasesByGenre, GetReleasesByArtist
  - Replace `CreateHostAsync()` calls with `_fixture.Host`
  - Remove `builder.ConfigureServices` blocks
  - Remove seeding code

### Step 12: Refactor QueryReleasesTests (Error-Case Methods)
- Add `[Collection("Discogs Error-Case Collection")]` attribute to error-case methods
- Add constructor accepting `DiscogsErrorCaseFixture` parameter
- For methods: GetMasterReleaseById_Should_Return_404, GetMasterReleaseById_Should_Return_400
  - Replace `CreateHostAsync()` calls with `_fixture.Host`
  - Remove `builder.ConfigureServices` blocks
  - Keep as-is (no seeding needed)

**Note**: QueryReleasesTests now uses two different fixtures via constructor overloading or by accepting both fixtures and using conditionally

### Step 13: Refactor ImportMasterReleaseTests
- Remove `DiscogsTestBase` inheritance
- Add `[Collection("Discogs Isolated Collection")]` attribute
- Add constructor accepting `DiscogsIsolatedFixture` parameter
- Replace all `CreateHostAsync()` calls with `_fixture.Host`
- Remove all `builder.ConfigureServices` blocks
- Keep seeding code in each test (each test needs fresh database)

### Step 14: Refactor CustomersFeaturesTests
- Remove `CustomersTestBase` inheritance
- Add `[Collection("Customers Collection")]` attribute
- Add constructor accepting `CustomersFixture` parameter
- Replace all `CreateHostAsync()` calls with `_fixture.Host`
- Remove all `builder.ConfigureServices` blocks
- Keep test method logic unchanged

### Step 15: Remove Old Base Classes
- Delete `PlateformeLocationDisques.Tests/Helpers/DiscogsTestBase.cs`
- Delete `PlateformeLocationDisques.Tests/Helpers/CustomersTestBase.cs`
- Delete `PlateformeLocationDisques.Tests/Helpers/TestHostBuilder.cs` (if exists)
- Verify no other files reference these base classes

### Step 16: Run Tests
- Run read-only collection: `dotnet test --filter "Discogs Read-Only Collection"`
- Run isolated collection: `dotnet test --filter "Discogs Isolated Collection"`
- Run error-case collection: `dotnet test --filter "Discogs Error-Case Collection"`
- Run all Discogs tests: `dotnet test --filter "DiscogsImportation"`
- Run all Customers tests: `dotnet test --filter "Customers"`
- Run full test suite: `dotnet test`

## Why This Solution Works

### Timing
- `builder.WithEnvironment("Test")` is called BEFORE Program.cs logic executes (in fixture)
- Program.cs checks environment and chooses InMemory provider from the start
- Only InMemory provider is ever registered

### No Conflicts
- Single provider (InMemory) in service collection
- No need to remove services
- No EF Core errors

### XUnit Fixtures Advantages
- **IAsyncLifetime**: Proper async initialization/cleanup lifecycle
- **Constructor Injection**: Fixtures injected by XUnit, no manual creation
- **Isolation**: Each test class gets its own fixture instance (by default)
- **Composition**: Multiple fixtures can be combined in collections
- **Idiomatic**: Fixtures are XUnit's recommended pattern for shared setup

### XUnit Collections Advantages
- **Named Collections**: Clear semantic grouping of related tests
- **Sequential Execution**: Tests in same collection run sequentially (prevents DB race conditions)
- **Shared Fixture**: All tests in collection share same fixture instance (if configured)
- **Discoverability**: Collection name appears in test output for better organization

### Maintainability
- Duplication is eliminated
- Logic is centralized in fixtures (not base classes)
- Future test classes inherit setup by joining collection
- Clear separation: fixtures handle setup, collections handle grouping, tests handle logic
- No inheritance chains; composition-based design

## Expected Outcome

### Before
- ~30 lines of duplicated code per test class
- Provider conflict errors
- Complex service removal logic
- Hard to maintain and extend
- Inheritance-based setup (base classes)
- No clear test grouping

### After
- Zero duplication in test classes (setup moved to fixtures)
- No provider conflicts
- Clean, simple setup via fixtures
- Easy to add new test classes (just add `[Collection]` attribute and inject fixture)
- Composition-based design (fixtures + collections)
- Clear test organization via named collections
- Sequential test execution prevents DB race conditions
- XUnit-idiomatic patterns throughout

## Risk Assessment

### Low Risk
- Only changes Program.cs environment check (additive, not breaking)
- Fixtures are standard XUnit pattern (well-tested framework feature)
- Collections are declarative (no runtime surprises)
- No changes to test logic itself
- Test classes become simpler (less code = fewer bugs)
- Fixtures handle lifecycle properly via `IAsyncLifetime`

### Mitigation Strategies
- Implement fixtures first, verify they work before refactoring test classes
- Refactor one test class at a time, run tests after each refactor
- Keep old base classes until all test classes are refactored (allows rollback)
- Use same test commands to verify behavior is unchanged

### Verification
- All existing tests should pass without modification to test logic
- No new test code needed; only setup code moves
- Can be verified with existing test suite
- Collection names will appear in test output (easy to verify grouping)

## Alternative Approaches Considered

### Approach 1: Separate Test Program.cs
- Create Program.Test.cs that only registers InMemory
- **Rejected**: Duplicates all of Program.cs, hard to maintain

### Approach 2: Conditional DbContext Registration
- Use factory pattern to create DbContexts
- **Rejected**: Overly complex, doesn't address root cause

### Approach 3: Mock Service Provider
- Replace entire service collection for tests
- **Rejected**: Fragile, breaks if Program.cs changes

### Approach 4: Environment-Based with Base Classes
- Check environment in Program.cs
- Set environment in test setup via base classes
- **Rejected**: Works but inheritance-based; XUnit fixtures are more idiomatic

### Approach 5: Environment-Based with XUnit Fixtures & Collections (Selected)
- Check environment in Program.cs
- Set environment in test setup via fixtures
- Use XUnit collections for test grouping
- **Selected**: Simple, clean, maintainable, XUnit-idiomatic, composition-based

## Files to Create

**Fixtures** (3 Discogs + 1 Customers):
1. `PlateformeLocationDisques.Tests/Helpers/DiscogsReadOnlyFixture.cs` - Shared fixture with pre-seeded data
2. `PlateformeLocationDisques.Tests/Helpers/DiscogsIsolatedFixture.cs` - Isolated fixture for mutating tests
3. `PlateformeLocationDisques.Tests/Helpers/DiscogsErrorCaseFixture.cs` - Lightweight fixture for error cases
4. `PlateformeLocationDisques.Tests/Helpers/CustomersFixture.cs` - Customers fixture

**Collection Definitions** (4 total):
5. `PlateformeLocationDisques.Tests/Modules/DiscogsImportation/DiscogsReadOnlyCollection.cs` - Read-only collection
6. `PlateformeLocationDisques.Tests/Modules/DiscogsImportation/DiscogsIsolatedCollection.cs` - Isolated collection
7. `PlateformeLocationDisques.Tests/Modules/DiscogsImportation/DiscogsErrorCaseCollection.cs` - Error-case collection
8. `PlateformeLocationDisques.Tests/Modules/Customers/CustomersCollection.cs` - Customers collection

## Files to Modify

1. `src/PlateformeLocationDisques.WebApi/Program.cs` - Line 23 (environment check)
2. `PlateformeLocationDisques.Tests/Modules/DiscogsImportation/Features/BrowseReleasesTests.cs` - Add collection & fixture, remove base class
3. `PlateformeLocationDisques.Tests/Modules/DiscogsImportation/Features/QueryReleasesTests.cs` - Add collections & fixtures (two different fixtures), remove base class
4. `PlateformeLocationDisques.Tests/Modules/DiscogsImportation/Features/ImportMasterReleaseTests.cs` - Add collection & fixture, remove base class
5. `PlateformeLocationDisques.Tests/Modules/Customers/Features/CustomersFeaturesTests.cs` - Add collection & fixture, remove base class

## Files to Delete

- `PlateformeLocationDisques.Tests/Helpers/DiscogsTestBase.cs` - Replaced by three specialized fixtures
- `PlateformeLocationDisques.Tests/Helpers/CustomersTestBase.cs` - Replaced by CustomersFixture
- `PlateformeLocationDisques.Tests/Helpers/TestHostBuilder.cs` - No longer needed (if exists)

## Success Criteria

**Fixture Isolation & Sharing**:
- [ ] DiscogsReadOnlyFixture seeds data once and is shared across read-only tests
- [ ] DiscogsIsolatedFixture provides fresh database per test for mutating tests
- [ ] DiscogsErrorCaseFixture has minimal setup (no seeding) for error-case tests
- [ ] BrowseReleasesTests and read-only QueryReleasesTests share pre-seeded dataset
- [ ] ImportMasterReleaseTests each get isolated database
- [ ] Error-case QueryReleasesTests use lightweight fixture

**Code Quality**:
- [ ] All tests pass without provider conflict errors
- [ ] No duplication in test setup code (all moved to fixtures)
- [ ] Fixtures implement `IAsyncLifetime` correctly
- [ ] Collections are defined and test classes reference them
- [ ] Test classes have zero `builder.ConfigureServices` blocks
- [ ] No changes needed to existing test method logic

**Test Organization**:
- [ ] Collection names appear in test output
- [ ] Tests in same collection run sequentially (prevents race conditions)
- [ ] Read-only collection executes faster (shared pre-seeded data)
- [ ] Isolated collection guarantees test independence
- [ ] Error-case collection has minimal overhead

**Extensibility**:
- [ ] New read-only tests can be added by joining DiscogsReadOnlyCollection
- [ ] New mutating tests can be added by joining DiscogsIsolatedCollection
- [ ] New error-case tests can be added by joining DiscogsErrorCaseCollection
- [ ] Fixture injection pattern is clear and consistent
