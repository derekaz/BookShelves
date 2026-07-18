Process to create/update an EF Migration

1. Update your model classes in the `BookShelves.Maui.Data` project.
1. Open the terminal or command prompt in the directory of the BookShelves.Maui.MigrationHost project.
1. Run the following command to create a new migration:
1. First Run:
   ```pwsh
   dotnet ef migrations add InitialCreate --context SyncDbContext --project ../BookShelves.Maui.Data/
   ```

   Futre Runs:
   ```pwsh
   dotnet ef migrations add <MigrationName> --context SyncDbContext --project ../BookShelves.Maui.Data/
   ```
   Replace `<MigrationName>` with a descriptive name for your migration.