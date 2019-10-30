curl -vs http://localhost:5001/api/people/1       \
    -H "Accept: application/vnd.api+json"           \
    -H "Content-Type: application/vnd.api+json"     \
    -X PATCH                                        \
    -d '{
            "data": {
                "type": "people",
                "attributes": {
                    "name": "Bob"
                }
            }
        }'