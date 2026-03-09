#!/bin/bash

outputFolder=_output
artifactsFolder=_artifacts
uiFolder="$outputFolder/net10.0/UI"
framework="${FRAMEWORK:=net10.0}"

rm -rf $artifactsFolder
mkdir $artifactsFolder

for runtime in _output/*
do
  name="${runtime##*/}"
  folderName="$runtime/$framework"
  playarrFolder="$folderName/Playarr"
  archiveName="Playarr.$BRANCH.$PLAYARR_VERSION.$name"

  if [[ "$name" == 'net10.0' ]]; then
    continue
  fi
    
  echo "Creating package for $name"

  echo "Copying UI"
  cp -r $uiFolder $playarrFolder
  
  echo "Setting permissions"
  find $playarrFolder -name "ffprobe" -exec chmod a+x {} \;
  find $playarrFolder -name "Playarr" -exec chmod a+x {} \;
  find $playarrFolder -name "Playarr.Update" -exec chmod a+x {} \;
  
  if [[ "$name" == *"osx"* ]]; then
    echo "Creating macOS package"
      
    packageName="$name-app"
    packageFolder="$outputFolder/$packageName"
      
    rm -rf $packageFolder
    mkdir $packageFolder
      
    cp -r distribution/macOS/Playarr.app $packageFolder
    mkdir -p $packageFolder/Playarr.app/Contents/MacOS
      
    echo "Copying Binaries"
    cp -r $playarrFolder/* $packageFolder/Playarr.app/Contents/MacOS
      
    echo "Removing Update Folder"
    rm -r $packageFolder/Playarr.app/Contents/MacOS/Playarr.Update
              
    echo "Packaging macOS app Artifact"
    (cd $packageFolder; zip -rq "../../$artifactsFolder/$archiveName-app.zip" ./Playarr.app)
  fi

  echo "Packaging Artifact"
  if [[ "$name" == *"linux"* ]] || [[ "$name" == *"osx"* ]] || [[ "$name" == *"freebsd"* ]]; then
    tar -zcf "./$artifactsFolder/$archiveName.tar.gz" -C $folderName Playarr
	fi
    
  if [[ "$name" == *"win"* ]]; then
    if [ "$RUNNER_OS" = "Windows" ]
      then
        (cd $folderName; 7z a -tzip "../../../$artifactsFolder/$archiveName.zip" ./Playarr)
      else
      (cd $folderName; zip -rq "../../../$artifactsFolder/$archiveName.zip" ./Playarr)
    fi
	fi
done
