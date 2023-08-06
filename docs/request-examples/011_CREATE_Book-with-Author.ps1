curl -s -f http://localhost:14141/api/books         `
    -H "Content-Type: application/vnd.api+json"     `
    -d '{
            \"data\": {
                \"type\": \"books\",
                \"attributes\": {
                    \"title\": \"Valperga\",
                    \"publishYear\": 1823
                },
                \"relationships\": {
                    \"author\": {
                        \"data\": {
                            \"type\": \"people\",
                            \"id\": \"1\"
                        }
                    }
                }
            }
        }'
