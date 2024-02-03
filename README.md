# .NET Microservice Nextcloud app demo

## General dev setup

- Run bash e.g. WSL not PowerShell! Some strange quote problems and execute the following steps
- `docker exec -it --user www-data nextcloud php occ config:system:set loglevel --value=0`
- `docker exec -it --user www-data nextcloud php occ app:install app_api`
- Now you need to create the manual_install daemon via UI (e. g. http://localhost:8080/settings/admin/app_api, host=host.docker.internal)
- Set the shared secret in the `docker-compose.yml` file via `Nextcloud__Secret` env var and use the same for the next cmd

### Register the app via local daemon

- `docker exec -it --user www-data nextcloud php occ app_api:app:register --force-scopes --wait-finish --json-info "{\"appid\":\"dotnet_microservice\",\"name\":\".NET Microservice\",\"daemon_config_name\":\"manual_install\",\"version\":\"1.0.0\",\"secret\":\"12345\",\"port\":8080,\"scopes\":[\"FILES\"],\"system_app\":0}" dotnet_microservice manual_install`

### Register the app via local daemon with Visual Studio debugger attached

- `docker exec -it --user www-data nextcloud php occ app_api:app:register --force-scopes --wait-finish --json-info "{\"appid\":\"dotnet_microservice\",\"name\":\".NET Microservice\",\"daemon_config_name\":\"manual_install\",\"version\":\"1.0.0\",\"secret\":\"12345\",\"port\":5047,\"scopes\":[\"FILES\"],\"system_app\":0}" dotnet_microservice manual_install`
