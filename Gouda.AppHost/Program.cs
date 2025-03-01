var builder = DistributedApplication.CreateBuilder(args);

var pg = builder.AddPostgres("postgres")
    .WithDataVolume(isReadOnly: false);

var goudaDb = pg.AddDatabase("gouda-db");

var migration = builder.AddProject<Projects.Gouda_MigrationService>("migration-service")
    .WithReference(goudaDb).WaitFor(goudaDb);

var apiService = builder.AddProject<Projects.Gouda_ApiService>("api-service")
    .WithExternalHttpEndpoints()
    .WithReference(goudaDb).WaitFor(goudaDb)
    .WaitForCompletion(migration);

var botService = builder.AddProject<Projects.Gouda_Bot>("bot-service")
    .WithReference(goudaDb).WaitFor(goudaDb)
    .WaitForCompletion(migration);

builder.Build().Run();
