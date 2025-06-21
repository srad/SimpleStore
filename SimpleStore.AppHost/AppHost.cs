using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var username = builder.AddParameter("keycloak-username");
var password = builder.AddParameter("keycloak-password", secret: true);
var clientSecret = builder.AddParameter("client-secret", secret: true);

var keycloak = builder.AddKeycloak("keycloak", adminUsername: username, adminPassword: password)
    .WithDataVolume()
    .WithRealmImport("../Resources/realm-export.json");

var api = builder.AddProject<SimpleStore>("api", launchProfileName: "https")
    .WithEnvironment("Realm", "Test")
    .WithReference(keycloak)
    .WaitFor(keycloak);

var frontend = builder.AddProject<SimpleStore_Admin>("frontend", launchProfileName: "https")
    .WithEnvironment("apikey", "")
    .WithEnvironment("Authentication:OpenIdConnect:Realm", "Test")
    .WithEnvironment("Authentication:OpenIdConnect:ClientId", "simplestore-admin")
    .WithEnvironment("Authentication:OpenIdConnect:ClientSecret", clientSecret)
    .WithExternalHttpEndpoints()
    .WithReference(keycloak)
    .WithReference(api)
    .WaitFor(api)
    .WaitFor(keycloak);

builder.Build().Run();