using Microsoft.Extensions.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var compose = builder.AddDockerComposeEnvironment("production")
    .WithDashboard(dashboard => dashboard.WithHostPort(9090));

var postgres = builder.AddPostgres("postgres-svc", port: 5431)
    .WithPgAdmin()
    .WithDataVolume("postgres-data");
// .WithEndpoint(5431, 5432, "postgresql", isExternal: true);

var keycloakDatabase = postgres.AddDatabase("keycloak", "keycloak");

var keycloakDatabaseUrl = builder.Environment.IsDevelopment()
    ? $"jdbc:postgresql://postgres-svc/{keycloakDatabase.Resource.DatabaseName}?currentSchema=public"
    : "${KEYCLOAK_DB_URL}";

var keycloakDatabaseUsername = builder.Environment.IsDevelopment()
    ? postgres.Resource.UserNameReference.ValueExpression
    : "${KEYCLOAK_DB_USERNAME}";

var keycloakDatabasePassword = builder.Environment.IsDevelopment()
    ? builder.Configuration["Parameters:postgres-svc-password"]
      ?? throw new InvalidOperationException("Could not get postgres db password")
    : "${POSTGRES_SVC_PASSWORD}";

var keycloakDbType = builder.Environment.IsDevelopment() ? "postgres" : "${KEYCLOAK_DB_TYPE}";

var keycloak = builder.AddKeycloak("keycloak-svc", 6001)
    .WaitFor(keycloakDatabase)
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithEnvironment("KC_DB", keycloakDbType)
    .WithEnvironment("KC_DB_URL", keycloakDatabaseUrl)
    .WithEnvironment("KC_PROXY_HEADERS", "xforwarded")
    .WithEnvironment("KC_DB_USERNAME", keycloakDatabaseUsername)
    .WithEnvironment("KC_DB_PASSWORD", keycloakDatabasePassword)
    .WithEnvironment("VIRTUAL_HOST", "id.overflow.local")
    .WithEnvironment("VIRTUAL_PORT", "8080");

var typesenseApiKey = builder.Environment.IsDevelopment()
    ? builder.Configuration["Parameters:typesense-api-key"]
      ?? throw new InvalidOperationException("Could not get typesense api key")
    : "${TYPESENSE_API_KEY}";

var typesense = builder.AddContainer("typesense-svc", "typesense/typesense")
    .WithImageTag("29.0")
    .WithArgs("--data-dir", "/data", "--api-key", typesenseApiKey, "--enable-cors")
    .WithVolume("typesense-data", "/data")
    .WithEnvironment("TYPESENSE_API_KEY", typesenseApiKey)
    .WithHttpEndpoint(8108, 8108, name: "typesense");

var typesenseContainer = typesense.GetEndpoint("typesense");

var questionDatabase = postgres.AddDatabase("question");

var rabbitMq = builder.AddRabbitMQ("rabbitmq-svc", port: 5673)
    .WithDataVolume("rabbitmq-data")
    .WithManagementPlugin(port: 15673);

var questionSvc = builder.AddProject<QuestionService>("question-svc")
    .WithReference(keycloak)
    .WithReference(questionDatabase)
    .WithReference(rabbitMq)
    .WaitFor(keycloak)
    .WaitFor(questionDatabase)
    .WaitFor(rabbitMq);

var searchSvc = builder.AddProject<SearchService>("search-svc")
    .WithEnvironment("typesense-api-key", typesenseApiKey)
    .WithReference(typesenseContainer)
    .WithReference(rabbitMq)
    .WaitFor(typesense)
    .WaitFor(rabbitMq);

var yarp = builder.AddYarp("gateway-svc")
    .WithConfiguration(yarpBuilder =>
    {
        yarpBuilder.AddRoute("/api/v1/questions/{**catch-all}", questionSvc);
        yarpBuilder.AddRoute("/api/v1/tags/{**catch-all}", questionSvc);
        yarpBuilder.AddRoute("/api/v1/tests/{**catch-all}", questionSvc);
        yarpBuilder.AddRoute("/api/v1/search/{**catch-all}", searchSvc);
    })
    .WithEnvironment("ASPNETCORE_URLS", "http://*:8002")
    .WithEndpoint(port: 8002, targetPort: 8002, scheme: "http", name: "gateway", isExternal: true)
    .WithEnvironment("VIRTUAL_HOST", "api.overflow.local")
    .WithEnvironment("VIRTUAL_PORT", "8002");

var webapp = builder.AddNpmApp("webapp", "../webapp", "dev")
    .WithReference(keycloak)
    .WithHttpEndpoint(env: "PORT", port: 3000);

if (!builder.Environment.IsDevelopment())
{
    builder.AddContainer("nginx-proxy", "nginxproxy/nginx-proxy", "1.8")
        .WithEndpoint(80, 80, "nginx", isExternal: true)
        .WithBindMount("/var/run/docker.sock", "/tmp/docker.sock", true);
}

builder.Build().Run();