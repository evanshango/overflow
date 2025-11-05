using Microsoft.Extensions.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var compose = builder.AddDockerComposeEnvironment("production")
    .WithDashboard(dashboard => dashboard.WithHostPort(9090));

var postgres = builder.AddPostgres("postgres-svc", port: 5431)
    .WithDataVolume("postgres-data")
    .WithPgAdmin();
    // .WithEndpoint(5431, 5432, "postgres", isExternal: true);

var postgresPassword = builder.Environment.IsDevelopment()
    ? builder.Configuration["Parameters:postgres-svc-password"]
      ?? throw new InvalidOperationException("Could not get postgres db password")
    : "${POSTGRES_SVC_PASSWORD}";

if (string.IsNullOrEmpty(postgresPassword))
    throw new InvalidOperationException("Postgres DB Password found in config");

var keycloakDb = postgres.AddDatabase("keycloak-db");

var keycloakDbUrl = builder.Environment.IsDevelopment()
    ? "jdbc:postgresql://postgres-svc/keycloak-db?currentSchema=public"
    : "${KEYCLOAK_DB_URL}";

var keycloakDbUser = builder.Environment.IsDevelopment()
    ? "postgres"
    : "${KEYCLOAK_DB_USERNAME}";

var keycloakDbType = builder.Environment.IsDevelopment()
    ? builder.Configuration["Parameters:keycloak-db-type"] ?? "postgres"
    : "${KEYCLOAK_DB_TYPE}";

var keycloak = builder.AddKeycloak("keycloak-svc", 6001)
    // .WithDataVolume("keycloak-data")
    .WaitFor(keycloakDb)
    .WithEnvironment("KC_HTTP_ENABLED", "true")
    .WithEnvironment("KC_HOSTNAME_STRICT", "false")
    .WithEnvironment("KC_DB", keycloakDbType)
    .WithEnvironment("KC_DB_USERNAME", keycloakDbUser)
    .WithEnvironment("KC_DB_PASSWORD", postgresPassword)
    .WithEnvironment("KC_DB_URL", keycloakDbUrl)
    .WithEnvironment("VIRTUAL_HOST", "id.overflow.local")
    .WithEnvironment("VIRTUAL_PORT", "8080");

var typesenseApiKey = builder.Environment.IsDevelopment()
    ? builder.Configuration["Parameters:typesense-api-key"]
      ?? throw new InvalidOperationException("Could not get typesense api key")
    : "${TYPESENSE_API_KEY}";

var typesense = builder.AddContainer(
        "typesense-svc", "typesense/typesense", "29.0"
    )
    .WithArgs("--data-dir", "/data", "--api-key", typesenseApiKey, "--enable-cors")
    .WithVolume("typesense-data", "/data")
    .WithEnvironment("TYPESENSE_API_KEY", typesenseApiKey)
    .WithHttpEndpoint(8108, 8108, name: "typesense");

var typesenseContainer = typesense.GetEndpoint("typesense");

var questionDb = postgres.AddDatabase("question-db");

var rabbitMq = builder.AddRabbitMQ("rabbitmq-svc", port: 5673)
    .WithDataVolume("rabbitmq-data")
    .WithManagementPlugin(port: 15673);

var questionSvc = builder.AddProject<QuestionService>("question-svc")
    .WithReference(keycloak)
    .WithReference(questionDb)
    .WithReference(rabbitMq)
    .WaitFor(keycloak)
    .WaitFor(questionDb)
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
        yarpBuilder.AddRoute("/api/v1/search/{**catch-all}", searchSvc);
    })
    .WithEnvironment("ASPNETCORE_URLS", "http://*:8002")
    .WithEndpoint(port: 8002, targetPort: 8002, scheme: "http", name: "gateway", isExternal: true)
    .WithEnvironment("VIRTUAL_HOST", "api.overflow.local")
    .WithEnvironment("VIRTUAL_PORT", "8002");

if (!builder.Environment.IsDevelopment())
{
    builder.AddContainer("nginx-proxy", "nginxproxy/nginx-proxy", "1.8")
        .WithEndpoint(80, 80, "nginx", isExternal: true)
        .WithBindMount("/var/run/docker.sock", "/tmp/docker.sock", true);
}

builder.Build().Run();