var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Gouda_ApiService>("apiservice");
var botService = builder.AddProject<Projects.Gouda_Bot>("botservice");
var pg = builder.AddPostgres("postgres").WithDataVolume(isReadOnly: false);
var pgDb = pg.AddDatabase("gouda");
var migration = builder.AddProject<Projects.Gouda_MigrationService>("migrationservice");

migration
     .WithReference(pgDb)
     .WaitFor(pgDb);

apiService
    // .WaitForCompletion(migration)
    .WithExternalHttpEndpoints()
    .WithReference(pgDb)
    .WithReference(botService)
    .WaitFor(botService);

botService
    // .WaitForCompletion(migration)
    .WithReference(pgDb);

builder.Build().Run();
