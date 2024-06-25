#!/bin/bash

# Define variables
APP_NAME="MSURandomizer.app"
ZIP_FILE="MSURandomizer.zip"
PUBLISH_OUTPUT_DIRECTORY="MSURandomizer/bin/Release/net8.0/osx-arm64/publish"
INFO_PLIST="Info.plist"
ICON_FILE="logo.icns"

# Remove old .app bundle if it exists
if [ -d "$APP_NAME" ]; then
    rm -rf "$APP_NAME"
fi

# Create the .app bundle structure
mkdir -p "$APP_NAME/Contents/MacOS"
mkdir -p "$APP_NAME/Contents/Resources"

# Copy the Info.plist file and the icon
# cp "$INFO_PLIST" "$APP_NAME/Contents/Info.plist"
cp "$ICON_FILE" "$APP_NAME/Contents/Resources/logo.icns"

# Copy the published output to the MacOS directory
cp -a "$PUBLISH_OUTPUT_DIRECTORY/." "$APP_NAME/Contents/MacOS"

echo "Packaged $APP_NAME successfully."

mkdir -p "Setup/output"

# Zip the .app bundle
zip -r "Setup/output/$ZIP_FILE" "$APP_NAME"