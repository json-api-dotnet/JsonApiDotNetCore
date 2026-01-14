#Requires -Version 7.0

# This script starts a PostgreSQL database in a docker container, which is required for running tests locally.
# When the -UI switch is passed, pgAdmin (a web-based PostgreSQL management tool) is started in a second container, which lets you query the database.
# To connect to pgAdmin, open http://localhost:5050 and login with user "admin@admin.com", password "postgres". Use hostname "db" when registering the server.

param(
    [switch] $UI=$false
)

docker container stop jsonapi-postgresql-db
docker container stop jsonapi-postgresql-management

docker run --pull always --rm --detach --name jsonapi-postgresql-db -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 postgres:latest -N 500

if ($UI) {
    docker run --pull always --rm --detach --name jsonapi-postgresql-management --link jsonapi-postgresql-db:db -e PGADMIN_DEFAULT_EMAIL=admin@admin.com -e PGADMIN_DEFAULT_PASSWORD=postgres -p 5050:80 dpage/pgadmin4:latest
}
