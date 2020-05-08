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

curl -vs http://localhost:5001/api/articles         \
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
                            "id": "2"
                        }
                    }
                }
            }
        }'