#!/bin/bash
# generates ./request-examples documents

function cleanup() {
    kill -9 $(lsof -ti tcp:5001) &2>/dev/null
}

cleanup
dotnet run -p ../src/Examples/GettingStarted/GettingStarted.csproj &
app_pid=$!
echo "Started app with PID $app_pid"

rm -rf ./request-examples/*.json
rm -rf ./request-examples/*.temp

{ # try
    sleep 10

    echo "sleep over"

    for path in ./request-examples/*.sh; do
        op_name=$(basename "$path" .sh)
        file="./request-examples/$op_name-Response.json"
        temp_file="./request-examples/$op_name-Response.temp"

        # 1. execute bash script
        # 2. redirect stdout to a temp file, this will be the JSON output
        # 3. redirect stderr to JSON file, this will be the curl verbose output
        #    we grab the last line, trim the prefix, add some newlines and the push
        #    it to the top of the JSON file
        bash $path \
            1> >(jq . > $temp_file) \
            2> >(grep "HTTP" | tail -n 1 | cut -c 3- | awk '{ printf "%s\n\n",$0 }' > "./request-examples/$op_name-Response.json")

        # append the actual JSON to the file
        cat $temp_file >> $file
        rm $temp_file
    done
}

# docfx metadata

cleanup
