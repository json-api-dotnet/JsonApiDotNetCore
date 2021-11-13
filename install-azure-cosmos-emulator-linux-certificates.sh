#!/bin/bash

result=1
count=0

while [[ "$result" != "0" && "$count" < "5" ]]; do
  echo "Trying to download certificate ..."
  curl -k https://localhost:8081/_explorer/emulator.pem > ~/emulatorcert.crt 2> /dev/null
  result=$?
  let "count++"

  if [[ "$result" != "0" && "$count" < "5"  ]]
  then
    echo "Could not download certificate. Waiting 10 seconds before trying again ..."
    sleep 10
  fi
done

if [[ $result -eq 0  ]]
then
  echo "Updating CA certificates ..."
  cp ~/emulatorcert.crt /usr/local/share/ca-certificates/
  update-ca-certificates
else
  echo "Could not download CA certificate!"
fi
