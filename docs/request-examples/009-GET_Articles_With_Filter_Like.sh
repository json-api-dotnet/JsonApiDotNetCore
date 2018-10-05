curl -vs http://localhost:5001/api/people?filter%5Bname%5D=like:Al     \
    -H "Accept: application/vnd.api+json"