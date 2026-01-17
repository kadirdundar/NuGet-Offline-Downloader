#!/bin/bash

# Temiz bir başlangıç yapalım
rm -rf dist

echo "🚀 Windows (x64) için derleniyor..."
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true -o ./dist/windows

echo "🍏 macOS (Apple Silicon - M1/M2/M3) için derleniyor..."
dotnet publish -c Release -r osx-arm64 -p:PublishSingleFile=true --self-contained true -o ./dist/macos

echo "✅ İşlem tamamlandı!"
echo "Dosyalarınızı şurada bulabilirsiniz:"
echo "   Windows: ./dist/windows/NuGetDownloader.exe"
echo "   macOS:   ./dist/macos/NuGetDownloader"
