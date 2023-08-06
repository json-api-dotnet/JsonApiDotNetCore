curl -s -f http://localhost:14141/api/people        `
    -H "Content-Type: application/vnd.api+json"     `
    -d '{
            \"data\": {
                \"type\": \"people\",
                \"attributes\": {
                    \"name\": \"Alice\"
                }
            }
        }'
