#!/bin/bash
# Script to format C# code and fix style issues

echo "🔍 Formatting C# code..."
dotnet format

if [ $? -eq 0 ]; then
    echo "✅ Code formatting complete!"
else
    echo "⚠️  Some formatting issues could not be fixed automatically."
fi
