curl -s -f http://localhost:14141/api/books/1       `
    -H "Content-Type: application/vnd.api+json"     `
    -X PATCH                                        `
    -d '{
            \"data\": {
                \"type\": \"books\",
                \"id\": "1",
                \"attributes\": {
                    \"publishYear\": 1820
                }
            }
        }'
