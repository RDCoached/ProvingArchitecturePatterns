#!/bin/bash

# Clean previous results
rm -rf TestResults

# Run tests with coverage
dotnet test \
  --collect:"XPlat Code Coverage" \
  --results-directory:./TestResults \
  --verbosity quiet \
  --nologo

# Generate full report
reportgenerator \
  -reports:"TestResults/**/coverage.cobertura.xml" \
  -targetdir:"TestResults/CoverageReport" \
  -reporttypes:"Html;TextSummary"

echo ""
echo "========================================="
echo "Code Coverage Report"
echo "========================================="
echo ""
echo "Full report: TestResults/CoverageReport/index.html"
echo ""
echo "Summary (excluding auto-generated code):"
echo ""
echo "Your Application Code Coverage:"
grep -A 1 "OnionArch.Api.Endpoints.OrderEndpoints" TestResults/CoverageReport/Summary.txt
grep -A 1 "OnionArch.Application " TestResults/CoverageReport/Summary.txt
grep -A 1 "OnionArch.Domain " TestResults/CoverageReport/Summary.txt
grep -A 1 "OnionArch.Infrastructure " TestResults/CoverageReport/Summary.txt
echo ""
echo "Note: Auto-generated code (Microsoft.AspNetCore.OpenApi.Generated,"
echo "      System.Runtime.CompilerServices, Migrations) is included in"
echo "      the overall stats but can be ignored."
echo ""
