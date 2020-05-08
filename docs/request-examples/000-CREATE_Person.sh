curl -vs http://localhost:5001/api/people           \
    -H "Content-Type: application/vnd.api+json"     \
    -d '{
            "data": {
                "type": "people",
                "attributes": {
                    "name": "Alice"
                }
            }
        }'