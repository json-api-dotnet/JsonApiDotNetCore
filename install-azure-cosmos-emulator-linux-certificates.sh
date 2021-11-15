#!/bin/bash

ipaddr="`ifconfig | grep "inet " | grep -Fv 127.0.0.1 | awk '{print $2}' | head -n 1`"
certfile=~/emulatorcert.crt
echo "Certificate file: ${certfile}"

result=1
count=0

while [[ "$result" != "0" && "$count" < "5" ]]; do
  echo "Trying to download certificate ..."
  curl -k https://$ipaddr:8081/_explorer/emulator.pem > $certfile
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
  sudo cp $certfile /usr/local/share/ca-certificates
  sudo update-ca-certificates
else
  echo "Could not download CA certificate!"
  false
fi
