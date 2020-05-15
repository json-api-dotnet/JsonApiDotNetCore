curl -vs http://localhost:5001/api/people/1         \
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