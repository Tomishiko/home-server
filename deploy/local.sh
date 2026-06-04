#!/bin/bash

# --- CONFIGURATION ---
APP_NAME="Filesharing"
BUILD_DIR="./publish"
REMOTE_USER="yura"
REMOTE_HOST="192.168.2.102"
REMOTE_DEST="/var/www/filesharing"
RUNTIME="linux-x64"

# --- 1. CLEAN & PUBLISH ---
echo "--- Starting build for $RUNTIME ---"
# Remove old build files
rm -rf $BUILD_DIR

# Publish as a self-contained single file for easier deployment
dotnet publish ./src/web/web.csproj\
               -c Release \
               -r $RUNTIME \
               -p:PublishSingleFile=true \
               -o $BUILD_DIR

if [ $? -ne 0 ]; then
    echo "Error: Build failed."
    exit 1
fi

# --- 2. TRANSFER TO HOST ---
echo "--- Transferring files to $REMOTE_HOST ---"

# Create destination directory if it doesn't exist
ssh $REMOTE_USER@$REMOTE_HOST "sudo mkdir -p $REMOTE_DEST && sudo chown $REMOTE_USER:$REMOTE_USER $REMOTE_DEST"

rsync -avz $BUILD_DIR/ $REMOTE_USER@$REMOTE_HOST:$REMOTE_DEST/

if [ $? -ne 0 ]; then
    echo "Error: Transfer failed."
    exit 1
fi

# --- 3. RESTART SERVICE ---
echo "--- Restarting the application service ---"
ssh $REMOTE_USER@$REMOTE_HOST "sudo systemctl restart filesharing.service"

echo "--- Deployment Successful! ---"
