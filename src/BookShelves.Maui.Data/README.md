Process to create/update an EF Migration

1. Update your model classes in the `BookShelves.Maui.Data` project.
1. Open the terminal or command prompt in the directory of the BookShelves.Maui.MigrationHost project.
1. Run the following command to create a new migration:
1. First Run:
   ```pwsh
   dotnet ef migrations add InitialCreate --context SyncDbContext --project ../BookShelves.Maui.Data/

   May not need the --context parameter if you have only one DbContext in your project.
   ```

   Future Runs:
   ```pwsh
   # dotnet ef migrations add <MigrationName> --context SyncDbContext --project ../BookShelves.Maui.Data/
   dotnet ef migrations add <MigrationName> --project ./BookShelves.Maui.Data  --startup-project ./BookShelves.Maui.MigrationHost
   ```
   Replace `<MigrationName>` with a descriptive name for your migration.