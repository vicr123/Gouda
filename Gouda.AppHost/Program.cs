var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.Gouda_ApiService>("apiservice");
var botService = builder.AddProject<Projects.Gouda_Bot>("botservice");
var pg = builder.AddPostgres("database");
var pgDb = pg.AddDatabase("gouda");

apiService
    .WithExternalHttpEndpoints()
    .WithReference(pgDb)
    .WithReference(botService)
    .WaitFor(botService);

builder.Build().Run();
