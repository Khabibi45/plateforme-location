# PlateformeLocationDisques Project Rules

## Code Change Verification Rule

**After every code change, you MUST verify:**

1. **Build succeeds**
   ```bash
   dotnet build
   ```
   No compilation errors or warnings related to changes.

2. **All tests pass**
   ```bash
   dotnet test --no-build
   ```
   Current baseline: **17/17 tests passing**

3. **Web app starts and responds**
   ```bash
   dotnet run --project src/PlateformeLocationDisques.WebApi/PlateformeLocationDisques.WebApi.csproj > /tmp/webapp.log 2>&1 &
   sleep 5
   curl -s http://localhost:5078
   pkill -f "dotnet run"
   ```
   Expected response: `Plateforme Location Disques API`

## Success Criteria
✅ Build succeeds with no errors  
✅ All 17 tests pass  
✅ Web app starts without errors  
✅ Web app responds to HTTP requests at `http://localhost:5078`

## Troubleshooting

**If tests fail:**
- Check test output for specific failures
- Review the changes made
- Fix code and repeat verification

**If web app fails to start:**
- Check `/tmp/webapp.log` for error details
- Look for database connection or configuration issues
- Verify `launchSettings.json` is properly configured
- Fix and repeat verification

## Technical Context

- Test environment uses `ASPNETCORE_ENVIRONMENT=Test` to enable InMemory databases
- Development environment uses PostgreSQL connections
- Never skip web app verification - ensures production readiness
- XUnit fixtures use `nameof` for type-safe collection references
