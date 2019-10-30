curl -vs http://localhost:5001/api/articles         \
    -H "Accept: application/vnd.api+json"           \
    -H "Content-Type: application/vnd.api+json"     \
    -d '{
            "data": {
                "type": "articles",
                "attributes": {
                    "title": "Moby"
                },
                "relationships": {
                    "author": {
                        "data": {
                            "type": "people",
                            "id": "1"
                        }
                    }
                }
            }
        }'