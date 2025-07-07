#!/bin/bash

# 🔧 Configuratie
PROJECT="IntermissionsPwaApp"
OUTPUT="build"
DEPLOY="docs"
GH_BASE="/Intermissions_PWA/"

echo "🧹 Cleaning up previous build..."
rm -rf $OUTPUT
rm -rf $DEPLOY

echo "🏗️ Building Blazor project..."
dotnet publish "./$PROJECT/$PROJECT.csproj" -c Release -o $OUTPUT

echo "📁 Copying build to docs/..."
mkdir $DEPLOY
cp -r $OUTPUT/wwwroot/* $DEPLOY/

echo "📝 Adding .nojekyll..."
touch $DEPLOY/.nojekyll

echo "🔁 Updating base href in index.html..."
INDEX="$DEPLOY/index.html"
sed -i '' "s|<base href=\".*\" />|<base href=\"$GH_BASE\" />|" "$INDEX"

echo "✅ Done! Commit and push the docs folder to GitHub Pages."
