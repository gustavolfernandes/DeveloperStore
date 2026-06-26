var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("ambev-developerevaluation-pgdata")
    .WithLifetime(ContainerLifetime.Persistent);

var database = postgres.AddDatabase("developer-evaluation", databaseName: "developer_evaluation");

builder.AddProject<Projects.Ambev_DeveloperEvaluation_WebApi>("webapi")
    .WithReference(database)
    .WaitFor(database)
    .WithEnvironment("ConnectionStrings__DefaultConnection", database.Resource.ConnectionStringExpression);

builder.Build().Run();
